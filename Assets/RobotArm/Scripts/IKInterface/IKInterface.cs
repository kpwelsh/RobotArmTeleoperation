using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class IKInterfaceException: Exception {
    public IKInterfaceException(string msg) : base(msg) { }
}
public class IKInterface
{

#if UNITY_EDITOR
    [DllImport("robot_ik_native", EntryPoint = "new_solver", CallingConvention = CallingConvention.Cdecl)]
    unsafe private static extern void* new_solver(string urdf, string ee_frame);

    [DllImport("robot_ik_native", EntryPoint = "solve", CallingConvention = CallingConvention.Cdecl)]
    unsafe private static extern bool solve(void* solver_ptr, float* current_q, float[] target, float* q);
    
    [DllImport("robot_ik_native", EntryPoint = "set_self_collision", CallingConvention = CallingConvention.Cdecl)]
    unsafe private static extern bool set_self_collision(void* solver_ptr, bool self_collision_enabled);

    [DllImport("robot_ik_native", EntryPoint = "deallocate", CallingConvention = CallingConvention.Cdecl)]
    unsafe private static extern void deallocate(void* solver_ptr);

#elif UNITY_WEBGL
    [DllImport("__Internal")]
    unsafe private static extern void* new_solver(string urdf, string ee_frame);
    [DllImport("__Internal")]
    unsafe private static extern bool solve(void* solver_ptr, float[] current_q, float[] target, float[] q);
    [DllImport("__Internal")]
    unsafe private static extern bool set_self_collision(void* solver_ptr, bool self_collision_enabled);
    [DllImport("__Internal")]
    unsafe private static extern void deallocate(void* solver_ptr);
#endif

    unsafe void* solver_ptr = null;

    public IKInterface(string urdf, string ee_frame) {
        unsafe {
            solver_ptr = new_solver(urdf, ee_frame);
            if (solver_ptr == null) {
                throw new IKInterfaceException("Failed to construct IK solver");
            }
        }
    }

    public bool TrySolve(float[] current_joints, Vector3 position, Quaternion rotation, out float[] q) {
        q = new float[current_joints.Length];

        float[] target = new float[7];
        target[0] = position.x;
        target[1] = position.y;
        target[2] = position.z;
        target[3] = rotation.w;
        target[4] = rotation.x;
        target[5] = rotation.y;
        target[6] = rotation.z;

        bool success = false;

        #if UNITY_EDITOR
            unsafe {
                fixed (float* current_q_ptr = current_joints) {
                    fixed (float* q_ptr = q) {
                        success = solve(solver_ptr, current_q_ptr, target, q_ptr);
                    }
                }
            }
        #elif UNITY_WEBGL
            unsafe {
                success = solve(solver_ptr, current_joints, target, q);
            }
        #endif
        return success;
    }

    ~IKInterface() {
        unsafe {
            deallocate(solver_ptr);
        }
    }
}
