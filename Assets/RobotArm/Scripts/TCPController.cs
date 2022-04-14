using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Net; 
using System.Net.Sockets; 
using System.IO;

public class TCPController : RobotController
{
    //public string Host = "127.0.0.1";
    public int Port = 8052;
    private TcpListener tcpListener;
    private UdpClient client;
    private Socket socket;
    private Queue<string> OutboundMessages = new Queue<string>();
    private Queue<string> InboundMessages = new Queue<string>();
    public int InboundMessageBufferSize = 1;
    public int OutboundMessageBufferSize = 1;
    protected override void Start()
    {
        base.Start();
        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        StartCoroutine(listener());
    }

    void OnDestroy() {
        tcpListener?.Stop();
    }

    private IEnumerator listener() {
        while (true) {
            var task = tcpListener.AcceptTcpClientAsync();
            while (!task.IsCompleted) {
                yield return null;
            }
            using (TcpClient client = task.Result) {
                sendInitialState();
                while (client.Connected) {
                    String msgs = null;
                    do {
                        msgs = readFromStream(client.GetStream());
                        if (msgs != null) {
                            foreach (string msg in msgs.Split('\n'))
                                storeMessage(msg);
                        }
                    } while (msgs != null);
                    
                    sendMessages(client.GetStream());
                    yield return null;
                }
            }
        }
    }

    private String readFromStream(NetworkStream stream) {
        if (!stream.CanRead || !stream.DataAvailable) return null;

        string res = "";
        byte[] buffer = new byte[1024];
        try {
            while (stream.DataAvailable) {
                int length = stream.Read(buffer, 0, buffer.Length);
                res += Encoding.ASCII.GetString(buffer, 0, length);
            }
        } catch (SocketException) { } 
          catch (IOException) { }
        return res;
    }

    private void storeMessage(string msg) {
        if (msg.Length == 0) return;
        InboundMessages.Enqueue(msg);
        while (InboundMessages.Count > InboundMessageBufferSize) {
            InboundMessages.Dequeue();
        }
    }

    private void sendMessages(NetworkStream stream) {
        if (!stream.CanWrite) return;
        string msg;
        try {
            while (stream.CanWrite && OutboundMessages.TryDequeue(out msg)) {
                stream.Write(Encoding.ASCII.GetBytes(msg + '\n'));
            }
        } catch (SocketException) { } 
          catch (IOException) { }
    }

    private void queueMessage(string msg) {
        OutboundMessages.Enqueue(msg);
        while (OutboundMessages.Count > OutboundMessageBufferSize) {
            OutboundMessages.Dequeue();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        sendCurrentState();
        current_target = Constrain(readCommand());
        SetJoints(current_target);
    }

    private List<float> readCommand() {
        string msg;
        List<float> command = null;
        while (InboundMessages.TryDequeue(out msg)) {
            command = JsonConvert.DeserializeObject<List<float>>(msg);
        }

        return command ?? current_target;
    }

    private void sendInitialState() {
        Dictionary<string, object> initial_state = new Dictionary<string, object>();
        initial_state.Add("q", current_target.ToArray());
        initial_state.Add("geometry", getStaticGeometry());

        Dictionary<string, object> msg = new Dictionary<string, object>();
        msg.Add("InitialState", initial_state);
        queueMessage(JsonConvert.SerializeObject(msg));
    }

    private List<Dictionary<string, Geometry>> getStaticGeometry() {
        List<Dictionary<string, Geometry>> result = new List<Dictionary<string, Geometry>>();
        foreach (Collider collider in GameObject.FindObjectsOfType<Collider>()) {

            if (!collider.gameObject.isStatic) 
                continue;

            BoxCollider box = collider as BoxCollider;
            SphereCollider sphere = collider as SphereCollider;

            Dictionary<string, Geometry> msg = new Dictionary<string, Geometry>();
            if (box != null) {
                (Vector3 translation, Quaternion rotation) = inRobotFrame(box.transform);
                Vector3 size = Vector3.Scale(box.size, box.transform.localScale);
                Vector3 robot_frame_size = new Vector3(size.z, size.x, size.y);
                msg.Add("Box", new Box(
                    translation,
                    rotation,
                    robot_frame_size
                ));
            }
            if (sphere != null) {
                (Vector3 translation, Quaternion rotation) = inRobotFrame(sphere.transform);
                msg.Add("Sphere", new Sphere(
                    translation,
                    sphere.radius
                ));
            }
            result.Add(msg);
        }

        return result;
    }

    private void sendCurrentState() {
        Dictionary<string, object> current_state = new Dictionary<string, object>();
        current_state.Add("q", current_target.ToArray());

        (Vector3 x_vec, Quaternion quat) = targetInRobotFrame();

        float[] x = new float[3];
        x[0] = x_vec.x; x[1] = x_vec.y; x[2] = x_vec.z;

        float[] q = new float[4];
        q[0] = quat.w; q[1] = quat.x; q[2] = quat.y; q[3] = quat.z;

        current_state.Add("pose_x", x);
        current_state.Add("pose_q", q);

        Dictionary<string, object> msg = new Dictionary<string, object>();
        msg.Add("CurrentState", current_state);

        queueMessage(JsonConvert.SerializeObject(msg));
    }
}


[Serializable]
class Geometry  {

}

[Serializable]
class Box : Geometry {
    public float[] x = new float[3];
    public float[] size = new float[3];
    public float[] rot = new float[4];
    public Box(BoxCollider box) : this(
        box.transform.position + box.center,
        box.transform.rotation,
        Vector3.Scale(box.size, box.transform.localScale)
    ) {}
    public Box(Vector3 position, Quaternion rotation, Vector3 size) {
        x[0] = position.x;
        x[1] = position.y;
        x[2] = position.z;
        
        this.size[0] = size.x;
        this.size[1] = size.y;
        this.size[2] = size.z;

        rot[0] = rotation.w;
        rot[1] = rotation.x;
        rot[2] = rotation.y;
        rot[3] = rotation.z;
    }
}

[Serializable]
class Sphere : Geometry {
    public float[] x = new float[3];
    public float r;
    public Sphere(SphereCollider sphere) : this(
        sphere.transform.position + sphere.center,
        sphere.radius
    ) { }
    public Sphere(Vector3 position, float radius) {
        x[0] = position.x;
        x[1] = position.y;
        x[2] = position.z;

        r = radius;
    }
}