public class InputSelector : ListSelect<SystemManager.InputDevice> {
    protected override void SetValue(SystemManager.InputDevice value)
    {
        SystemManager.Input = value;
    }
}