using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputPoseProvider : MonoBehaviour
{
    public Transform ControlPerspective = null;
    public Transform Pose {
        get;
        protected set;
    }
    public virtual void SetPoseFromExternal(Transform trans) {
        if (trans == null) return;
        transform.position = trans.position;
        transform.rotation = trans.rotation;
    }

    public virtual void Reset() { }
}
