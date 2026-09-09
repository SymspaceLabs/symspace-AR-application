using UnityEngine;
using UnityEngine.UI;

public static class FilterRowFactory
{
    private const float CheckboxSize = 38f;

    public static CategoryRow BuildRow(Transform parent)
    {
        if (parent == null)
            return null;

        GameObject root = new GameObject("Row", typeof(RectTransform));
        root.transform.SetParent(parent, false);

        Image rootImage = root.AddComponent<Image>();
        rootImage.color = new Color(1f, 1f, 1f, 0f);
        rootImage.raycastTarget = false;

        HorizontalLayoutGroup layout = root.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        root.AddComponent<LayoutElement>().preferredHeight = 55f;

        CategoryRow row = root.AddComponent<CategoryRow>();
        BuildCheckbox(row, root.transform);
        BuildLabel(row, root.transform);

        return row;
    }

    private static void BuildCheckbox(CategoryRow row, Transform parent)
    {
        GameObject go = new GameObject("Checkbox", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        Image image = go.AddComponent<Image>();
        image.color = Color.white;
        image.raycastTarget = true;

        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;

        LayoutElement element = go.AddComponent<LayoutElement>();
        element.preferredWidth = CheckboxSize;
        element.preferredHeight = CheckboxSize;

        row.checkboxImage = image;
        row.selectedImage = CreateOverlay("Selected", go.transform, new Color(0.24f, 0.55f, 1f, 1f));
        row.checkmarkImage = CreateOverlay("Checkmark", go.transform, Color.white);
        row.minusImage = CreateOverlay("Minus", go.transform, Color.white);
        row.checkboxButton = button;
    }

    private static void BuildLabel(CategoryRow row, Transform parent)
    {
        GameObject go = new GameObject("Label", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        TMPro.TextMeshProUGUI label = go.AddComponent<TMPro.TextMeshProUGUI>();
        label.text = "Category";
        label.fontSize = 24f;
        label.fontStyle = TMPro.FontStyles.Normal;
        label.color = Color.black;
        label.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
        label.raycastTarget = false;

        LayoutElement element = go.AddComponent<LayoutElement>();
        element.flexibleWidth = 1f;
        element.preferredHeight = 55f;

        row.label = label;
    }

    private static Image CreateOverlay(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = go.AddComponent<Image>();
        image.raycastTarget = false;
        image.color = color;
        go.SetActive(false);

        return image;
    }
}
