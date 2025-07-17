using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public static class AuthAPI
{
    public static string baseAPI = "https://backend.symspacelabs.com";
    public static string api = baseAPI + "/auth/";

    //Data send by Json
    public static IEnumerator PostRequest(string url, string jsonData, Action<string> onSuccess, Action<string> onError)
    {
        UnityWebRequest request = new UnityWebRequest(api + url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(request.downloadHandler.text);
        else
            onError?.Invoke(request.error);
    }
    

    //Data send by form fields
    public static IEnumerator PostRequest(string url, WWWForm formData, Action<string> onSuccess, Action<string> onError)
    {
        UnityWebRequest request = UnityWebRequest.Post(api + url, formData);
        request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(request.downloadHandler.text);
        else
            onError?.Invoke(request.error);
    }
}
