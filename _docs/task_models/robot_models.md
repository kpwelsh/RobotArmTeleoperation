---
layout: default
title: Robot Arms
parent: Task Models
nav_order: 1
---

# Robot Models

This project focuses on the dexterity of human + robot systems on a table-top scale with robot arms. When comparing which robot arms are effective for teleoperation, the two main factors to consider are the kinematic and dynamic profiles. 

## Kinematics
In simulation, the most obvious challenge when controlling a robot arm is generating good solutions to the inverse kinematics problem for control. The performance of a human + robot teleoperation system is highly dependent on the ability to generate feasbile and smooth robot motions from human inputs. And in turn, the ability to generate such motions can depend on subtleties of the particular robot kinematics[^1]. 

## Dynamics
When simulating a robot arm, it is possible to completely ignore the dynamic constraints by generating un-physical motions. While this choice simplifies the system significantly, it is often the case that robot arms with more favorable kinematics have stricter dynamic constraints and ignoring them would lead one to conclude that certain arms are better than they are for a real task. This project aims to both offer a *reasonable* facsimile of real robot dynamics as well as the option to change the dynamics to explore the effect on task performance.

## Included Robots

### Franka Emika Panda

![Panda](/assets/imgs/panda_specs.png)


### Kuka iiwa7

![IIWA7](/assets/imgs/iiwa7_specs.png)

### Sawyer


### Baxter


### UR5


[^1]: A thorough description of the complexities of robot inverse kinematics can be found [here](https://motion.cs.illinois.edu/RoboticSystems/InverseKinematics.html).

