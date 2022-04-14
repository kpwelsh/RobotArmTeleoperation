using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Newtonsoft.Json;
using System;

public class TaskRecord {
    public enum Mode {
        Recording,
        Playing
    };
    public Mode mode {
        get;
        private set;
    }

    [Serializable]
    private struct TaskHeader {
        public string Name;
        public float CompletionTime;
        public TaskHeader(string name, float completionTime) {
            Name = name;
            CompletionTime = completionTime;
        }
    }

    public float PositionThreshold = 0.01f;
    public float RotationThreshold = 0.01f;

    protected Queue<Frame> FrameBuffer = new Queue<Frame>();
    
    private HashSet<string> unfoundNames = new HashSet<string>();
    private Dictionary<Transform, SerializableTrans> Transforms = new Dictionary<Transform, SerializableTrans>();
    private Dictionary<string, Transform> namedTransforms = new Dictionary<string, Transform>();
    private float time = 0;
    private Transform transform = null;
    private TaskHeader taskHeader;

    public TaskRecord(GameObject task) {
        transform = task.transform;
    }

    public TaskRecord(Queue<string> buffer) {
        taskHeader = JsonConvert.DeserializeObject<TaskHeader>(buffer.Dequeue());
        while (buffer.Count > 0) {
            FrameBuffer.Enqueue(JsonConvert.DeserializeObject<Frame>(buffer.Dequeue()));
        }
    }

    public void StartRecording() {
        FrameBuffer.Clear();
        initializeTransforms();
        mode = Mode.Recording;
    }

    public void StartReplaying() {
        GameObject task = GameObject.Instantiate(Resources.Load(taskHeader.Name, typeof(GameObject))) as GameObject;
        transform = task.transform;
        initializeTransforms();
        DisableComponents();
        mode = Mode.Playing;
    }
    
    private void setTransforms(Frame frame) {
        foreach (var sTrans in frame.transforms) {
            Transform trans;
            if (!namedTransforms.TryGetValue(sTrans.id, out trans)) {
                if (!unfoundNames.Contains(sTrans.id)) {
                    unfoundNames.Add(sTrans.id);
                    Debug.LogWarningFormat("Could not find game object in scene: {0}", sTrans.id);
                }
                continue;
            }

            trans.position = sTrans.position;
            trans.rotation = sTrans.rotation;
        }
    }

    public void PlayUntil(float time) {
        Frame? frame = null;
        while (FrameBuffer.Count > 0 && FrameBuffer.Peek().timestamp < time) {
            frame = FrameBuffer.Dequeue();
        }

        if (frame.HasValue) {
            setTransforms(frame.Value);
        }
    }

    public void WriteTo(Endpoint endpoint) {
        string name = transform.gameObject.name.Replace("(Clone)", "");
        endpoint.WriteLine(JsonConvert.SerializeObject(new TaskHeader(name, time)));
        foreach (var frame in FrameBuffer) {
            endpoint.WriteLine(JsonConvert.SerializeObject(frame));
        }
    }
    
    public void RecordFrame(float dt) {
        time += dt;
        Frame frame = new Frame();
        frame.timestamp = time;
        frame.transforms = new List<SerializableTrans>();
        foreach (var trans in namedTransforms.Values) {
            SerializableTrans sTrans = new SerializableTrans(trans, transform.gameObject);
            if (!Transforms.ContainsKey(trans) || Transforms[trans].neq(sTrans, PositionThreshold, RotationThreshold)) {
                Transforms[trans] = sTrans;
                frame.transforms.Add(sTrans);
            }
        }

        if (frame.transforms.Count > 0) {
            FrameBuffer.Enqueue(frame);
        }
    }

    private void initializeTransforms() {
        foreach (var t in transform.AllChildren()) {
            var go = t.gameObject;
            string fullName = go.FullName(transform.gameObject);
            if (namedTransforms.ContainsKey(fullName)) {
                Debug.LogWarningFormat("Found multiple gameobjects with full name: {0}", fullName);
            } else {
                namedTransforms[fullName] = go.transform;
            }
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
