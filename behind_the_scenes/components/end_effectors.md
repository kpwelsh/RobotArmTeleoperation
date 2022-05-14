---
layout: default
title: End Effectors
parent: Components
grand_parent: Behind the Scenes
math: mathjax
mermaid: True
---

# Usage

The ```EndEffector``` component is an abstract class that can be inherited and implemented by a gripper. Examples of this are the [```StickGripper```]({{ site.baseurl }}{% link behind_the_scenes/components/sticky_gripper.md %}), which can be used to simulate realistic grasping, the ```PendEffector```, which acts a marker for [surface painting]({{ site.baseurl }}{% link behind_the_scenes/components/surface_painting.md %}), and the [```InspectionCamera```]({{ site.baseurl }}{% link behind_the_scenes/components/inspection_camera.md %}), which can take images from the point of view of the robot arm.

When creating a new end effector, you can specify the ```IKPoint``` and handle activation triggers by overriding the ```Activate``` functions. When the ```IKPoint``` is specified, the robot arm will automatically track to match that point with the commanded pose instead of the default pose located at the base of the hand.

| Example Gripper End Effector |
| :---: |
| ![Gripper]({{site.baseurl}}/assets/imgs/2022-04-27-10-31-19.png) |