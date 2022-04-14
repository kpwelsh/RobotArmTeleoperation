using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
public class Task : HasSystemManager
{
    [Serializable]
    public struct DifficultyMap {
        public SystemManager.Difficulty TaskDifficulty;
        public GameObject TaskConfig;
    }
    public static Task ActiveTask;
    public List<DifficultyMap> difficultyMaps;
    public Transform RobotPosition;
    public GameObject EndEffector;
    public List<Camera> SceneCameras;
    public Transform StartingPosition = null;
    private bool isPaused = false;

    public virtual void OnEnable() {
        Task.ActiveTask = this;
        SelectConfiguration();
        LoadRobot();
    }
    
    public virtual void OnDisable() {
        if (Task.ActiveTask == this) {
            Task.ActiveTask = null;
        }
        UnloadRobot();
    }

    public void SelectConfiguration() {
        SystemManager.Difficulty taskDifficulty = SystemManager.TaskDifficulty;
        bool found = false;
        foreach (DifficultyMap map in difficultyMaps) {
            map.TaskConfig.SetActive(map.TaskDifficulty == taskDifficulty);
            found = found || map.TaskDifficulty == taskDifficulty;
        }
        if (!found && difficultyMaps.Count > 0) {
            difficultyMaps[0].TaskConfig.SetActive(true);
            Debug.LogWarning("Could not find configuration associated with selected difficulty. Loading first config.");
        } else if (!found) {
            Debug.LogWarning("Could not find configuration associated with selected difficulty. Loading implicit config.");
        }
    }

    private void LoadRobot() {
        GameObject robot = Instantiate(SystemManager.TargetRobot, RobotPosition.transform);
        RobotController controller = robot.GetComponent<RobotController>();
        controller.SetEE(EndEffector);
        controller.SetEEPoseOnce(StartingPosition);
    }

    private void UnloadRobot() {
        foreach (Transform trans in RobotPosition) {
            Destroy(trans.gameObject);
        }
    }

    public Transform GetStartingPosition() {
        if (StartingPosition == null) return transform;
        return StartingPosition;
    }

    public void Pause() {
        isPaused = true;
        foreach (RobotController controller in RobotPosition.GetComponentsInChildren<RobotController>()) {
            controller.enabled = false;
        }
    }

    public void UnPause() {
        isPaused = false;
        foreach (RobotController controller in RobotPosition.GetComponentsInChildren<RobotController>()) {
            controller.enabled = true;
        }
    }

    public virtual void Reset() {
        gameObject.SetActive(false);
        Delay(() => gameObject.SetActive(true), 0.1f);
    }

    public static void ResetActiveTask() {
        if (Task.ActiveTask == null || Task.ActiveTask.Equals(null)) {
            return;
        }
        Task.ActiveTask.Reset();
    }

    public void OnReset() {
        Reset();
    }
    public void OnMenu(InputValue value) {
        TogglePause();
    }
    
    public void OnXRMenu(object value) {
        bool? vb = value as bool?;
        if (vb.HasValue && vb.Value) {
            TogglePause();
        }
    }

    private void TogglePause() {
        if (isPaused) UnPause();
        else Pause();
    }

    IEnumerator Delay(Action action, float time) {
        yield return new WaitForSecondsRealtime(time);
        action.Invoke();
    }
}
