using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class PriceRangeSlider : MonoBehaviour
{
    public event System.Action OnRangeChanged;

    public float minLimit = 0f;
    public float maxLimit = 1000f;
    public float step = 10f;

    public RectTransform track;
    public RectTransform fill;
    public RectTransform minHandle;
    public RectTransform maxHandle;
    public TextMeshProUGUI minPriceText;
    public TextMeshProUGUI maxPriceText;

    public string currency = "$";
    public string numberFormat = "{0:F0}";

    public float MinPrice { get; private set; }
    public float MaxPrice { get; private set; }

    bool initialized;
    float lastFiredMin;
    float lastFiredMax;

    public void RegisterHandle(PriceRangeHandle handle)
    {
    }

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        if (initialized) return;
        if (track == null || minHandle == null || maxHandle == null) return;
        initialized = true;
        lastFiredMin = MinPrice;
        lastFiredMax = MaxPrice;
        SetValues(minLimit, maxLimit);
    }

    public void MoveHandle(PriceRangeHandle handle, PointerEventData eventData)
    {
        if (!initialized) Initialize();
        if (handle == null || track == null) return;

        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(track, eventData.position, eventData.pressEventCamera, out local);

        float width = track.rect.width;
        float normalized = Mathf.Clamp01(width > 0.0001f ? Mathf.InverseLerp(track.rect.xMin, track.rect.xMax, local.x) : 0f);

        float value = Mathf.Lerp(minLimit, maxLimit, normalized);
        if (step > 0f) value = Mathf.Round(value / step) * step;
        value = Mathf.Clamp(value, minLimit, maxLimit);

        if (handle.isMin)
        {
            if (value > MaxPrice) value = MaxPrice;
            MinPrice = value;
        }
        else
        {
            if (value < MinPrice) value = MinPrice;
            MaxPrice = value;
        }

        UpdateUI();
    }

    public void SetValues(float min, float max)
    {
        MinPrice = Mathf.Clamp(min, minLimit, maxLimit);
        MaxPrice = Mathf.Clamp(max, minLimit, maxLimit);
        if (MinPrice > MaxPrice) MaxPrice = MinPrice;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (track == null) return;
        float width = track.rect.width;
        if (width <= 0f) return;

        float minNorm = Mathf.InverseLerp(minLimit, maxLimit, MinPrice);
        float maxNorm = Mathf.InverseLerp(minLimit, maxLimit, MaxPrice);

        if (minHandle != null)
            minHandle.anchoredPosition = new Vector2(minNorm * width, minHandle.anchoredPosition.y);
        if (maxHandle != null)
            maxHandle.anchoredPosition = new Vector2(maxNorm * width, maxHandle.anchoredPosition.y);

        if (fill != null)
        {
            fill.anchorMin = new Vector2(minNorm, 0f);
            fill.anchorMax = new Vector2(maxNorm, 1f);
            fill.offsetMin = new Vector2(0f, fill.offsetMin.y);
            fill.offsetMax = new Vector2(0f, fill.offsetMax.y);
        }

        if (minPriceText != null) minPriceText.text = currency + string.Format(numberFormat, MinPrice);
        if (maxPriceText != null) maxPriceText.text = currency + string.Format(numberFormat, MaxPrice);

        if (Mathf.Abs(MinPrice - lastFiredMin) > 0.0001f ||
            Mathf.Abs(MaxPrice - lastFiredMax) > 0.0001f)
        {
            lastFiredMin = MinPrice;
            lastFiredMax = MaxPrice;
            OnRangeChanged?.Invoke();
        }
    }
}
