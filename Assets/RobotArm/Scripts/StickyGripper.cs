using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

public class StickyGripper : EndEffector
{
    public float Force = 1e10f;
    public float Stiffness = 200f;
    public float Damping = 100f;
    public float GripperLowerLimit = 0;
    public float GripperUpperLimit = 0.04f;
    public List<Finger> Fingers = new List<Finger>();
    public bool InvertActivation = false;
    private List<Rigidbody> held = new List<Rigidbody>();

    public void OnTrigger(InputValue value) {
        Activate(value.Get<float>());
    }
    public void OnXRTrigger(object value) {
        float? vf = value as float?;
        Activate(vf.GetValueOrDefault(0));
    }

    public override void Activate(float target) {
        if (InvertActivation) target = 1 - target;

        target = target * (GripperUpperLimit - GripperLowerLimit) + GripperLowerLimit;
        
        foreach (var finger in Fingers) {
            ArticulationDrive drive = finger.Body.xDrive;
            drive.target = Mathf.Clamp(target, drive.lowerLimit, drive.upperLimit);
            drive.forceLimit = Force;
            drive.stiffness = Stiffness;
            drive.damping = Damping;
            finger.Body.xDrive = drive;
        }
    }

    void hold(Rigidbody rb) {
        if (held.Contains(rb)) {
            return;
        }
        var fj = rb.gameObject.AddComponent<FixedJoint>();
        fj.connectedArticulationBody = GetComponent<ArticulationBody>();
        fj.enableCollision = true;
        held.Add(rb);
    }

    void drop(Rigidbody rb) {
        held.Remove(rb);
        rb.isKinematic = false;
        Destroy(rb.gameObject.GetComponent<FixedJoint>());
    }

    void FixedUpdate()
    {
        HashSet<Rigidbody> touching = null;
        foreach (Finger finger in Fingers) {
            if (touching == null) {
                touching = new HashSet<Rigidbody>(finger.InContact);
            } else {
                touching.IntersectWith(finger.InContact);
            }
        }
        
        if (touching != null) {
            foreach (var rb in touching) {
                hold(rb);
            }
        }
    
        for (int i = held.Count - 1; i >= 0; i--) {
            if (touching == null || !touching.Contains(held[i])) {
                drop(held[i]);
            }
        }
    }
}
