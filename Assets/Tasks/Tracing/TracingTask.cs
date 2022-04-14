using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TracingTask : Task
{
    public Clock clock;
    // Start is called before the first frame update
    void Start()
    {
        clock.StartTimer(() => {
            Pause();
        }, 240);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
