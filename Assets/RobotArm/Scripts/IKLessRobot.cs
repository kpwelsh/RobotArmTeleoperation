using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class IKLessRobot : RobotController
{
    protected override void Start() {
        SetIKTargetDynamics();
    }

    void FixedUpdate() {
        Matrix4x4 T = target.localToWorldUnscaled();
        // If the robot has an end effector, then invert that position to 
        // get the desired hand position.
        Matrix4x4 eeOffset = Matrix4x4.identity;
        if (EE?.IKPoint != null) {
            eeOffset = HandTrans.worldToLocalMatrix * EE.IKPoint.localToWorldUnscaled();
        }
        T = T * eeOffset.inverse;

        var body = HandTrans.gameObject.GetComponent<ArticulationBody>();
        body.linearDamping = Damping;
        body.angularDamping = Damping;

        var dx = T.GetPosition() - HandTrans.position;
        body.velocity = Time.fixedDeltaTime * Stiffness * dx;

        var torque = Vector3.Cross(HandTrans.up, T.rotation * Vector3.up);
        torque += Vector3.Cross(HandTrans.right, T.rotation * Vector3.right);
        torque += Vector3.Cross(HandTrans.forward, T.rotation * Vector3.forward);

        body.angularVelocity = Time.fixedDeltaTime * Stiffness * torque;

        // .TeleportRoot(
        //     T.GetPosition(),
        //     T.rotation
        // );
        
    }

}
