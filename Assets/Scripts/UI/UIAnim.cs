using System;
using System.Collections;
using UnityEngine;

public static class UIAnim
{
    public static float EaseOutCubic(float t)
    {
        t = Mathf.Clamp01(t);
        return 1 - Mathf.Pow(1 - t, 3f);
    }

    public static float EaseInCubic(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * t;
    }

    public static float EaseOutBack(float t)
    {
        t = Mathf.Clamp01(t);
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    public static IEnumerator CoTween(
        float duration,
        Func<float, float> ease,
        Action<float> onUpdate)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            if (onUpdate != null)
                onUpdate(ease(Mathf.Clamp01(t)));

            yield return null;
        }

        if (onUpdate != null)
            onUpdate(ease(1f));
    }

    public static IEnumerator CoScale(
        Transform target,
        float from,
        float to,
        float duration,
        Func<float, float> ease)
    {
        yield return CoTween(
            duration,
            ease,
            (t) =>
            {
                if (target == null)
                    return;

                target.localScale = Vector3.one * Mathf.LerpUnclamped(from, to, t);
            });
    }
}