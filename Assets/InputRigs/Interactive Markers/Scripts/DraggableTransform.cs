using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DraggableTransform : DraggableAxis
{
    public float MaxDragDistance = 0.1f;
    private Vector3 anchor;
    // Start is called before the first frame update
    void Start()
    {
        FindAxes();
    }

    public void OnActivate(object data) {
        Unselect();
        var activateData = (RedirectablePointer.ActivateData) data;
        if (activateData.Activate) {
            Select(activateData.Hit.collider.gameObject);
            Vector3 axis = ToAxis(Selected);
            LineSegment ls = 
                new LineSegment(
                    Selected.transform.position - 0.5f * axis, 
                    Selected.transform.position + 0.5f * axis
                );
            anchor = ls.Project(activateData.Hit.point) - transform.position;
            Pointer = activateData.Pointer;
            Pointer.UnClickListener += Unselect;
            Pointer.PointListener += Point;
        }
    }

    public override void Point(IEnumerable<(LineSegment, List<RaycastHit>)> segments) {
        Vector3 axis = ToAxis(Selected);
        LineSegment ls = 
            new LineSegment(
                Selected.transform.position - 0.5f * axis, 
                Selected.transform.position + 0.5f * axis
            );
        
        float best_d = float.MaxValue;
        Vector3 best_p = Vector3.zero;
        foreach((LineSegment seg, List<RaycastHit> hit) in segments) {
            (Vector3 a, Vector3 b) = ls.ClosestPoints(seg);
            float d = (b - a).magnitude;
            if (d < best_d) {
                best_p = a;
                best_d = d;
            }
        }

        Vector3 dp = best_p - transform.position - anchor;
        if (best_d < float.MaxValue && dp.magnitude <= MaxDragDistance) {
            ControlledTransform.position += dp;
        } else {
            Unselect();
        }
    }
}
