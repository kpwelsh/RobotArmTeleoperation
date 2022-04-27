---
layout: default
title: Sticky Gripper
parent: Components
grand_parent: Behind the Scenes
math: mathjax
mermaid: True
---

# Usage

The ```Stick Gripper``` component can be attached to a robot end effector and receive end effector [activation triggers]({{ site.baseurl }}{% link behind_the_scenes/components/input_devices.md %}). To function correctly, this component requires that you have one or more fingers listed.

![Sticky Gripper Example](/assets/imgs/2022-04-27-10-35-12.png)

Each finger needs to have an ```Articulation Body``` specified, either explicitly or present on the component. Similarly, the ```Detector``` can either be set explicitly to a ```Box Collider``` component, or it will automatically look for one on the finger. The detector is needed to determine whether or not a finger is in graspable contant with an object. 

![Finger](/assets/imgs/2022-04-27-10-36-33.png)

When the gripper is activated, the ```Gripper Lower Limit``` and ```Gripper Upper Limit``` variables are used along with the activation level ([0, 1]), to determine what command should be sent to the fingers. 


# Technical Details

When interacting with the environment, it is typically not sufficient to rely on just Unity physics to grab and manipulate things. Opposing fingers that are constantly in contact with an object will require the physics engine to maintain tight position margins to prevent object clipping. The lack of necessary precision in this respect means that the friction phsyics options aren't consistent and allow for frequent slipping[^1].

The ```StickyGripper``` and ```Finger``` components were designed to improve this behavior. A ```StickyGripper``` keeps track of ```Rigidbody```s that are being held, and connects them to the gripper via ```FixedJoint``` components. When the object is no longer being held, the ```FixedJoint``` is removed. While an object is being held, it is removed from Unity's dynamics pipeline by setting it to be a *kinematic* ```Rigidbody```. 

The ```StickyGripper``` relies on its ```Finger``` child components. The ```Finger``` component represents an articulable finger joint that can detect whether or not its in contact with an object. Each ```Finger``` contains a [Trigger Collider](https://docs.unity3d.com/ScriptReference/Collider.html) that detects intesecting objects with the [```OnTriggerStay```](https://docs.unity3d.com/ScriptReference/Collider.OnTriggerStay.html) and [```OnTriggerExit```](https://docs.unity3d.com/ScriptReference/Collider.OnTriggerExit.html) methods. Because some gripper models contain multiple objects, links or joints, there is also the ```FingerPad``` component that can hold the contact detector and forward it's contants to the main ```Finger```. 

| ![Franka Emika Panda Gripper Finger](/assets/imgs/2022-04-22-10-37-41.png) | 
|:--:|
| Shown is a single finger from the Franka Emika Panda gripper. The finger has normal colliders that coincide with the visual representation. However, the wireframe box on the finger pad shows the trigger collider that is used for grasp detection. |


At each [```FixedUpdate```](https://docs.unity3d.com/ScriptReference/MonoBehaviour.FixedUpdate.html), the ```StickyGripper``` determines which objects are in contact with *all* fingers, and holds onto objects that are while dropping objects that are no longer in contact.


[^1]: The phsyics materials are already configured for the end-effectors. To experience this for yourself, simply disable the ```StickyGripper``` component.