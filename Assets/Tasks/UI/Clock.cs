using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System;

public class Clock : MonoBehaviour
{
    private enum Mode {
        Timer,
        Stopwatch
    };

    private bool Paused = false;
    public UnityEngine.UI.Text text;
    public float time {
        get;
        private set;
    }
    private Action Callback;
    private Mode? mode = null;

    public void StartTimer(Action callback, float duration) {
        Callback = callback;
        time = duration;
        mode = Mode.Timer;
    }

    public void StopTimer() {
        Callback = null;
        mode = null;
    }

    public void StartStopwatch() {
        Callback = null;
        mode = Mode.Stopwatch;
        time = 0;
    }

    public void StopStowatch() {
        Callback = null;
        mode = null;
    }

    public void Pause() {
        Paused = true;
    }
    public void UnPause() {
        Paused = false;
    }

    void FixedUpdate() {
        if (Paused) return;
        float dt = Time.fixedDeltaTime;
        switch (mode) {
            case Mode.Timer: {
                timerUpdate(dt);
                break;
            }
            case Mode.Stopwatch: {
                stopwatchUpdate(dt);
                break;
            }
        }
        displayTime();
    }

    void stopwatchUpdate(float dt) {
        time += dt;
    }

    void timerUpdate(float dt) {
        time -= dt;
        if (time <= 0) {
            time = 0;
            StopTimer();
        }
    }

    void displayTime() {
        text.text = String.Format("{0:F0}", time);
    }

}
