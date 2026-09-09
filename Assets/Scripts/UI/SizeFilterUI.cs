using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SizeFilterUI : MonoBehaviour
{
    public Color selectedColor = new Color(0.24f, 0.55f, 1f, 1f);

    [Header("Animation")]
    public float fillFadeDuration = 0.15f;
    public float scaleBlipPeak = 1.12f;

    private readonly List<Image> chipImages = new List<Image>();
    private readonly List<Color> baseColors = new List<Color>();
    private readonly HashSet<int> selectedIndexes = new HashSet<int>();
    private readonly Dictionary<int, Coroutine> activeAnims = new Dictionary<int, Coroutine>();

    public IReadOnlyCollection<int> SelectedIndexes => selectedIndexes;

    void Start()
    {
        foreach (Transform child in transform)
        {
            Image image = child.GetComponent<Image>();
            if (image == null) continue;

            int index = chipImages.Count;
            chipImages.Add(image);
            baseColors.Add(image.color);

            Button button = child.GetComponent<Button>();
            if (button == null)
                button = child.gameObject.AddComponent<Button>();

            button.transition = Button.Transition.None;
            button.targetGraphic = image;
            button.onClick.AddListener(() => Toggle(index));
        }
    }

    public void Toggle(int index)
    {
        SetSelected(index, !IsSelected(index));
    }

    public void SetSelected(int index, bool selected)
    {
        if (index < 0 || index >= chipImages.Count)
            return;

        if (selected)
            selectedIndexes.Add(index);
        else
            selectedIndexes.Remove(index);

        AnimateChip(index);
    }

    public bool IsSelected(int index)
    {
        return index >= 0 && index < chipImages.Count && selectedIndexes.Contains(index);
    }

    public void ClearSelection()
    {
        selectedIndexes.Clear();
        for (int i = 0; i < chipImages.Count; i++)
            AnimateChip(i);
    }

    void AnimateChip(int index)
    {
        if (chipImages[index] == null)
            return;

        if (activeAnims.TryGetValue(index, out Coroutine running) && running != null)
            StopCoroutine(running);

        activeAnims[index] = StartCoroutine(AnimateChipRoutine(index));
    }

    IEnumerator AnimateChipRoutine(int index)
    {
        Image image = chipImages[index];
        Color fromColor = image.color;
        Color toColor = selectedIndexes.Contains(index)
            ? selectedColor
            : baseColors[index];

        float startFill = fromColor.a;
        float deltaFill = toColor.a - startFill;

        Vector3 baseScale = Vector3.one;
        Vector3 peakScale = Vector3.one * scaleBlipPeak;

        yield return UIAnim.CoTween(
            fillFadeDuration,
            UIAnim.EaseOutCubic,
            (t) =>
            {
                if (image == null)
                    return;

                float fill = startFill + deltaFill * t;
                image.color = new Color(
                    Mathf.LerpUnclamped(fromColor.r, toColor.r, t),
                    Mathf.LerpUnclamped(fromColor.g, toColor.g, t),
                    Mathf.LerpUnclamped(fromColor.b, toColor.b, t),
                    fill);

                float blip = 1f + (peakScale.x - 1f) * Mathf.Sin(t * Mathf.PI);
                image.transform.localScale = baseScale * blip;
            });

        if (image != null)
            image.color = toColor;

        if (image != null)
            image.transform.localScale = baseScale;

        activeAnims.Remove(index);
    }
}