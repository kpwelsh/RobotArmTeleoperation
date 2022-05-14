---
layout: default
title: Task Models
nav_order: 2
has_children: true
---

# Tasks

Each `Task` is comprised of several components: a timer, a collection of sub-tasks, an informational display, static camera positions, and the position of the robot arm. 

![Task in Unity]({{site.baseurl}}/assets/imgs/2022-05-13-17-55-08.png)

Each sub-task is required to contain an objective task completion measure and a name. The task scene will then automatically display the completion measures of all of the sub-tasks on the information display along with the current timer value. Much of the logic for a task is driven from the Unity transform hierarchy. For example, the task display and sub-task aggregation is handled by using the `GetComponentsInChildren<T>` function to find all of the necessary components without explicitly listing them in the editor. For details on how to create a new task, see [Creating a New Task]({{site.baseurl}}{% link modification_workflow/new_task.md %}).