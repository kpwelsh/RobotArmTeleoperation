using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Utils;
using System;
public class CompositeCamera : MonoBehaviour
{
    public RenderTexture TargetTexture = null;
    public bool AutoArrange = true;
    public Rect Viewport = new Rect(0,0,1,1);
    public Rect PixelRect {
        get {
            Vector2 ScreenSize = new Vector2(Screen.width, Screen.height);
            if (TargetTexture != null) {
                ScreenSize = new Vector2(TargetTexture.width, TargetTexture.height);
            }
            return new Rect(
                Vector2.Scale(Viewport.position, ScreenSize),
                Vector2.Scale(Viewport.size, ScreenSize)
            );
        }
    }
    public Transform Perspective {
        get {
            if (_sourceCameras.Count > 0) {
                return _sourceCameras[0].transform;
            }
            return transform;
        }
    }
    private int? _cameraLimit = null;
    public int? CameraLimit
    {
        get {
            return _cameraLimit;
        }
        set {
            if (value.GetValueOrDefault(_sourceCameras.Count) != _cameraLimit.GetValueOrDefault(_sourceCameras.Count)) {
                _cameraLimit = value;
                arrange();
            } else {
                _cameraLimit = value;
            }
        }
    }

    public List<Camera> StaticCameraList = new List<Camera>();
    private List<Camera> _sourceCameras = new List<Camera>();
    public List<Camera> SourceCameras
    {
        get {
            return _sourceCameras;
        }
        set {
            // Disable all current source cameras
            foreach (Camera cam in _sourceCameras) cam.enabled = false;
            _sourceCameras = value;
            arrange();
        }
    }

    void Start() {
        if (StaticCameraList.Count > 0) SourceCameras = StaticCameraList;
    }
    
    private void arrange() {
        List<Camera> sourceCameras = _sourceCameras.GetRange(
            0, 
            Math.Min(
                _cameraLimit.GetValueOrDefault(_sourceCameras.Count),
                _sourceCameras.Count
            )
        );
        
        foreach (Camera cam in _sourceCameras)  {
            cam.enabled = false;
        }
        foreach (Camera cam in sourceCameras)  {
            cam.enabled = true;
            cam.targetTexture = TargetTexture;
        }

        if (!AutoArrange) return;
        
        List<Vector2> rectangles = new List<Vector2>();
        foreach (Camera cam in sourceCameras) {
            rectangles.Add(new Vector2(cam.rect.width, cam.rect.height));
        }
        RectanglePacking.RectangleArrangement displayArrangement = RectanglePacking.Arrange(rectangles, 1, hJustification : RectanglePacking.Justification.Center);
        if (displayArrangement == null) return;
        foreach ((Camera cam, (Vector2 xy, Vector2 wh))  in sourceCameras.Zip(displayArrangement.children, (a, b) => (a, b))) {
            cam.rect = new Rect(
                Viewport.x + xy.x / displayArrangement.dimensions.x,
                Viewport.y + xy.y / displayArrangement.dimensions.y,
                Viewport.width * wh.x / displayArrangement.dimensions.x,
                Viewport.height * wh.y / displayArrangement.dimensions.x
            );
        }
    }

    public Ray? ViewportPointToRay(Vector2 viewPoint) {
        List<Camera> sourceCameras = _sourceCameras.GetRange(0, _cameraLimit.GetValueOrDefault(_sourceCameras.Count));
        foreach (Camera cam in sourceCameras) {
            if (cam.rect.Contains(viewPoint)) return cam.ViewportPointToRay(cam.rect.GetRelative(viewPoint));
        }
        return null;
    }

    public Ray? ScreenPointToRay(Vector3 screenPoint) {
        return ViewportPointToRay(PixelRect.GetRelative(screenPoint));
    }
}
