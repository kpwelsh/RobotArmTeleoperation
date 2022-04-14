using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public static class RustControlWrapper
{

    [DllImport("compliant_control_native.dll", EntryPoint = "init", CallingConvention = CallingConvention.Cdecl)]
    private static extern int init(string urdf, int dof);
    [DllImport("compliant_control_native.dll", EntryPoint = "solve", CallingConvention = CallingConvention.Cdecl)]
    unsafe private static extern int solve(int robotId, float* current_q, float* current_v, string frame, float[] target, float* q, float stiffness, float damping, float dt);


    public static int Init(string urdf, int dof) {
        int code = init(urdf, dof);
        if (code < 0) {
            Debug.Log("Failed to initialize robot");
            Debug.Log(code);
        }
        return code;
    }

    public static bool TrySolve(int robotId, int dof, float[] current_joints, float[] current_velocity, string EE_frame, 
                                Vector3 position, Quaternion rotation, out float[] q, float stiffness, float damping,
                                float dt = 0.02f) {
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

        unsafe {
            fixed (float* current_q_ptr = current_joints) {
                fixed (float* q_ptr = q) {
                    fixed (float* current_v_ptr = current_velocity) {
                        v = solve(robotId, current_q_ptr, current_v_ptr, EE_frame, target, q_ptr, stiffness, damping, dt);
                    }
                }
            }
        }
        switch (v) {
            case 0: return true;
            default: Debug.Log(string.Format("IK Error Code: {0}", v)); break;
        }
        return false;
    }
}
