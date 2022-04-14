using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConditionalButton : HasSystemManager
{
    private Button button;
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
    }

    void Update() {
        if (button != null) {
            button.interactable = Enabled();
        }
    }

    protected virtual bool Enabled() {
        return true;
    }
}
