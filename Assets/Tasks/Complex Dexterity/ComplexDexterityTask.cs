using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ComplexDexterityTask : Task
{
    public Clock clock;
    private List<SubTask> subTasks;
    void Start() {
        subTasks = GetComponentsInChildren<SubTask>().ToList();

        clock.StartTimer(() => {
            Pause();
        }, 240);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        clock.StartStopwatch();
    }
}
