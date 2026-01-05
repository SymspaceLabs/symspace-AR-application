using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class DirectionalKeyMovement : MonoBehaviour
{
    public bool enableDirectionalKeyMovement = false;
    public float movementSpeed = 5.0f;

    private Vector3 movementDirection = Vector3.zero;
    XRGrabInteractable interactable;
    ARTransformer transformer;

    public float rotationSpeed = 90f;
    public float tapRotationAmount = 15f;

    public bool isRotating = false;
    private float rotationDirection = 0f;

    private IEnumerator Start()
    {
        bool MovementState = enableDirectionalKeyMovement;
        enableDirectionalKeyMovement = false;
        yield return new WaitForSeconds(1f);
        interactable = GetComponent<XRGrabInteractable>();
        transformer = GetComponent<ARTransformer>();
        enableDirectionalKeyMovement = MovementState;
    }

    private void Update()
    {
        if (enableDirectionalKeyMovement)
        {
            // Only move if there’s a direction set
            if (movementDirection != Vector3.zero)
            {
                //Debug.Log("Moving", gameObject);
                transform.Translate(movementDirection * movementSpeed * Time.deltaTime, Space.World);
            }

            // Rotation while holding button
            if (isRotating)
            {
                //Debug.Log("rotating", gameObject);
                float rotationAmount = rotationSpeed * rotationDirection * Time.deltaTime;
                transform.Rotate(Vector3.up, rotationAmount, Space.Self);
            }
        }
    }

    // Called when directional button pressed down
    public void SetMovementDirection(Vector3 direction)
    {
        Debug.Log("Direction " +  direction);
        movementDirection = direction;
    }

    // Called when directional button released
    public void StopMovement()
    {
        movementDirection = Vector3.zero;
    }

    // Called when rotate button pressed down
    public void SetRotationDirection(float direction)
    {
        isRotating = true;
        rotationDirection = direction;
    }

    // Called when rotate button released
    public void StopRotation()
    {
        isRotating = false;
        rotationDirection = 0f;
    }
}
