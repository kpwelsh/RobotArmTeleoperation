using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonitorRig : InputRig
{
    public override void ObserveScene()
    {
        base.ObserveScene();
        sceneDisplay.enabled = sceneDisplay.SourceCameras.Count > 0;
    }

    protected override void InitializeInputProvider(SystemManager.InputDevice inputDevice)
    {
        switch (inputDevice) {
            case SystemManager.InputDevice._6DOF: {
                throw new System.Exception("Cannot use 6DOF input with monitor display");
            }
            default: {
                base.InitializeInputProvider(inputDevice);
                break;
            }
        }
    }
    protected override void InitializeFeedbackProvider(SystemManager.VisualFeedback visualFeedback)
    {
        switch (visualFeedback) {
            case SystemManager.VisualFeedback.VR: {
                throw new System.Exception("Cannot use VR feedback with monitor rig");
            }
            case SystemManager.VisualFeedback.VRMono: {
                throw new System.Exception("Cannot use VR feedback with monitor rig");
            }
            case SystemManager.VisualFeedback.SingleCam: {
                sceneDisplay.CameraLimit = 1;
                break;
            }
            case SystemManager.VisualFeedback.MultiCam: {
                sceneDisplay.CameraLimit = null;
                break;
            }
        }
        UICamera.enabled = false;
        EachInput(ipp => ipp.ControlPerspective = sceneDisplay.Perspective);
    }

    protected override void NoTask()
    {
        base.NoTask();
        UICamera.enabled = true;
    }
}
