using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public class XRButton : XRSimpleInteractable
{
    public Button button {
        get;
        private set;
    }
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
    }

    protected override void Reset() {
        BoxCollider collider = GetComponent<BoxCollider>();
        if (collider == null) {
            collider = gameObject.AddComponent<BoxCollider>();
            var rectTrans = GetComponent<RectTransform>();
            collider.size = new Vector3(rectTrans.rect.width, rectTrans.rect.height, 0);
        }
        base.Reset();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        button?.onClick?.Invoke();
    }

    public void Select() {
        button?.onClick?.Invoke();
    }
}
