---
layout: default
title: Installation
parent: Behind the Scenes
---

# Installation

## Initializing the Unity Project

The minimum required to run this project is to
1. clone this repo with ```git clone https://github.com/kpwelsh/RobotArmTeleoperation.git```
2. Follow the steps [here](https://support.unity.com/hc/en-us/articles/4402520287124-How-do-I-add-a-project-saved-on-my-computer-into-the-Unity-Hub-) to add this project in Unity Hub.
3. Use Unity Hub to install version 2021.2.7. Some WebXR related components may not work with higher versions. Follow [these instructions](https://support.unity.com/hc/en-us/articles/4402520309908-How-do-I-add-a-version-of-Unity-that-does-not-appear-in-the-Hub-installs-window-) if that version is not immediately available in Unity Hub.

Then, you should be able to open and run the Unity Editor to explore the project. See the [Build Settings]({{ site.baseurl }}{% link behind_the_scenes/build_settings.md %}) page for details on how to build and deploy the project for a WebGL or native target.

## Building Robot Arm IK Controller

The code that determines the joint angles from a target end-effector pose written in Rust with interfaces for both native C libraries and WebAssembly. To build and deploy this Unity project as a web application, you will need the WebAssembly files. To build this project for a native target, or to run it in the editor, you will need to build the native rust package from source and install it into the appropriate Unity project folder. See the [IK Plugins]({{ site.baseurl }}{% link behind_the_scenes/components/ik_plugins.md %}) page for details on how these packages connect to Unity.

### Building From Source

Building the IK plugin from source is not always necessary. A DLL is included in the project, but may not be compatible with your operating system. If you experience issues, follow these instructions for building the Rust plugin from source.

#### Building Rust

To compile the source code, you will need to have rust + cargo (rust package manager) installed. Follow the instructions on the [Rust Homepage](https://www.rust-lang.org/tools/install) if you don't already.

#### Installing RobotIKNative

Clone the source code and compile with[^1]:

    git clone https://github.com/kpwelsh/RobotIKNative
    cd RobotIKNative
    cargo build --release

This will create dynamic library at ```target/release/robot_ik_native```. Then, simply place this file in the ```Assets/Plugins``` folder in the Unity project. If you are replacing a ```robot_ik_native.dll``` file that is already there, you may need to restart the Unity Editor to be able remove the old one, since Unity caches these plugins and prevents the modification of DLLs while they are cached. Simply placing this DLL in this plugins folder with the correct name allows allows the code contained in ```IKInterface``` to find it. 


#### Installing RobotIKWASM

The RobotIKWASM js plugin and WASM binaries are included in this project. If they are not found in the Unity build folder, then copy the contents of ```/RobotIKWASM-Plugin``` at the project root directory to the ```Build``` folder in the build directory (default location: ```/build/Build```).

If you want to rebuild the robot_ik_wasm plugin, you will need to install ```wasm-pack``` with 
    cargo install wasm-pack


Then, you can perform a similar build step as with the native plugin to clone and build the source code:

    git clone https://github.com/kpwelsh/RobotIKWASM
    cd RobotIKWASM
    cargo build --release
    wasm-pack build --target web

Building for the web target generates js with the correct paradigm for easy inclusion into a Unity build.

Finally, you can take ```robot_ik_wasm.js``` and ```robot_ik_wasm_bg.wasm``` and place them in the ```Build``` directory.

Then, if everything went corretly, you should be able to build your Unity project with ```ctrl+b```, ```ctrl+shift+b``` or from the build menu. 


## Running the Unity Project

This project makes use of multiple input devices (most notably VR), and typically compiles for a WebGL target. You can change the compile target under **File->Build Settings**.

![Build Settings]({{site.baseurl}}/assets/imgs/2022-04-21-15-09-07.png)



[^1]: Several of the dependencies in these trees point to git branches. Cargo will cache this kind of code. If there appears to be a mismatch between whats on GitHub and what your compiler is telling you, try updating the packages with ```cargo update```.