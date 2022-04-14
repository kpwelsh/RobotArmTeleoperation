using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.XR;
using UnityEngine.InputSystem;
public class SystemManager : MonoBehaviour
{   
    [Serializable]
    public enum InputDevice {
        _6DOF,
        Gamepad,
        KeyboardMouse
    }

    [Serializable]
    public enum VisualFeedback {
        VR,
        VRMono,
        SingleCam,
        MultiCam
    }

    [Serializable]
    public enum ModifierLevel {
        VeryLow,
        Low,
        Medium,
        High,
        VeryHigh
    }

    [Serializable]
    public enum Difficulty {
        Easy,
        Medium,
        Hard
    }
    public InputDevice Input;
    public VisualFeedback Feedback;
    public ModifierLevel RobotSpeed;
    public ModifierLevel RobotResponsiveness;
    public ModifierLevel Latency;
    public Difficulty TaskDifficulty;
    public GameObject TargetScene = null;
    public GameObject TargetRobot = null;
    public GameObject Hub;
    public GameObject TaskParent;

    public TaskRecord taskRecord;
    private float time = 0;
    void FixedUpdate() {
        if (taskRecord != null) {
            switch (taskRecord.mode) {
                case TaskRecord.Mode.Recording: {
                    taskRecord.RecordFrame(Time.fixedDeltaTime);
                    break;
                }
                case TaskRecord.Mode.Playing: {
                    time += Time.fixedDeltaTime;
                    taskRecord.PlayUntil(time);
                    break;
                }
            }
        }
    }

    public bool IsComplete() {
        return true
            && TargetScene != null
            && TargetRobot != null;
    }

    public string Repr() {
        return string.Format(
            "Input: {0}, Feedback: {1}, Speed: {2}, Responsive: {3}, Latency: {4}",
            Input, Feedback, RobotSpeed, RobotResponsiveness, Latency
        );
    }

    public void StartScene() {
        if (!IsComplete()) {
            Debug.LogError("Attempted to start scene without fully specifying it.");
            return;
        }
        if (Task.ActiveTask != null) return;
        Hub?.SetActive(false);

        GameObject task = Instantiate(TargetScene, TaskParent.transform);
        task.SetActive(true);
        taskRecord = new TaskRecord(task);

        GameObject.FindObjectOfType<InputRig>()?.ObserveScene();
    }

    public void ReplayScene(TaskRecord record) {
        Hub?.SetActive(false);
        taskRecord = record;
        taskRecord.StartReplaying();
        time = 0;
        GameObject.FindObjectOfType<InputRig>()?.ObserveScene();
    }

    public void EndScene() {
        foreach (Task task in GameObject.FindObjectsOfType<Task>()) {
            task.gameObject.SetActive(false);
        }
        Hub?.SetActive(true);
        GameObject.FindObjectOfType<InputRig>()?.ObserveScene();
    }

    public void OnMenu(InputValue value) {
        if (!Hub.activeSelf) {
            EndScene();
        }
    }
    public void OnXRMenu(object value) {
        bool? vb = value as bool?;
        if (!vb.HasValue || !vb.Value) return;
        if (!Hub.activeSelf) {
            EndScene();
        }
    }

    public bool XREnabled() {
    #if UNITY_EDITOR || !UNITY_WEBGL
        return nativeXREnabled();
    #elif UNITY_WEBGL
        return webXREnabled();
    #else
        return false;
    #endif
    }

    private bool nativeXREnabled() {
        var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(xrDisplaySubsystems);
        return xrDisplaySubsystems.Count > 0;
    }
    private bool webXREnabled() {
        return WebXR.WebXRManager.Instance.isSupportedVR
            || WebXR.WebXRManager.Instance.isSupportedAR;
    }

    public bool xrRunning {
        get;
        private set;
    }

    public void DisableXR() {
        var subsystems = new List<ISubsystem>();
        SubsystemManager.GetSubsystems<ISubsystem>(subsystems);
        if (xrRunning) {
            foreach (var inputSubsystem in subsystems) {
                inputSubsystem.Stop();
            }
        }
    }

    public void ToggleXR() {
    #if UNITY_EDITOR || !UNITY_WEBGL
        var subsystems = new List<ISubsystem>();
        SubsystemManager.GetSubsystems<ISubsystem>(subsystems);
        foreach (var inputSubsystem in subsystems) {
            if (xrRunning) {
                inputSubsystem.Stop();
            } else {
                inputSubsystem.Start();
            }
        }
        xrRunning = !xrRunning;
        return;
    #elif UNITY_WEBGL
        var subsystems = new List<ISubsystem>();
        SubsystemManager.GetSubsystems<ISubsystem>(subsystems);
        foreach (var inputSubsystem in subsystems) {
            if (xrRunning) {
                inputSubsystem.Stop();
            } else {
                inputSubsystem.Start();
            }
        }
        xrRunning = !xrRunning;
        WebXR.WebXRManager.Instance.ToggleVR();
    #endif
    }
}
