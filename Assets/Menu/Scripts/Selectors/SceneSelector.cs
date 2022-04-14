using UnityEngine;

public class SceneSelector : ListSelect<GameObject> {
    protected override void SetValue(GameObject value)
    {
        SystemManager.TargetScene = value;
    }
}