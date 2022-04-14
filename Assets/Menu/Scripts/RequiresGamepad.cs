using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RequiresGamepad : ConditionalButton
{
    protected override bool Enabled()
    {
        return Gamepad.all.Count > 0;
    }
}
