using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Newtonsoft.Json;
using System;

[Serializable]
public class TaskRecord {
    public enum Mode {
        Recording,
        Playing,
        None
    };
    
    public Mode mode {
        get;
        private set;
    }

    public string Name;
    public float CompletionTime;

    [SerializeField] public List<Frame> OrderedFrames = new List<Frame>();
    [NonSerialized] private int currentIndex = 0;

    public float PositionThreshold = 0.01f;
    public float RotationThreshold = 0.01f;
    
    [NonSerialized] private float time = 0;
    [NonSerialized] private Transform transform = null;
    [NonSerialized] private TransformTree transformTree;
    [NonSerialized] private SingleWarning singleWarning = new SingleWarning();
    [NonSerialized] private Dictionary<Transform, SerializableTrans> LastState = new Dictionary<Transform, SerializableTrans>();

    public TaskRecord(GameObject task) {
        Init(task);
    }

    public TaskRecord() { }

    private void Init(GameObject task) {
        singleWarning.Clear();
        LastState.Clear();
        transform = task.transform;
        Name = task.name.Replace("(Clone)", "");
        transformTree = new TransformTree(transform);
        currentIndex = 0;
        time = 0;
    }

    public void StartRecording() {
        singleWarning.Clear();
        OrderedFrames.Clear();
        mode = Mode.Recording;
    }

    public void StartReplaying() {
        GameObject task = GameObject.Instantiate(Resources.Load(Name, typeof(GameObject))) as GameObject;
        Init(task);
        DisableComponents();
        mode = Mode.Playing;
    }

    public void Stop() {
        mode = Mode.None;
    }
    
    private void setTransforms(Frame frame) {
        foreach (var sTrans in frame.transforms) {
            Transform trans;
            if (!transformTree.TryGetValue(sTrans.id, out trans)) {
                singleWarning.LogWarning(String.Format("Could not find game object in scene: {0}", sTrans.id));
                continue;
            }

            trans.localPosition = sTrans.position;
            trans.localRotation = sTrans.rotation;
        }
    }

    public void PlayUntil(float time) {
        Frame? frame = null;
        while (currentIndex < OrderedFrames.Count && OrderedFrames[currentIndex].timestamp < time) {
            frame = OrderedFrames[currentIndex];
            currentIndex++;
        }

        if (frame.HasValue) {
            setTransforms(frame.Value);
        }
    }

    public void RecordFrame(float dt) {
        time += dt;
        if (transform == null)  {
            return;
        }
        Frame frame = new Frame();
        frame.timestamp = time;
        frame.transforms = new List<SerializableTrans>();
        foreach (var kvp in transformTree.IterItems()) {
            string id = kvp.Key;
            Transform trans = kvp.Value;
            var sTrans = new SerializableTrans(id, trans.localPosition, trans.localRotation);
            if (!LastState.ContainsKey(trans) || LastState[trans].neq(sTrans, PositionThreshold, RotationThreshold)) {
                LastState[trans] = sTrans;
                frame.transforms.Add(sTrans);
            }
        }

        if (frame.transforms.Count > 0) {
            OrderedFrames.Add(frame);
        }
    }
    
    private void DisableComponents() {
        // If there is a Robot controller in this, then just turn that off.
        foreach (RobotController rc in transform.GetComponentsInChildren<RobotController>()) {
            rc.enabled = false;
        }

        // Make all of the rigid bodies kinematic so physics doesn't try to change things.
        foreach (Rigidbody rb in transform.GetComponentsInChildren<Rigidbody>()) {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Not sure what to do with this, so Ill just disable these.
        foreach (ArticulationBody ab in transform.GetComponentsInChildren<ArticulationBody>()) {
            ab.enabled = false;
        }
    }
}
