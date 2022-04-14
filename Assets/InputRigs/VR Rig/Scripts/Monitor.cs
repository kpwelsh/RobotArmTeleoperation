using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using System.Linq;

public class Monitor : MonoBehaviour
{
    public List<Camera> Cameras = new List<Camera>();
    public RenderTexture Rt;
    private RectanglePacking.RectangleArrangement displayArrangement;
    // Start is called before the first frame update
    void Start()
    {
        var rectangles = Cameras.Select(cam => new Vector2(cam.targetTexture.width, cam.targetTexture.height)).ToList();
        displayArrangement = RectanglePacking.Arrange(rectangles, 16f / 9, hJustification: RectanglePacking.Justification.Center);

        foreach (var camPos in Cameras.Zip(displayArrangement.children, (a, b) => (a, b))) {
            (Camera cam, (Vector2 xy, Vector2 wh)) = camPos;
            cam.targetTexture = Rt;
            cam.rect = new Rect(
                xy.x / displayArrangement.dimensions.x, xy.y / displayArrangement.dimensions.y,
                wh.x / displayArrangement.dimensions.x, wh.y / displayArrangement.dimensions.y
            );
        }
    }
}
