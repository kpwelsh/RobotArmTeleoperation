---
layout: default
title: Feedback Devices
parent: Components
grand_parent: Behind the Scenes
math: mathjax
mermaid: True
---

# Usage

This project currently supports several visual feedback modalities, and opporunity to add others. They can currently be broken into two categories -- VR and Non-VR. Initializing and providing the correct feedback modality is responsibility of the active ```InputRig``` component. In the current implementation, only the ```VRInputRig``` is used. However, for non-VR applications, the ```MonitorInputRig``` supports rendering a limited set of feedback sources directly to the monitor. The ```MonitorInputRig``` supports static single-cam feedback as well as static multi-cam feedback. The ```VRInputRig``` supports both of these modalities in addition to both stereo and mono VR. 


# Technical Details

## ```VRInputRig```

When the user is wearing a VR headset, the simplest form of visual feedback to provide is stereoscopic VR. This is supported out of the box in a Unity environemnt for both native builds and WebGL builds by making use of the [WebXR Exporter](https://github.com/De-Panther/unity-webxr-export). This component provides an ```XR Rig``` which connects to the running VR backend using the [WebXR](https://developer.mozilla.org/en-US/docs/Web/API/WebXR_Device_API) communication specification. It similarly provides gameobjects that track the user's hands and methods for reading user input.

### Mono VR

To truly enable monoscopic vision, it is necessary to feed the same image to both eyes. In lieu of making modifications to core XR camera modules to change the multi-eye render process, this project implements an external solution that works for general VR headset drivers. This is acheived by effectively enlarging the scene and placing it very far away from the eyes such that the interpupillary distance is effectively 0 when computing the perspective shift. To get this effect without having to disrupt the physics, we instead switch from rendering the scene directly to the eye cameras, and instead render it once from a perspective between the eyes and project it on a head-mounted screen that is placed at a signficant distance. 


| ![Mono Cam]({{site.baseurl}}/assets/imgs/2022-05-12-19-39-38.png) |
| :---: |
| An example of the mono rendering strategy. The eye cameras don't render the real scene and can only see the large projection in the distance. The viewpoint of the mono cam tracking the head motion is shown in the picture-in-picture view for illustration purposes. Similarly, the size and distance of the projected screen was reduced so that all of the elements in the scene could be seen at once. |


### Static Camera Feedback

When using the static camera feedback modalities, the control modality is unaffected, but the user is placed in small virtual room with a monitor on top of it. In reality, the user is still technically collocated with the robot, but that scene layer is not rendered and this monitor room is rendered instead. when initializing the task, the active ```InputRig```'s ```ObserveScene``` method will be called to look through the active task and arrange the video feeds from the static cameras found in the scene to display to the user. The ```VRInputRig``` does this by stitching the video feeds together with the [Rectangle Packing]({{ site.baseurl }}{% link behind_the_scenes/components/rectangle_packing.md %}) component and rendering the result to a [RenderTexture](8https://docs.unity3d.com/Manual/class-RenderTexture.html).