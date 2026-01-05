using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class OcclusionSubsystemChecker : MonoBehaviour
{
    [SerializeField]
    private AROcclusionManager occlusionManager;

    private void OnEnable()
    {
        ARSession.stateChanged += OnARSessionStateChanged;
    }

    private void OnDisable()
    {
        ARSession.stateChanged -= OnARSessionStateChanged;
    }

    private void OnARSessionStateChanged(ARSessionStateChangedEventArgs args)
    {
        if (args.state == ARSessionState.SessionTracking)
        {
            CheckOcclusionSubsystem();
        }
    }

    private void CheckOcclusionSubsystem()
    {
        if (occlusionManager == null)
        {
            Debug.LogWarning("OcclusionManager reference not set!");
            return;
        }

        if (occlusionManager.subsystem != null)
        {
            Debug.Log("Occlusion subsystem is available and running!");
        }
        else
        {
            Debug.LogWarning("Occlusion subsystem is NOT available.");
        }
    }
}
