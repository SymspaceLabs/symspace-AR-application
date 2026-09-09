using System.Collections;
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
    private readonly HashSet<string> selectedProductIds = new HashSet<string>();
    private readonly Dictionary<string, FavoriteItemUI> rowCache = new Dictionary<string, FavoriteItemUI>();
    private bool wasFavoritesPanelActive;
    private TextMeshProUGUI exploreLabel;

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

        RefreshExploreButton();

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

    void Update()
    {
        if (favoritesPanel == null) return;

        bool isActive = favoritesPanel.activeSelf;
        if (isActive == wasFavoritesPanelActive) return;
        wasFavoritesPanelActive = isActive;

        if (isActive)
        {
            BlurPanelManager.Cover();
            RefreshUI();
            RefreshCurrentToggleIcon();
        }
        else
        {
            BlurPanelManager.Uncover();
        }
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

    void RefreshExploreButton()
    {
        if (exploreBtn == null) return;

        bool empty = items.Count == 0;

        if (exploreLabel == null)
            exploreLabel = exploreBtn.GetComponentInChildren<TextMeshProUGUI>(true);
        if (exploreLabel != null)
            exploreLabel.text = empty ? "Explore" : "Add to Cart";

        if (noItemsInFavorites != null)
            noItemsInFavorites.SetActive(empty);

        exploreBtn.onClick.RemoveAllListeners();
        exploreBtn.onClick.AddListener(empty ? (UnityEngine.Events.UnityAction)OnExploreHome : AddSelectedToCart);
    }

    void OnExploreHome()
    {
        if (BottomBarController.Instance != null)
            BottomBarController.Instance.OnHomeClicked();
        else
        {
            ClosePanel();
            if (CartManager.Instance != null)
                CartManager.Instance.CloseCart();
        }
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

        bool wasFavorited = IsFavorited(pd.product.id);

        if (wasFavorited)
            Remove(pd.product.id);
        else
            Add(pd.product.id, pd);

        AnimateHeartToggle(!wasFavorited);
    }

    private Coroutine heartRoutine;

    private void AnimateHeartToggle(bool nowFavorited)
    {
        if (heartRoutine != null)
            StopCoroutine(heartRoutine);

        heartRoutine = StartCoroutine(AnimateHeartToggleRoutine(nowFavorited));
    }

    private System.Collections.IEnumerator AnimateHeartToggleRoutine(bool nowFavorited)
    {
        Image[] icons = CollectToggleIcons();
        if (icons.Length == 0)
            yield break;

        if (nowFavorited)
        {
            LightHaptic();

            foreach (Image icon in icons)
            {
                if (icon == null) continue;

                Transform t = icon.transform;
                Vector3 rest = t.localScale;

                yield return UIAnim.CoTween(
                    0.11f,
                    UIAnim.EaseInCubic,
                    (f) =>
                    {
                        if (t == null) return;
                        t.localScale = rest * Mathf.LerpUnclamped(1f, 1.35f, f);
                    });

                yield return UIAnim.CoTween(
                    0.18f,
                    UIAnim.EaseOutBack,
                    (f) =>
                    {
                        if (t == null) return;
                        t.localScale = rest * Mathf.LerpUnclamped(1.35f, 1f, f);
                    });

                if (t == null) break;
                t.localScale = rest;
            }
        }
        else
        {
            foreach (Image icon in icons)
            {
                if (icon == null) continue;

                Transform t = icon.transform;
                Vector3 rest = t.localScale;
                Color c = icon.color;

                yield return UIAnim.CoTween(
                    0.15f,
                    UIAnim.EaseInCubic,
                    (f) =>
                    {
                        if (icon == null || t == null) return;

                        t.localScale = rest * Mathf.LerpUnclamped(1f, 0.7f, f);
                        icon.color = new Color(c.r, c.g, c.b, Mathf.LerpUnclamped(1f, 0.2f, f));
                    });

                if (t == null) continue;
                t.localScale = rest;

                if (icon != null)
                    icon.color = new Color(c.r, c.g, c.b, 1f);
            }
        }
    }

    private Image[] CollectToggleIcons()
    {
        var icons = new List<Image>();

        if (favoriteToggleIcon != null)
            icons.Add(favoriteToggleIcon);

        if (UIManagerAR.instance != null)
        {
            if (UIManagerAR.instance.favoriteIcon_SD != null)
                icons.Add(UIManagerAR.instance.favoriteIcon_SD);
            if (UIManagerAR.instance.favoriteIcon_LD != null)
                icons.Add(UIManagerAR.instance.favoriteIcon_LD);
        }

        return icons.ToArray();
    }

    private static void LightHaptic()
    {
        if (Application.isMobilePlatform)
            Handheld.Vibrate();
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
        if (imgIndex < 0 && pd.product.images.Count > 0)
            imgIndex = 0;
        if (imgIndex >= 0)
        {
            if (imgIndex < pd.sprites.Count)
                productSprite = pd.sprites[imgIndex];
            if (imgIndex < pd.imagesUrl.Count)
                imageUrl = pd.imagesUrl[imgIndex];
        }

        int colorIdx = pd.selectedColorIndex > 0 ? pd.selectedColorIndex : 0;
        int tempSizeIdx = pd.selectedSizeIndex;
        bool isSizeSel = pd.isSizeSelected;
        if (pd.product.sizes.Count > 1)
        {
            if (!isSizeSel)
            {
                tempSizeIdx = 0;
            }
            else
            {
                tempSizeIdx = pd.selectedSizeIndex - 1;
                if (tempSizeIdx < 0) tempSizeIdx = 0;
            }
        }

        var matchedVariant = pd.product.variants.FirstOrDefault(v =>
            v.color.id == pd.product.colors[colorIdx].id &&
            (!isSizeSel || (v.size != null && v.size.id == pd.product.sizes[tempSizeIdx].id)));

        var newItem = new FavoriteItem
        {
            productId = productId,
            productName = pd.product.name,
            slug = pd.product.slug,
            colorName = pd.product.colors[colorIdx].name,
            colorCode = colorCode,
            imageUrl = imageUrl,
            productImage = productSprite,
            variantId = matchedVariant != null ? matchedVariant.id : null,
            sizeName = matchedVariant != null && matchedVariant.size != null ? matchedVariant.size.size : "",
            colorIndex = colorIdx,
            sizeIndex = tempSizeIdx,
            price = matchedVariant != null ? matchedVariant.price : 0f,
            salePriceFloat = matchedVariant != null ? matchedVariant.salePrice : 0f,
            maxStock = matchedVariant != null ? matchedVariant.stock : 0,
            hasVariant = matchedVariant != null
        };

        items.Add(newItem);

        SaveFavorites();
        RefreshUI();
        UpdateUI();
        UpdateToggleIcon(productId);
    }

    public void Remove(string productId)
    {
        items.RemoveAll(i => i.productId == productId);
        selectedProductIds.Remove(productId);
        SaveFavorites();
        RefreshUI();
        UpdateUI();
        UpdateToggleIcon(productId);
    }

    void RefreshUI()
    {
        if (favoritesItemsParent == null) return;

        RefreshExploreButton();

        if (items.Count == 0)
        {
            foreach (var kvp in rowCache)
            {
                if (kvp.Value != null && kvp.Value.gameObject != null)
                    Destroy(kvp.Value.gameObject);
            }
            rowCache.Clear();
            return;
        }

        var present = new HashSet<string>();
        foreach (var fi in items)
            if (!string.IsNullOrEmpty(fi.productId))
                present.Add(fi.productId);

        var toRemove = new List<string>();
        foreach (var key in rowCache.Keys)
            if (!present.Contains(key))
                toRemove.Add(key);

        foreach (var key in toRemove)
        {
            if (rowCache.TryGetValue(key, out var row) && row != null && row.gameObject != null)
                Destroy(row.gameObject);
            rowCache.Remove(key);
        }

        foreach (var fi in items)
        {
            if (string.IsNullOrEmpty(fi.productId)) continue;

            if (rowCache.TryGetValue(fi.productId, out var existing) && existing != null)
            {
                existing.Setup(fi, this);
            }
            else
            {
                GameObject row = Instantiate(favoriteItemPrefab, favoritesItemsParent);
                var ctrl = row.GetComponent<FavoriteItemUI>();
                if (ctrl != null)
                {
                    ctrl.Setup(fi, this);
                    rowCache[fi.productId] = ctrl;
                }
            }
        }
    }

    void UpdateUI()
    {
        int selected = selectedProductIds.Count;

        if (countText != null)
        {
            countText.text = selected > 0 ? $"{selected} Selected" : "";
            countText.gameObject.SetActive(selected > 0);
        }
    }

    public bool IsProductSelected(string productId)
    {
        return productId != null && selectedProductIds.Contains(productId);
    }

    public void ToggleSelected(string productId, bool selected)
    {
        if (string.IsNullOrEmpty(productId))
            return;

        if (selected)
            selectedProductIds.Add(productId);
        else
            selectedProductIds.Remove(productId);

        UpdateUI();
    }

    public void ClearSelections()
    {
        selectedProductIds.Clear();
        UpdateUI();
        RefreshRowMarks();
    }

    void RefreshRowMarks()
    {
        if (favoritesItemsParent == null) return;

        foreach (Transform child in favoritesItemsParent)
        {
            var ctrl = child.GetComponent<FavoriteItemUI>();
            if (ctrl != null) ctrl.RefreshSelectedState();
        }
    }

    public void AddSelectedToCart()
    {
        if (selectedProductIds.Count == 0 || CartManager.Instance == null)
            return;

        List<FavoriteItem> selected = items
            .Where(i => selectedProductIds.Contains(i.productId))
            .ToList();

        foreach (FavoriteItem fi in selected)
        {
            if (!fi.hasVariant || string.IsNullOrEmpty(fi.variantId))
                continue;

            var cartItem = new CartItem
            {
                productId = fi.productId,
                variantId = fi.variantId,
                productName = fi.productName,
                colorName = fi.colorName,
                colorCode = fi.colorCode,
                sizeName = fi.sizeName,
                colorIndex = fi.colorIndex,
                sizeIndex = fi.sizeIndex,
                quantity = 1,
                price = fi.price,
                salePrice = fi.salePriceFloat,
                maxStock = fi.maxStock,
                productImage = fi.productImage,
                imageUrl = fi.imageUrl
            };

            CartManager.Instance.AddItem(cartItem);
        }

        foreach (FavoriteItem fi in selected)
        {
            items.RemoveAll(i => i.productId == fi.productId);
            selectedProductIds.Remove(fi.productId);
        }

        SaveFavorites();
        RefreshUI();
        UpdateUI();
        RefreshCurrentToggleIcon();
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
