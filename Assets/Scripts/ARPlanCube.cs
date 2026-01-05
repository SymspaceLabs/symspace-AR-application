using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlanCube : MonoBehaviour
{
    public GameObject objectToPlace;
    public ARRaycastManager raycastManager;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
#if UNITY_EDITOR
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    // -------------------- EDITOR (Mouse) --------------------
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //// Delete object if clicked
            //if (Physics.Raycast(ray, out RaycastHit hitObj))
            //{
            //    if (hitObj.collider.tag == "item")
            //    {
            //        Destroy(hitObj.transform.gameObject);
            //        return;
            //    }

            //}

            TryPlace(Input.mousePosition);
            // PLACE ONLY ON DETECTED AR PLANE
        }
    }

    // -------------------- MOBILE (Touch) --------------------
    private void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            //Ray ray = Camera.main.ScreenPointToRay(touch.position);

            //// Delete object if tapped
            //if (Physics.Raycast(ray, out RaycastHit hitObj))
            //{
            //    Destroy(hitObj.transform.gameObject);
            //    return;
            //}

            // PLACE ONLY ON DETECTED AR PLANE
            TryPlace(touch.position);
        }
    }

    // -------------------- PLACE OBJECT ONLY ON PLANE --------------------
    private void TryPlace(Vector2 screenPos)
    {
        // Raycast only hits detected AR planes
        if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            objectToPlace.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            //Instantiate(objectToPlace, hitPose.position, hitPose.rotation);
        }
        else
        {
            Debug.Log("No AR plane detected at this position.");
        }
    }
}