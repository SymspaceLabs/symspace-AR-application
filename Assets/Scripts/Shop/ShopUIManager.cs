using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ShopUIManager : MonoBehaviour
{
    [Header("Item List UI")]
    public Transform itemListContainer;
    public GameObject itemButtonPrefab;

    [Header("Item Detail UI")]
    public GameObject itemDetailPanel;
    public Image bigImage;
    public Text nameText;
    public Text companyText;
    public Text priceText;
    public Text discountedPriceText;
    public Transform colorOptionsContainer;
    public GameObject colorButtonPrefab;
    public Transform sizeOptionsContainer;
    public GameObject sizeButtonPrefab;
    public Button arRoomButton;
    public Button addToCartButton;
    public Text descriptionText;

    private void Start()
    {
        //OnCategoryButtonClicked("shirts");
    }

    public void OnCategoryButtonClicked(string categoryName)
    {
        StartCoroutine(FetchCategoryItems(categoryName));
    }

    IEnumerator FetchCategoryItems(string categoryName)
    {
        string endpoint = $"/shop/items?category={categoryName}"; // Adjust your endpoint path here

        yield return AuthAPI.PostRequest(
            url: endpoint,
            jsonData: "",
            onSuccess: (response) =>
            {
                List<ItemData> items = JsonUtility.FromJson<List<ItemData>>(response);
                DisplayItems(items);
            },
            onError: (error) =>
            {
                Debug.LogError("Failed to fetch items: " + error);
            }
        );
    }

    void DisplayItems(List<ItemData> items)
    {
        foreach (Transform child in itemListContainer)
            Destroy(child.gameObject);

        foreach (ItemData item in items)
        {
            GameObject btn = Instantiate(itemButtonPrefab, itemListContainer);
            StartCoroutine(LoadImage(item.imageUrl, btn.transform.Find("ItemImage").GetComponent<Image>()));
            btn.transform.Find("ItemName").GetComponent<Text>().text = item.itemName;
            btn.transform.Find("CompanyName").GetComponent<Text>().text = item.companyName;
            btn.transform.Find("Price").GetComponent<Text>().text = $"${item.price}";

            Text discountText = btn.transform.Find("DiscountedPrice").GetComponent<Text>();
            if (item.discountedPrice < item.price)
                discountText.text = $"${item.discountedPrice}";
            else
                discountText.gameObject.SetActive(false);

            btn.GetComponent<Button>().onClick.AddListener(() => ShowItemDetails(item));
        }
    }

    void ShowItemDetails(ItemData item)
    {
        itemDetailPanel.SetActive(true);
        StartCoroutine(LoadImage(item.imageUrl, bigImage));
        nameText.text = item.itemName;
        companyText.text = item.companyName;
        priceText.text = $"${item.price}";
        discountedPriceText.text = item.discountedPrice < item.price ? $"${item.discountedPrice}" : "";
        descriptionText.text = item.description;

        foreach (Transform c in colorOptionsContainer) Destroy(c.gameObject);
        foreach (Transform s in sizeOptionsContainer) Destroy(s.gameObject);

        foreach (string color in item.availableColors)
        {
            GameObject btn = Instantiate(colorButtonPrefab, colorOptionsContainer);
            btn.GetComponentInChildren<Text>().text = color;
        }

        foreach (string size in item.availableSizes)
        {
            GameObject btn = Instantiate(sizeButtonPrefab, sizeOptionsContainer);
            btn.GetComponentInChildren<Text>().text = size;
        }

        addToCartButton.onClick.RemoveAllListeners();
        addToCartButton.onClick.AddListener(() => AddToCart(item));
    }

    void AddToCart(ItemData item)
    {
        Debug.Log($"{item.itemName} added to cart!");
    }

    IEnumerator LoadImage(string imageUrl, Image targetImage)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            targetImage.sprite = sprite;
        }
        else
        {
            Debug.LogError("Image load failed: " + imageUrl);
        }
    }

    public void LoadARScene()
    {
        SceneManager.LoadScene("AR Scene");
    }

    [System.Serializable]
    public class ItemData
    {
        public string itemName;
        public string companyName;
        public string imageUrl; // Use this instead of Sprite
        public float price;
        public float discountedPrice;
        public string[] availableColors;
        public string[] availableSizes;
        public string description;
    }
}
