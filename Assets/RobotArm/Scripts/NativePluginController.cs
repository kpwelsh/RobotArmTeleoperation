using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NativePluginController : RobotController
{
    public string HandJointName;
    public TextAsset URDF;
    private int IKRobotID = -1;
    // Start is called before the first frame update
    protected override void Start() {
        base.Start();
        
        if (URDF != null) {
            IKRobotID = RustIKWrapper.Init(URDF.text, InitialJoints.Count);
            if (IKRobotID < 0) Debug.Log(URDF.text);
        }
    }

    void FixedUpdate() {
        if (target != null && IKRobotID >= 0)
            updateIK();
    }

    void updateIK() {
        (Vector3 translation, Quaternion rotation) = targetInRobotFrame();
        float[] q;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        bool success = RustIKWrapper.TrySolve(
            IKRobotID,
            InitialJoints.Count,
            current_target.ToArray(),
            HandJointName,
            translation,
            rotation,
            out q
        );
        watch.Stop();
        if (watch.ElapsedMilliseconds >= 1) {
            Debug.LogWarning($"Warning, IK Solution took {watch.ElapsedMilliseconds}ms");
        }
        if (success) {
            for (int i = 0; i < q.Length; i++) current_target[i] = q[i];
        }
        SetJoints(current_target);
    }


    ~NativePluginController() {
        if (IKRobotID >= 0) {
            RustIKWrapper.Deallocate(IKRobotID);
        }
    }
}
