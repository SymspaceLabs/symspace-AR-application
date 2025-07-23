using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CategoryItemUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI categoryNameText;
    public TextMeshProUGUI categoryIdText;
    public Button selectButton;

    private string categoryId;

    public void Initialize(string id, string name)
    {
        categoryId = id;

        if (categoryNameText != null)
            categoryNameText.text = name;

        if (categoryIdText != null)
            categoryIdText.text = id;

        if (selectButton != null)
        {
            selectButton.onClick.AddListener(() => OnCategorySelected(id, name));
        }
    }

    private void OnCategorySelected(string id, string name)
    {
        // Handle category selection
        Debug.Log($"Category selected - ID: {id}, Name: {name}");

        // You might want to store the selected category or trigger an event
        // For example:
        // CategorySelectionManager.Instance.SelectCategory(id, name);
    }
}