using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class JsonBlobAPI
{
    public static string blobID;
    public static string json;
    public static long responseCode;

    public const string RegisterID = "942499936566788096";
    const string API = "https://jsonblob.com/api/jsonBlob";

    public static IEnumerator CreateJsonBlobCo(string json)
    {
        using (var request = new UnityWebRequest(API, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.uploadHandler.contentType = "application/json";
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("accept", "application/json");
            yield return request.SendWebRequest();
            responseCode = request.responseCode;
            if (request.responseCode != 201)
                Debug.Log(request.result + ", " + request.downloadHandler.text);
            var blobUrl = request.GetResponseHeader("Location");
            blobID = blobUrl.Substring(blobUrl.LastIndexOf('/') + 1);
        }
    }

    public static IEnumerator GetJsonBlobCo(string blobID)
    {
        using(var request = UnityWebRequest.Get(API + "/" + blobID))
        {
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            responseCode = request.responseCode;
            if (request.responseCode != 200)
                Debug.Log(request.result + ", " + request.downloadHandler.text);
            json = request.downloadHandler.text;
        }
    }

    public static IEnumerator SetJsonBlobCo(string blobID, string json)
    {
        using(var request = UnityWebRequest.Put(API + "/" + blobID, json))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            responseCode = request.responseCode;
            if (request.responseCode != 200)
                Debug.Log(request.result + ", " + request.downloadHandler.text);
        }
    }

    public static IEnumerator DeleteJsonBlobCo(string blobID)
    {
        using(var request = UnityWebRequest.Delete(API + "/" + blobID))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            responseCode = request.responseCode;
            if (request.responseCode != 200)
                Debug.Log(request.result + ", " + request.downloadHandler.text);
        }
    }
}
