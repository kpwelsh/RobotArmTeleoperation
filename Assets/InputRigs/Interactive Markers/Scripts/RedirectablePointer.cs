using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public static class RayExtensions {
    public static (Vector3, Vector3) ClosestPoints(this Ray A, Ray B) {
        Vector3 v3 = Vector3.Cross(A.direction, B.direction);
        Matrix4x4 M = new Matrix4x4(
            new Vector4(A.direction.x, A.direction.y, A.direction.z),
            new Vector4(-B.direction.x, -B.direction.y, -B.direction.z),
            new Vector4(v3.x, v3.y, v3.z),
            new Vector4(0, 0, 0, 1)
        );
        Vector3 ba = B.origin - A.origin;
        Vector4 t = M.inverse * ba;

        return (A.GetPoint(t.x), B.GetPoint(t.y));
    }
}
public class RedirectablePointer : MonoBehaviour
{
    public struct ActivateData {
        public RedirectablePointer Pointer;
        public RaycastHit Hit;
        public bool Activate;
        public Ray Ray;
        public int SegmentIndex;

        public ActivateData(RedirectablePointer pointer, RaycastHit hit, bool activate, Ray ray, int index) {
            Pointer = pointer;
            Hit = hit;
            Activate = activate;
            this.Ray = ray;
            SegmentIndex = index;
        }
    }
    public delegate void PointCallback(IEnumerable<(LineSegment, List<RaycastHit>)> segments);
    public delegate void UnClickCallback();
    public PointCallback PointListener;
    public UnClickCallback UnClickListener;
    public bool SendPoint = true;
    public bool SendClick = true;
    public int Mask = ~0;
    public float MaxDistance = 100;
    public CompositeCamera Cam;
    private Ray? ray;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnPoint(InputValue value) {
        Vector2 p = value.Get<Vector2>();
        Vector3 screenPoint = new Vector3(
            p.x,
            p.y,
            0
        );
        ray = Cam.ScreenPointToRay(screenPoint);
        if (!ray.HasValue) return;
        
        var casted = Cast(ray.Value);

        PointListener?.Invoke(casted);

        if (SendPoint) {
            foreach ((LineSegment ls, List<RaycastHit> stuff) in casted) {
                foreach (RaycastHit hit in stuff) {
                    hit.collider.gameObject.SendMessageUpwards("OnMouseOver", (this, ls), SendMessageOptions.DontRequireReceiver);
                }
            }
        }

    }

    public void OnClick(InputValue value) {
        if (!ray.HasValue) return;
        bool v = value.Get<float>() > 0.5 ;
        if (!v) UnClickListener?.Invoke();
        if (SendClick) {
            int index = 0;
            foreach ((LineSegment ls, List<RaycastHit> stuff) in Cast(ray.Value)) {
                foreach (RaycastHit hit in stuff) {
                    var data = new ActivateData(this, hit, v, ls.Ray, index);
                    hit.collider.gameObject.SendMessageUpwards("OnActivate", data, SendMessageOptions.DontRequireReceiver);
                }
                index++;
            }
        }
    }

    public IEnumerable<(LineSegment, List<RaycastHit>)> Cast(Ray ray, float? maxDistance = null) {
        List<RaycastHit> touchedThings = new List<RaycastHit>();
        List<(LineSegment, List<RaycastHit>)> result = new List<(LineSegment, List<RaycastHit>)>();

        float max = maxDistance.GetValueOrDefault(MaxDistance);

        foreach(RaycastHit hit in Physics.RaycastAll(ray, max, Mask).OrderBy(hit => hit.distance)) {
            touchedThings.Add(hit);
            GameObject go = hit.collider.gameObject;
            IRayRedirector redirector = go.GetComponent<IRayRedirector>();
            IRayStopper stopper = go.GetComponent<IRayStopper>();
            if (redirector != null) {
                Ray newRay = ray;
                if (stopper != null) {
                    result.Add(
                        (
                            new LineSegment(ray.origin, ray.direction, hit.distance),
                            touchedThings
                        )
                    );
                    break;
                } else if (redirector.Redirect(ref newRay, hit)) {
                    result.Add(
                        (
                            new LineSegment(ray.origin, ray.direction, hit.distance),
                            touchedThings
                        )
                    );
                    return result.AsEnumerable().Concat(Cast(newRay, max - hit.distance));
                }
            }
        }

        return new List<(LineSegment, List<RaycastHit>)>{(
            new LineSegment(ray.origin, ray.direction, max),
            touchedThings
        )}.AsEnumerable();
    }
}
