---
layout: default
title: Overview
nav_order: 1
---

# Editing the Project/Technical Details

Details on how to download, run and modify the Unity project for yourself as well as descriptions for all of the components involved can be found [here]({{ site.baseurl }}{% link behind_the_scenes/index.md %}).



# Project Overview

## Motivation

Compared to autonomous robotics, developing a system for robotic teleoperation is typically seen as straightforward. By introducing a human operator the challenge of creating software that exhibits complex decision making, planning, and responsiveness are side stepped. However, simply introducing a human agent into the system does not result in a high performance system. Just last month, Daniel Rea and Stela Seo surveyed the field of teleoperation and concluded that effective teleoperation still requires highly trained expert users and calls for a 

>...[R]enewed focus in broad, user-centered research goals to improve teleoperation interfaces in everyday applications for non-experts...[^1]

While robotic teleoperation has the potential to achieve super-human performance, looking closesly at state of the art teleoperation systems suggests we have yet to achieve performance that is even comparable to a human. The following video shows several teleoperation systems, including a robot breaking the "break in case of emergency" glass with somewhat less urgency than optimal in an "emergency" situation.

<iframe width="560" height="315" src="https://www.youtube.com/embed/66ZMUaBLjaM?start=10" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

Comparing this to [*The Box and Block Test (BBT)*](https://www.physio-pedia.com/Box_and_Block_Test), a human dexterity and motor function test designed to evaluate individuals with a range of neurological diagnoses, it is clear that modern human + robot systems, even with expert users, have yet to surpass a lone human system in terms of absolute performance[^2].

<iframe width="560" height="315" src="https://www.youtube.com/embed/8nsn91JFYgE?start=55" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>


## Contents

This project was designed to further explore the performance gap between human and human + robot teleoperation performance. Developed in VR using Unity, this platform offers several useful components for measuring the impact of different performance factors:

* Realistic Task Models:
  * Several robotic arm platforms with realistic dynamics
  * Task models based on real human dexterity evaluation tools
    * Including variations that recreate the human tasks and those that are more kinematically favorable for robot manipulation
  * Realistic physical interactions
* Robust Control Systems:
  * An arm-agnostic realtime control system based on modern inverse kinematics algorithms
  * Knobs to control low level control variables
    * Lowpass filters
    * Dynamic constraints (joint velocity and torque limits)
    * Latency
* Variable Visual Feedback Modalities:
  * Stereo VR
  * Mono VR
  * Single static camera
  * Muliple static cameras
* Variable Input Device Support:
  * 6DOF Unity XR/WebXR tracked devices
  * Interactive markers for keyboard and mouse
  * Unity supported gamepads (Xbox, PlayStation, Switch)
* Web Deployability
* Object Task Metrics
  * Sub-task progress and completion time (WiP)
  * Remote task recording and replay (WiP)


[^1]: Still Not Solved: a call for renewed focus on user-centered teleoperation interfaces
[^2]: This is not to dismiss the ROI of adding a robot when there are other factors involved (e.g. safety or convenience).

https://motion.cs.illinois.edu/RoboticSystems/InverseKinematics.html