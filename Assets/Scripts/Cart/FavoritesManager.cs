using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FavoritesManager : MonoBehaviour
{
    public static FavoritesManager Instance;

    public GameObject favoritesPanel;
    public Transform favoritesItemsParent;
    public GameObject favoriteItemPrefab;
    public Button exploreBtn;
    public Button closeBtn;
    public Button openBtn;
    public TextMeshProUGUI countText;
    public GameObject noItemsInFavorites;

    public Button favoriteToggleBtn;
    public Image favoriteToggleIcon;
    public Sprite favoriteOnIcon;
    public Sprite favoriteOffIcon;

    private List<FavoriteItem> items = new List<FavoriteItem>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        LoadFavorites();

        if (openBtn != null)
            openBtn.onClick.AddListener(OpenPanel);
        if (closeBtn != null)
            closeBtn.onClick.AddListener(ClosePanel);
        if (exploreBtn != null)
            exploreBtn.onClick.AddListener(OnExploreAll);

        if (UIManagerAR.instance != null)
        {
            if (UIManagerAR.instance.favoriteBtn_SD != null)
                UIManagerAR.instance.favoriteBtn_SD.onClick.AddListener(ToggleCurrent);
            if (UIManagerAR.instance.favoriteBtn_LD != null)
                UIManagerAR.instance.favoriteBtn_LD.onClick.AddListener(ToggleCurrent);
        }
        else
        {
            if (favoriteToggleBtn != null)
                favoriteToggleBtn.onClick.AddListener(ToggleCurrent);
        }

        if (favoritesPanel != null)
            favoritesPanel.SetActive(false);

        UpdateUI();
    }

    void OpenPanel()
    {
        if (favoritesPanel == null || favoritesPanel.activeSelf) return;
        favoritesPanel.SetActive(true);
        RefreshUI();
    }

    public void ClosePanel()
    {
        if (favoritesPanel != null)
            favoritesPanel.SetActive(false);
    }

    public void ToggleCurrent()
    {
        ProductDetails pd = null;
        int colorIdx = 0;

        if (UIManagerAR.instance != null && UIManagerAR.instance.selectedModelDetails != null)
        {
            pd = UIManagerAR.instance.selectedModelDetails;
            colorIdx = pd.selectedColorIndex;
        }
        else if (CategoriesUI.Instance != null && CategoriesUI.Instance.selectedProduct != null)
        {
            pd = CategoriesUI.Instance.selectedProduct;
            colorIdx = CategoriesUI.Instance.selectedMatIndex;
        }

        if (pd == null || pd.product == null) return;

        if (IsFavorited(pd.product.id))
            Remove(pd.product.id);
        else
            Add(pd.product.id, pd);
    }

    public bool IsFavorited(string productId)
    {
        return items.Any(i => i.productId == productId);
    }

    void Add(string productId, ProductDetails pd)
    {
        string colorCode = pd.product.colors[pd.selectedColorIndex > 0 ? pd.selectedColorIndex : 0].code;
        string imageUrl = "";
        Sprite productSprite = null;
        int imgIndex = pd.product.images.FindIndex(img => img.colorCode == colorCode);
        if (imgIndex >= 0)
        {
            if (imgIndex < pd.sprites.Count)
                productSprite = pd.sprites[imgIndex];
            if (imgIndex < pd.imagesUrl.Count)
                imageUrl = pd.imagesUrl[imgIndex];
        }

        items.Add(new FavoriteItem
        {
            productId = productId,
            productName = pd.product.name,
            slug = pd.product.slug,
            colorName = pd.product.colors[pd.selectedColorIndex > 0 ? pd.selectedColorIndex : 0].name,
            colorCode = colorCode,
            imageUrl = imageUrl,
            productImage = productSprite
        });

        SaveFavorites();
        UpdateUI();
        UpdateToggleIcon(productId);
    }

    public void Remove(string productId)
    {
        items.RemoveAll(i => i.productId == productId);
        SaveFavorites();
        RefreshUI();
        UpdateUI();
        UpdateToggleIcon(productId);
    }

    void RefreshUI()
    {
        if (favoritesItemsParent == null) return;

        foreach (Transform child in favoritesItemsParent)
            Destroy(child.gameObject);

        bool empty = items.Count == 0;
        if (noItemsInFavorites != null)
            noItemsInFavorites.SetActive(empty);
        if (empty) return;

        foreach (var fi in items)
        {
            GameObject row = Instantiate(favoriteItemPrefab, favoritesItemsParent);
            var ctrl = row.GetComponent<FavoriteItemUI>();
            if (ctrl != null) ctrl.Setup(fi, this);
        }
    }

    void UpdateUI()
    {
        if (countText != null)
        {
            countText.text = items.Count.ToString();
            countText.gameObject.SetActive(items.Count > 0);
        }
    }

    public void UpdateToggleIcon(string productId)
    {
        Sprite s = IsFavorited(productId) ? favoriteOnIcon : favoriteOffIcon;
        if (favoriteToggleIcon != null)
            favoriteToggleIcon.sprite = s;
        if (UIManagerAR.instance != null)
        {
            if (UIManagerAR.instance.favoriteIcon_SD != null)
                UIManagerAR.instance.favoriteIcon_SD.sprite = s;
            if (UIManagerAR.instance.favoriteIcon_LD != null)
                UIManagerAR.instance.favoriteIcon_LD.sprite = s;
        }
    }

    public void RefreshCurrentToggleIcon()
    {
        string pid = null;
        if (UIManagerAR.instance != null && UIManagerAR.instance.selectedModelDetails != null)
            pid = UIManagerAR.instance.selectedModelDetails.product.id;
        else if (CategoriesUI.Instance != null && CategoriesUI.Instance.selectedProduct != null)
            pid = CategoriesUI.Instance.selectedProduct.product.id;

        if (pid != null)
        {
            if (favoriteToggleIcon != null)
                favoriteToggleIcon.sprite = IsFavorited(pid) ? favoriteOnIcon : favoriteOffIcon;

            if (UIManagerAR.instance != null)
            {
                if (UIManagerAR.instance.favoriteIcon_SD != null)
                    UIManagerAR.instance.favoriteIcon_SD.sprite = IsFavorited(pid) ? favoriteOnIcon : favoriteOffIcon;
                if (UIManagerAR.instance.favoriteIcon_LD != null)
                    UIManagerAR.instance.favoriteIcon_LD.sprite = IsFavorited(pid) ? favoriteOnIcon : favoriteOffIcon;
            }
        }
    }

    public void OnExplore(FavoriteItem fi)
    {
        if ((UIManagerAR.instance != null && CategoryManager.Instance.isDebugMode) || (CategoriesUI.Instance != null && CategoriesUI.Instance.isDebug)) Debug.Log("Explore: " + fi.productName);
    }

    void OnExploreAll()
    {
        if ((UIManagerAR.instance != null && CategoryManager.Instance.isDebugMode) || (CategoriesUI.Instance != null && CategoriesUI.Instance.isDebug)) Debug.Log("Explore all favorites clicked");
    }

    void SaveFavorites()
    {
        var data = new FavoritesSaveData { items = this.items };
        PlayerPrefs.SetString("FavoritesData", JsonUtility.ToJson(data));
    }

    void LoadFavorites()
    {
        if (!PlayerPrefs.HasKey("FavoritesData")) return;
        var data = JsonUtility.FromJson<FavoritesSaveData>(PlayerPrefs.GetString("FavoritesData"));
        if (data != null && data.items != null)
            items = data.items;
    }

    [System.Serializable]
    public class FavoritesSaveData
    {
        public List<FavoriteItem> items;
    }
}
