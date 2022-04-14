public class RobotSpeedSelector : ListSelect<SystemManager.ModifierLevel> {
    protected override void SetValue(SystemManager.ModifierLevel value)
    {
        SystemManager.RobotSpeed = value;
    }
}