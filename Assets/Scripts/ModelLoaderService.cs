using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityGLTF;

public static class ModelLoaderService
{
    private static string ModelsDir => Path.Combine(Application.persistentDataPath, "models");
    
    public static IEnumerator DownloadAndLoad(
        string url,
        Action<GameObject> onSuccess,
        Action<float> onProgress = null,
        Action onError = null)
    {
        Directory.CreateDirectory(ModelsDir);
        string localPath = Path.Combine(ModelsDir, Path.GetFileName(url));
        
        // Check cache first
        if (!File.Exists(localPath))
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.downloadHandler = new DownloadHandlerFile(localPath);
                www.SendWebRequest();
                
                while (!www.isDone)
                {
                    onProgress?.Invoke(www.downloadProgress);
                    yield return null;
                }
                
                if (www.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(/*www.error*/);
                    yield break;
                }
                onProgress?.Invoke(1f);
            }
        }
        else
        {
            onProgress?.Invoke(1f);
        }
        
        // Load GLB
        using (FileStream stream = new FileStream(localPath, FileMode.Open, FileAccess.Read))
        {
            var importer = new GLTFSceneImporter(stream, new ImportOptions());
            yield return importer.LoadSceneAsync();
            
            GameObject model = importer.LastLoadedScene;
            if (model == null)
            {
                onError?.Invoke(/*"Failed to parse GLB model"*/);
                yield break;
            }
            
            onSuccess?.Invoke(model);
        }
    }


    public static bool IsCached(string productId)
    {
        return File.Exists(Path.Combine(ModelsDir, productId + ".glb"));
    }
    
    public static void ClearCache()
    {
        if (Directory.Exists(ModelsDir))
            Directory.Delete(ModelsDir, true);
    }
}