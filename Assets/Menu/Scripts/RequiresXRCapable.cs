using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequiresXRCapable : ConditionalButton
{
    protected override bool Enabled()
    {
        return SystemManager.XREnabled();
    }
}