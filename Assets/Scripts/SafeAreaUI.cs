using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaUI : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect lastSafeArea;

    [Header("Safe Area Padding (Pixels)")]
    [Tooltip("Positive values shrink the safe area, negative values expand it")]
    [Header("Safe Area Padding (%)")]
    [Range(-0.1f, 0.1f)] public float leftPercent;
    [Range(-0.1f, 0.1f)] public float rightPercent;
    [Range(-0.1f, 0.1f)] public float topPercent;
    [Range(-0.1f, 0.1f)] public float bottomPercent;

    void Awake()
    {
        
    }

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;

        if (safeArea == lastSafeArea)
            return;

        lastSafeArea = safeArea;

        // Apply padding
        safeArea.xMin += Screen.width * leftPercent;
        safeArea.xMax -= Screen.width * rightPercent;
        safeArea.yMin += Screen.height * bottomPercent;
        safeArea.yMax -= Screen.height * topPercent;


        // Convert to normalized anchors
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;

        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    void Update()
    {
        ApplySafeArea();
    }
}
