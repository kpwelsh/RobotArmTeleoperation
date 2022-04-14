using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using System.Linq;

public class PicDisplayer : MonoBehaviour
{
    public int Rows = -1;
    public int Cols = -1;
    public RenderTexture RenderBuffer;
    private Texture2D textureBuffer;
    private List<Texture2D> pics = new List<Texture2D>();
    void Start()
    {
        clear();
    }

    void clear() {
        RenderTexture last = RenderTexture.active;
        RenderTexture.active = RenderBuffer;
        GL.Clear(true, true, Color.gray);
        RenderTexture.active = last;

        textureBuffer = new Texture2D(RenderBuffer.width, RenderBuffer.height, TextureFormat.RGB24, false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddPic(Texture2D tex) {
        clear();
        pics.Add(tex);
        RectanglePacking.RectangleArrangement arrangment = RectanglePacking.Arrange(pics.Select(pic => new Vector2(pic.width, pic.height)).ToList(), 
            (float)RenderBuffer.width / RenderBuffer.height, 
            rows: Rows, cols: Cols,
            hJustification: RectanglePacking.Justification.Center, 
            vJustification: RectanglePacking.Justification.Center
        );
        Vector2 scale = new Vector2(
            (float) RenderBuffer.width / arrangment.dimensions.x,
            (float) RenderBuffer.height / arrangment.dimensions.y
        );

        RenderTexture last = RenderTexture.active;
        RenderTexture.active = RenderBuffer;
        foreach (var picPos in pics.Zip(arrangment.children, (a, b) => (a, b))) {
            (Texture2D pic, (Vector2 xy, Vector2 wh)) = picPos;
            xy.Scale(scale);
            wh.Scale(scale);

            Texture2D scaledPic = Utils.TextureScaler.scaled(pic, (int)wh.x, (int)wh.y);
            scaledPic.Apply();

            Graphics.CopyTexture(
                scaledPic, 0, 0, 0, 0, scaledPic.width, scaledPic.height,
                textureBuffer, 0, 0, (int)xy.x, textureBuffer.height - (int)xy.y - scaledPic.height
            );
        }
        Graphics.Blit(textureBuffer, RenderBuffer);
        RenderTexture.active = last;
    }
}
