using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker : MonoBehaviour
{
    public Color DrawColor;
    public float Size = 0.02f;
    public Vector3? LastPosition = null;
    public Transform Tip;
    private Dictionary<DrawableMesh, Vector3?> MeshDrawHistory = new Dictionary<DrawableMesh, Vector3?>();

    void OnTriggerStay(Collider other) {
        DrawableMesh dm = other.gameObject.GetComponent<DrawableMesh>();
        if (dm != null) {
            
            if (!MeshDrawHistory.ContainsKey(dm)) {
                MeshDrawHistory.Add(dm, null);
            }
            Vector3 point = other.ClosestPoint(Tip.position);
            Vector3 last = MeshDrawHistory[dm].GetValueOrDefault(point);
            MeshDrawHistory[dm] = point;
            dm.Draw(point, Size, DrawColor, last);
        }
    }

    void OnTriggerExit(Collider other) {
        DrawableMesh dm = other.gameObject.GetComponent<DrawableMesh>();
        if (dm != null && MeshDrawHistory.ContainsKey(dm)) {
            MeshDrawHistory.Remove(dm);
        }
    }

}
