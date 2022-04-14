public class DifficultySelector : ListSelect<SystemManager.Difficulty> {
    protected override void SetValue(SystemManager.Difficulty value)
    {
        SystemManager.TaskDifficulty = value;
    }
}