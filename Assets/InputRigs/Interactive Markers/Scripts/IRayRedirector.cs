using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IRayRedirector : MonoBehaviour
{
    public virtual bool Redirect(ref Ray ray, RaycastHit hit) { return false; }
}
