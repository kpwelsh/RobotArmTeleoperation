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


TODO: Add a figure describing this.