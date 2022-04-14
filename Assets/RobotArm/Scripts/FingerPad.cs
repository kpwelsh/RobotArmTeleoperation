using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerPad : MonoBehaviour
{
    public Finger Finger;


    void OnTriggerStay(Collider other) {
        Finger?.OnTriggerStay(other);
    }

    void OnTriggerExit(Collider other) {
        Finger?.OnTriggerExit(other);
    }
}
