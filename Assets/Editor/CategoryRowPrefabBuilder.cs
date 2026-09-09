using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CategoryRowPrefabBuilder
{
    private const string FontGuid = "ce085c12ffe165d46a868c52ad78a8ca";
    private const string OutputFolder = "Assets/Resources/UI";
    private const string OutputPath = "Assets/Resources/UI/CategoryRow.prefab";

    [MenuItem("Tools/Symspace/Build Category Row Prefab")]
    public static void Build()
    {
        GameObject root = new GameObject("CategoryRow", typeof(RectTransform));
        root.layer = 5;

        // --- Root: image + layout + layout element ---
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

        LayoutElement rootElement = root.AddComponent<LayoutElement>();
        rootElement.preferredHeight = 55f;

        CategoryRow row = root.AddComponent<CategoryRow>();

        // --- Checkbox (left) ---
        GameObject checkboxObject = CreateRect("Checkbox", root.transform);
        Image checkboxImage = checkboxObject.AddComponent<Image>();
        checkboxImage.color = new Color(1f, 1f, 1f, 1f);
        checkboxImage.raycastTarget = true;

        Button checkboxButton = checkboxObject.AddComponent<Button>();
        checkboxButton.targetGraphic = checkboxImage;

        LayoutElement checkboxElement = checkboxObject.AddComponent<LayoutElement>();
        checkboxElement.preferredWidth = 38f;
        checkboxElement.preferredHeight = 38f;

        // Selected overlay
        GameObject selectedObject = CreateRect("Selected", checkboxObject.transform);
        StretchToFill(selectedObject);
        Image selectedImage = selectedObject.AddComponent<Image>();
        selectedImage.color = new Color(0.24f, 0.55f, 1f, 1f);
        selectedImage.raycastTarget = false;
        selectedImage.gameObject.SetActive(false);

        // Checkmark
        GameObject checkmarkObject = CreateRect("Checkmark", checkboxObject.transform);
        StretchToFill(checkmarkObject);
        Image checkmarkImage = checkmarkObject.AddComponent<Image>();
        checkmarkImage.color = Color.white;
        checkmarkImage.raycastTarget = false;
        checkmarkImage.gameObject.SetActive(false);

        // Minus
        GameObject minusObject = CreateRect("Minus", checkboxObject.transform);
        StretchToFill(minusObject);
        Image minusImage = minusObject.AddComponent<Image>();
        minusImage.color = Color.white;
        minusImage.raycastTarget = false;
        minusImage.gameObject.SetActive(false);

        // --- Label (right) ---
        GameObject labelObject = CreateRect("Label", root.transform);
        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = "Category";
        label.fontSize = 24f;
        label.fontStyle = FontStyles.Normal;
        label.color = Color.black;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.raycastTarget = false;

        string fontPath = AssetDatabase.GUIDToAssetPath(FontGuid);

        if (!string.IsNullOrEmpty(fontPath))
        {
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            label.font = font;
        }

        LayoutElement labelElement = labelObject.AddComponent<LayoutElement>();
        labelElement.flexibleWidth = 1f;
        labelElement.preferredHeight = 55f;

        // --- Wire up CategoryRow references ---
        row.checkboxImage = checkboxImage;
        row.selectedImage = selectedImage;
        row.checkmarkImage = checkmarkImage;
        row.minusImage = minusImage;
        row.label = label;
        row.checkboxButton = checkboxButton;
        row.layout = layout;

        if (!Directory.Exists(OutputFolder))
            Directory.CreateDirectory(OutputFolder);

        AssetDatabase.DeleteAsset(OutputPath);
        PrefabUtility.SaveAsPrefabAsset(root, OutputPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("CategoryRow prefab created at " + OutputPath);
    }

    private static GameObject CreateRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void StretchToFill(GameObject go)
    {
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
