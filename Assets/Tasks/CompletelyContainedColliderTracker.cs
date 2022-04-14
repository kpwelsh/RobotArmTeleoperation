using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
public class CompletelyContainedColliderTracker : ColliderTracker
{
    private List<BoxCollider> Triggers = new List<BoxCollider>();
    protected override void Start() {
        foreach (var collider in gameObject.GetComponentsInChildren<BoxCollider>()) {
            if (collider.isTrigger) {
                Triggers.Add(collider);
                numberOfTriggers++;
            }
        }

        if (this.filter == null)
            SetFilter((Collider c) => true);
    }

    public override void SetFilter(ColliderFilter filter) {
        base.SetFilter(
            (Collider c) => {
                return (c is BoxCollider)
                    && (IsContained(c as BoxCollider))
                    && filter(c);
            }
        );
    }

    public bool IsContained(BoxCollider c) {

        foreach (BoxCollider trigger in Triggers) {
            if (!trigger.Contains(c))
                return false;
        }

        return true;
    }
}
