using UnityEngine;
using UnityEngine.UI;

public class HoldButton : MonoBehaviour
{
    public enum ButtonAction
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        RotateLeft,
        RotateRight
    }

    public ButtonAction action;

    private RectTransform rectTransform;
    private int activeFingerId = -1;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            Vector2 screenPos = touch.position;

            bool inside =
                RectTransformUtility.RectangleContainsScreenPoint(
                    rectTransform,
                    screenPos,
                    null);

            // Finger starts on button
            if (touch.phase == TouchPhase.Began &&
                inside &&
                activeFingerId == -1)
            {
                activeFingerId = touch.fingerId;
                StartAction();
            }

            // Active finger released
            if (touch.fingerId == activeFingerId &&
                (touch.phase == TouchPhase.Ended ||
                 touch.phase == TouchPhase.Canceled))
            {
                StopAction();
                activeFingerId = -1;
            }

            // Optional: stop if finger drags outside button
            if (touch.fingerId == activeFingerId &&
                touch.phase == TouchPhase.Moved &&
                !inside)
            {
                StopAction();
                activeFingerId = -1;
            }
        }
    }

    private void StartAction()
    {
        switch (action)
        {
            case ButtonAction.MoveUp:
                UIManagerAR.instance.MoveUp();
                break;

            case ButtonAction.MoveDown:
                UIManagerAR.instance.MoveDown();
                break;

            case ButtonAction.MoveLeft:
                UIManagerAR.instance.MoveLeft();
                break;

            case ButtonAction.MoveRight:
                UIManagerAR.instance.MoveRight();
                break;

            case ButtonAction.RotateLeft:
                UIManagerAR.instance.RotateLeft();
                break;

            case ButtonAction.RotateRight:
                UIManagerAR.instance.RotateRight();
                break;
        }
    }

    private void StopAction()
    {
        switch (action)
        {
            case ButtonAction.MoveUp:
            case ButtonAction.MoveDown:
            case ButtonAction.MoveLeft:
            case ButtonAction.MoveRight:
                UIManagerAR.instance.StopMovement();
                break;

            case ButtonAction.RotateLeft:
            case ButtonAction.RotateRight:
                UIManagerAR.instance.StopRotating();
                break;
        }
    }
}