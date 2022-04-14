using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SubTaskCompletionDisplay : MonoBehaviour
{
    public GameObject SubTaskParent;
    public GameObject ScoreDisplayPrefab;
    public RectTransform ScoreDisplayParent;
    private List<SubTask> subTasks;

    private List<Text> subTasksDisplay = new List<Text>();

    void Start()
    {
        subTasks = new List<SubTask>(
            SubTaskParent.GetComponentsInChildren<SubTask>()
        );

        foreach (var st in subTasks) {
            var go = Instantiate(ScoreDisplayPrefab, ScoreDisplayParent);
            subTasksDisplay.Add(go.GetComponentInChildren<Text>());
        }
    }

    void Update()
    {
        for (var i = 0; i < subTasks.Count; i++) {
            var st = subTasks[i];
            var display = subTasksDisplay[i];

            display.text = string.Format("{0}: {1}", st.Name, st.ProgressDisplay());
        }
    }
}
