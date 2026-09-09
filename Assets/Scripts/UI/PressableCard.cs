using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PressableCard : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    [Header("Press")]
    public float pressedScale = 0.97f;
    public float pressDuration = 0.09f;
    public float releaseDuration = 0.16f;

    [Header("Shadow Lift")]
    public bool useShadowLift = true;
    public float shadowLift = 24f;

    private Shadow shadow;
    private Coroutine scaleRoutine;
    private Vector3 restScale = Vector3.one;
    private Color restShadowColor = new Color(0f, 0f, 0f, 0.35f);
    private Vector2 restShadowDistance = Vector2.zero;

    void Awake()
    {
        restScale = transform.localScale;

        if (useShadowLift)
        {
            bool masked = GetComponent<Mask>() != null || GetComponent<RectMask2D>() != null;

            if (!masked)
            {
                if (shadow == null)
                    shadow = GetComponent<Shadow>();

                if (shadow == null)
                    shadow = gameObject.AddComponent<Shadow>();
            }
        }

        if (shadow != null)
        {
            restShadowColor = shadow.effectColor;
            restShadowDistance = shadow.effectDistance;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        AnimateScale(pressedScale, pressDuration, UIAnim.EaseInCubic);
        AnimateShadow(true, pressDuration);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        AnimateScale(restScale.x, releaseDuration, UIAnim.EaseOutBack);
        AnimateShadow(false, releaseDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AnimateScale(restScale.x, releaseDuration, UIAnim.EaseOutBack);
        AnimateShadow(false, releaseDuration);
    }

    void AnimateScale(float target, float duration, System.Func<float, float> ease)
    {
        if (scaleRoutine != null)
            StopCoroutine(scaleRoutine);

        float from = transform.localScale.x;
        scaleRoutine = StartCoroutine(UIAnim.CoScale(transform, from, target, duration, ease));
    }

    void AnimateShadow(bool pressed, float duration)
    {
        if (shadow == null)
            return;

        Vector2 target = pressed
            ? new Vector2(0f, -shadowLift)
            : restShadowDistance;

        Color targetColor = pressed
            ? new Color(restShadowColor.r, restShadowColor.g, restShadowColor.b, 0.14f)
            : restShadowColor;

        StartCoroutine(AnimateShadowRoutine(target, targetColor, duration));
    }

    IEnumerator AnimateShadowRoutine(Vector2 targetDistance, Color targetColor, float duration)
    {
        Vector2 fromDistance = shadow.effectDistance;
        Color fromColor = shadow.effectColor;

        yield return UIAnim.CoTween(
            duration,
            UIAnim.EaseOutCubic,
            (t) =>
            {
                if (shadow == null)
                    return;

                shadow.effectDistance = Vector2.Lerp(fromDistance, targetDistance, t);
                shadow.effectColor = Color.Lerp(fromColor, targetColor, t);
            });

        if (shadow != null)
        {
            shadow.effectDistance = targetDistance;
            shadow.effectColor = targetColor;
        }
    }
}