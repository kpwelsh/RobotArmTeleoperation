using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;

public class RayCaster : MonoBehaviour
{
    public Material LineMaterial;
    public int Mask = ~0;
    public float Length = 10f;
    public float Width = 0.02f;
    public Color DefaultColor = new Color(1,1,1,1);
    public Color PointingColor = new Color(0,1,0,1);
    private LineRenderer lineRenderer;
    private List<RaycastHit> Hits = new List<RaycastHit>();
    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = Width;
        lineRenderer.endWidth = Width;
        lineRenderer.material = LineMaterial;
    }

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        Hits = Physics.RaycastAll(ray, Length, Mask).OrderBy(hit => hit.distance).ToList();

        float distance = Length;
        Color lineColor = DefaultColor;
        foreach (var hit in Hits) {
            if (hit.collider.gameObject.GetComponent<XRButton>()) {
                distance = hit.distance;
                lineColor = PointingColor;
                break;
            }
        }


        Vector3[] positions = new Vector3[]{
            transform.position,
            transform.position + transform.forward * distance
        };
        lineRenderer.SetPositions(positions);
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
    }

    void OnEnable() {
        if (lineRenderer != null && !lineRenderer.Equals(null)) lineRenderer.enabled = true;
    }

    void OnDisable() {
        if (lineRenderer != null && !lineRenderer.Equals(null)) lineRenderer.enabled = false;
    }

    public void OnTrigger(InputValue value) {
        Trigger();
    }
    
    public void OnXRTrigger(object value) {
        Trigger();
    }

    private void Trigger() {
        foreach (var hit in Hits) {
            XRButton xrb;
            if (hit.collider.gameObject.TryGetComponent<XRButton>(out xrb)) {
                xrb.Select();
            }
        }
    }

}
