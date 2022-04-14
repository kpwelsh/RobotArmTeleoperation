using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slider : MonoBehaviour
{
    public Transform InputHand;
    private bool tracking = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (tracking && InputHand != null)
            setValue();
    }

    private void setValue() {
        // Vector3 ray = InputHand.forward;
        // float distanceToPlane = (transform.z - InputHand.z) / ray.z;
        // if (distanceToPlane < 0)
        //     return;
        
        // Vector2 xy = new Vector2(InputHand.x, InputHand.y) 
        //     + distanceToPlane * new Vector2(ray.x, ray.y);
        // Vector2 myXY = new Vector2(transform.x, transform.y);



    }

    public void OnSelect() {

    }
    public void OnDeselect() {

    }
}
