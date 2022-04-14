using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class PoseMatcher : MonoBehaviour
{
    public Vector3 TargetPosition;
    public Quaternion TargetRotation;
    public float PositionThreshold = 0.01f;
    public float AngleThreshold = 0.01f;

    private (Vector3, Quaternion)? initialPose = null;

    public bool Match() {
        return (transform.position - TargetPosition).magnitude <= PositionThreshold
            && Quaternion.Angle(transform.rotation, TargetRotation) <= AngleThreshold;
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(PoseMatcher))]
    public class PoseMatcherEditor : Editor {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector ();
            PoseMatcher poseMatcher = (PoseMatcher)target;

            if (GUILayout.Button("Set Start Pose")) {
                poseMatcher.initialPose = (poseMatcher.transform.position, poseMatcher.transform.rotation);
            }
            if (GUILayout.Button("Reset Pose")) {
                if (!poseMatcher.initialPose.HasValue) {
                    Debug.LogError("Set the intial pose before reseting to it.");
                } else {
                    (Vector3 t, Quaternion rot) = poseMatcher.initialPose.Value;
                    poseMatcher.transform.position = t;
                    poseMatcher.transform.rotation = rot;
                }
            }
            if (GUILayout.Button("Set Goal Pose")) {
                poseMatcher.TargetPosition = poseMatcher.transform.position;
                poseMatcher.TargetRotation = poseMatcher.transform.rotation;
            }
        }
    }
    #endif
}

