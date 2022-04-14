using UnityEngine;

public class RobotSelector : ListSelect<GameObject> {
    protected override void SetValue(GameObject value)
    {
        SystemManager.TargetRobot = value;
    }
}