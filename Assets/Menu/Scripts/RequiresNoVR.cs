using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequiresNoVR : ConditionalButton
{
    protected override bool Enabled()
    {
        return !SystemManager.xrRunning;
    }
}
