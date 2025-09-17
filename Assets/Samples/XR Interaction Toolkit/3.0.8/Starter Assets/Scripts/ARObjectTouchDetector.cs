using UnityEngine;
using UnityEngine.EventSystems;

public class ARObjectTouchDetector : MonoBehaviour
{
    public delegate void OnTouchedDelegate(GameObject touchedObject);
    public static event OnTouchedDelegate OnObjectTouched;

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Skip if over UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return;

            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform == transform)
                    {
                        // Call event when this object is touched
                        OnObjectTouched?.Invoke(gameObject);
                    }
                }
            }
        }
    }
}
