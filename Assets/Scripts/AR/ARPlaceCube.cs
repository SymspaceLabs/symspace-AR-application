using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;
using System.Collections;

public class ARPlaceCube : MonoBehaviour
{
    [SerializeField] private ARRaycastManager raycastManager;
    bool isPlacing = false;

    // Update is called once per frame
    void Update()
    {
        if (raycastManager == null)
            return;

        if(((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButton(0)) && !isPlacing)
        {
            isPlacing = true;

            if(Input.touchCount > 0)
            {
                PlaceObject(Input.GetTouch(0).position);
            }
            else
            {
                PlaceObject(Input.mousePosition);
                Debug.Log("place mouse ");
            }
        }
    }

    void PlaceObject(Vector2 touchPosition)
    {
        var rayHits = new List<ARRaycastHit>();

        raycastManager.Raycast(touchPosition, rayHits, TrackableType.PlaneWithinPolygon);

        if(rayHits.Count > 0 )
        {
            Debug.Log("raycast Hit count > 0");
            Vector3 hitPosPosition = rayHits[0].pose.position;
            Quaternion hitPosRotation = rayHits[0].pose.rotation;
            Instantiate(raycastManager.raycastPrefab, hitPosPosition, hitPosRotation);
        }

        StartCoroutine(SetIsPlacingToFalseWithDelay());
    }

    IEnumerator SetIsPlacingToFalseWithDelay()
    {
        yield return new WaitForSeconds(0.25f);
        isPlacing = false;
    }
}
