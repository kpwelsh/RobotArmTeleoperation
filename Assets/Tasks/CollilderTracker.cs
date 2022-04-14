using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Utils;

public class ColliderTracker : MonoBehaviour
{
    public delegate bool ColliderFilter(Collider collider);
    protected ColliderFilter filter;
    public delegate void CountListener(HashSet<Collider> counted);
    public CountListener OnCountChange;
    protected int numberOfTriggers = 0;
    private int _lastCount = 0;
    private int lastCount {
        get {
            return _lastCount;
        }
        set {
            if (value != _lastCount && OnCountChange != null) {
                OnCountChange(CountedColliders);
                _lastCount = value;
            }
        }
    }
    private Dictionary<Collider, int> containedColliders = new Dictionary<Collider, int>();
    public HashSet<Collider> CountedColliders = new HashSet<Collider>();
    // Start is called before the first frame update
    protected virtual void Start()
    {
        foreach (var collider in gameObject.GetComponentsInChildren<Collider>()) {
            if (collider.isTrigger && collider.IsConvex()) {
                numberOfTriggers++;
            }
        }

        if (this.filter == null)
            SetFilter((Collider c) => true);
    }

    void Update()
    {
        if (OnCountChange != null) {
            Count();
        }
    }

    public virtual void SetFilter(ColliderFilter filter) {
        this.filter = filter;
        Count();
    }

    public int Count() {
        CountedColliders.Clear();
        foreach ((Collider c, int i) in containedColliders) {
            if (i >= numberOfTriggers && filter(c))
                CountedColliders.Add(c);
        }
        lastCount = CountedColliders.Count;
        return lastCount;
    }

    void OnTriggerEnter(Collider other) {
        if (containedColliders.ContainsKey(other)) {
            containedColliders[other]++;
        } else {
            containedColliders[other] = 1;
        }
    }
    void OnTriggerExit(Collider other) {
        if (containedColliders.ContainsKey(other)) {
            containedColliders[other]--;
        }
    }
}
