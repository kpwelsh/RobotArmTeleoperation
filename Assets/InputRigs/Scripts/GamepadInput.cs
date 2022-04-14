using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadInput : InputPoseProvider
{
    public float TranslationVelocity = 1f;
    public float RotationVelocity = 1f;
    private Vector3 velocity = Vector3.zero;
    private Vector3 rotationVelocity = Vector3.zero;
    private Gamepad gp;

    private IEnumerator Start() {
        while (gp == null)
        {
            gp = InputSystem.GetDevice<Gamepad>();
            yield return null;
        }
        Pose = transform;
    }
    void Update() {
        if (gp != null) {
            Vector2 ls = gp.leftStick.ReadValue();
            float up = gp.rightShoulder.ReadValue() - gp.leftShoulder.ReadValue();

            velocity = new Vector3(
                ls.x,
                up,
                ls.y
            );

            Vector2 rs = gp.rightStick.ReadValue();
            float rz = gp.rightTrigger.ReadValue() - gp.leftTrigger.ReadValue();
            rotationVelocity = new Vector3(
                rs.x,
                rs.y,
                rz
            );
        }
    }
    void FixedUpdate()
    {
        Vector3 transformedVelocity = velocity;
        if (ControlPerspective != null) {
            transformedVelocity = ControlPerspective.rotation * transformedVelocity;
        }
        transform.position += TranslationVelocity * transformedVelocity * Time.fixedDeltaTime;

        transform.rotation = Quaternion.Euler(
            RotationVelocity * rotationVelocity.y, 
            -RotationVelocity * rotationVelocity.x, 
            -RotationVelocity * rotationVelocity.z
        ) * transform.rotation;
    }
}
