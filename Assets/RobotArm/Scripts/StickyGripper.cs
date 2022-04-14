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

    private struct HeldObject {
        public Rigidbody rb;
        public Matrix4x4 Transform;
    }
    private List<HeldObject> held = new List<HeldObject>();

    // Start is called before the first frame update
    void Start()
    {
    }

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
        foreach (var ho in held) {
            if (ho.rb == rb) {
                return;
            }
        }
        Debug.Log("Holding" + rb.ToString());
        HeldObject new_ho = new HeldObject();
        new_ho.rb = rb;
        new_ho.Transform = transform.worldToLocalMatrix * rb.transform.localToWorldUnscaled();
        foreach(var finger in Fingers) {
            foreach (var collider in rb.gameObject.GetComponentsInChildren<Collider>()) {
                finger.IgnoreCollision(collider, true);
            }
        }
        var fj = rb.gameObject.AddComponent<FixedJoint>();
        fj.connectedArticulationBody = GetComponent<ArticulationBody>();
        fj.enableCollision = true;
        held.Add(new_ho);
    }

    void drop(HeldObject ho) {
        Debug.Log("Dropping" + ho.ToString());
        foreach(var finger in Fingers) {
            foreach (var collider in ho.rb.gameObject.GetComponentsInChildren<Collider>()) {
                finger.IgnoreCollision(collider, false);
            }
        }
        held.Remove(ho);
        ho.rb.isKinematic = false;
        Destroy(ho.rb.gameObject.GetComponent<FixedJoint>());
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
            if (touching == null || !touching.Contains(held[i].rb)) {
                drop(held[i]);
            }
        }
        foreach (HeldObject ho in held) {
            var trans = transform.localToWorldUnscaled() * ho.Transform;
            //ho.rb.MovePosition(trans.GetPosition());
            //ho.rb.MoveRotation(trans.rotation);
        }
    }
}
