using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracker : MonoBehaviour
{
    public Transform Target;
    public Vector3 OffsetPosition = Vector3.zero;
    public Vector3 OffsetRotation = Vector3.zero;
    private (Vector3, Vector3) initialOffset;
    public float Latency_ms = 0;
    public float LowpassFilter = 1e10f;
    private float time = 0;
    private Queue<(float, Vector3, Quaternion)> latencyBuffer = new Queue<(float, Vector3, Quaternion)>();
    public bool Tracking {
        get;
        private set;
    }

    void Start() {
        Tracking = true;
        initialOffset = (OffsetPosition, OffsetRotation);
    }
    
    void FixedUpdate()
    {
        time += Time.fixedDeltaTime;
        if (Target == null) return;

        latencyBuffer.Enqueue((time + Latency_ms / 1000, Target.position, Target.rotation));
        
        while (latencyBuffer.Count > 0 && latencyBuffer.Peek().Item1 <= time) {
            (float t, Vector3 position, Quaternion rotation) = latencyBuffer.Dequeue();
            updateTransform(t, position, rotation);
        }
    }

    void updateTransform(float t, Vector3 position, Quaternion rotation) 
    {
        float dt = Time.fixedDeltaTime;
        float gain = dt / (dt + 1 / (LowpassFilter));
        if (Tracking) {
            transform.position = Vector3.Lerp(transform.position, position + OffsetPosition, gain);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                rotation * Quaternion.Euler(OffsetRotation.x, OffsetRotation.y, OffsetRotation.z),
                gain
            );
        } else {
            OffsetPosition = transform.position - position;
            OffsetRotation = (Quaternion.Inverse(rotation) * transform.rotation).eulerAngles;
        }
    }

    public void Track() {
        Tracking = true;
    }

    public void UnTrack() {
        Tracking = false;
    }

    public void ResetOffset() {
        (OffsetPosition, OffsetRotation) = initialOffset;
    }
}
