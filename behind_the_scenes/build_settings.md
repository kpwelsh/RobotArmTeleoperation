---
layout: default
title: Build Settings
parent: Behind the Scenes
nav_order: 2
---


# Building the Project

While it is possible to run the simulations directly through the editor's play button, it is likely that you will want to actually build and deploy it at some point. To do so, first read [this page](https://docs.unity3d.com/Manual/PublishingBuilds.html) on building Unity projects. 

Specific to this project, there are a few things you might want to modify. Firstly, using the ```File->Build Settings``` menu, you can switch the build target between WebGL to host on a web server and run in the browser and a native build to run as a desktop application. 

![Build Settings]({{site.baseurl}}/assets/imgs/2022-05-13-15-59-38.png)

Alternatively, if you do not have a VR system at hand, or you want to disable VR integration temporarily for testing, you can do so by going to the `Player Settings` (located on the `Build Settings` window) and change whether or not you start VR on initialization. When enabled, Unity will wait for the VR backend before starting the simulation.

![Player Settings]({{site.baseurl}}/assets/imgs/2022-05-13-16-02-32.png)


# Deploying 

If you are building for a WebGL target, the WebAssembly, Javascript, and HTML output can be found in the `build` directory. Then, you can serve the contents at an appropriate web endpoint. keep in mind that this project contains multiple plugins that have separate WebGL and native DLL versions. Failure to install these correctly can result in discrepancies between what you see in the Unity Editor and what is run in the browser.