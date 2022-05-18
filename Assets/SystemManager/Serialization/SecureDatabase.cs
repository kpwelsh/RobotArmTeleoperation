using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

using Thread = Cysharp.Threading.Tasks;

public abstract class SecureDatabase<T> where T : class {
    protected string URI;
    public SecureDatabase(string URI) {
        this.URI = URI;
    }
    public abstract Thread.UniTask<bool> Authorize(string credentials);
    public abstract bool IsAuthorized();
    public abstract Thread.UniTask<List<string>> ListRecords();
    public abstract Thread.UniTask WriteRecord(T record, string identifier);

    public abstract Thread.UniTask<T> ReadRecord(string identifier);
}

public class RemoteDatabase<T> : SecureDatabase<T> where T : class {

    private string AccessToken = null;
    

    public RemoteDatabase(string uri) : base(uri) { }

    private Uri MakeUri(string path, string query = null) {
        UriBuilder uri = new UriBuilder();
        uri.Scheme = "https";
        uri.Host = URI;
        uri.Path = path;
        if (query != null) uri.Query = query;
        return uri.Uri;
    }

    private void SetHeaders(UnityWebRequest request) {
        request.SetRequestHeader("Accept", "application/json");
        if (request.method != "GET") {
            request.SetRequestHeader("Content-Type", "application/json");
        }
        if (AccessToken != null) {
            request.SetRequestHeader("Authorization", String.Format("Bearer {0}", AccessToken));
        }
    }

    public async override Thread.UniTask<bool> Authorize(string credentials) {
        var request = new UnityWebRequest();
        request.url = MakeUri("login").ToString();
        request.method = "POST";
        SetHeaders(request);
        var content = JsonConvert.SerializeObject(
            new Dictionary<string, string>{
                { "username", credentials }
            }
        );
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(content));
        request.downloadHandler = new DownloadHandlerBuffer();

        Dictionary<string, string> body = null;
        try {
            var response = await request.SendWebRequest();
            if (response.result == UnityWebRequest.Result.Success) {
                body = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.downloadHandler.text);
            }
        } catch (JsonReaderException) {
            return false;
        } catch (Exception e) {
            return false;
        } finally {
            request.uploadHandler.Dispose();
        }

        AccessToken = body?.GetValueOrDefault("access_token", null);
        return IsAuthorized();
    }

    public override bool IsAuthorized() {
        return AccessToken != null;
    }

    public async override Thread.UniTask<List<string>> ListRecords() {
        UnityWebRequest request = new UnityWebRequest();
        request.url = MakeUri("listRecords").ToString();
        request.method = "GET";
        SetHeaders(request);
        request.downloadHandler = new DownloadHandlerBuffer();

        Dictionary<string, List<string>> body = null;
        try {
            var response = await request.SendWebRequest();
            if (response.result == UnityWebRequest.Result.Success) {
                body = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(response.downloadHandler.text);
            }
        } catch (JsonSerializationException) {
            return null;
        } catch (Exception e) {
            Debug.LogError(e.ToString());
            return null;
        }
        return body?.GetValueOrDefault("names", null);
    }

    public async override Thread.UniTask WriteRecord(T record, string identifier) {
        var request = new UnityWebRequest();
        request.url = MakeUri("record", String.Format("name={0}", identifier)).ToString();
        request.method = "POST";
        SetHeaders(request);
        var content = JsonConvert.SerializeObject(
            new Dictionary<string, string>{
                { "frameData", JsonConvert.SerializeObject(record)}
            }
        );
        try {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(content));
            await request.SendWebRequest();
        } finally {
            request.uploadHandler.Dispose();
        }
        return;
    }

    public async override Thread.UniTask<T> ReadRecord(string identifier) {
        Uri uri = MakeUri("record", String.Format("name={0}", identifier));
        var request = new UnityWebRequest();
        request.url = uri.ToString();
        request.method = "GET";
        SetHeaders(request);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        T record = null;
        try {
            var response = await request.SendWebRequest();
            if (response.result == UnityWebRequest.Result.Success) {
                record = JsonConvert.DeserializeObject<T>(response.downloadHandler.text);
            }
        } catch (JsonReaderException) { }
        catch (Exception e) {
            Debug.LogError(e.ToString());
        } finally {
            request.uploadHandler.Dispose();
        }
        return record;
    }
}