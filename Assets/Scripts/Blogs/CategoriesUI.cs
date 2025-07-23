using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CategoriesUI : MonoBehaviour
{
    #region Parameters
    [Header("UI Elements")]
    public Transform contentParent;
    public GameObject categoryItemPrefab;
    public Button refreshButton;
    public TextMeshProUGUI statusText;

    private string categoriesUrl = AuthAPI.api + "categories";
    #endregion

    private void OnEnable()
    {
        statusText.gameObject.SetActive(false);
        LoadCategories();
    }

    void Start()
    {
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(LoadCategories);
        }
    }

    #region API Call
    public void LoadCategories()
    {
        MenuManager.Instance.loadingPanel.SetActive(true);
        ClearCategories();

        StartCoroutine(AuthAPI.PostRequest(categoriesUrl, "", // Empty string for no body
            (response) =>
            {
                Debug.Log("Categories loaded: " + response);

                // Parse the response
                CategoriesResponse responseData = JsonUtility.FromJson<CategoriesResponse>("{\"categories\":" + response + "}");

                if (responseData.categories != null && responseData.categories.Length > 0)
                {
                    PopulateCategories(responseData.categories);
                    statusText.gameObject.SetActive(false);
                }
                else
                {
                    ShowStatus("No categories found", false);
                }

                MenuManager.Instance.loadingPanel.SetActive(false);
            },
            (error) =>
            {
                Debug.LogError("Failed to load categories: " + error);
                ShowStatus("Failed to load categories", true);
                MenuManager.Instance.loadingPanel.SetActive(false);
            }, "GET"));
    }
    #endregion

    #region UI Methods
    private void PopulateCategories(CategoryData[] categories)
    {
        foreach (CategoryData category in categories)
        {
            GameObject item = Instantiate(categoryItemPrefab, contentParent);
            CategoryItemUI itemUI = item.GetComponent<CategoryItemUI>();

            if (itemUI != null)
            {
                itemUI.Initialize(category.id, category.name);
            }
            else
            {
                // Fallback if the prefab doesn't have the CategoryItemUI component
                TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = $"{category.name} (ID: {category.id})";
                }
            }
        }
    }

    private void ClearCategories()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void ShowStatus(string message, bool isError)
    {
        statusText.gameObject.SetActive(true);
        statusText.text = message;
        statusText.color = isError ? Color.red : Color.white;
    }
    #endregion

    #region Structure Classes
    [System.Serializable]
    private class CategoriesResponse
    {
        public CategoryData[] categories;
    }

    [System.Serializable]
    private class CategoryData
    {
        public string id;
        public string name;
    }
    #endregion
}