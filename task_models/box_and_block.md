---
layout: default
title: Box and Block
parent: Task Models
nav_order: 3
---

# Box and Block

Because pick-and-place applications represent such a large part of robotics, we wanted one of the tasks to be primarily focused on simple pick-and-place. For this, we looked to the Box and Blocks Test that was developed to evaluate gross manual dexterity in populations with upper extremity impairment or severe cognitive impairment. During this task, shown below, the subject is asked to move as many blocks as they can from one box to another in a given time. To count as a moved block, the subject must carry the blocks past the barrier in the center at which point they are allowed to release the block into the other box. The object metric used is simply the number of moved blocks during the time frame. While the human test design recommends a time limit of 1 minute, we found that under certain configurations users were only able to move a small number of blocks in that time and so to ensure appropriate performance resolution, we use times as large as 3 minutes.


![Box and Block]({{site.baseurl}}/assets/imgs/2022-05-05-10-34-26.png)

# Technical Details

The Box and Block task represents the simplest task implementation, technically speaking. The `BoxDexterity` class implements the `Task` interface and keeps track of the objective metric by using the `ColliderCounter` component to count how many blocks are in the target box.