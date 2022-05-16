---
layout: default
title: Tracing
parent: Task Models
nav_order: 2
---

# Tracing

Another manipulability benchmark that we considered fundamental was controlled, precise surface interaction. The ability to interact within a constrained task space that affords fewer dimensions than the input device is common in a wide range of applications, including sanding, painting, and composite lay-up. Additionally, tracing tests have been effectively used to asses human dexterity as well as teleoperation dexterity. To objectively measure the quality of the trace, we look at the number of pixels that are correctly painted in and compare that to the number of pixels that are incorrectly painted in. This score can vary significantly depending on brush stroke size and target image resolution, but can be used to consistently compare two different control configurations with the same tracing parameters.

![Tracing]({{site.baseurl}}/assets/imgs/Tracing.png)


# Technical Details

The tracing task is enabled by two components, a `Marker` end-effector and a `DrawableMesh` canvas. For details on how this was created, see the [Painting]({{site.baseurl}}{% link behind_the_scenes/components/surface_painting.md %}) component page. In addition to the painting component, there is also a scoring metric contained in the `SurfacePaintingSubTask` component. This metric uses a source image (the cursive "lab" in the above figure) and generates a score map. When evaluating the user's performance, marker overlay is compared against the score map and the user recieves either positive or negative points for each pixel they have marked. To efficiently compute this, the score map is generated and then used in the context of a Unity [Compute Shader](https://docs.unity3d.com/Manual/class-ComputeShader.html), which enables general purpose GPU computation. 


At design time, the Signed Distance Field of the binary image is generated using the [SDF-Toolkit (Free)](https://catlikecoding.com/sdf-toolkit/docs/texture-generator/) Unity Plugin. 

| Original Stencil | SDF (Whiter is Closer) |
| ![Stencil]({{site.baseurl}}/assets/imgs/lab_curve 1.png) | ![SDF]({{site.baseurl}}/assets/imgs/2022-05-16-10-42-45.png) |


Then, the value of a mark is accumulated by multiplying the alpha channel of the mark with the score for that particular pixel generated from the SDF. SDF values equal to 0 have a score multiplier of 1, while SDF values greater than 0 have a negative score multiplier that scales with distance to the curve.


The `TracingSubTask` component contains definitions for `ParallelScorer` and `ParallelSum` objects. Each rely on the `ScoreTracing.compute` shader definition, each of which manage the memory allocation for their respective GPU kernels. 

## `ParallelScorer`

This object implements slightly modified component-wise matrix multiplication. The alpha channel of the `Texture2D` `ScoreMap` holds the SDF for the curve, which is mapped to a score multiplier via a simple scoring function. 


{% highlight csharp %}
void Score(uint id: SV_DispatchThreadID) {
    uint2 xy = uint2(id / res_y, id % res_x);
    float a = ScoreMap[xy][3];
    float score = a > 0.99 ? 1 : a-1;
    if (id < res_x * res_y) Result[id] = score * Marking[xy][3];
}
{% endhighlight %}

## `ParallelSummer`

This object implements a standard parallel sum reduction, and is used to accumulate the results of the `ParallelScorer`. The included shader and scoring function is an implementation of the Blelloch modification to the [Hillis and Steele](https://people.cs.pitt.edu/~bmills/docs/teaching/cs1645/lecture_scan.pdf) parallel scan operation, except without the down sweep operation. 
{% highlight csharp %}
groupshared float bucket[THREADS_PER_GROUP];
void Scan(uint id, uint gi, uint gnumber, float x) {
    bucket[gi] = x;
    
    [unroll]
    for (uint t = 1; t < THREADS_PER_GROUP; t <<= 1) {
        GroupMemoryBarrierWithGroupSync();
        float temp = bucket[gi];
        uint right_index = gi + t;
        if (right_index < THREADS_PER_GROUP) temp += bucket[right_index];
        GroupMemoryBarrierWithGroupSync();
        bucket[gi] = temp;
    }
    
    if (gi == 0) OutputBuf[gnumber] = bucket[gi];
}


uint length;
#pragma kernel Sum
[numthreads(THREADS_PER_GROUP, 1, 1)]
void Sum(uint id: SV_DispatchThreadID, uint gid : SV_GroupIndex, uint gnumber : SV_GroupID) {
    float x = id < length ? InputBuf[id] : 0;
    Scan(id, gid, gnumber, x);
}
{% endhighlight %}

One invokation of the above Sum reduction call will produce a new array with the length reduced by a factor of `THREADS_PER_GROUP`. Using a typical value of 512, it is necessary to call this function multiple times in a row (3-4, usually) to accumulate the total result.

