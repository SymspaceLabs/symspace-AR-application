using UnityEngine.XR.ARFoundation;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ForceFrontCamera : MonoBehaviour
{
    public ARCameraManager cameraManager;

    void Start()
    {
        if (cameraManager != null && cameraManager.subsystem != null)
        {
            if(SceneManager.GetActiveScene().buildIndex == 3)
            {
                cameraManager.requestedFacingDirection = CameraFacingDirection.World; // Back camera
                Debug.Log("Switched to back camera (World-facing)");
            }
            else
            {
                cameraManager.requestedFacingDirection = CameraFacingDirection.User; // Front camera
                Debug.Log("Switched to front camera (User-facing)");
            }
        }
    }
}
