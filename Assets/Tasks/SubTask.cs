using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubTask : MonoBehaviour
{
    public string Name = "Task";
    public virtual bool Complete() {
        return false;
    }

    public virtual string ProgressDisplay() {
        return Complete().ToString();
    }
}
