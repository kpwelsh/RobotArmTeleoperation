using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializationTest : HasSystemManager
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(InASec());
    }

    IEnumerator InASec() {
        yield return new WaitForSeconds(1);
        SystemManager.StartScene();
        StartCoroutine(stopLater());
    }

    IEnumerator stopLater() {
        yield return new WaitForSeconds(5);
        SystemManager.EndScene();
        Endpoint ep = new NativeFileEndpoint("here.txt");
        ep.Open(Endpoint.Mode.Write);
        SystemManager.taskRecord.WriteTo(ep);
        ep.Close();
        if (ep.Open(Endpoint.Mode.Read)) {
            SystemManager.ReplayScene(new TaskRecord(new Queue<string>(ep.ReadAll())));
        } else {
            Debug.Log("Couildn't replay.");
        }
    }
}
