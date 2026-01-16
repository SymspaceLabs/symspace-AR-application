using UnityEngine;

public class ARObjectManipulator : MonoBehaviour
{
    public enum Orientation { Horizontal, Vertical }
    public Orientation orientation = Orientation.Horizontal;

    private Camera arCamera;
    private bool isDragging = false;
    private Vector3 grabOffset;

    // Touch ownership
    private int ownerFingerId = -1;

    // Rotation tracking
    private float lastAngleBetweenTouches;

    [Header("Editor (Simulator) Controls")]
    public float editorRotationSpeed = 90f;

    private Plane dragPlane;
    private Vector3 dragStartWorldPos;
    private Vector3 wallNormal;

    void Start()
    {
        arCamera = Camera.main;
    }

    void Update()
    {
        if (Application.isEditor)
        {
            HandleMouseInput();
            HandleEditorRotationKeys();
        }
        else
        {
            HandleTouchInput();
        }
    }

    // ============================================================
    // TOUCH INPUT
    // ============================================================

    private void HandleTouchInput()
    {
        if (Input.touchCount == 0)
        {
            ResetTouchState();
            return;
        }

        // Detect object selection
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = arCamera.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
                {
                    SelectObject(touch.fingerId, touch.position);
                }
            }
        }

        // Two-finger rotation (only when owner finger is one of them)
        if (ownerFingerId != -1 && Input.touchCount >= 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            if (t0.fingerId == ownerFingerId || t1.fingerId == ownerFingerId)
            {
                HandleTwoFingerRotation(t0, t1);
                return; // prevent dragging during rotation
            }
        }

        // Single finger drag
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);
            if (t.fingerId == ownerFingerId && isDragging)
            {
                if (t.phase == TouchPhase.Moved)
                    DragToPosition(t.position);
            }
        }

        // Finger lifted
        foreach (Touch t in Input.touches)
        {
            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                if (t.fingerId == ownerFingerId)
                {
                    ResetTouchState();
                }
            }
        }
    }

    private void SelectObject(int fingerId, Vector3 screenPos)
    {
        ownerFingerId = fingerId;
        isDragging = true;

        Ray ray = arCamera.ScreenPointToRay(screenPos);

        if (orientation == Orientation.Horizontal)
        {
            dragPlane = new Plane(Vector3.up, transform.position);
        }
        else // Vertical (wall)
        {
            wallNormal = transform.up; // Y axis points out of wall
            dragPlane = new Plane(-arCamera.transform.forward, transform.position);
        }

        if (dragPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            grabOffset = transform.position - hitPoint;
        }

        dragStartWorldPos = transform.position;
    }


    private void HandleTwoFingerRotation(Touch t0, Touch t1)
    {
        float currentAngle = GetAngleBetweenTouches(t0, t1);

        if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
        {
            lastAngleBetweenTouches = currentAngle;
            return;
        }

        float delta = Mathf.DeltaAngle(lastAngleBetweenTouches, currentAngle);
        lastAngleBetweenTouches = currentAngle;

        RotateY(delta);
    }

    private float GetAngleBetweenTouches(Touch a, Touch b)
    {
        Vector2 diff = b.position - a.position;
        return Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
    }

    // ============================================================
    // MOUSE INPUT
    // ============================================================

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = arCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
            {
                SelectObject(-999, Input.mousePosition);
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            DragToPosition(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            ResetTouchState();
        }
    }

    private void HandleEditorRotationKeys()
    {
        if (!isDragging) return;

        float rot = 0;
        if (Input.GetKey(KeyCode.Q)) rot -= 1f;
        if (Input.GetKey(KeyCode.E)) rot += 1f;

        if (rot != 0)
            RotateY(rot * editorRotationSpeed * Time.deltaTime);
    }

    // ============================================================
    // CORE LOGIC
    // ============================================================

    private void DragToPosition(Vector3 screenPos)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPos);

        if (!dragPlane.Raycast(ray, out float enter))
            return;

        Vector3 hitPoint = ray.GetPoint(enter);
        Vector3 desiredPos = hitPoint + grabOffset;

        if (orientation == Orientation.Vertical)
        {
            Vector3 delta = desiredPos - dragStartWorldPos;

            // Remove movement into / out of wall
            delta -= Vector3.Dot(delta, wallNormal) * wallNormal;

            transform.position = dragStartWorldPos + delta;
        }
        else
        {
            transform.position = desiredPos;
        }
    }


    private void RotateY(float deltaAngle)
    {
        if (orientation == Orientation.Horizontal)
        {
            // Rotate on floor
            transform.Rotate(Vector3.up, -deltaAngle, Space.World);
        }
        else // Vertical (wall)
        {
            // Rotate around wall normal (Y axis points out of wall)
            transform.Rotate(transform.up, -deltaAngle, Space.World);
        }
    }

    private void ResetTouchState()
    {
        isDragging = false;
        ownerFingerId = -1;
    }
}
