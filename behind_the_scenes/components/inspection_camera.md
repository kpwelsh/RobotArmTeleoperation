---
layout: default
title: Inspection Camera
parent: Components
grand_parent: Behind the Scenes
math: mathjax
mermaid: True
---


# Usage

The inspection camera can be used as an end effector and capture images upon activation. 

![Camera](/assets/imgs/2022-04-27-10-44-43.png)

It contains a camera component that renders to a ```RenderTexture``` named ```CameraBuffer```. Either on ```Activation```, or ad-hoc via the ```TakePic``` function, the camera will render its current view and store it in a ```Texture2D```. This, along with the [```RectanglePacking```]({{ site.baseurl }}{% link behind_the_scenes/components/rectangle_packing.md %}) utility can be used to create a collage of collected images.