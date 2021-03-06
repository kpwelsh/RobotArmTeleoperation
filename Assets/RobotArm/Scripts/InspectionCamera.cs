using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class InspectionCamera : EndEffector
{
    public List<Texture2D> Images = new List<Texture2D>();
    private Camera camera;
    void Start()
    {
        camera = GetComponent<Camera>();
    }

    void Update()
    {
    }

    public override void Activate(bool value)
    {
        if (value) {
            Images.Add(TakePic());
        }
    }

    public Texture2D TakePic()
    {
        int width = camera.targetTexture.width;
        int height = camera.targetTexture.height;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        RenderTexture last = RenderTexture.active;
        RenderTexture.active = camera.targetTexture;
        camera.Render();
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        RenderTexture.active = last;
        return tex;
    }
}
