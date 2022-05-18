using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class Finger : MonoBehaviour
{
    public HashSet<Rigidbody> InContact = new HashSet<Rigidbody>();
    private float minClose;
    public ArticulationBody Body = null;
    public BoxCollider detector;

    void Start() {
        Body = GetComponent<ArticulationBody>();
        ArticulationDrive drive = Body.xDrive;
        minClose = drive.lowerLimit;
        if (detector == null || detector.Equals(null))
            detector = GetComponentInChildren<BoxCollider>();
    }
    public void Freeze() {

        ArticulationDrive drive = Body.xDrive;
        float x = Body.jointPosition[0];
        if (Body.jointType == ArticulationJointType.RevoluteJoint) {
            x *= 180f / Mathf.PI;
            
        }
        drive.lowerLimit = x - 0.005f;
        drive.target = drive.lowerLimit;
        Body.xDrive = drive;
    }

    public void UnFreeze() {
        // Dear Future Kevin. 
        // If you want to allow for maintaining multiple contacts
        // You really need to keep track of all of the joint values at which you 
        // Contacted each object, and then not set it to minClose, but to min(of those positions)
        // instead. This doesn't make much sense practically, but might be a source of bugs later.
        ArticulationDrive drive = Body.xDrive;
        drive.lowerLimit = minClose;
        Body.xDrive = drive;
    }

    // Update is called once per frame
    public void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.gameObject.findRigidBody();
        touch(rb);
    }
    public void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.gameObject.findRigidBody();
        untouch(rb);
    }

    private void touch(Rigidbody rb) {
        if (rb == null) return;
        if (!InContact.Contains(rb)) {
            Freeze();
            InContact.Add(rb);
        }
    }
    private void untouch(Rigidbody rb) {
        if (rb == null) return;
        if (InContact.Contains(rb)) {
            InContact.Remove(rb);
        }
        if (InContact.Count == 0) {
            //UnFreeze();
        }
    }
}
