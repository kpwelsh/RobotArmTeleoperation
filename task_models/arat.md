---
layout: default
title: ARAT
parent: Task Models
nav_order: 4
---

# Action Research Arm Test

In addition to the tests that cover specific manipulation modalities, the Action Research Arm Test (ARAT) was included to cover a wide range of general manipulation skills. This test was designed for use with patients with stroke, brain injury, multiple sclerosis, and Parkinson’s disease and asks subjects to go through a range of manipulations including placing a ball on a dimpled platform, stacking rectangular blocks on a shelf, sliding cylinders onto small posts, and poring liquid from one cup to another. When performed with human hands, the test includes a few other similar tasks, which were cut from this implementation because the size of the robot end-effectors and each arm's kinematic workspace require the tasks to be spaced somewhat less densely. The figure below shows the configuration of the task in this simulation. This task is evaluated on a somewhat subjective rating scale for each sub-task. On each sub-task, the user scores 0 points if there is no movement, 1 point if the task is partially complete, 2 points if the task is completed but takes abnormally long time, and a maximum of 3 points if the task is performed normally. Crucially, the definition of this scoring system is relative to a typically functioning adult. Instead of this rating scale, each sub-task was given its own objective measure. For most sub-tasks, this is a binary value representing whether or not it was completed. For some, like the poring task, the number of completed components are counted and used as a fractional score.

![ARAT]({{site.baseurl}}/assets/imgs/ARAT.png)

# Technical Details

The ARAT is implemented in the `ComplexDexterity` class, and makes use of the `ContainerCounter`, `PoseMatcher`, and the `CompletelyContainedColliderTracker` components. 

## `PoseMatcher`

This component offers a convenient way for task designers specify a pose matching task in the Unity Editor. It exposes a few editor buttons that allow the designer to set the desired pose by moving the object instead of typing in numbers by hand. Additionally, it affords an error tolerance threshold for both rotation and translation.

![Pose Matcher]({{site.baseurl}}/assets/imgs/2022-05-13-18-13-02.png)

To use this component, simply attach it to a game object, position it where you want it to start, hit the "Set Start Pose" button to save the current pose, position it where you want it to be placed by the user, hit the "Set Goal Pose" button (or alternatively edit the target pose properties directly), and then hit the "Reset Pose" button to set the object back to the start pose.


## `ColliderTracker`

Unity supports [Trigger Colliders](https://docs.unity3d.com/Manual/CollidersOverview.html), which expose the event handling typically offered by a physics simulation without actually influencing the physics. This can be very useful for detecting if certain kinds of objects are present in a region and is used by the `StickyGripper` class to detect grabable things. In this case, the `ColliderTracker` exposes an interface keeping track of which colliders intersect and subsequently filtering them down to meet certain criteria. This is used as the base class for the following two components, both of which are implemented by adding their own filter criteria on top of the options of the `ColliderTracker`.

## `CompletelyContainedColliderTracker`

This component applies an additional filter on top of whether or not a collider *intersects* with the detection area. If both the detection area and the detected collider are `BoxCollider`s, this component additionally checks whether or not the box is completely contained within the detection area. In the ARAT simulation, this is used to determine if the blocks are stacked on the shelves or not.


| ![Shelf Stacking]({{site.baseurl}}/assets/imgs/ShelfBox.png) |
| :---: |
| Shelf stacking area. The green rectangle boxes need to be placed on the center shelf. The wireframe box shown here (not visible during operation) represents the detection area that the blocks need to be contained within to count. |

### `ContainerCounter`[^1]

In addition to testing for convex objects within a another convex object, it can be useful to test whether or not a convex object is contained within a convex hull induced by the empty space in a concave object. Specifically, this is used for counting the number of balls inside of a cup. While this is somewhat complex in general, this implementation was simplified through several assumptions. Firstly that both the object being checked for containment and the bounds of the container are solid physical objects that do not intersect. Secondly, that the container is a concave object that can be made convex through the addition of a single convex shape (i.e. there is only one *interior* of the container and it is convex). And lastly that it is acceptable to give a non-deterministic answer if the object is partially contained (this limitation can be mitigated by combining the result with the use of other containment checks). 

With these assumptions, the algorithm is straightforward. Given a container and an object, we choose a point inside of the container and a point inside of the object. Then, we simply cast a ray from one to the other. If the ray intersects with our container, the object is guaranteed to be not contained. Similarly, if the object is completely contained, the ray is guaranteed to not intersect with the container. However, if the ray does not intersect with the container, it is possible that another choice of points cause an intersection. As such, this algorithm has a false positive rate if the object is partially contained.

The `Count` function performs two checks. Firstly that the collider in question intersects with all of the specified containment regions. In the case of the cup, this is a single cylinder. This check is needed to bound the convex interior of the cup.

{% highlight csharp %}

HashSet<Collider> colliders = new HashSet<Collider>(Boundaries[0].Colliders);
foreach (var boundary in Boundaries) {
    colliders.IntersectWith(boundary.Colliders);
}

{% endhighlight %}


Secondly, it casts a ray from a point in the interior of the cup to a point in the interior of the detected collider. In doing so, it checks if the ray intersects with the cup model. If it does, then the detected collider is certainly not contained in the cup. If it does not, then the detected collider *may* be contained in the cup in general, and *is* contained given the previous assumptions.

{% highlight csharp %}

foreach (var collider in colliders) {
    if (!filter(collider))
        continue;

    Ray ray = new Ray(origin, collider.transform.position);
    List<RaycastHit> hits = new List<RaycastHit>();

    // Assuming that the origin of the transform is within the collider
    foreach (var hit in Physics.RaycastAll(ray, (collider.transform.position - origin).magnitude)) {

        // If we hit ourself, then we are done looking.
        if (hit.collider.gameObject.HasParent(gameObject))
            break;
        
        // Otherwise, if we hit the collider we are looking for, then we count it as inside.
        if (hit.collider == collider) {
            count++;
            break;
        }
    }
}

{% endhighlight %}



[^1]: The current design of the cup model allows this to be solved much more simply by doing a cylinder containment check. However, this was not the case with previous cup models, and this section is written with general models in mind.