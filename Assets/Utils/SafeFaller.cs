using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeFaller : MonoBehaviour
{
    new private Rigidbody rigidbody;
    public float SafeTime = 0.5f;
    private float time = 0;
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.freezeRotation = true;
    }

    void FixedUpdate()
    {
        time += Time.fixedDeltaTime;
        if (time >= SafeTime && rigidbody.velocity.magnitude <= 1e-5) {
            rigidbody.freezeRotation = false;
            Destroy(this);
        }
    }
}
