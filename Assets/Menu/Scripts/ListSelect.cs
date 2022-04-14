using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListSelect<T> : IOptionComponent<T>
{
    public GameObject Default;
    public T SelectedValue {
        get;
        private set;
    }
    public bool Selected {
        get;
        private set;
    }
    public Color SelectedColor = new Color(1, 1, 1, 1);
    public Color NotSelectedColor = new Color(1, 1, 1, 0.4f);
    private List<GameObject> Options = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        Init();
        
        foreach (Button button in GetComponentsInChildren<Button>()) {
            GameObject obj = button.gameObject;
            if (button != null) {
                button.onClick.AddListener(() => Select(obj));
                Options.Add(obj);
            }
        }

        if (Default != null) Select(Default);
    }
    private void Select(GameObject selected) 
    {
        T t = GameObjectToT(selected);
        SelectedValue = t;
        Selected = true;
        SetValue(t);
        foreach (GameObject child in Options) 
        {
            Image img = child.GetComponent<Image>();
            if (img != null) img.color = selected == child ? SelectedColor : NotSelectedColor;
        }
    }

    public void AddOption(GameObject option, T value) {
        option.transform.SetParent(transform);
        _mapping.Add(option, value);
        Button button = option.GetComponent<Button>();
        button.onClick.AddListener(() => Select(option));
        Options.Add(option);
    }
    
    public void ClearOptions() {
        _mapping.Clear();
        Options.Clear();
    }

    protected virtual void SetValue(T value) { }
}
