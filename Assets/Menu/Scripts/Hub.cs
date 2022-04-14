using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hub : HasSystemManager
{
    public GameObject XRRig;
    public GameObject MonitorRig;
    private InputRig Rig;

    public Color DisabledColor;
    // Start is called before the first frame update
    void Start()
    {
        foreach (Button button in GetComponentsInChildren<Button>()) {
            var colorBlock = button.colors;
            colorBlock.disabledColor = DisabledColor;
            button.colors = colorBlock;
        }
        
        Rig = MonitorRig.GetComponent<InputRig>();
        XRRig.SetActive(false);
        MonitorRig.SetActive(true);
        GetComponentInChildren<Canvas>().worldCamera = Rig.UICamera;
        SystemManager.DisableXR();
    }

    public void ToggleXR() {
        if (XRRig == Rig.gameObject) {
            Rig = MonitorRig.GetComponent<InputRig>();
            XRRig.SetActive(false);
            MonitorRig.SetActive(true);
        } else {
            XRRig.SetActive(true);
            MonitorRig.SetActive(false);
            Rig = XRRig.GetComponent<InputRig>();
        }
        GetComponentInChildren<Canvas>().worldCamera = Rig.UICamera;

        SystemManager.ToggleXR();
    }
}
