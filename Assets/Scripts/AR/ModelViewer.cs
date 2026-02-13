using UnityEngine;
using UnityEngine.EventSystems;

public class ModelViewer : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("Camera Orbit")]
    public Camera modelCamera;
    public float rotationSpeed = 0.3f;
    public float autoRotateSpeed = 20f;
    public float minPitch = -30f;
    public float maxPitch = 70f;

    float yaw;
    float pitch = 15f;
    float distance;

    Vector3 focusPoint;
    bool isDragging;

    float idleTimer;
    public float autoRotateDelay = 3f;

    float defaultPitch = 15f;

    [Header("Rotation")]
    public Transform targetModel;

    [Header("Auto Rotation")]
    public float autoRotationSpeed = 10f;           // Degrees per second
    public float interactionPauseDuration = 2f;     // Time in seconds to pause auto rotation after interaction

    [Header("Zoom")]
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
        if (isDragging)
            return;

        idleTimer += Time.deltaTime;

        if (idleTimer < autoRotateDelay)
            return;

        // Smoothly reset pitch to default
        pitch = Mathf.Lerp(pitch, defaultPitch, Time.deltaTime * 2f);

        // Rotate only on Y axis
        yaw += -autoRotateSpeed * Time.deltaTime;

        UpdateCamera();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        lastPointerPosition = eventData.position;
        idleTimer = 0f;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        idleTimer = 0f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.delta;

        yaw += delta.x * rotationSpeed;
        pitch -= delta.y * rotationSpeed;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        UpdateCamera();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        idleTimer = 0f;
    }

    Bounds GetObjectBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return new Bounds(obj.transform.position, Vector3.one * 0.01f);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds;
    }

    Vector3 CalculateFocusPoint(GameObject obj)
    {
        Bounds bounds = GetObjectBounds(obj);

        // True visual center of geometry
        return bounds.center;
    }

    void UpdateCamera()
    {
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 dir = rot * Vector3.forward;

        modelCamera.transform.position = focusPoint - dir * distance;
        modelCamera.transform.LookAt(focusPoint);
    }

    public void NormalizeModelSize(GameObject obj, float targetSize = 10f)
    {
        Bounds bounds = GetObjectBounds(obj);

        float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

        if (maxSize <= 0f)
            return;

        float scaleFactor = targetSize / maxSize;

        obj.transform.localScale *= scaleFactor;
    }

    public void FrameObject(GameObject obj)
    {
        NormalizeModelSize(obj);

        Bounds bounds = GetObjectBounds(obj);

        focusPoint = bounds.center;   // 🔒 LOCKED

        float height = bounds.size.y;
        float width = Mathf.Max(bounds.size.x, bounds.size.z);

        float fovRad = modelCamera.fieldOfView * Mathf.Deg2Rad;
        float aspect = modelCamera.aspect;

        float distH = (height * 0.5f) / Mathf.Tan(fovRad * 0.5f);
        float hFov = 2f * Mathf.Atan(Mathf.Tan(fovRad * 0.5f) * aspect);
        float distW = (width * 0.5f) / Mathf.Tan(hFov * 0.5f);

        distance = Mathf.Max(distH, distW) * 1.6f;

        // ✅ correct safety for ALL sizes
        distance += bounds.extents.magnitude * 0.5f;

        modelCamera.nearClipPlane = Mathf.Max(0.01f, distance * 0.03f);

        yaw = 180f;   // face the front of the model
        pitch = 15f;

        UpdateCamera();
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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(focusPoint, 0.02f);
    }
}
