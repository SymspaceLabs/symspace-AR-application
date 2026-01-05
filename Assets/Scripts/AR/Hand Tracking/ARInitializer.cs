using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARInitializer : MonoBehaviour
{
    [SerializeField] private ARSession arSession;
    void Start()
    {
        if (arSession == null) arSession = FindObjectOfType<ARSession>();
        arSession.enabled = true;
    }
}