public class LatencySelector : ListSelect<SystemManager.ModifierLevel> {
    protected override void SetValue(SystemManager.ModifierLevel value)
    {
        SystemManager.Latency = value;
    }
}