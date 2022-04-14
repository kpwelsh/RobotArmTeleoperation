using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class CameraRedirector : IRayRedirector
{
    public CompositeCamera Cam;

    public override bool Redirect(ref Ray ray, RaycastHit hit)
    {
        Ray? redirected = Cam.ViewportPointToRay(hit.textureCoord);
        if (redirected.HasValue) {
            ray = redirected.Value;
            return true;
        }
        return false;
    }
}
