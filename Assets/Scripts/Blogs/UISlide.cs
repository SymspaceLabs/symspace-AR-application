using UnityEngine;

public class UISlide : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public float duration = 0.5f;

    public RectTransform rect;
    private float timer;
    private bool isMoving = false;

    void Start()
    {
        // Set starting Y position
        Vector2 pos = rect.anchoredPosition;
        pos.y = startPoint.localPosition.y;
        rect.localPosition = pos;
    }

    public void StartMove()
    {
        timer = 0f;
        isMoving = true;
    }

    void Update()
    {
        if (!isMoving) return;

        timer += Time.deltaTime;
        float t = timer / duration;

        float newY = Mathf.Lerp(startPoint.localPosition.y, endPoint.localPosition.y, t);

        // Apply new Y
        Vector2 pos = rect.anchoredPosition;
        pos.y = newY;
        rect.localPosition = pos;

        if (t >= 1f)
            isMoving = false;
    }
}
