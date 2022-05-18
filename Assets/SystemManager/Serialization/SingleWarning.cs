using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleWarning
{
    private HashSet<string> PreviousWarnings;

    public SingleWarning() {
        PreviousWarnings = new HashSet<string>();
    }

    public void Clear() {
        PreviousWarnings.Clear();
    }

    public void LogWarning(string msg) {
        if (PreviousWarnings.Add(msg)) {
            Debug.LogWarning(msg);
        }
    }
}
