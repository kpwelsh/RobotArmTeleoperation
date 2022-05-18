using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class TransformTree
{
    Dictionary<string, Transform> NamedTransforms;
    public TransformTree(Transform transform) {
        NamedTransforms = new Dictionary<string, Transform>();

        foreach (var t in transform.AllChildren()) {
            var go = t.gameObject;
            Insert(go.FullName(transform.gameObject), t);
        }
    }

    private void Insert(string name, Transform trans) {
        int i = 0;
        string modifiedName = name;
        while (NamedTransforms.ContainsKey(modifiedName)) {
            i++;
            modifiedName = string.Format("{0}-{1}", name, i);
        }

        NamedTransforms.Add(modifiedName, trans);
    }

    public bool TryGetValue(string name, out Transform trans) {
        trans = NamedTransforms.GetValueOrDefault(name, null);
        return trans != null;
    }

    public IEnumerable<Transform> IterTransforms() {
        return NamedTransforms.Values;
    }

    public IEnumerable<KeyValuePair<string, Transform>> IterItems() {
        return NamedTransforms;
    }
}
