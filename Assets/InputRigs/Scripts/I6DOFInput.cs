using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class I6DOFInput : InputPoseProvider
{
    private Tracker tracker;
    void Start() {
        Pose = transform;
        tracker = GetComponent<Tracker>();
    }
    public override void SetPoseFromExternal(Transform trans) {
        if (trans == null) return;
        // transform.position = trans.position;
        // transform.rotation = trans.rotation;
    }

    public void OnXRGrip(object value) {
        bool gripping = (bool)value;
        if (gripping) {
            tracker.UnTrack();
        } else {
            tracker.Track();
        }
    }

    public override void Reset() {
        tracker?.ResetOffset();
    }
}
