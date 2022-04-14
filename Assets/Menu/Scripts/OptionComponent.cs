using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class IOptionComponent<T> : HasSystemManager
{
    [Serializable]
    public struct Mapping {
        public GameObject A;
        public T B;
    }
    public Mapping[] Map;

    protected Dictionary<GameObject, T> _mapping = new Dictionary<GameObject, T>();

    public void Init() {
        foreach (Mapping m in Map) {
            _mapping.Add(m.A, m.B);
        }
    }

    protected T GameObjectToT(GameObject o) {
        return _mapping[o];
    }
}
