---
layout: default
title: URDF Importer
parent: Modified Components
grandparent: Behind the Scenes
---


# URDF Importer


## AssimpNet

The [Unity-Robotics-Hub](https://github.com/Unity-Technologies/Unity-Robotics-Hub) is a repository for creating robotic simulations in Unity. As a sub-component, it includes [URDF Importer](https://github.com/Unity-Technologies/URDF-Importer). The primary function of the importer is to take a URDF file along with a robot description (visual meshs, collision meshes, and other collision geometry) and create a Unity object with the appropriate visual representation and articulation. 

This package was not created to work with WebGL builds. When building for a WebGL target, the Unity Editor attempts to include two native DLLs with conflicting names in the build (/Runtime/UnityMeshImporter/Plugins/AssimpNet/Native/win/\[x86/x86_64\]/assimp_x86.dll). To resolve the build error, I simply removed the dll that my system was not using (.../x86/assimp_x86.dll).

## Capsules

Capsule models were not originally supported in the URDF spec, and are not consistently supported now. Originally, URDF Importer also only supported Meshes, Boxes, Cylinders, and Spheres as geometries. To support the visualization and debugging of capsule colliders used for the IK Solver, small modifications were made to this Unity package to support the loading and generation of capsule colliders.