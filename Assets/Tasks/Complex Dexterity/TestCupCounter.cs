using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCupCounter : MonoBehaviour
{
    public ColliderTracker container;
    // Update is called once per frame
    void Update()
    {
        Debug.Log(container.Count());
    }
}
