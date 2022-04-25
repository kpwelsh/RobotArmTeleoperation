---
layout: default
title: Painting
parent: Components
grand_parent: Behind the Scenes
math: mathjax
mermaid: True
---


# Surface Painting

Writing directly to a texture based on spatial proximity is not something that is available in Unity by default. This is where the ```Canvas``` and ```Marker``` components come in. 


## ```Canvas```

The ```DrawableMesh``` component exposes two operations: ```Clear```, which removes all current markings on the texture, and ```Draw```, which adds new markings to the mesh. The ```Draw``` function takes a 3D capsule in world-space and compares it the mesh, adding the specified color over the texture wherever they intersect. This effect is achieved by maintaining 3 separate textures with a custom surface shader to layer the marker with the original texture, and a custom unlit shader that checks each pixel for intersection with the world-space capsule to determine whether or not it should be marked.


### Unity Shaders

A ["shader"](https://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html) in Unity is simply a program that is compiled for and run on the GPU. A shader can be attached to a [material](https://docs.unity3d.com/Manual/class-Material.html), and is used to determine the color and lighting properties of a specific point on the surface of a mesh. The Unity shader infrastruture is set up to minimize the amount of work that is required of developers. For a simple surface shader, all that is needed is to create a function that maps the UV cooridnates to a struct of color properties. An example can be seen in the following section.

### ```Canvas.shader```

This custom shader is a simple surface shader that performs an alpha blending to layer the marker on top of the original mesh texture[^1]. The following is an excerpt from ```Canvas.shader``` with the boilerplate code elided:

{% highlight HLSL %}
...
sampler2D _MainTex;
sampler2D _Marker;

struct Input
{
    float2 uv_MainTex;
};
...
void surf (Input IN, inout SurfaceOutputStandard o)
{
    // Albedo comes from a texture tinted by color
    fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
    fixed4 marker_color = tex2D(_Marker, IN.uv_MainTex);
    o.Albedo = marker_color.rgb * marker_color.a + c.rgb * (1 - marker_color.a);
    // Metallic and smoothness come from slider variables
    o.Metallic = _Metallic * (1 - marker_color.a);
    o.Smoothness = _Glossiness * (1 - marker_color.a);
    o.Alpha = marker_color.a * marker_color.a + c.a * (1 - marker_color.a);
}
...
{% endhighlight %}

In this example, the function ```surf``` takes a UV coordinate as an input, in the form of an ```Input``` struct.


{% highlight HLSL %}
...

struct Input
{
    float2 uv_MainTex;
};
...
void surf (Input IN ...
...
{% endhighlight %}

It then looks up the color of the original texture as well as the marker texture to overlay, blends them together, and stores the result in the output parameter, ```..., inout SurfaceOutputStandard o)```.

{% highlight HLSL %}
...

sampler2D _MainTex;
sampler2D _Marker;
...
    
    // Albedo comes from a texture tinted by color
    fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
    fixed4 marker_color = tex2D(_Marker, IN.uv_MainTex);
    o.Albedo = marker_color.rgb * marker_color.a + c.rgb * (1 - marker_color.a);
    // Metallic and smoothness come from slider variables
    o.Metallic = _Metallic * (1 - marker_color.a);
    o.Smoothness = _Glossiness * (1 - marker_color.a);
    o.Alpha = marker_color.a * marker_color.a + c.a * (1 - marker_color.a);
...
{% endhighlight %}

The thread-global parameters ```_MainTex``` and ```_Marker``` are set by the Unity material object and the ```DrawableMesh``` component, respectively.


{% highlight csharp %}
    // Start is called before the first frame update
    void Start()
    {
        // Read the texture attached to our gameObject.
        Texture mainTex = GetComponent<MeshRenderer>().material.mainTexture;
        // Create a new blank texture to hold the marker layer.
        MarkerTex = new Texture2D(1024, 1024, TextureFormat.RGBAFloat, false);
        fill(MarkerTex, new Color(0,0,0,0));
        ...
        // Give the shader a pointer to the marker texture.
        GetComponent<MeshRenderer>().material.SetTexture("_Marker", MarkerTex);
    }
{% endhighlight %}


| Marker Texture | Main Texture | Rendered Result |
| :---: | :---: | :---: |
| ![Marker Overlay](/assets/imgs/marker_overlay.png) | ![UW Madison Logo](/assets/imgs/uw-madisonlogo.png) | ![UW Madison Logo](/assets/imgs/uw-madisonlogo-drawn.png) |


## Creating the Marker Overlay

To determine whether or not a pixel should be marked, we need to generate a map from UV space to world-space[^2]. To do this, we are going to create a texture the same size as ```_Marker``` that will hold the 3D coordinates of each point in the RGB fields[^3]. This approach requires rasterizing the mesh to the texture, using the UV coordinates of each vertex instead of its world coordinates. To avoid writing a custom CPU rasterization algorithm and make use of Unity's render pipeline, we will opt to create a mesh in the unity world and take a picture of it using an orthographic camera.

| World Space Cup Model | UV Unwrapped, Position Colored Mesh |
| :---: | :---: |
| ![Cup Model](/assets/imgs/2022-04-25-14-32-17.png) | ![UV Rasterisation](/assets/imgs/2022-04-25-14-32-55.png) |

### ```RasterizeUV``` Function

To generate the colored mesh, we only need to loop through all of the vertices of the current mesh and make a new one with the vertex positions equal to the UV positions, and the color equal to the vertex position. 

We begin by looping through the vertices to create an axis aligned bounding box specified by the minimum corner and the size along each axis. For convenience later, we will store the *inverse* of the size in a vector as ```scale```.

{% highlight csharp %}
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
    Vector3 scale = new Vector3(
        1 / (0.000001f + max[0] - min[0]), 
        1 / (0.000001f + max[1] - min[1]), 
        1 / (0.000001f + max[2] - min[2])
    );
    ...
{% endhighlight %}

Next, we create a list of vertices from the UV coordinates. Here, mesh.uv stores the uv coordinates for each vertex[^4]. We will choose ```z = 0``` for simplicity. We then loop again to set the color of our new vertices. The [Color](https://docs.unity3d.com/ScriptReference/Color.html) struct stores rgba values as floats in [0, 1], so we transform our object coordinates to normalized coordinates using the bounding box we computed previously. Finally, we store a transformation matrix that maps normalized coordinates back to object coordinates to use in the shader later. 

{% highlight csharp %}
    ...
    // Set the positions
    float aspectRatio = ((float)MarkerTex.width) / MarkerTex.height;
    Vector3[] newVertices = new Vector3[mesh.uv.Length];
    for (int i = 0; i < newVertices.Length; i++) {
        newVertices[i] = new Vector3(mesh.uv[i].x * aspectRatio, mesh.uv[i].y, 0);
    }
    // Set the colors
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
    ...
{% endhighlight %}

Now that we have the vertices and colors correct, we just need to create and render our mesh. For this, we will create two gameobjects: a camera and the UV mesh. To ensure the color of the new mesh is not affected by lighting, we assign an unlit material (called ```UVRaster``` here) to it. To ensure this camera only sees our new mesh, we create a [Layer](https://docs.unity3d.com/Manual/Layers.html) in the Unity Editor, assign it to our mesh, and tell the camera to only render that layer. Additionally, we will disable the camera script to prevent it from rendering every frame, since we only want to take a single picture, and assign a [Render Texture](https://docs.unity3d.com/Manual/class-RenderTexture.html) as the target to render to.

{% highlight csharp %}
    ...
    GameObject raster_obj = new GameObject();
    raster_obj.AddComponent<MeshFilter>().mesh = new Mesh();
    var uv_mesh = raster_obj.GetComponent<MeshFilter>().mesh;

    uv_mesh.vertices = newVertices;
    uv_mesh.colors = colors;
    uv_mesh.triangles = mesh.triangles;
    // Mesh postprocessing
    uv_mesh.RecalculateNormals();
    uv_mesh.RecalculateTangents();
    // Create the game object and assign our 
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
    ...
{% endhighlight %}

Finally, we ask the camera to render to the texture. Since the Render Texture buffer data is stored on the GPU, we will create a new ```Texture2D``` and copy the data from the Render Texture into that. Then we can clean up by destroying both the camera and the UV mesh. We will also go ahead and set the ```_UVPosition``` parameter of our shader so it has it when it comes time to draw.

{% highlight csharp %}
    ...
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
{% endhighlight %}


### ```Draw``` Function

Now that we have rasterized the UV mesh onto a texture, we have all of the peices we need to start drawing. The simplest thing to draw is a single point with a specified radius. However, we may not be calling this function with a super high frequency, it allows the caller to specify a value for ```lastDrawPoint``` to interpolate from. This amounts to intersecting the mesh with a cylinder specified by a line segment and a radius.


{% highlight csharp %}
public void Draw(Vector3 drawPoint, float radius, 
                Color drawColor, Vector3? lastDrawPoint = null) {
    if (!Initialized) return;
    ...
{% endhighlight %}

The ```DrawMatShader``` shader is going to need several parameters, so we set those here. The radius, color, and line segment are taken as inputs from the caller, but we also will pass it a transformation matrix to transform the coordinates stored in the color values of the ```_UVPosition``` texture into world coordinates based on our object's current pose.

{% highlight csharp %}
    ...
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
    ...
{% endhighlight %}

Then, the actual work is done by using Unity's [Blit](https://docs.unity3d.com/ScriptReference/Graphics.Blit.html) function to update our current marker overlay with any new marks. Then we can simply read the result back into the marker overlay texture, knowing that it will get rendered over our base material texture when a camera is looking at it.

{% highlight csharp %}
    ...
    RenderTexture tmp = RenderTexture.GetTemporary(MarkerTex.width, MarkerTex.height);
    // This first call to Blit stores the current marker overlay texture 
    // into tmp
    Graphics.Blit(MarkerTex, tmp);
    // Then we invoke DrawMatShader on the uvPositionTexture with the parameters
    // we just set, and store the result in tmp as well.
    // By passing a material with our shader attached into Blit, it will run that
    // shader
    Graphics.Blit(uvPositionTexture, tmp, DrawMat);

    RenderTexture.active = tmp;
    MarkerTex.ReadPixels(new Rect(0, 0, MarkerTex.width, MarkerTex.height), 0, 0);
    MarkerTex.Apply();
    RenderTexture.active = null;

    RenderTexture.ReleaseTemporary(tmp);
}
{% endhighlight %}

### ```DrawMatShader``` Fragment Shader

The final piece of code here is the shader that checks each pixel in the marker texture to see if it should be marked or not. There are two things to call out here.

Firstly, this shader uses an [alpha blend option](https://docs.unity3d.com/Manual/SL-Blend.html) of 

{% highlight hlsl %}
    ...
    Blend SrcAlpha OneMinusSrcAlpha
    ...
{% endhighlight %}

This just ensures that we don't forget about old markings on the marker overlay when we Blit the new marks onto it.

Secondly, the actual code that is being run for each pixel:
{% highlight hlsl %}
    ...
    float4 frag (v2f i) : SV_Target
    {
        if (_Radius < 0) return float4(0,0,0,0);
        float r = _Radius * _Radius;

        float3 pos = tex2D(_UVPosition, i.uv).rgb;
        pos = mul(_NormalizedToWorld, float4(pos, 1)).xyz; 

        float3 a = pos - _DrawPoint;
        float3 b = pos - _LastDrawPoint;
        
        if (dot(a, a) <= r || dot(b, b) <= r) {
            return _DrawColor;
        }
        float3 n = _LastDrawPoint - _DrawPoint;
        float l = sqrt(dot(n,n));
        n = n / l;
        float projectedDistance = dot(a, n);
        if (projectedDistance >= 0 
            && projectedDistance <= l
            && dot(a, a) - projectedDistance * projectedDistance <= r) {
            
            return _DrawColor;
        }
        return float4(0,0,0,0);
    }
    ...
{% endhighlight %}

In this function, we extract the position from the texture color, transform it from normalized to world coordinates, and check the distance to the line segment to see if that pixel should be the marker color, or transparent.



## ```Marker``` 

The ```Marker``` component serves only to trigger ```Draw``` calls to ```DrawableMesh```es in the scene. To do this, it has a trigger collider that represents the marker "tip". When it intersects with a ```DrawableMesh```, it draws on the mesh at the closest point to the tip.

{% highlight csharp %}
void OnTriggerStay(Collider other) {
    DrawableMesh dm = other.gameObject.GetComponent<DrawableMesh>();
    if (dm != null) {
        
        if (!MeshDrawHistory.ContainsKey(dm)) {
            MeshDrawHistory.Add(dm, null);
        }
        Vector3 point = other.ClosestPoint(Tip.position);
        Vector3 last = MeshDrawHistory[dm].GetValueOrDefault(point);
        MeshDrawHistory[dm] = point;
        dm.Draw(point, Size, DrawColor, last);
    }
}
{% endhighlight %}


[^1]: This is also possible to do by adding a second material to the mesh and using Unity's built in texture blending options.
[^2]: Shader code gets called for every pixel in the camera that is occupied by our object. To determine which things are visible, Unity already maps world-space coords to UV coords and you can receive it as an additional parameter in your shader function. So, it might be tempting to try to use the already known values. However, this proves difficult, since the resolution and whether or not the function is even called is determined by the camera position and parameters, which means things wouldn't be drawn if you weren't looking at them. Also, since you are not allowed to write global data in a shader thread, you can't accumulate markings. So, we are out of luck and need to do it ourselves.
[^3]: Overloading texture color data is a common way to do other things with shaders than render colors.
[^4]: Unity [Mesh](https://docs.unity3d.com/ScriptReference/Mesh.html)s can have multiple sets of UV coordinates. Loading uvs to the wrong channel when importing, or not explicitly generating them at all when making the mesh would cause this code to fail.