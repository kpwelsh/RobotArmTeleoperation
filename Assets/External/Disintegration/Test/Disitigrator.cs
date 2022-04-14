using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disitigrator : MonoBehaviour
{
    void OnTriggerEnter(Collider other) {
        var disintigrable = other.GetComponent<A5BGames.DisintegrationEffect.Disintegratable>();
        if (disintigrable != null) {
            AudioSource sound;
            disintigrable.Disintegrate(Vector3.zero);
            if (other.TryGetComponent<AudioSource>(out sound))
                sound?.PlayDelayed(0f);
        }
    }
}
