using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using System;

public class InputRig : HasSystemManager
{
    public Transform CommandTrans;
    public Camera UICamera;
    public CompositeCamera sceneDisplay;
    protected RobotController robot;
    IEnumerator delayedAction(Action action, int nFrames)
    {
        yield return null;
        for(int i=0; i<nFrames; i++)
        {
            yield return new WaitForEndOfFrame();
        }
        action.Invoke();
    }
    public void OnEnable() {
        StartCoroutine(delayedAction(ObserveScene, 2));
    }

    public virtual void ObserveScene()
    {
        sceneDisplay = GetComponentInChildren<CompositeCamera>(true);
        Task task = GameObject.FindObjectOfType<Task>();

        if (task == null || task.Equals(null) || !task.gameObject.activeSelf) {
            NoTask();
        } else {
            SetCameras(task.SceneCameras);
            InitializeFeedbackProvider(SystemManager.Feedback);
            InitializeInputProvider(SystemManager.Input);
            InitializeCommandPosition(task.GetStartingPosition());

            robot = GameObject.FindObjectOfType<RobotController>();

            robot?.TrackTransform(CommandTrans);
        }
    }
    protected virtual void NoTask() {
        EachInput( input => input.Reset() );
    }

    public virtual void SetCameras(List<Camera> cameras) {
        sceneDisplay.SourceCameras = cameras;
    }

    protected void DisableInputDevices() {
        EachInput( ipp => ipp.gameObject?.SetActive(false) );
    }

    protected virtual void InitializeInputProvider(SystemManager.InputDevice inputDevice) {
        DisableInputDevices();
        InputPoseProvider ipp = null;
        switch (inputDevice) {
            case SystemManager.InputDevice._6DOF: {
                ipp = GetComponentInChildren<I6DOFInput>(true);
                break;
            }
            case SystemManager.InputDevice.Gamepad: {
                ipp = GetComponentInChildren<GamepadInput>(true);
                break;
            }
            case SystemManager.InputDevice.KeyboardMouse: {
                ipp = GetComponentInChildren<InteractiveMarker>(true);
                break;
            }
        }
        
        if (ipp != null) {
            ipp.enabled = true;
            ipp.gameObject.SetActive(true);
            CommandTrans = ipp.Pose;
        }
    }
    
    protected virtual void InitializeFeedbackProvider(SystemManager.VisualFeedback visualFeedback) {
        throw new System.Exception();
    }

    protected virtual void InitializeCommandPosition(Transform trans) {
        Debug.Log(trans);
        EachInput(ipp => ipp.SetPoseFromExternal(trans));
    }

    protected void EachInput(Action<InputPoseProvider> f, bool includeInactive = true) {
        foreach (InputPoseProvider ipp in GetComponentsInChildren<InputPoseProvider>(includeInactive)) {
            f(ipp);
        }
    }
}
