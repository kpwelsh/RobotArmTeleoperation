using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Utils;

public class RobotController : HasSystemManager
{
    public Transform HandTrans;
    public List<float> InitialJoints = new List<float>();
    public float Force = 1e10f;
    public float Stiffness = 10f;
    public float Damping = 1f;
    public EndEffector EE;
    public Transform target;
    protected ArticulationBody[] arm;
    protected List<float> current_target = new List<float>();
    protected virtual void Start()
    {
        arm = GetComponentsInChildren<ArticulationBody>();
        if (EE == null) {
            EE = GetComponentInChildren<EndEffector>();
        }
        current_target = Constrain(InitialJoints);

        SetDynamics();
        SetIKTargetDynamics();
        SetJoints(current_target);
    }

    public void TrackTransform(Transform trans) {
        target.GetComponent<Tracker>().Target = trans;
    }

    protected void SetDynamics() {
        foreach (ArticulationBody joint in arm) {
            if (joint.jointType != ArticulationJointType.FixedJoint) {
                var pos = joint.jointPosition;
                ArticulationDrive drive = joint.xDrive;
                drive.stiffness = drive.stiffness * getStiffness();
                //drive.damping = drive.damping;// * getStiffness();
                //drive.forceLimit = Force;
                joint.xDrive = drive;
            }
        }
    }

    protected float getStiffness() {
        SystemManager.ModifierLevel speed = SystemManager.RobotSpeed;
        switch (speed) {
            case SystemManager.ModifierLevel.VeryLow:   return 0.1f;
            case SystemManager.ModifierLevel.Low:       return 0.2f;
            case SystemManager.ModifierLevel.Medium:    return 0.5f;
            case SystemManager.ModifierLevel.High:      return 0.7f;
            case SystemManager.ModifierLevel.VeryHigh:  return 1.0f;
            default: throw new Exception();
        }
    }

    protected float getLatency() {
        SystemManager.ModifierLevel latency = SystemManager.Latency;
        switch (latency) {
            case SystemManager.ModifierLevel.VeryLow:   return 0;
            case SystemManager.ModifierLevel.Low:       return 30;
            case SystemManager.ModifierLevel.Medium:    return 100;
            case SystemManager.ModifierLevel.High:      return 200;
            case SystemManager.ModifierLevel.VeryHigh:  return 500;
            default: throw new Exception();
        }
    }

    protected float getFilterFrequency() {
        SystemManager.ModifierLevel responsiveness = SystemManager.RobotResponsiveness;
        switch (responsiveness) {
            case SystemManager.ModifierLevel.VeryLow:   return 0.1f;
            case SystemManager.ModifierLevel.Low:       return 1f;
            case SystemManager.ModifierLevel.Medium:    return 4f;
            case SystemManager.ModifierLevel.High:      return 10f;
            case SystemManager.ModifierLevel.VeryHigh:  return 1000f;
            default: throw new Exception();
        }
    }


    protected void SetIKTargetDynamics() {
        Tracker tracker = target?.GetComponent<Tracker>();
        if (tracker != null) {
            tracker.Latency_ms = getLatency();
            tracker.LowpassFilter = getFilterFrequency();
        }
    }
    protected void SetJoints(List<float> q) {
        q = Constrain(q);
        int i = 0;
        foreach (ArticulationBody joint in arm) {
            if (joint.jointType != ArticulationJointType.FixedJoint) {
                var pos = joint.jointPosition;
                ArticulationDrive drive = joint.xDrive;
                drive.target = (float)q[i] * 180f / Mathf.PI;
                joint.xDrive = drive;
                i++;
            }
            if (i >= q.Count) break;
        }
    }
    protected List<float> Constrain(List<float> q) {
        List<float> constrained = new List<float>();
        int i = 0;
        foreach (ArticulationBody joint in arm) {
            if (joint.jointType != ArticulationJointType.FixedJoint) {
                ArticulationDrive drive = joint.xDrive;
                constrained.Add(Mathf.Min(drive.upperLimit * Mathf.PI / 180f, Mathf.Max(drive.lowerLimit * Mathf.PI / 180f, q[i])));
                i++;
            }
            if (i >= q.Count) break;
        }
        return constrained;
    }

    public (Vector3, Quaternion) targetInRobotFrame() {
        return inRobotFrame(target);
    }

    public (Vector3, Quaternion) inRobotFrame(Transform trans) {
        Matrix4x4 T = transform.worldToLocalMatrix * trans.localToWorldUnscaled();
        // If the robot has an end effector, then invert that position to 
        // get the desired hand position.
        Matrix4x4 eeOffset = Matrix4x4.identity;
        if (EE?.IKPoint != null) {
            eeOffset = HandTrans.worldToLocalMatrix * EE.IKPoint.localToWorldUnscaled();
        }
        T = T * eeOffset.inverse;
        return toRightHanded(T);
    }

    public (Vector3, Quaternion) toRightHanded(Matrix4x4 m) {

        Matrix4x4 M = new Matrix4x4(
            new Vector4( 0, -1, 0, 0),
            new Vector4( 0, 0, 1, 0),
            new Vector4( 1, 0, 0, 0),
            new Vector4( 0, 0, 0, 1)
        ).transpose;
        m = M.inverse * m * M;
        Vector3 t = m.GetPosition();
        Quaternion q = m.rotation;
        return (new Vector3(t.x, t.y, t.z), new Quaternion(q.x, q.y, q.z, q.w));
    }

    public void ClearChildren(Transform parent) {
        for (int i = 0; i < parent.childCount; i++) {
            Transform child = parent.GetChild(i);
            DestroyImmediate(child.gameObject);
        }
    } 
    public virtual void SetEE(GameObject ee) {
        if (EE != null) Destroy(EE.gameObject);
        var eeObj = Instantiate(ee, HandTrans);
        EE = eeObj.GetComponent<EndEffector>();
    }

    public void SetEEPoseOnce(Transform trans) {
        target.position = trans.position;
        target.rotation = trans.rotation;
    }
}
