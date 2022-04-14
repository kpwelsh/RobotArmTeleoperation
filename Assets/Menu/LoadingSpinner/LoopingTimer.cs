using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class LoopingTimer : MonoBehaviour
{
    
    private float time = 0;
    private float? endTime = null;
    private UnityEvent onExpire = null;
    private Image image;
    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
    }

    private void updateFill() {
        if (image.type == Image.Type.Filled) {
            float fillAmount = 0;
            if (endTime.HasValue) fillAmount = Mathf.Clamp(time / endTime.Value, 0, 1);
            image.fillAmount = fillAmount;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        updateFill();
        if (!endTime.HasValue) return;

        time += Time.fixedDeltaTime;
        if (time >= endTime.Value) {
            if (onExpire != null) onExpire.Invoke();
            Cancel();
        }
    }

    public void StartTimer(float duration, UnityEvent callback) {
        onExpire = callback;
        endTime = duration;
        time = 0;
    }

    public void Cancel() {
        endTime = null;
        onExpire = null;
        time = 0;
    }
}
