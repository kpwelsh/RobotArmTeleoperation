using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class DrawableMesh : MonoBehaviour
{
    public Material DrawMat;
    public Material UVRaster;
    private Vector3[,] positions;
    public Texture2D MarkerTex;
    private Texture2D uvPositionTexture;

    private Matrix4x4 NormalizedToObj;
    private bool Initialized = false;

    // Start is called before the first frame update
    void Start()
    {
        Texture mainTex = GetComponent<MeshRenderer>().material.mainTexture;
        MarkerTex = new Texture2D(1024, 1024, TextureFormat.RGBAFloat, false);
        fill(MarkerTex, new Color(0,0,0,0));
        RasterizeUV();
        GetComponent<MeshRenderer>().material.SetTexture("_Marker", MarkerTex);
    }

    private void fill(Texture2D tex, Color color) {
        var fillColorArray =  tex.GetPixels();
        for(var i = 0; i < fillColorArray.Length; ++i)
            fillColorArray[i] = color;
        tex.SetPixels(fillColorArray);
        tex.Apply();
    }

    public void Clear() {
        fill(MarkerTex, new Color(0,0,0,0));
        Draw(Vector3.zero, -1, new Color(0,0,0,0));
    }

    public void Draw(Vector3 drawPoint, float radius, Color drawColor, Vector3? lastDrawPoint = null) {
        if (!Initialized) return;

        DrawMat.SetColor("_DrawColor", drawColor);
        DrawMat.SetFloat("_Radius", radius);
        DrawMat.SetMatrix(
            "_NormalizedToWorld", 
            Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale)
            * NormalizedToObj
        );
        DrawMat.SetVector("_DrawPoint", new Vector4(drawPoint.x, drawPoint.y, drawPoint.z, 0));
        Vector3 last = lastDrawPoint.GetValueOrDefault(drawPoint);
        DrawMat.SetVector("_LastDrawPoint", new Vector4(last.x, last.y, last.z, 0));

        RenderTexture tmp = RenderTexture.GetTemporary(MarkerTex.width, MarkerTex.height);
        Graphics.Blit(MarkerTex, tmp);
        Graphics.Blit(uvPositionTexture, tmp, DrawMat);

        RenderTexture.active = tmp;
        MarkerTex.ReadPixels(new Rect(0, 0, MarkerTex.width, MarkerTex.height), 0, 0);
        MarkerTex.Apply();
        RenderTexture.active = null;

        RenderTexture.ReleaseTemporary(tmp);
    }

    private void RasterizeUV() {
        var mesh = GetComponent<MeshFilter>().mesh;
        float[] min = new float[]{float.MaxValue, float.MaxValue, float.MaxValue};
        float[] max = new float[]{float.MinValue, float.MinValue, float.MinValue};

        foreach (Vector3 v in mesh.vertices) {
            for (int i = 0; i < 3; i++) {
                min[i] = Mathf.Min(min[i], v[i]);
                max[i] = Mathf.Max(max[i], v[i]);
            }
        }

        Vector3 lower = new Vector3(min[0], min[1], min[2]);
        Vector3 scale = new Vector3(1 / (0.000001f + max[0] - min[0]), 1 / (0.000001f + max[1] - min[1]), 1 / (0.000001f + max[2] - min[2]));

        float aspectRatio = ((float)MarkerTex.width) / MarkerTex.height;
        Vector3[] newVertices = new Vector3[mesh.uv.Length];
        float[] testMin = new float[]{float.MaxValue, float.MaxValue};
        float[] testMax = new float[]{float.MinValue, float.MinValue};
        for (int i = 0; i < newVertices.Length; i++) {
            newVertices[i] = new Vector3(mesh.uv[i].x * aspectRatio, mesh.uv[i].y, 0);
            for (int j = 0; j < 2; j++) {
                testMin[j] = Mathf.Min(testMin[j], newVertices[i][j]);
                testMax[j] = Mathf.Max(testMax[j], newVertices[i][j]);
            }
        }

        Color[] colors = new Color[newVertices.Length];
        for (int i = 0; i < colors.Length; i++) {
            Vector3 c = mesh.vertices[i] - lower;
            c.Scale(scale);
            colors[i] = new Color(c.x, c.y, c.z, 1);
        }

        NormalizedToObj = Matrix4x4.TRS(
            lower,
            Quaternion.identity,
            new Vector3(max[0] - min[0], max[1] - min[1], max[2] - min[2])
        );

        GameObject raster_obj = new GameObject();
        raster_obj.AddComponent<MeshFilter>().mesh = new Mesh();
        // Hmmm....
        var uv_mesh = raster_obj.GetComponent<MeshFilter>().mesh;
       

        uv_mesh.vertices = newVertices;
        uv_mesh.colors = colors;
        uv_mesh.triangles = mesh.triangles;
        uv_mesh.RecalculateNormals();
        uv_mesh.RecalculateTangents();

        raster_obj.layer = LayerMask.NameToLayer("UVRaster");
        raster_obj.AddComponent<MeshRenderer>();
        raster_obj.GetComponent<MeshRenderer>().material = UVRaster;

        GameObject cam_obj = new GameObject();
        cam_obj.transform.localPosition = new Vector3(aspectRatio / 2, 0.5f, -1);
        cam_obj.AddComponent<Camera>();
        Camera cam = cam_obj.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 0.5f;
        cam.cullingMask = LayerMask.GetMask("UVRaster");
        cam.enabled = false;
        cam.targetTexture = new RenderTexture(MarkerTex.width, MarkerTex.height, 0, RenderTextureFormat.ARGBFloat);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0,0,0,0);

        cam.Render();

        uvPositionTexture = new Texture2D(MarkerTex.width, MarkerTex.height, TextureFormat.RGBAFloat, false);
        
        RenderTexture.active = cam.targetTexture;
        uvPositionTexture.ReadPixels(new Rect(0, 0, MarkerTex.width, MarkerTex.height), 0, 0);
        uvPositionTexture.Apply();
        RenderTexture.active = null;
        
        DrawMat.SetTexture("_UVPosition", uvPositionTexture);
        Destroy(raster_obj);
        Destroy(cam_obj);
        Initialized = true;
    }
}
