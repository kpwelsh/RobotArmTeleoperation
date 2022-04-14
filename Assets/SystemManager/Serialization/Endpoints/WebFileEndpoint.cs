using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using Unity;

public class WebFileEndpoint : Endpoint {
#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void WriteFile(string name, string text);
    
    [DllImport("__Internal")]
    private static extern void ReadFile();

    [DllImport("__Internal")]
    private static extern bool IsWaitingForText();

    [DllImport("__Internal")]
    private static extern string GetText();
#endif

    protected string outBuffer = "";
    protected Queue<string> readBuffer = new Queue<string>();

    public WebFileEndpoint(string name) : base(name) { }

    protected override bool open(Mode mode) {
        switch (mode) {
            case Mode.Write : {
                outBuffer = "";
                break;
            }
            case Mode.Read : {
                ReadFile();
                // This aint it. 
                while (IsWaitingForText()) {
                    continue;
                }
                string text = GetText();
                readBuffer.Clear();
                foreach (string line in text.Split('\n')) {
                    readBuffer.Enqueue(line);
                }
                break;
            }
        }
        return true;
    }

    public override void Write(string value) {
        outBuffer += value;
    }

    public override string ReadLine()
    {
        if (readBuffer.Count == 0) return null;

        return readBuffer.Dequeue();
    }

    protected override void close() {
        WriteFile(Uri, outBuffer);
        outBuffer = "";
    }
}
