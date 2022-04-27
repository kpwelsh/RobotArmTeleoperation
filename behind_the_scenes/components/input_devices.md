---
layout: default
title: Input Devices
parent: Components
grand_parent: Behind the Scenes
math: mathjax
mermaid: True
---

# Usage

This project currently supports several visual feedback modalities and input devices, as well as opportunity to add others. The visual feedback can be put into two categories -- VR and Non-VR. While there are currently three input devices supported: Gamepad, 6DoF Pose Tracker (typical VR input), and Keyboard + Mouse via an [Interactive Markers interface]({{ site.baseurl }}{% link behind_the_scenes/components/end_effectors.md %}). Initializing and providing the correct feedback modality is responsibility of the active ```InputRig```. In the current implementation, only the ```VRInputRig``` is supported. However, for non-VR applications, the ```MonitorInputRig```.

## Visual Feedback

### Stereo VR
The simplest form of feedback for VR application is the Stereo VR view. This simply places the user's head in the scene as a typical VR game might, and lets them move around as usual. This modality is not supported when VR is not being used.

### Mono VR

This is functionally similar to the Stereo VR feedback modality, but lacks stereo vision. This is achieved by placing a new camera between the eyes of the participant in the virtual world, and making the sceen invisible to the normal VR cameras. The view from the new ```MonoCam``` is then projected onto a head-mounted display that is placed ~1000 meters in front of the user. The effect is that both eyes see the same image and stereo vision is lost. This modality is not supported when VR is not being used.

### Single Static Camera

The 