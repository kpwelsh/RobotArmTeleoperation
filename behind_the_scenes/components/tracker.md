---
layout: default
title: Tracker
parent: Components
grand_parent: Behind the Scenes
math: mathjax
mermaid: True
---

# Usage

The `Tracker` component is straightforward to use. Simply attach it to the object that should track another transform, and tell it which transform to track. The difference between this adn simply making one object a parent of another is the ability to introduce latency and low-pass filtering.

| ![Tracker Editor]({{site.baseurl}}/assets/imgs/2022-05-13-18-41-18.png) |
| :---: |
| Example of the tracker component on a robot's IKTarget. The IKTarget tracks the user input with configurable dynamic constraints. |


# Technical Details

At a fixed interval, the tracking object will read the current pose of the tracked transform and interperet it as a pose command. This command then goes through two stages of processing.

First, it is placed in a buffer along with the current timestamp. Then, older items are removed from the buffer by comparing their timestamps with the current timestamp and the desired latency.

{% highlight csharp %}
latencyBuffer.Enqueue((time + Latency_ms / 1000, Target.position, Target.rotation));

while (latencyBuffer.Count > 0 && latencyBuffer.Peek().Item1 <= time) {
    (float t, Vector3 position, Quaternion rotation) = latencyBuffer.Dequeue();
    updateTransform(t, position, rotation);
}
{% endhighlight %}

Secondly, the command is actually received and applied to the current transform. However, this is not before modifying it with a low-pass filter.


{% highlight csharp %}
float dt = Time.fixedDeltaTime;
float gain = dt / (dt + 1 / (LowpassFilter));
...
transform.position = Vector3.Lerp(transform.position, position + OffsetPosition, gain);
transform.rotation = Quaternion.Slerp(
    transform.rotation, 
    rotation * Quaternion.Euler(OffsetRotation.x, OffsetRotation.y, OffsetRotation.z),
    gain
);
{% endhighlight %}

Lastly, the tracking component can be temporarily disabled. During this time, instead of processing the input commands, the `OffsetPosition` and `OffsetRotation` are modified to keep the tracking object stationary while the tracked object changes pose.