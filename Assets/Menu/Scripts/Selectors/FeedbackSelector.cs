public class FeedbackSelector : ListSelect<SystemManager.VisualFeedback> {
    protected override void SetValue(SystemManager.VisualFeedback value)
    {
        SystemManager.Feedback = value;
    }
}