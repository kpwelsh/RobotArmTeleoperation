using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class PoringSubTask : SubTask
{
    public GameObject BallParent;
    public SelfBoundedColliderTracker TargetCup;
    private List<GameObject> Balls = new List<GameObject>();
    private int completedCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in BallParent.transform) {
            Balls.Add(child.gameObject);
        }

        TargetCup.SetFilter((Collider c) => c.gameObject.HasParent(BallParent));

        TargetCup.OnCountChange += (HashSet<Collider> colliders) => {
            completedCount = colliders.Count;
        };
    }

    public override bool Complete() {
        return completedCount == Balls.Count;
    }

    public override string ProgressDisplay() {
        return string.Format("{0}/{1}", completedCount, Balls.Count);
    }
}
