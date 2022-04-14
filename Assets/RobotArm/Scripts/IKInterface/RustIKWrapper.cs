using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public static class RustIKWrapper
{

#if UNITY_EDITOR
    [DllImport("robot_ik_native.dll", EntryPoint = "init", CallingConvention = CallingConvention.Cdecl)]
    private static extern int init(string urdf, int dof);
    [DllImport("robot_ik_native.dll", EntryPoint = "solve", CallingConvention = CallingConvention.Cdecl)]
    unsafe private static extern int solve(int robotId, float* current_q, string frame, float[] target, float* q);
    
    [DllImport("robot_ik_native.dll", EntryPoint = "deallocate", CallingConvention = CallingConvention.Cdecl)]
    unsafe private static extern void deallocate();
#elif UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern int init(string urdf, int dof);
    [DllImport("__Internal")]
    private static extern int solve(int robotId, int dof, float[] current_q, string frame, float[] target, float[] q);
#endif

    private static HashSet<int> Allocated = new HashSet<int>();


    public static int Init(string urdf, int dof) {
        int code = init(urdf, dof);
        if (code < 0) {
            Debug.Log("Failed to initialize robot");
            Debug.Log(code);
        } else {
            Debug.Log("Initialized robot");
            Debug.Log(code);
            Allocated.Add(code);
        }
        return code;
    }

    public static bool TrySolve(int robotId, int dof, float[] current_joints, string EE_frame, Vector3 position, Quaternion rotation, out float[] q) {
        q = new float[current_joints.Length];
        if (robotId < 0) {
            Debug.Log("Invalid robot id");
            return false;
        }

        float[] target = new float[7];

        target[0] = position.x;
        target[1] = position.y;
        target[2] = position.z;
        target[3] = rotation.w;
        target[4] = rotation.x;
        target[5] = rotation.y;
        target[6] = rotation.z;

        int v = -1;

    #if UNITY_EDITOR
        unsafe {
            fixed (float* current_q_ptr = current_joints) {
                fixed (float* q_ptr = q) {
                    v = solve(robotId, current_q_ptr, EE_frame, target, q_ptr);
                }
            }
        }
    #elif UNITY_WEBGL
        v = solve(robotId, dof, current_joints, EE_frame, target, q);
    #endif
        switch (v) {
            case 0: return true;
            default: Debug.Log(string.Format("IK Error Code: {0}", v)); break;
        }
        return false;
    }

    public static void Deallocate(int robotId) {
        Allocated.Remove(robotId);
    #if UNITY_EDITOR
        if (Allocated.Count == 0) {
            deallocate();
        }
    #endif
    }
}
