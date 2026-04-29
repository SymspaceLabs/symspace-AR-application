using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class ARObjectSwitcher : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public GameObject[] prefabs;

    private GameObject spawnedObject;
    private Camera arCamera;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        arCamera = Camera.main;
    }

    //void Update()
    //{
    //    // 🔁 Always face camera (only Y axis for natural look)
    //    if (spawnedObject != null)
    //    {
    //        Vector3 direction = arCamera.transform.position - spawnedObject.transform.position;
    //        direction.y = 0; // keep upright

    //        if (direction != Vector3.zero)
    //        {
    //            Quaternion lookRotation = Quaternion.LookRotation(direction);
    //            spawnedObject.transform.rotation = lookRotation;
    //        }
    //    }
    //}

    public void SpawnSelectedObject(int index)
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            foreach (var hit in hits)
            {
                ARPlane plane = hit.trackable as ARPlane;

                // ✅ Only horizontal upward planes
                if (plane.alignment == PlaneAlignment.HorizontalUp)
                {
                    Pose hitPose = hit.pose;

                    // Remove old object
                    if (spawnedObject != null)
                        Destroy(spawnedObject);

                    // Spawn new one
                    spawnedObject = Instantiate(prefabs[index], hitPose.position, hitPose.rotation);
                    return;
                }
            }
        }

        Debug.Log("No horizontal plane detected at center!");
    }
}