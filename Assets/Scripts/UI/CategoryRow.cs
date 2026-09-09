using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CategoryRow : MonoBehaviour
{
    public Image checkboxImage;
    public Image selectedImage;
    public Image checkmarkImage;
    public Image minusImage;
    public TextMeshProUGUI label;
    public Button checkboxButton;
    public HorizontalLayoutGroup layout;

    public void Setup(string text, int leftPadding)
    {
        if (label != null)
            label.text = text;

        if (leftPadding == 0)
            return;

        if (layout != null && layout.enabled)
        {
            RectOffset padding = layout.padding;
            padding.left = leftPadding;
            layout.padding = padding;
            return;
        }

        // Manual layout (layout group disabled): shift all children right
        RectTransform rowRect = transform as RectTransform;

        if (rowRect == null)
            return;

        for (int i = 0; i < rowRect.childCount; i++)
        {
            RectTransform child = rowRect.GetChild(i) as RectTransform;

            if (child == null)
                continue;

            Vector2 pos = child.anchoredPosition;
            pos.x += leftPadding;
            child.anchoredPosition = pos;
        }

        // Keep the label's right edge aligned with the row
        if (label != null)
        {
            RectTransform labelRect = label.transform as RectTransform;

            if (labelRect != null)
            {
                Vector2 size = labelRect.sizeDelta;
                size.x = Mathf.Max(10f, size.x - leftPadding);
                labelRect.sizeDelta = size;
            }
        }
    }

    public void SetLabelBold(bool bold)
    {
        if (label == null)
            return;

        label.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
    }

    public void SetLabelColor(Color color)
    {
        if (label != null)
            label.color = color;
    }

    public void SetState(bool selected, bool partial)
    {
        if (selectedImage != null)
            selectedImage.gameObject.SetActive(selected || partial);

        if (checkmarkImage != null)
            checkmarkImage.gameObject.SetActive(selected);

        if (minusImage != null)
            minusImage.gameObject.SetActive(!selected && partial);
    }

    public void SetEnabled(bool enabled)
    {
        if (checkboxButton != null)
            checkboxButton.interactable = enabled;
    }
}
