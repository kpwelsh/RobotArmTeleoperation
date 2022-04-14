using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
public class InteractiveMarker : InputPoseProvider
{
    [Serializable]
    public enum ControlMode {
        Translation,
        Rotation
    }

    private ControlMode _mode = ControlMode.Translation;
    public ControlMode Mode {
        get { return _mode; }
        set { 
            switch (value) {
                case ControlMode.Translation: {
                    Translation?.SetActive(true);
                    Rotation?.SetActive(false);
                    break;
                }
                case ControlMode.Rotation: {
                    Translation?.SetActive(false);
                    Rotation?.SetActive(true);
                    break;
                }
            }
            _mode = value;
        }
    }
    private GameObject Translation;
    private GameObject Rotation;
    
    void Awake() {
        Pose = transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        var dragTrans = GetComponentInChildren<DraggableTransform>(true);
        var dragRot = GetComponentInChildren<DraggableRotation>(true);

        Translation = dragTrans.gameObject;
        Rotation = dragRot.gameObject;

        if (Translation == null || Rotation == null) {
            Debug.LogError("Need a DraggableTransform and a DraggableRotation child.");
        }

        dragTrans.ControlledTransform = transform;
        dragRot.ControlledTransform = transform;

        Mode = _mode;
    }

    void OnEnable() {
        Mode = _mode;
    }

    void OnDisable() {
        Translation?.SetActive(false);
        Rotation?.SetActive(false);
    }

    void OnShift(InputValue value) {
        if (value.Get<float>() > 0.5) {
            Mode = ControlMode.Rotation;
        } else {
            Mode = ControlMode.Translation;
        }
    }
}
