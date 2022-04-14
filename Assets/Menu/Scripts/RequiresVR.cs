using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebXR;
using UnityEngine.UI;

public class RequiresVR : ConditionalButton
{
    protected override bool Enabled()
    {
        return SystemManager.xrRunning;
    }
}
