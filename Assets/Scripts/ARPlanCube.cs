using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlanCube : MonoBehaviour
{
    public Camera cam;                 // Assign your AR camera here
    public GameObject objectToPlace;   // The object you want to reposition
    public float distance = 0.5f;      // Distance in front of the camera
    void Update()
    {
        // Mouse click (Editor)
        if (Input.GetMouseButtonDown(0))
            PlaceObject();
        // Touch input (Mobile / AR)
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            PlaceObject();
    }
    void PlaceObject()
    {
        if (cam == null || objectToPlace == null) return;
        // Position directly in front of the camera
        objectToPlace.transform.position =
            cam.transform.position + cam.transform.forward * distance;
        // Optional: match camera rotation
        objectToPlace.transform.rotation = Quaternion.LookRotation(-cam.transform.forward);
    }
}