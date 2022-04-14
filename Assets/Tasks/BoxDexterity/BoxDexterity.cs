using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxDexterity : Task
{
    public ColliderCounter EndBox;
    public float TimeLimit;
    public Clock TaskTimer;
    private bool started = false;
    private int Score;

    public override void OnDisable()
    {
        base.OnDisable();
        TaskTimer?.StopTimer();
        
    }
    private void StartTask() {
        if (started) return;
        started = true;
        TaskTimer.StartTimer(
            StopTask, 
            TimeLimit
        );
    }

    private void StopTask() {
        if (!started) return;
        Score = EndBox.Count;
        Pause();
    }

    void Update()
    {
        if (EndBox.Count > 0) {
            StartTask();
        }
    }

    public override void Reset()
    {
        started = false;
        base.Reset();
    }
}
