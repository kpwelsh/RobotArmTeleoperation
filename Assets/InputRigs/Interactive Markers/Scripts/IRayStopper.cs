using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IRayStopper : IRayRedirector
{
    public override bool Redirect(ref Ray ray, RaycastHit hit)
    {
        ray = new Ray(Vector3.positiveInfinity, Vector3.up);
        return true;
    }
}
