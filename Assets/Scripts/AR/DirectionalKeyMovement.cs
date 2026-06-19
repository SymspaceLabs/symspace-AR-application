using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class DirectionalKeyMovement : MonoBehaviour
{
    public enum MovementDirection { None, Forward, Back, Left, Right }
    public enum RotationDirection { None, Left, Right }

    public bool enableDirectionalKeyMovement = false;
    public float movementSpeed = 5.0f;
    public float rotationSpeed = 90f;

    private Vector3 currentMovementVector = Vector3.zero;
    private Vector3 currentRotationAxis = Vector3.zero;
    private float currentRotationSign = 0f;
    private bool isRotating = false;

    private Transform cameraTransform;
    private ARTransformer transformer;

    private IEnumerator Start()
    {
        bool initialMovementState = enableDirectionalKeyMovement;
        enableDirectionalKeyMovement = false;

        yield return new WaitForSeconds(1f);

        transformer = GetComponent<ARTransformer>();
        if (Camera.main != null) cameraTransform = Camera.main.transform;

        enableDirectionalKeyMovement = initialMovementState;
    }

    private void Update()
    {
        if (!enableDirectionalKeyMovement) return;

        // Handle Translation
        if (currentMovementVector != Vector3.zero)
        {
            transform.Translate(currentMovementVector * movementSpeed * Time.deltaTime, Space.World);
        }

        // Handle Adaptive Rotation
        if (isRotating && currentRotationAxis != Vector3.zero)
        {
            float rotationAmount = rotationSpeed * currentRotationSign * Time.deltaTime;
            transform.Rotate(currentRotationAxis, rotationAmount, Space.Self);
        }
    }

    /// <summary>
    /// Processes camera-relative and surface-aware directions internally.
    /// </summary>
    public void SetMovement(MovementDirection direction)
    {
        if (direction == MovementDirection.None)
        {
            StopMovement();
            return;
        }

        if (transformer == null) transformer = GetComponent<ARTransformer>();

        // ---- HORIZONTAL PLANE MOVEMENT (Floors / Tables) ----
        if (transformer != null && transformer.objectPlaneTranslationMode == ARTransformer.PlaneTranslationMode.Horizontal)
        {
            if (cameraTransform == null) return;

            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;

            switch (direction)
            {
                case MovementDirection.Forward: currentMovementVector = forward; break;
                case MovementDirection.Back: currentMovementVector = -forward; break;
                case MovementDirection.Left: currentMovementVector = -right; break;
                case MovementDirection.Right: currentMovementVector = right; break;
            }
        }
        // ---- VERTICAL PLANE MOVEMENT (Walls) ----
        else if (transformer != null && transformer.objectPlaneTranslationMode == ARTransformer.PlaneTranslationMode.Vertical)
        {
            // Keeps your custom orientation rule neatly packaged inside the object
            Vector3 planeRight = transform.forward;
            Vector3 planeUp = -transform.right;

            switch (direction)
            {
                case MovementDirection.Left: currentMovementVector = -planeRight; break;
                case MovementDirection.Right: currentMovementVector = planeRight; break;
                case MovementDirection.Forward: currentMovementVector = planeUp; break;   // Slide up wall
                case MovementDirection.Back: currentMovementVector = -planeUp; break;  // Slide down wall
            }
        }
    }

    /// <summary>
    /// Selects the correct rotation axis dynamically depending on surface orientation.
    /// </summary>
    public void SetRotation(RotationDirection direction)
    {
        if (direction == RotationDirection.None)
        {
            StopRotation();
            return;
        }

        if (transformer == null) transformer = GetComponent<ARTransformer>();
        isRotating = true;

        // Determine clockwise / counter-clockwise
        currentRotationSign = (direction == RotationDirection.Left) ? 1f : -1f;

        // Pick the correct local axis to spin around depending on surface
        if (transformer != null && transformer.objectPlaneTranslationMode == ARTransformer.PlaneTranslationMode.Vertical)
        {
            // Vertical objects spin flat against the wall using local X
            currentRotationAxis = Vector3.up;
        }
        else
        {
            // Horizontal objects spin on the ground using local Y
            currentRotationAxis = Vector3.up;
        }
    }

    public void StopMovement() => currentMovementVector = Vector3.zero;

    public void StopRotation()
    {
        isRotating = false;
        currentRotationSign = 0f;
        currentRotationAxis = Vector3.zero;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            StopMovement();
            StopRotation();
        }
    }
}