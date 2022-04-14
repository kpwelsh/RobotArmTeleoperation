using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ColliderCounter : MonoBehaviour
{
    public Text Display;
    public Collider DetectionArea;
    public string Tag;
    public int Count {
        get {
            return Colliders.Count;
        }
    }
    public HashSet<Collider> Colliders = new HashSet<Collider>();
    void Start()
    {
        if (DetectionArea == null) {
            DetectionArea = GetComponentInChildren<Collider>();
        }
    }

    void Update() {
        if (!(Display == null || Display.Equals(null))) {
            Display.text = String.Format("{0:d}", Count);
        }
    }

    public void OnTriggerEnter(Collider other) {
        if (other.tag.CompareTo(Tag) != 0) {
            Debug.Log(other.tag);
            return;
        }
        Colliders.Add(other);
        AudioSource source;
        if (other.TryGetComponent<AudioSource>(out source)) {
            source.PlayDelayed(0f);
        }
    }

    public void OnTriggerExit(Collider other) {
        Colliders.Remove(other);
    }
}
