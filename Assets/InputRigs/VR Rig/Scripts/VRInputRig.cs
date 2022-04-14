using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRInputRig : InputRig
{
    public RayCaster UIRayCaster;
    public GameObject VideoRoom;
    public Camera MainCamera;
    public GameObject CameraHolder;
    private int defaultMask = ~0;
    public MonoCamera monoCamera;
    private GameObject Hub;
    // Start is called before the first frame update
    void Start()
    {
        defaultMask = MainCamera.cullingMask;
        UICamera = MainCamera;
        Hub = GameObject.Find("Hub");
    }

    void Update() {
        UIRayCaster.enabled = Hub.activeSelf;
    }

    protected override void NoTask()
    {
        base.NoTask();
        EnableMono(false);
        EnableVideoRoom(false);
    }

    public bool EnableMono(bool enable) {
        monoCamera?.gameObject.SetActive(enable);
        if (enable)
            SetCullingMask(LayerMask.GetMask("Mono"));
        else
            SetCullingMask(defaultMask);
        return true;
    }

    public bool EnableVideoRoom(bool enable) {
        VideoRoom?.SetActive(enable);
        if (enable)
            SetCullingMask(LayerMask.GetMask("VideoRoom"));
        else
            SetCullingMask(defaultMask);
        return true;
    }

    private void SetCullingMask(int mask) {
        foreach (Transform trans in CameraHolder.transform) {
            Camera cam;
            if (trans.TryGetComponent<Camera>(out cam)) {
                cam.cullingMask = mask;
            }
        }
    }

    protected override void InitializeFeedbackProvider(SystemManager.VisualFeedback visualFeedback)
    {
        Transform controlPerspective = null;
        switch (visualFeedback) {
            case SystemManager.VisualFeedback.VR: {
                EnableMono(false);
                EnableVideoRoom(false);
                controlPerspective = UICamera.transform;
                break;
            }
            case SystemManager.VisualFeedback.VRMono: {
                EnableVideoRoom(false);
                EnableMono(true);
                controlPerspective = UICamera.transform;
                break;
            }
            case SystemManager.VisualFeedback.SingleCam: {
                EnableMono(false);
                EnableVideoRoom(true);
                sceneDisplay.CameraLimit = 1;
                controlPerspective = sceneDisplay.Perspective;
                break;
            }
            case SystemManager.VisualFeedback.MultiCam: {
                EnableMono(false);
                EnableVideoRoom(true);
                sceneDisplay.CameraLimit = null;
                controlPerspective = sceneDisplay.Perspective;
                break;
            }
        }
        Debug.Log("Main Camera culling mask: " + MainCamera.cullingMask.ToString());
        EachInput(ipp => ipp.ControlPerspective = controlPerspective);
    }

}
