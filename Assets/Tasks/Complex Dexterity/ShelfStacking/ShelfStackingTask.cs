using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class ShelfStackingTask : SubTask
{

    public GameObject Stackables;
    public CompletelyContainedColliderTracker StackArea;
    private int completed = 0;
    private int total = 0;
    void Start()
    {
        foreach (var child in Stackables.transform) {
            total++;
        }
        StackArea.SetFilter((Collider c) => c.gameObject.HasParent(Stackables));
        StackArea.OnCountChange += (HashSet<Collider> counted) => { completed = counted.Count; };
    }

    public override bool Complete() {
        return completed >= total;
    }

    public override string ProgressDisplay()
    {
        return string.Format("{0}/{1}", completed, total);
    }
}
