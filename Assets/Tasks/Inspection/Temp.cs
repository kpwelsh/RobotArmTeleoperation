using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebXR;

public class Temp : MonoBehaviour
{
    public InspectionCamera inspectionCamera;
    public PicDisplayer picDisplayer;
    public float rate = 1;
    float time = 0;
    public WebXRController xRController;
    private bool clicked = false;
    void Start()
    {
    }
    void takePic() {
        picDisplayer.AddPic(inspectionCamera.TakePic());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float value = xRController.GetAxis(WebXRController.AxisTypes.Trigger);
        if (value >= 0.6 && !clicked) {
            takePic();
            clicked = true;
        } else if (value <= 0.4) {
            clicked = false;
        }
    }


}
