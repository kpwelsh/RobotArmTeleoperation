using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartesianPath : MonoBehaviour
{
    public Transform Target;
    public float Speed = 0.1f;
    private float s;
    private List<Transform> Waypoints = new List<Transform>();
    private List<float> Distances = new List<float>();
    private float totalLength = 0;
    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform trans in transform) {
            Waypoints.Add(trans);
        }
        Waypoints.Add(Waypoints[0]);
        for (var i = 1; i < Waypoints.Count; i++) {
            Distances.Add(
                (Waypoints[i].position - Waypoints[i-1].position).magnitude
                + Quaternion.Angle(Waypoints[i-1].rotation, Waypoints[i].rotation) / 180
            );
            totalLength += Distances[Distances.Count-1];
        }
        if (Target == null || Target.Equals(null)) {
            var go = new GameObject("target");
            go.transform.SetParent(transform);
            Target = go.transform;
        }
    }

    void FixedUpdate()
    {
        s += Time.fixedDeltaTime * Mathf.Max(Speed, 0);
        while (s >= totalLength) s -= totalLength;

        var pose = PieceWiseLerp(s);
        Target.transform.position = pose.Item1;
        Target.transform.rotation = pose.Item2;
    }

    (Vector3, Quaternion) PieceWiseLerp(float t) {
        for (var i = 0; i < Distances.Count; i++) {
            if (t >= Distances[i]) 
                t -= Distances[i];
            else {
                float rel_t = t / Distances[i];
                return (
                    Vector3.Lerp(Waypoints[i].position, Waypoints[i+1].position, rel_t),
                    Quaternion.Slerp(Waypoints[i].rotation, Waypoints[i+1].rotation, rel_t)
                );
            }
        }

        throw new System.Exception("Cant interpolate with the given value of t");
    }
}
