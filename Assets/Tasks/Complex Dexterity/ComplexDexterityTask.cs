using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ComplexDexterityTask : Task
{
    public Clock clock;
    void Start() {

        clock.StartTimer(() => {
            Pause();
        }, 240);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        clock.StartStopwatch();
    }
}
