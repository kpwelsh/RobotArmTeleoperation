using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PoseMatchingSubTask : SubTask
{
    private List<PoseMatcher> matchTasks;
    void Start()
    {
        matchTasks = new List<PoseMatcher>(gameObject.GetComponentsInChildren<PoseMatcher>());
    }

    public override bool Complete()
    {
        return matchTasks.All((PoseMatcher pm) => pm.Match());
    }

    public override string ProgressDisplay()
    {
        int complete = matchTasks.Count((PoseMatcher pm) => pm.Match());
        return string.Format("{0}/{1}", complete, matchTasks.Count);
    }
}
