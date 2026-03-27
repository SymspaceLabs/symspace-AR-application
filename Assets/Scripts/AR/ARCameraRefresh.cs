using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARCameraRefresh : MonoBehaviour
{
    void OnEnable()
    {
        var bg = GetComponent<ARCameraBackground>();
        if (bg != null)
        {
            bg.enabled = false;
            bg.enabled = true;
        }
    }
}