using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using Unity.MARS;
using Unity.XRTools.ModuleLoader; // Important for force-reloading modules
using UnityEngine.XR.Management;

public static class ARSceneHelper
{
    public static void CleanLoad(string sceneName)
    {
        // 1. Handle standard ARFoundation shutdown
        ARSession arSession = Object.FindFirstObjectByType<ARSession>();
        if (arSession != null)
        {
            arSession.Reset();
            arSession.enabled = false;
        }

        if(sceneName.Equals(SceneNames.ARBodyTrackingMars))
            LoaderUtility.Deinitialize();
        else
        {

        }
        
        SceneManager.LoadScene(sceneName);
    }
}