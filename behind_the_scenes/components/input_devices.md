---
layout: default
title: Input Devices
parent: Components
grand_parent: Behind the Scenes
math: mathjax
mermaid: True
---

# Usage

Inlcuded in this project is support for three types of user input devices: Gamepads, 6DoF XR controllers, and mouse and keyboard. All three types of user interface implement the ```InputPoseProvider``` interface, the primary responsibility of which is to update its associated ```Transform``` to be the currently commanded pose. In general, each of the input systems can be placed in a scene and will give the user a way to command a pose. 

{% highlight csharp %}
public class InputPoseProvider : MonoBehaviour
{
    public Transform ControlPerspective = null;
    public Transform Pose {
        get;
        protected set;
    }
    public virtual void SetPoseFromExternal(Transform trans) {
        if (trans == null) return;
        transform.position = trans.position;
        transform.rotation = trans.rotation;
    }

    public virtual void Reset() { }
}
{% endhighlight %}

In addition to the publically readable ```Pose``` property, it exposes methods for reseting or reinitializing input when a new task is initialized as well as setting the initial commanded pose to a task specific value. The ```ControlPerspective``` is used for differential control mode (i.e. a gamepad) that may prefer to control relative to the viewing perspective instead of relative to the world. Upon initialization, the ```InputProvider``` scans the transform tree for the correct type of pose provider depending on the system settings and throws an exception if it cannot find the specified provider, or it is not supported by the ```InputRig``` implementation.

# Technical Details

## ```I6DOFInput```
The 6DOF input modality is available to the ```VRInputRig``` uses a [Tracker]({{ site.baseurl }}{% link behind_the_scenes/components/tracker.md %}) component to drive the output pose from the 6DOF controller pose. This additional transform layer is introduced to support a "clutching" control paradigm, where the user can temporarily disable the pose tracking to adjust their hand posture. 

![Clutching]({{site.baseurl}}/assets/imgs/2022-05-13-15-44-00.png)


## ```GamepadInput```
The gamepad input component uses Unity's Input System and Gamepad classes to read the value of state of the two joysticks, the shoulder buttons, and and the trigger buttons. On a fixed update interval, it translates the currently active buttons into a translational and rotational velocity using the values of ```GamepadInput.TranslationVelocity``` and ```GamepadInput.RotationalVelocity``` that are set can be set in the Unity Editor. Once the world-space translational velocity is computed it is rotated to match the current control frame if one is present.

## Interactive Markers

The keyboard and mouse interface is modeled after the ["Interactive Markers"](http://wiki.ros.org/rviz/Tutorials/Interactive%20Markers%3A%20Getting%20Started) control paradigm present in RViz, which provides an intuitive click and drag interface for adjusting a target pose. The Unity implementation provided behaves identically, except that the translational axes and the rotational axes are not shown simultaneously, but can be toggled with the left shift key. 

To control the dragging, an interactive marker contains 3 axes for both rotation and translation modeled as either rings and line segments, respectively. When the user clicks, a ray is casted into the scene. If it collides with one of the axis objects, that object is "selected," and the ray is projected onto the axis by finding the closest point on the axis to the ray. This point is stored as the "anchor" point. Then, when the user moves their cursor, another ray is cast into the scene and again projected onto the selected axis geometry, and the position/rotation of the interactive marker is updated such that the "anchor" coincides with this new point, resulting in a slight shift. This tracking process continues while the user is still engaged with the marker by holding down the mouse button or until an extreme, discontinuous motion is detected (such motions can occur when using multi-cam interfaces).