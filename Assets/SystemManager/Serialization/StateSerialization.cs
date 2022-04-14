using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Utils;


[Serializable]
public struct Frame {
    [SerializeField]
    public float timestamp;
    [SerializeField]
    public List<SerializableTrans> transforms;
}

[Serializable]
public struct SerializableTrans {
    [SerializeField]
    public string id;
    [SerializeField]
    public SerializableVector3 position;
    [SerializeField]
    public SerializableQuaternion rotation;

    public SerializableTrans(string id, Vector3 p, Quaternion q) {
        this.id = id;
        position = p;
        rotation = q;
    }
    public SerializableTrans(Transform trans, GameObject root = null) {
        this.id = trans.gameObject.FullName(root);
        position = trans.position;
        rotation = trans.rotation;
    }

    public bool neq(SerializableTrans other, float dx, float da) {
        return ((Vector3)other.position - position).magnitude >= dx
            || Quaternion.Angle(other.rotation, rotation) >= da;
    }
}

