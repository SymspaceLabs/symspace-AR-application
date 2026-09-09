using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompletionAnimation : MonoBehaviour
{
    [Header("Nodes (optional, auto-discovered when empty)")]
    public RectTransform badge;
    public Image progressBarFill;
    public CanvasGroup sparkles;
    public RectTransform rowsContainer;

    [Header("Timing")]
    public float progressDuration = 0.3f;
    public float badgeDuration = 0.4f;
    public float sparkleFadeDuration = 0.35f;
    public float rowDuration = 0.25f;
    public float rowStagger = 0.06f;

    static readonly List<string> RowNames = new List<string>
    {
        "Height", "Weight", "Measurement", "Style", "Description"
    };

    bool reduceMotion;
    List<RectTransform> rows;

    void Awake()
    {
        reduceMotion = RulerHaptics.IsReduceMotionEnabled();
    }

    public void Play()
    {
        if (badge == null) badge = FindChild<RectTransform>("Tick Img");
        if (rowsContainer == null) rowsContainer = FindChild<RectTransform>("Center Rectangle");

        if (rows == null) rows = new List<RectTransform>();
        rows.Clear();
        if (rowsContainer != null)
        {
            for (int i = 0; i < rowsContainer.childCount; i++)
            {
                RectTransform row = rowsContainer.GetChild(i) as RectTransform;
                if (row != null && RowNames.Contains(row.name))
                    rows.Add(row);
            }
        }

        StopAllCoroutines();
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        if (reduceMotion)
        {
            ApplyFinalState();
            yield break;
        }

        ResetToHidden();

        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = 0f;
            while (progressBarFill.fillAmount < 1f)
            {
                progressBarFill.fillAmount = Mathf.MoveTowards(progressBarFill.fillAmount, 1f, Time.deltaTime / progressDuration);
                yield return null;
            }
            progressBarFill.fillAmount = 1f;
        }

        if (badge != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / badgeDuration;
                float s = OutBack(t);
                badge.localScale = new Vector3(s, s, s);
                yield return null;
            }
            badge.localScale = Vector3.one;
            RulerHaptics.Success();
        }

        if (sparkles != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / sparkleFadeDuration;
                sparkles.alpha = Mathf.Clamp01(t);
                yield return null;
            }
            sparkles.alpha = 1f;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            StartCoroutine(PopRow(rows[i], rowDuration));
            yield return new WaitForSeconds(rowStagger);
        }
    }

    IEnumerator PopRow(RectTransform row, float duration)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float s = OutBack(t);
            row.localScale = new Vector3(s, s, s);
            yield return null;
        }
        row.localScale = Vector3.one;
    }

    void ResetToHidden()
    {
        if (badge != null) badge.localScale = Vector3.zero;
        if (sparkles != null) sparkles.alpha = 0f;
        for (int i = 0; i < rows.Count; i++)
            if (rows[i] != null) rows[i].localScale = Vector3.zero;
    }

    void ApplyFinalState()
    {
        if (badge != null) badge.localScale = Vector3.one;
        if (sparkles != null) sparkles.alpha = 1f;
        for (int i = 0; i < rows.Count; i++)
            if (rows[i] != null) rows[i].localScale = Vector3.one;
    }

    static float OutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float x = t - 1f;
        return 1f + c3 * x * x * x + c1 * x * x;
    }

    T FindChild<T>(string name) where T : Component
    {
        foreach (Transform t in transform.GetComponentsInChildren<Transform>(true))
        {
            if (t.name != name) continue;
            T comp = t.GetComponent<T>();
            if (comp != null) return comp;
        }
        return null;
    }
}