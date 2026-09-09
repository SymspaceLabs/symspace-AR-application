using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GlowButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public Shadow glowShadow;
    [Range(4f, 64f)] public float restRadius = 28f;
    [Range(4f, 64f)] public float pressedRadius = 16f;
    [Range(1f, 2f)] public float releaseOvershoot = 1.3f;
    [Range(0.05f, 0.3f)] public float speed = 0.12f;

    float baseRadius;
    float currentRadius;
    bool reduceMotion;

    void Awake()
    {
        if (glowShadow == null) glowShadow = GetComponent<Shadow>();
        reduceMotion = RulerHaptics.IsReduceMotionEnabled();
        baseRadius = restRadius;
        currentRadius = restRadius;
        ApplyRadius(currentRadius);
    }

    void OnEnable()
    {
        baseRadius = restRadius;
        currentRadius = restRadius;
        if (glowShadow != null) ApplyRadius(currentRadius);
    }

    void OnDisable()
    {
        baseRadius = restRadius;
        currentRadius = restRadius;
    }

    void Update()
    {
        if (glowShadow == null) return;
        float k = 1f - Mathf.Exp(-Time.deltaTime / speed);
        if (reduceMotion) k = 1f;
        currentRadius = Mathf.Lerp(currentRadius, baseRadius, k);
        ApplyRadius(currentRadius);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        baseRadius = pressedRadius;
        if (reduceMotion) currentRadius = pressedRadius;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        baseRadius = restRadius;
        if (reduceMotion) { currentRadius = restRadius; return; }
        baseRadius = restRadius * releaseOvershoot;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        baseRadius = restRadius;
        if (reduceMotion) currentRadius = restRadius;
    }

    void ApplyRadius(float radius)
    {
        if (glowShadow == null) return;
        glowShadow.effectDistance = new Vector2(radius, -radius) * 0.5f;
        Color c = glowShadow.effectColor;
        c.a = Mathf.Clamp01(radius / restRadius) * 0.85f;
        glowShadow.effectColor = c;
    }
}