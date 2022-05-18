using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class SerializationTest : HasSystemManager
{
    public string Folder = "./";
    public string Url = "authenticatedserver-template-vsu6yklrnq-uk.a.run.app";

    SecureDatabase<TaskRecord> taskDb;

    void Start()
    {
        taskDb = new LocalDatabase<TaskRecord>(Folder);
        //taskDb = new RemoteDatabase<TaskRecord>(Url);
        StartCoroutine(InASec());
    }

    IEnumerator InASec() {
        yield return new WaitForSeconds(1);
        SystemManager.StartScene();
        StartCoroutine(stopLater());
    }

    IEnumerator stopLater() {
        yield return new WaitForSeconds(1);
        SystemManager.EndScene();

        taskDb.Authorize("hcilab").ContinueWith(async (result) => {
            if (result) {
                await taskDb.WriteRecord(SystemManager.taskRecord, SystemManager.taskRecord.Name);
                var taskRecord = await taskDb.ReadRecord(SystemManager.taskRecord.Name);
                SystemManager.ReplayScene(taskRecord);
            }
        });
    }
}
