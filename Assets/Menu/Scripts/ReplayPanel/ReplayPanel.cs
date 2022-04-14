using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class ReplayPanel : HasSystemManager
{
    public GameObject SaveButton;
    public GameObject RefreshButton;
    public GameObject UploadButton;
    public ListSelect<TaskRecord> FileList;
    public GameObject FileItem;

    void Start() {
    #if UNITY_EDITOR || !UNITY_WEBGL
        RefreshButton.SetActive(true);
        RefreshButton.SetActive(false);
    #else
        RefreshButton.SetActive(false);
        RefreshButton.SetActive(true);
    #endif

        SaveButton.GetComponent<Button>().onClick.AddListener(() => {
            if (SystemManager.taskRecord != null) {
                SystemManager.taskRecord.WriteTo(newEP("logs/" + new System.DateTime().ToString() + ".log"));
            }
        });

        RefreshButton.GetComponent<Button>().onClick.AddListener(() => {
            FileList.ClearOptions();
            foreach (RectTransform trans in FileList.transform) {
                Destroy(trans.gameObject);
            }

        #if UNITY_EDITOR || !UNITY_WEBGL
            foreach (string fp in Directory.GetFiles("logs/")) {
                Endpoint ep = new NativeFileEndpoint(fp);
                if (ep.Open(Endpoint.Mode.Read)) {
                    Queue<string> file = new Queue<string>(ep.ReadAll());
                    FileList.AddOption(
                        Instantiate(FileItem),
                        new TaskRecord(file)
                    );
                } else {
                    Debug.LogError("Failed to open task record file: " + fp);
                }
            }
        #endif
        });
    }

    void Update() {
        SaveButton.GetComponent<Button>().enabled = SystemManager.taskRecord != null;
    }

    private Endpoint newEP(string path) {
    #if UNITY_EDITOR || !UNITY_WEBGL
        return new NativeFileEndpoint(path);
    #else
        return new WebFileEndpoint(path);
    #endif
    }
}
