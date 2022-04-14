using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoCamera : MonoBehaviour
{
    new private Camera camera;
    public Transform Screen;
    private int lastMask = ~0;
    public void EnableMono(Camera cam, bool enable = true) {
        Debug.Log((cam.cullingMask, lastMask));
        if (cam.cullingMask != LayerMask.GetMask("Mono")) {
            lastMask = cam.cullingMask;
        }
        if (enable) {
            cam.cullingMask = LayerMask.GetMask("Mono");
            Debug.Log(cam.cullingMask);
        }
        else {
            cam.cullingMask = lastMask;
        }
    }

    void Start() {
        camera = GetComponent<Camera>();
    }
    void Update() {
        Vector3[] corners = new Vector3[4];
        float zDist = camera.farClipPlane - 1;
        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), zDist, Camera.MonoOrStereoscopicEye.Mono, corners);
        float minx = float.MaxValue, maxx = float.MinValue, miny = float.MaxValue, maxy = float.MinValue;
        foreach (var corner in corners) {
            minx = Mathf.Min(minx, corner.x);
            maxx = Mathf.Max(maxx, corner.x);
            miny = Mathf.Min(miny, corner.y);
            maxy = Mathf.Max(maxy, corner.y);
        }

        float dx = maxx - minx;
        float dy = maxy - miny;

        Screen.localScale = new Vector3(dx / 10, 1, dy / 10);
        Screen.localPosition = new Vector3(0, 0, zDist);
    }
}
