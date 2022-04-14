using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TracingSubTask : SubTask
{
    public Texture2D ReferenceCurve;
    public DrawableMesh Canvas;
    public float CompletionThreshold = 0.95f;

    private float correctPercent = 0;
    private float errorPercent = 0;

    void Update() {
        //updateCompletion();
    }

    public override bool Complete()
    {
        return correctPercent >= CompletionThreshold;
    }

    public override string ProgressDisplay()
    {
        return string.Format("{0}|{1}", correctPercent, errorPercent);
    }

    private void updateCompletion() {
        Texture2D marker = Canvas.MarkerTex;
        if (marker == null) {
            correctPercent = 0;
            errorPercent = 0;
            return;
        }


        int errorCount = 0;
        int correctCount = 0;
        Color[] colors = marker.GetPixels(0, 0, marker.width, marker.height);
        for (var i = 0; i < marker.height; i++) {
            float u = (float)i / marker.height;
            int ri = (int) (u * ReferenceCurve.height);
            for (var j = 0; j < marker.width; j++) {
                float v = (float)j / marker.width;
                Color c = colors[i * marker.width + j];
                if (isBlank(c))
                    continue;

                int rj = (int) (v * ReferenceCurve.width);

                Color rc = ReferenceCurve.GetPixel(ri, rj);

                if (isBlank(rc)) {
                    errorCount++;
                } else {
                    correctCount++;
                }
            }
        }

        correctPercent = correctCount;
        errorPercent = errorCount;
    }

    private bool isBlank(Color c) {
        return c.a >= 0.95f;
    }
}
