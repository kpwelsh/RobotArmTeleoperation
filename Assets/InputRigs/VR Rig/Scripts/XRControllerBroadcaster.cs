using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebXR;

public class XRControllerBroadcaster : MonoBehaviour
{
    public GameObject BroadcastSource;
    private WebXRController controller;
    private float trigger;
    private bool menu;
    private bool grip;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<WebXRController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (BroadcastSource == null || BroadcastSource.Equals(null)) {
            return;
        }
        float _trigger = controller.GetAxis(WebXRController.AxisTypes.Trigger);
        if (_trigger != trigger) {
            BroadcastSource.BroadcastMessage("OnXRTrigger", _trigger, SendMessageOptions.DontRequireReceiver);
            trigger = _trigger;
        }
        bool _menu = controller.GetButton(WebXRController.ButtonTypes.Thumbstick) || controller.GetButton(WebXRController.ButtonTypes.Touchpad);
        if (_menu != menu) {
            BroadcastSource.BroadcastMessage("OnXRMenu", _menu, SendMessageOptions.DontRequireReceiver);
            menu = _menu;
        }
        
        bool _grip = controller.GetButton(WebXRController.ButtonTypes.Grip);
        if (_grip != grip) {
            BroadcastSource.BroadcastMessage("OnXRGrip", _grip, SendMessageOptions.DontRequireReceiver);
            grip = _grip;
        }
    }
}
