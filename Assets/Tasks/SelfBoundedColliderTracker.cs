using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using System.Linq;

public class SelfBoundedColliderTracker : ColliderTracker
{
    public Vector3 Origin = Vector3.zero;
    public override void SetFilter(ColliderFilter filter)
    {
        base.SetFilter(
            (Collider c) => {
                return filter(c) && Inside(c);
            }
        );
    }

    private bool Inside(Collider collider) {
        Vector3 origin = transform.position + Origin;
        Ray ray = new Ray(
            origin, 
            (collider.transform.position - origin).normalized
        );

        // Assuming that the origin of the transform is within the collider
        var hits = Physics.RaycastAll(ray, 10f);
        foreach (var hit in hits.OrderBy((RaycastHit h) => h.distance)) {

            // If we hit ourself, then we are done looking.
            if (hit.collider.gameObject.HasParent(gameObject))
                return false;
            
            // Otherwise, if we hit the collider we are looking for, then we count it as inside.
            if (hit.collider == collider)
                return true;
        }

        // We shouldn't get here.
        return false;
    }
}
