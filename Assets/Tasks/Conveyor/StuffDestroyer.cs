using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class StuffDestroyer : MonoBehaviour
{
    public List<GameObject> DontDestroy = new List<GameObject>();
    bool canDestroy(GameObject o) {
        foreach (var listed in DontDestroy) {
            if (o == listed) {
                return false;
            }
        }
        return true;
    }
    
    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.gameObject.findRigidBody();
        if (rb == null) return;

        if (canDestroy(rb.gameObject.getRoot())) {
            Destroy(rb.gameObject);
        }
    }
}
