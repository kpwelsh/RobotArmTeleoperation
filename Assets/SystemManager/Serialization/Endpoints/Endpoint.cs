using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Endpoint {
    public enum Mode {
        Read,
        Write
    }

    public string Uri;
    public bool IsOpen = false;
    public Endpoint(string uri) {
        Uri = uri;
    }
    public bool Open(Mode mode) {
        IsOpen = open(mode);
        return IsOpen;
    }
    protected abstract bool open(Mode mode);
    public abstract void Write(string value);
    public virtual void WriteLine(string value) {
        Write(value + "\n");
    }
    public virtual List<string> ReadAll() {
        List<string> res = new List<string>();
        string line = null;

        while ((line = ReadLine()) != null) {
            res.Add(line);
        }
        
        return res;
    }
    public abstract string ReadLine();

    public void Close() {
        if (!IsOpen) return;
        
        close();
        IsOpen = false;
    }

    protected abstract void close();
}
