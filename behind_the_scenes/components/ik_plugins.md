---
layout: default
title: IK Plugins
parent: Components
grand_parent: Behind the Scenes
math: mathjax
mermaid: True
---
  
# Usage

The provided IK Solving Plugins are made available via the ```NativeRobotController``` component. In general, an implementation of ```RobotController``` should be present on each robot arm. The controller allows for the specification of a hand joint (where end effectors will be attached to), URDF model, initial joint configuration for seeding the solver, a pointer to the IK Target that should be tracked, and the name of the hand joint to give to the IK Plugin.

![Native Plugin Controller]({{site.baseurl}}/assets/imgs/2022-04-27-11-11-41.png)

While running, the controller will transform the IK Target into its local frame, transform it into a canonical right handed coordinate frame for the IK plugin, and command the positions of the joints. The dynamics of each joint can be controlled on a global level as well to modify behavior to be more or less sluggish and responsive.

# Technical Details

The core IK solver and Unity robot controller plugin is in 3 components that is built off of the [RobotIKBase](https://github.com/kpwelsh/RobotIKBase) rust package.
<div class="mermaid">
graph TD;
    Base[RobotIKBase]-->WASM[robot_ik_wasm.js];
    Base-->Native[robot_ik_native.dll];
    Native-->IKInterface[IKInterface.cs];
    WASM-->IKInterface[IKInterface.cs];
</div>

The core, meaningful code is written in Rust, and is subsequently compiled for the native machine (to run the simulation through the Unity Editor) and for WebAssembly to run in the browser. For detailed instructions on how to correctly install and build these components, see the [Installation]({{ site.baseurl }}{% link behind_the_scenes/installation.md %}) page. IKInterface.cs conditionally compiles to look for one end point or the other depending on the build target.

## IKInterface

The ```IKInterface``` class serves to safely abstract which IK plugin is being used behind the scenes. It leverages some straightforward conditional compiling to either link to a native library, or one provided by a JS interface.

{% highlight csharp %}
public class IKInterface
{
#if UNITY_EDITOR
    [DllImport("robot_ik_native", EntryPoint = "new_solver", 
                                  CallingConvention = CallingConvention.Cdecl)]
    unsafe private static extern void* new_solver(string urdf, string ee_frame);

    [DllImport("robot_ik_native", EntryPoint = "solve", 
                                  CallingConvention = CallingConvention.Cdecl)]
    unsafe private static extern bool solve(void* solver_ptr, float* current_q, 
                                            float[] target, float* q);
    
    [DllImport("robot_ik_native", EntryPoint = "set_self_collision", 
                                  CallingConvention = CallingConvention.Cdecl)]
    unsafe private static extern bool set_self_collision(void* solver_ptr, 
                                                         bool self_collision_enabled);

    [DllImport("robot_ik_native", EntryPoint = "deallocate", 
                                  CallingConvention = CallingConvention.Cdecl)]
    unsafe private static extern void deallocate(void* solver_ptr);

#elif UNITY_WEBGL
    [DllImport("__Internal")]
    unsafe private static extern void* new_solver(string urdf, string ee_frame);
    [DllImport("__Internal")]
    unsafe private static extern bool solve(void* solver_ptr, float[] current_q, 
                                            float[] target, float[] q);
    [DllImport("__Internal")]
    unsafe private static extern bool set_self_collision(void* solver_ptr,
                                                         bool self_collision_enabled);
    [DllImport("__Internal")]
    unsafe private static extern void deallocate(void* solver_ptr);
#endif
...
}
{% endhighlight %}


In addition to wrapping the function end-points, it ensures that the memory allocated by the IK Solver is cleaned up when the IKInterface object is destroyed.

{% highlight csharp %}
...
    ~IKInterface() {
        unsafe {
            deallocate(solver_ptr);
        }
    }
...
{% endhighlight %}

## RobotIKNative

The native plugin exposes the RobotIKBase rust package to Unity via a C interface. For details on how FFI's in Rust work, check out the [Rustonomicon](https://doc.rust-lang.org/nomicon/ffi.html). Code for this repo can be found [here](https://github.com/kpwelsh/RobotIKNative). Unity can load native plugins from dynamic libraries if they exist in the ```Assets\Plugins``` folder. 

## RobotIKWASM

While Unity support bundling a mix of C++, C# and JS directly into the WebAssembly build, it doesn't (as of writing) support native inclusion of WebAssembly files. So the WebAssembly version of the plugin has three sub-componets. 

Firstly, the actual plugin is compiled from the [RobotIKWASM](https://github.com/kpwelsh/RobotIKWASM) Rust package. This produces a ```.wasm``` file and a js file to interface with it. 

Excerpt from the generated file, ```robot_ik_wasm.js```:

{% highlight javascript %}
...
/**
* @param {string} urdf
* @param {string} ee_frame
* @returns {number}
*/
export function new_solver(urdf, ee_frame) {
    var ptr0 = passStringToWasm0(urdf, wasm.__wbindgen_malloc, wasm.__wbindgen_realloc);
    var len0 = WASM_VECTOR_LEN;
    var ptr1 = passStringToWasm0(ee_frame, wasm.__wbindgen_malloc, wasm.__wbindgen_realloc);
    var len1 = WASM_VECTOR_LEN;
    var ret = wasm.new_solver(ptr0, len0, ptr1, len1);
    return ret;
}
...
{% endhighlight %}

This JS layer handles the memory management, data type transformation, and invokation of the WASM code. One thing to note is that, just like the native version, the wasm library call actually returns a ```*const IKSolver```. However, we take advantage of the duplicity of pointers and integers and the permissiveness of JavaScript to simply reinterpret the bytes as a pointer when we pass them to the wasm code in the future.

In addition to the layer from wasm <-> JS, there is another layer, located at ```Assets\Plugins\robot_ik_js_interface.jslib```. This uses Unity API to transform the data between C# and JS as well as expose the libraries to the Unity WASM compiler. However, because the actual robot IK driver can't be built with the rest of the project, it points the API to a placeholder object, ```RobotIK```.

The last piece of the puzzle is in ```Assets\Plugins\robot_ik_loader.jspre```. Unity will execute ```*.jspre``` files before initialization of the core application. This one is used to declare the RobotIK placeholder, as well as dynamically load the actual IK plugin at runtime.

Once these steps are in place, it functions much in the same way as the native plugin, with the exception that some data types (arrays, strings) need to be handled slightly differently.
