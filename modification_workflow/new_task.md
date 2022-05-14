---
layout: default
title: New Task
nav_order: 2
parent: Modification Workflow
---


# Creating a New Task

Creating a new task is a relatively straightforward process, although it requires specifying several properties of the task. The simplest method is to copy and existing task and make modifications. However, the following guide highlights all of the necessary components to make a task from scratch.

## 1. Design the Task

The most important part of creating a new task is to have the necessary task components. This includes 3D models, textures, and any necessary new script components. It is often useful to visit the [Unity Asset Store](https://assetstore.unity.com/?gclid=Cj0KCQjwg_iTBhDrARIsAD3Ib5i19cRskkVwEdTVqlGbZzUwpmHlvub_NMdsFRKHvdA474BXzic1xfYaAhzxEALw_wcB&gclsrc=aw.ds) to find existing components or models. There are often many free assets that can be useful, and there are several paid assets that are worth considering if you have the budget. 


## 2. Create the Right Structure

To be a properly formed task that supports all of the feedback and input modalities, there are a few things that need to be in place. As an example, we will walk through the [ARAT]({{site.baseurl}}{% link task_models/arat.md %}) task shown below in the Unity Prefab isolation editor view.

![Arat Editor]({{site.baseurl}}/assets/imgs/2022-05-13-20-50-32.png)

Firstly, you need a new class that inherits from the `Task` component. If we take a look at the `ComplexDexterityTask`, we can see that this does not have any implementation requirements, but allows you to include task-level scripting if necessary. This task only uses these hooks to start and stop a clock.

{% highlight csharp %}
public class ComplexDexterityTask : Task
{
    public Clock clock;
    void Start() {

        clock.StartTimer(() => {
            Pause();
        }, 240);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        clock.StartStopwatch();
    }
}
{% endhighlight %}

In addition to new class, the `Task` component requires specification of:

1. A robot start position transform (`RobotPosition`)
2. The commanded pose start position where the robot will place its end-effector initially (`StartPostion`)
3. The end-effector model that the robot arm will use (`End Effector`)
4. A timer that is referenced inside of the `Task` script
5. A list of cameras placed in the scene (`Scene Cameras`)
6. A set of task configurations that correspond to semantic difficulty levels (`Task Configurations`)
   * Accompanied with a difficulty map that takes a `SystemManager.Difficulty` enum value and maps it to a parent game object.


If we take a look at the object hierarchy and the component inspector, we can see the variables and configuration that is required.

| GameObject Hierarchy | Component Inspector |
| :---: | :---: |
| ![Arat Hierarchy]({{site.baseurl}}/assets/imgs/2022-05-13-20-52-52.png) | ![Arat Inspector]({{site.baseurl}}/assets/imgs/2022-05-13-20-53-15.png) |


If all of these components are present, then all of the input and feedback systems should function as well.

Optionally, to enable task completion display, you should include a `SubTaskCompletionDisplay` component in your scene. This will automatically look for active sub tasks in the scene and display their progress.

![Sub Task Completion Display]({{site.baseurl}}/assets/imgs/2022-05-13-21-03-47.png)


## 3. Create Your Task Prefab

Once you have syntactically complete task, you need to create a new prefab or place the existing prefab into the Resources folder. This is critical to enable task logging and replay, as the `TaskRecord` component needs to be able to find the task in the folder hierarchy.

## 4. Add the Task to the Menu

Lastly, to enable the task for user selection, head over to the main scene and expand the `Hub->Canvas->Scene` gameobject. The `Scene` object contains a `SceneSelector` component that maps the button to the correct task prefab object.

![Scene Selector]({{site.baseurl}}/assets/imgs/2022-05-13-21-09-09.png)

Copy one of the existing buttons and configure it to your liking. You can even create a [Unity Sprite](https://docs.unity3d.com/Manual/Sprites.html) out of an existing image file to use as a thumbnail image. Once you have created the button, link it to the appropriate prefab by adding an entry in the `Scene Selector` Map property.

Now your task should be accessible from the main menu and it should support all of the configurations and modalities.