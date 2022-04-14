using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnterSceneButton : HasSystemManager
{
    private Button button;
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(SystemManager.StartScene);
    }

    // Update is called once per frame
    void Update()
    {
        if (button != null) button.interactable = SystemManager.IsComplete();
    }
}
