using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using Utils;

public class DraggableAxis : IRayStopper
{
    public Material HighLightMat;
    public Transform ControlledTransform = null;
    protected List<(GameObject, Vector3)> Axes;
    protected GameObject Selected;
    protected RedirectablePointer Pointer;
    public virtual void OnEnable() {
        if (ControlledTransform == null) {
            ControlledTransform = transform;
        }
    }
    public virtual void OnDisable() {
        Unselect();
    }
    protected void FindAxes() {
        Axes = new List<(GameObject, Vector3)>{
            (transform.Find("X").gameObject, Vector3.right),
            (transform.Find("Y").gameObject, Vector3.up),
            (transform.Find("Z").gameObject, Vector3.forward)
        };
    }
    protected Vector3 ToAxis(GameObject go) {
        if (Axes.Count == 0) FindAxes();
        foreach ((GameObject axOb, Vector3 ax) in Axes) {
            if (go.HasParent(axOb)) {
                return transform.rotation * ax;
            }
        }
        Debug.Log(go);
        throw new System.Exception("Wth. Get some axes.");
    }

    public virtual void Point(IEnumerable<(LineSegment, List<RaycastHit>)> segments) { }

    protected virtual void Unselect() {
        if (Selected != null) {
            if (HighLightMat != null) {
                Selected.GetComponent<MeshRenderer>()?.PopMaterial();
            }

            Selected = null;
        }
        if (Pointer != null)  {
            Pointer.UnClickListener -= Unselect;
            Pointer.PointListener -= Point;
            Pointer = null;
        }
    }

    protected virtual void Select(GameObject selected) {
        Selected = selected;
        selected.GetComponent<MeshRenderer>()?.AddMaterial(HighLightMat);
    }
}
