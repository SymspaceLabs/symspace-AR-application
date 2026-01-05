using UnityEngine;
using UnityEngine.EventSystems;

public class ModelViewer : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("Rotation")]
    public Transform targetModel;
    public float rotationSpeed = 0.2f;

    [Header("Auto Rotation")]
    public float autoRotationSpeed = 10f;           // Degrees per second
    public float interactionPauseDuration = 2f;     // Time in seconds to pause auto rotation after interaction

    [Header("Zoom")]
    public Camera modelCamera;
    public float zoomSpeed = 2f;
    public float minZoom = 5f;
    public float maxZoom = 20f;

    private Vector2 lastPointerPosition;

    private bool isUserInteracting = false;
    private float interactionTimer = 0f;

    void Update()
    {
        //HandleMouseScrollZoom();
        //HandlePinchZoom();

        // Resume auto-rotation if enough time has passed
        if (isUserInteracting)
        {
            interactionTimer -= Time.deltaTime;
            if (interactionTimer <= 0f)
            {
                isUserInteracting = false;
            }
        }

        if (!isUserInteracting)
        {
            AutoRotateModel();
        }
        modelCamera.transform.LookAt(targetModel.transform);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        lastPointerPosition = eventData.position;
        PauseAutoRotation();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 currentPointerPosition = eventData.position;
        Vector2 delta = currentPointerPosition - lastPointerPosition;

        if (targetModel != null)
        {
            float rotationY = -delta.x * rotationSpeed;
            targetModel.Rotate(Vector3.up, rotationY, Space.World);
        }

        lastPointerPosition = currentPointerPosition;
        PauseAutoRotation();
    }

    private void HandleMouseScrollZoom()
    {
        if (modelCamera == null) return;

        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scrollDelta) > 0.01f)
        {
            float newDistance = modelCamera.transform.localPosition.z + scrollDelta * zoomSpeed * -1f;
            newDistance = Mathf.Clamp(newDistance, -maxZoom, -minZoom);
            modelCamera.transform.localPosition = new Vector3(
                modelCamera.transform.localPosition.x,
                modelCamera.transform.localPosition.y,
                newDistance
            );
            PauseAutoRotation();
        }
    }

    private void HandlePinchZoom()
    {
        if (modelCamera == null) return;

        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            Vector2 prevTouch0 = touch0.position - touch0.deltaPosition;
            Vector2 prevTouch1 = touch1.position - touch1.deltaPosition;

            float prevDistance = Vector2.Distance(prevTouch0, prevTouch1);
            float currentDistance = Vector2.Distance(touch0.position, touch1.position);

            float delta = currentDistance - prevDistance;

            float newDistance = modelCamera.transform.localPosition.z - delta * Time.deltaTime * zoomSpeed;
            newDistance = Mathf.Clamp(newDistance, -maxZoom, -minZoom);
            modelCamera.transform.localPosition = new Vector3(
                modelCamera.transform.localPosition.x,
                modelCamera.transform.localPosition.y,
                newDistance
            );

            PauseAutoRotation();
        }
    }

    private void AutoRotateModel()
    {
        if (targetModel != null)
        {
            targetModel.Rotate(Vector3.up, autoRotationSpeed * Time.deltaTime, Space.World);
        }
    }

    private void PauseAutoRotation()
    {
        isUserInteracting = true;
        interactionTimer = interactionPauseDuration;
    }
}
