using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine;

public class DepthChecker : MonoBehaviour
{
    [SerializeField] private ARCameraManager cameraManager;
    void Start()
    {
        var depthSupport = cameraManager.descriptor?.supportsCameraGrain
            ?? false;
        Debug.Log(depthSupport ? "LiDAR supported" : "Fallback to raycast only");
    }
}