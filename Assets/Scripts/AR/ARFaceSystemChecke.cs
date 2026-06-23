using UnityEngine;
using System.Collections;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARFaceSystemChecke : MonoBehaviour
{
    public ARFaceManager faceManager;
    public ARSession arSession;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    /*IEnumerator Start()
    {
        if(faceManager != null && faceManager.subsystem != null && faceManager.subsystem.running)
        {
            Debug.Log("Face Tracking subsystem is running");
        }
        else
        {
            Debug.Log("Face tracking subsystem not available");
        }

        if(ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.CheckingAvailability)
        {
            yield return ARSession.CheckingAvailability();
        }

        if(ARSession.state == ARSessionState.UnSupported)
        {
            Debug.Log("AR not Supported on this device");
            yield break;
        }

        arSession.enabled = true;
        if(faceManager.descriptor.supportsFacePos)
        {
            Debug.Log("Face Tracking Supported");
            faceManager.enabled = true;
        }
        else
        {
            Debug.Log("Face tracking not supported 3");
        }

        if(faceManager.subsystem != null && faceManager.subsystem.subsystemDescriptor.supportsFacePos)
        {
            Debug.Log("Face Tracking initialized successfully");
        }
        else
        {
            Debug.Log("Face Tracking subsystem not available");
        }
    }
*/
}
