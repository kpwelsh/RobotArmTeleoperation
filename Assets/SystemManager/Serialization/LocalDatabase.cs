using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.InteropServices;
using System;
using UnityEngine;
using AOT;

using Thread = Cysharp.Threading.Tasks;


public partial class LocalDatabase<T> : SecureDatabase<T> where T : class {
    public async override Thread.UniTask<bool> Authorize(string credentials) => true;
    public override bool IsAuthorized() => true;
    public LocalDatabase(string directory) : base(directory) { }
}


#if UNITY_EDITOR // Define the native local database implementation
public partial class LocalDatabase<T> {
    public async override Thread.UniTask<List<string>> ListRecords() {
        return new List<string>(Directory.GetFiles(URI));
    }
    public async override Thread.UniTask WriteRecord(T record, string identifier) {
        await File.WriteAllTextAsync(
            System.IO.Path.Combine(URI, identifier), 
            JsonConvert.SerializeObject(record)
        );
        return; 
    }
    public async override Thread.UniTask<T> ReadRecord(string identifier) {
        string recordData = await File.ReadAllTextAsync(System.IO.Path.Combine(URI, identifier));
        return JsonConvert.DeserializeObject<T>(recordData);
    }
}
#elif UNITY_WEBGL // Define the browser local database implementation

public static class Pointer {
    private static readonly Dictionary<int, object> Pointers = new Dictionary<int, object>();
    private static int CurrentPtr = 0;

    public static int RegisterObject(object obj) {
        int ptr = CurrentPtr++;
        Pointers[ptr] = obj;
        return ptr;
    }

    public static T Deref<T>(int ptr) where T : class {
        if (!Pointers.ContainsKey(ptr)) {
            Debug.LogErrorFormat("Invalid pointer: {0}", ptr);
            Debug.LogError(Pointers.ToString());
        }
        return Pointers.GetValueOrDefault(ptr, null) as T;
    }

    public static void Deregister(int ptr) {
        Pointers.Remove(ptr);
    }
}
public class HasPointer {
    public int Ptr {
        get;
        private set;
    }
    public HasPointer() {
        Ptr = Pointer.RegisterObject(this);
    }

    ~HasPointer() {
        Pointer.Deregister(Ptr);
    }
}

static class WebDBJSAPI {
    // Defined in WebDBJSPI.jslib

    // Be careful about strings and ints when going from C# to JS and back.
    // When transitioning from one to another, strings are converted to pointers and stored as ints.
    [DllImport("__Internal")]
    public static extern void ReadRecord(int taskPtr, Action<int, string> callback, string name); // In C# land, however, strings are strings. so call them strings.
	delegate void ReadCallback (int taskPtr, int recordData);  // But when defining callback signatures, call it an int (since the JSBridge will see an int)
    [MonoPInvokeCallback(typeof(ReadCallback))] // This supports auto string marshalling 
    public static void ReadRecordCallback(int taskPtr, string recordData) { // So you can call it a string again here. 
        Thread.UniTaskCompletionSource<object> task = Pointer.Deref<Thread.UniTaskCompletionSource<object>>(taskPtr);
        object record = null;
        try {
            record = JsonConvert.DeserializeObject(recordData);
            Debug.Log(record);
        } catch (JsonReaderException) { }
        catch (Exception e) {
            Debug.LogError(e.ToString());
        }
        
        task?.TrySetResult(record);
    }

    [DllImport("__Internal")]
    public static extern void WriteRecord(int taskPtr, Action<int> callback, string name, string record);
	delegate void WriteCallback (int taskPtr);
    [MonoPInvokeCallback(typeof(WriteCallback))]
    public static void WriteRecordCallback(int taskPtr) {
        Thread.UniTaskCompletionSource<bool> task = Pointer.Deref<Thread.UniTaskCompletionSource<bool>>(taskPtr);
        task?.TrySetResult(true);
    }

    [DllImport("__Internal")]
    public static extern void ListRecords(int taskPtr, Action<int, string[]> callback);
	delegate void ListCallback (int taskPtr, string[] records);
    [MonoPInvokeCallback(typeof(ListCallback))]
    public static void ListRecordsCallback(int taskPtr, string[] records) {
        Thread.UniTaskCompletionSource<List<string>> task = Pointer.Deref<Thread.UniTaskCompletionSource<List<string>>>(taskPtr);
        task?.TrySetResult(new List<string>(records));
    }
}

public partial class LocalDatabase<T> where T : class {
    public async override Thread.UniTask<T> ReadRecord(string identifier) {
        Thread.UniTaskCompletionSource<object> tcs = new Thread.UniTaskCompletionSource<object>();
        Thread.UniTask<object> task = tcs.Task;

        int taskPtr = Pointer.RegisterObject(tcs);

        // Call the JS API. 
        // The JS API then invokes the callback at a later date to resolve the Task.
        WebDBJSAPI.ReadRecord(taskPtr, WebDBJSAPI.ReadRecordCallback, identifier);
        return (await task) as T;
    }

    public override Thread.UniTask WriteRecord(T record, string identifier) {
        // Task<bool> is used because TaskCompletionSource does not have a 
        // typeless generic base class.
        // The bool doesn't matter.
        Thread.UniTaskCompletionSource<bool> tcs = new Thread.UniTaskCompletionSource<bool>();
        Thread.UniTask<bool> task = tcs.Task;

        int taskPtr = Pointer.RegisterObject(tcs);

        // Call the JS API. 
        // The JS API then invokes the callback at a later date to resolve the Task.
        WebDBJSAPI.WriteRecord(taskPtr, WebDBJSAPI.WriteRecordCallback, identifier, JsonConvert.SerializeObject(record));

        return task;
    }

    public override Thread.UniTask<List<string>> ListRecords() {
        Thread.UniTaskCompletionSource<List<string>> tcs = new Thread.UniTaskCompletionSource<List<string>>();
        Thread.UniTask<List<string>> task = tcs.Task;

        int taskPtr = Pointer.RegisterObject(tcs);

        // Call the JS API. 
        // The JS API then invokes the callback at a later date to resolve the Task.
        WebDBJSAPI.ListRecords(taskPtr, WebDBJSAPI.ListRecordsCallback);

        return task;
    }
}
#endif
