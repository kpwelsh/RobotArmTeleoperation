using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NativePluginController : RobotController
{
    public string HandJointName;
    public TextAsset URDF;
    protected IKInterface iKInterface = null;
    // Start is called before the first frame update
    protected override void Start() {
        base.Start();
        
        if (URDF != null) {
            try {
                iKInterface = new IKInterface(URDF.text, HandJointName);
            } catch (IKInterfaceException e) {
                Debug.LogError(e.ToString());
                iKInterface = null;
            }
        }
    }

    void FixedUpdate() {
        if (target != null && iKInterface != null)
            updateIK();
    }

    void updateIK() {
        (Vector3 translation, Quaternion rotation) = targetInRobotFrame();
        float[] q;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        bool success = iKInterface.TrySolve(
            current_target.ToArray(),
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
}
