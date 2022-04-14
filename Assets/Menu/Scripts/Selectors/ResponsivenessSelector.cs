public class ResponsivenessSelector : ListSelect<SystemManager.ModifierLevel> {
    protected override void SetValue(SystemManager.ModifierLevel value)
    {
        SystemManager.RobotResponsiveness = value;
    }
}