using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DraggableRotation : DraggableAxis
{
    public float MaxDrag = 50f;
    private Vector3 lastDirection;
    private int lsIndex;
    void Start()
    {
        FindAxes();
    }

    public void OnActivate(object data) {
        Unselect();
        var activateData = (RedirectablePointer.ActivateData) data;
        if (!activateData.Activate) {
            return;
        }
        lsIndex = activateData.SegmentIndex;
        Select(activateData.Hit.collider.gameObject);
        Vector3 axis = ToAxis(Selected);
        Ray ray = activateData.Ray;
        
        lastDirection = ray.GetPoint(Vector3.Dot(transform.position - ray.origin, axis) / Vector3.Dot(ray.direction, axis))
                            - transform.position;
        lastDirection.Normalize();


        Pointer = activateData.Pointer;
        Pointer.UnClickListener += Unselect;
        Pointer.PointListener += Point;
    }

    public override void Point(IEnumerable<(LineSegment, List<RaycastHit>)> segments) {
        var segList = segments.ToList();
        if (segList.Count <= lsIndex) {
            Unselect();
            return;
        }
        if (Pointer == null) return;

        (LineSegment ls, List<RaycastHit> stuff) = segList[lsIndex];

        Ray ray = ls.Ray;
        
        Vector3 axis = ToAxis(Selected);
        Vector3 direction = ray.GetPoint(Vector3.Dot(transform.position - ray.origin, axis) / Vector3.Dot(ray.direction, axis))
                                - transform.position;
        direction.Normalize();
        float dot = Mathf.Clamp(Vector3.Dot(lastDirection, direction), -1, 1);
        float angle = Mathf.Acos(dot) * 180 / Mathf.PI;
        if (Mathf.Abs(angle) <= 0.01f) return;
        Vector3 rotationAxis = Vector3.Cross(lastDirection, direction);
        rotationAxis.Normalize();
        rotationAxis = axis * Mathf.Sign(Vector3.Dot(rotationAxis, axis));
        
        if (Mathf.Abs(angle) <= MaxDrag) {
            ControlledTransform.rotation = Quaternion.AngleAxis(angle, rotationAxis) * ControlledTransform.rotation;
            lastDirection = direction;
        } else {
            Unselect();
        }
    }
}
