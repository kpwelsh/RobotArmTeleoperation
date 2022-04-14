using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class ContainerCounter : MonoBehaviour
{
    public delegate bool ColliderFilter(Collider collider);
    public Transform InteriorPoint = null;
    public List<ColliderCounter> Boundaries = new List<ColliderCounter>();

    public int Count(ColliderFilter filter) {
        int count = 0;
        if (Boundaries.Count == 0) return 0;

        HashSet<Collider> colliders = new HashSet<Collider>(Boundaries[0].Colliders);
        foreach (var boundary in Boundaries) {
            colliders.IntersectWith(boundary.Colliders);
        }

        Vector3 origin = InteriorPoint?.position ?? Vector3.zero;

        foreach (var collider in colliders) {
            if (!filter(collider))
                continue;

            Ray ray = new Ray(origin, collider.transform.position);
            List<RaycastHit> hits = new List<RaycastHit>();

            // Assuming that the origin of the transform is within the collider
            foreach (var hit in Physics.RaycastAll(ray, (collider.transform.position - origin).magnitude)) {

                // If we hit ourself, then we are done looking.
                if (hit.collider.gameObject.HasParent(gameObject))
                    break;
                
                // Otherwise, if we hit the collider we are looking for, then we count it as inside.
                if (hit.collider == collider) {
                    count++;
                    break;
                }
            }
        }

        return count;
    }
}
