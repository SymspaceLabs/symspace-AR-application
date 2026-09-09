using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class PriceRangeHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool isMin;

    PriceRangeSlider slider;
    bool dragging;

    void Start()
    {
        slider = GetComponentInParent<PriceRangeSlider>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || slider == null) return;
        slider.MoveHandle(this, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
    }
}
