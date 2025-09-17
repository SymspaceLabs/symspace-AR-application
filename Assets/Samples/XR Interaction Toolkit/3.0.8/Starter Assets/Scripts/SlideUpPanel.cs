using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class SlideUpPanel : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public RectTransform panel; // Assign in inspector
    public float dragThreshold = 50f; // Pixels
    public float slideDuration = 0.3f;

    private Vector2 startDragPos;
    private bool isPanelUp = false;

    public Vector2 hiddenPos;
    public Vector2 shownPos;

    void Start()
    {
        panel.anchoredPosition = hiddenPos;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startDragPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Optional: Show small visual movement
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float dragDistance = eventData.position.y - startDragPos.y;

        if (!isPanelUp && dragDistance > dragThreshold)
            UIManagerAR.instance.ShowShop();
        else if (isPanelUp && dragDistance < -dragThreshold)
            HidePanel();
    }

    // Slide up
    public void ShowPanel()
    {
        StartCoroutine(SlideTo(shownPos));
        isPanelUp = true;
        UIManagerAR.instance.crossBtn.SetActive(true);
        
    }

    // Slide down
    public void HidePanel()
    {
        StartCoroutine(SlideTo(hiddenPos));
        isPanelUp = false;
        UIManagerAR.instance.crossBtn.SetActive(false);
    }

    IEnumerator SlideTo(Vector2 target)
    {
        Vector2 start = panel.anchoredPosition;
        float time = 0;

        while (time < slideDuration)
        {
            panel.anchoredPosition = Vector2.Lerp(start, target, time / slideDuration);
            time += Time.deltaTime;
            yield return null;
        }

        panel.anchoredPosition = target;
    }
}
