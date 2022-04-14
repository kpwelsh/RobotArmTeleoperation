using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class ForceField : MonoBehaviour
{
    public List<GameObject> Ignore = new List<GameObject>();
    public Vector3 Direction = Vector3.up;
    public float Speed = 1;
    public float Gain = 1;
    private HashSet<Rigidbody> alreadyForced = new HashSet<Rigidbody>();

    void OnTriggerStay(Collider obj) {
        Rigidbody rb = obj.gameObject.findRigidBody();
        if (rb != null && !Ignore.Contains(obj.gameObject.getRoot())) {
            if (!alreadyForced.Contains(rb)) {
                Vector3 dir = transform.rotation * (Direction / Direction.magnitude);
                float speed = Vector3.Dot(rb.velocity, dir);
                rb.AddForce(Gain * (Speed - speed) * dir);
                alreadyForced.Add(rb);
            }
        }
    }
    
    void FixedUpdate()
    {
        alreadyForced.Clear();
    }
}
