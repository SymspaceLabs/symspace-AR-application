using UnityEngine;
using UnityEngine.UI;

public class SyncScrollRect : MonoBehaviour
{
    [Header("Source ScrollRect")]
    public ScrollRect sourceScrollRect;

    [Header("Sync Options")]
    public bool syncHorizontal = true;
    public bool syncVertical = true;

    private ScrollRect targetScrollRect;
    private bool isUpdating = false;

    private void Awake()
    {
        targetScrollRect = GetComponent<ScrollRect>();
    }

    private void OnEnable()
    {
        if (sourceScrollRect != null)
            sourceScrollRect.onValueChanged.AddListener(SyncScroll);
    }

    private void OnDisable()
    {
        if (sourceScrollRect != null)
            sourceScrollRect.onValueChanged.RemoveListener(SyncScroll);
    }

    private void SyncScroll(Vector2 position)
    {
        if (isUpdating || targetScrollRect == null)
            return;

        isUpdating = true;

        Vector2 targetPos = targetScrollRect.normalizedPosition;

        if (syncHorizontal)
            targetPos.x = sourceScrollRect.normalizedPosition.x;

        if (syncVertical)
            targetPos.y = sourceScrollRect.normalizedPosition.y;

        targetScrollRect.normalizedPosition = targetPos;

        isUpdating = false;
    }
}