using System.IO;

public class BufferedStreamWriter
{
    protected Endpoint endpoint;
    protected string buffer = "";
    public BufferedStreamWriter(Endpoint endpoint = null) {
        this.endpoint = endpoint;
    }

    public void AttachEndpoint(Endpoint endpoint) {
        this.endpoint = endpoint;
        Flush();
    }

    public bool Flush() {
        if (endpoint == null || !endpoint.IsOpen) {
            return false;
        }

        if (buffer.Length == 0) {
            return true;
        }

        try {
            endpoint.Write(buffer);
            buffer = "";
            return true;
        } catch (IOException) {
            return false;
        }
    }

    public void Write(string value, bool flush = true) {
        buffer += value;
        if (flush) {
            Flush();
        }
    }

    public void WriteLine(string value, bool flush = true) {
        Write(value + "\n", flush);
    }

    ~BufferedStreamWriter() {
        Close();
    }

    public void Close() {
        endpoint.Close();
    }
}
