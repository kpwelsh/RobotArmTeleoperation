using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class NativeFileEndpoint : Endpoint {
    protected StreamWriter writer;
    protected StreamReader reader;
    public NativeFileEndpoint(string path) : base(path) { }
    protected override bool open(Mode mode) {
        try {
            switch (mode) {
                case Mode.Write: {
                    writer = new StreamWriter(Uri);
                    reader = null;
                    break;
                }
                case Mode.Read: {
                    reader = new StreamReader(Uri);
                    break;
                }
            }
            return true;
        } catch (IOException e) {
            Debug.LogError(e.ToString());
            return false;
        }
    }

    public override void Write(string value) {
        try {
            writer.Write(value);
        } catch (IOException) { }
    }

    public override string ReadLine() {
        return reader?.ReadLine();
    }

    protected override void close() {
        writer?.Close();
        writer = null;
        reader?.Close();
        reader = null;
    }
}
