---
layout: default
title: New Robot
nav_order: 1
parent: Modification Workflow
has_children: false
---


# Creating a New Robot

Creating a new robot arm model is a straightforward process that requires just a few steps. 

## 1. Find a Model

The easiest way to create a new Unity robot is to start from an existing, well defined arm model. In general, you will need two things: a URDF file that describes the robot's kinematics and dynamics, and a description folder that contains the necessary mesh files. Examples of this can be found in the existing ```Assets/RobotArm/RobotModels``` folder. Similarly, the [Robot Models]({{ site.baseurl }}{% link task_models/robot_models.md %}) contains links to the source descriptions and URDF files for each arm. 


Show below is the recommended folder structure, with the URDF file at the top level next to the description file, along with other robot arm specific resources (e.g. a button thumbnail).
![Folder Structure]({{site.baseurl}}/assets/imgs/2022-04-29-14-58-08.png){:.centered}


## 2. Import the Robot and Add Components

Next, simply import the URDF file with the by invoking the URDF Importer from the context menu. This should create a game object in the scene.

![URDF Importer]({{site.baseurl}}/assets/imgs/2022-04-29-15-01-06.png){:.centered}

URDF Importer also attaches a controller component to the base of the model. For our purposes, this is typically insufficient. You can feel free to remove any and all URDF related components from the model at this point, and attach a ```RobotController``` to the base of the model and create an IKTarget for the end effector.

![Robot Controller]({{site.baseurl}}/assets/imgs/2022-04-29-15-04-38.png){:.centered}

With all controllers, you will need to fill out several fields about the robot and link certain child object. With the ```NativePluginController```, you will also have to link a URDF text resource. Unity doesn't recognize "*.urdf" files as text assets, so you will have to save the urdf as a ".txt" file before linking it to the controller.

## 3. Testing the control

Often there small errors that occur during the import process, and some models may show up rotated from what you expected. To identify and debug these, you can load the "IK Test" Unity scene. This contains two useful objects, a Box, which is an instantiable container to for testing multiple arms; and a Manager, which can be used to instantiate and command your robot just as the task scenes would. To make use of this, simply add your new robot as an item in ```Robots``` field of the ```IK Test``` component.

![IK Test]({{site.baseurl}}/assets/imgs/2022-04-29-15-12-33.png){:.centered}

Then, for each robot in the list, it will instantiate a copy of the Box and ask the robot to follow the InputCommand transform. The InputCommand transform is then controlled by the enabled paths in the Box (only one should be enabled at a time). These will cyclically lerp/slerp between each child pose. If all works correctly, your robot should be moving along with the rest of them.


![IK Testing]({{site.baseurl}}/assets/imgs/2022-04-29-15-15-23.png){:.centered}


This environment can be used to test the controller for robustness and accuracy. Additionally, it is often necessary to tweak the dynamic parameters of the robot models once imported to achieve good, stable, realistic results. 

