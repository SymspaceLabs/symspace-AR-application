using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CartManager : MonoBehaviour
{
    #region Static

    public static CartManager Instance;

    public static event Action OnCartOpened;
    public static event Action OnCartClosed;

    #endregion

    #region Inspector

    [Header("Cart")]
    public GameObject cartPanel;
    public Transform cartItemsParent;
    public GameObject cartItemPrefab;

    [Space]
    [Header("Pricing")]
    public GameObject checkoutSummary;
    public GameObject subTotalLabel;
    public TextMeshProUGUI subTotalPrice;
    public TextMeshProUGUI shippingPriceText;
    public TextMeshProUGUI totalPriceText;
    public Button proceedToCheckoutBtn;
    public Button applyPayBtn;
    public Button shopBtn;

    [Space]
    [Header("Empty Cart")]
    public GameObject noItemsInCart;
    public Button addToCartBtn_SD;
    public Button addToCartBtn_LD;

    [Space]
    [Header("Navigation")]
    public Button cartOpenBtn;
    public Button cartCloseBtn;

    [Space]
    [Header("Badge")]
    public GameObject cartBadgeParent;
    public TextMeshProUGUI cartBadge;

    #endregion

    #region Runtime

    private List<CartItem> items = new();
    private Dictionary<string, CartItemUI> rowMap = new();

    #endregion


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        LoadCart();

        if (cartOpenBtn == null && UIManagerAR.instance != null)
            cartOpenBtn = UIManagerAR.instance.cartOpenBtn;

        if (cartOpenBtn != null)
            cartOpenBtn.onClick.AddListener(OpenCart);

        if (cartCloseBtn != null)
            cartCloseBtn.onClick.AddListener(CloseCart);

        if (UIManagerAR.instance != null)
        {
            if (UIManagerAR.instance.addToCartBtn_SD != null)
                UIManagerAR.instance.addToCartBtn_SD.onClick.AddListener(AddCurrentToCart);
            if (UIManagerAR.instance.addToCartBtn_LD != null)
                UIManagerAR.instance.addToCartBtn_LD.onClick.AddListener(AddCurrentToCart);
        }
        else
        {
            if (addToCartBtn_SD != null)
                addToCartBtn_SD.onClick.AddListener(AddCurrentToCart);
            if (addToCartBtn_LD != null)
                addToCartBtn_LD.onClick.AddListener(AddCurrentToCart);
        }

        if (proceedToCheckoutBtn != null)
            proceedToCheckoutBtn.onClick.AddListener(OnProceedToCheckout);

        if (cartPanel != null)
            cartPanel.SetActive(false);

        UpdateTotals();
        RefreshCartUI();
    }

    void OpenCart()
    {
        if (cartPanel == null || cartPanel.activeSelf) return;

        cartPanel.SetActive(true);
        if (UIManagerAR.instance != null)
            UIManagerAR.instance.ShowShop();
        //RefreshCartUI();
        HideBlurPanels();
        if (OnCartOpened != null) OnCartOpened();
    }

    public void CloseCart()
    {
        if (cartPanel == null || !cartPanel.activeSelf) return;
        cartPanel.SetActive(false);
        RestoreBlurPanels();
        if (OnCartClosed != null) OnCartClosed();
    }

    void HideBlurPanels()
    {
        var blogs = FindFirstObjectByType<BlogsUI>();
        if (blogs != null) blogs.HideBlurPanels();
    }

    void RestoreBlurPanels()
    {
        var blogs = FindFirstObjectByType<BlogsUI>();
        if (blogs != null) blogs.RestoreBlurPanels();
    }

    public void AddCurrentToCart()
    {
        ProductDetails pd = null;
        int colorIdx = 0;
        int sizeIdx = 0;
        bool isSizeSel = false;

        if (UIManagerAR.instance != null && UIManagerAR.instance.selectedModelDetails != null)
        {
            pd = UIManagerAR.instance.selectedModelDetails;
            colorIdx = pd.selectedColorIndex;
            sizeIdx = UIManagerAR.instance.selectedSizeIndex;
            isSizeSel = pd.isSizeSelected;
        }
        else if (CategoriesUI.Instance != null && CategoriesUI.Instance.selectedProduct != null)
        {
            pd = CategoriesUI.Instance.selectedProduct;
            colorIdx = CategoriesUI.Instance.selectedMatIndex;
            sizeIdx = CategoriesUI.Instance.selectedSizeIndex;
            isSizeSel = CategoriesUI.Instance.isSizeSelected;
        }

        if (pd == null || pd.product == null) return;

        int tempSizeIdx = sizeIdx;
        if (pd.product.sizes.Count > 1)
        {
            if (!isSizeSel) return;
            tempSizeIdx = sizeIdx - 1;
        }

        var matchedVariant = pd.product.variants.FirstOrDefault(v =>
            v.color.id == pd.product.colors[colorIdx].id &&
            (!isSizeSel || (v.size != null && v.size.id == pd.product.sizes[tempSizeIdx].id)));

        if (matchedVariant == null) return;

        int addQty = 1;
        if (UIManagerAR.instance != null && UIManagerAR.instance.selectedModelDetails != null)
        {
            if (UIManagerAR.instance.stocksSelected != null)
                int.TryParse(UIManagerAR.instance.stocksSelected.text, out addQty);
        }
        else if (CategoriesUI.Instance != null && CategoriesUI.Instance.selectedProduct != null)
        {
            if (CategoriesUI.Instance.stocksSelected != null)
                int.TryParse(CategoriesUI.Instance.stocksSelected.text, out addQty);
        }
        if (addQty < 1) addQty = 1;

        int stock = matchedVariant.stock;
        if (stock < 1) return;

        string variantId = matchedVariant.id;
        var existing = items.FirstOrDefault(i => i.variantId == variantId);

        int currentQtyInCart = existing?.quantity ?? 0;
        if (currentQtyInCart >= stock) return;

        int newQty = Mathf.Min(currentQtyInCart + addQty, stock);

        string colorCode = pd.product.colors[colorIdx].code;
        Sprite productSprite = null;
        string imageUrl = "";
        int imgIndex = pd.product.images.FindIndex(img => img.colorCode == colorCode);
        if (imgIndex >= 0)
        {
            if (imgIndex < pd.sprites.Count)
                productSprite = pd.sprites[imgIndex];
            if (imgIndex < pd.imagesUrl.Count)
                imageUrl = pd.imagesUrl[imgIndex];
        }

        if (existing != null)
        {
            existing.quantity = newQty;
            if (rowMap.TryGetValue(variantId, out var row) && row != null)
                row.RefreshQuantity(existing.quantity);
        }
        else
        {
            var newItem = new CartItem
            {
                productId = pd.product.id,
                variantId = variantId,
                productName = pd.product.name,
                colorName = pd.product.colors[colorIdx].name,
                colorCode = colorCode,
                sizeName = matchedVariant.size?.size ?? "",
                colorIndex = colorIdx,
                sizeIndex = tempSizeIdx,
                quantity = newQty,
                price = matchedVariant.price,
                salePrice = matchedVariant.salePrice,
                maxStock = matchedVariant.stock,
                productImage = productSprite,
                imageUrl = imageUrl
            };
            items.Add(newItem);
            if (cartItemsParent != null)
            {
                GameObject row = Instantiate(cartItemPrefab, cartItemsParent);
                var ctrl = row.GetComponent<CartItemUI>();
                if (ctrl != null) ctrl.Setup(newItem, this);
                rowMap[variantId] = ctrl;
            }
        }

        noItemsInCart.SetActive(false);
        shopBtn.gameObject.SetActive(false);
        checkoutSummary.SetActive(true);

        //RefreshCartUI();
        UpdateTotals();
        SaveCart();
    }

    public void ChangeQuantity(string variantId, int delta)
    {
        var item = items.FirstOrDefault(i => i.variantId == variantId);
        if (item == null) return;

        int newQty = item.quantity + delta;
        if (newQty < 1)
            newQty = 1;
        else if (newQty > item.maxStock)
            return;

        item.quantity = newQty;
        if (rowMap.TryGetValue(variantId, out var row) && row != null)
            row.RefreshQuantity(item.quantity);

        UpdateTotals();
        SaveCart();
    }

    public void RemoveItem(string variantId)
    {
        items.RemoveAll(i => i.variantId == variantId);
        if (rowMap.TryGetValue(variantId, out var row) && row != null && row.gameObject != null)
            Destroy(row.gameObject);
        rowMap.Remove(variantId);

        if (items.Count == 0)
            RefreshCartUI();

        UpdateTotals();
        SaveCart();
    }

    void RefreshCartUI()
    {
        rowMap.Clear();
        if (cartItemsParent == null) return;

        foreach (Transform child in cartItemsParent)
            Destroy(child.gameObject);

        if (items.Count == 0)
        {
            if (checkoutSummary != null)
                checkoutSummary.SetActive(false);
            if(noItemsInCart != null)
                noItemsInCart.SetActive(true);
            if(shopBtn != null)
                shopBtn.gameObject.SetActive(true);
            return;
        }
        else
        {
            if (noItemsInCart != null)
                noItemsInCart.SetActive(false);
            if(shopBtn != null)
               shopBtn.gameObject.SetActive(false);
            if(checkoutSummary != null)
                checkoutSummary.SetActive(true);
        }

        foreach (var ci in items)
        {
            GameObject row = Instantiate(cartItemPrefab, cartItemsParent);
            var ctrl = row.GetComponent<CartItemUI>();
            if (ctrl != null) ctrl.Setup(ci, this);
            rowMap[ci.variantId] = ctrl;
        }

        UpdateTotals();
    }

    void UpdateTotals()
    {
        if(subTotalPrice != null)
            subTotalPrice.text = "$" + items.Sum(i => i.LineTotal).ToString("F2");

        if(shippingPriceText != null)
            shippingPriceText.text = "$" + (items.Count > 0 ? 5.00f : 0.00f).ToString("F2");

        if (totalPriceText != null)
            totalPriceText.text = "$" + (subTotalPrice != null ? float.Parse(subTotalPrice.text.Substring(1)) + 
                (shippingPriceText != null ? float.Parse(shippingPriceText.text.Substring(1)) : 0) : 0).ToString("F2");

        if (cartBadge != null)
            cartBadge.text = items.Sum(i => i.quantity).ToString();
        if (cartBadgeParent != null)
            cartBadgeParent.SetActive(items.Count > 0);
    }

    void SaveCart()
    {
        var data = new CartSaveData { items = this.items };
        PlayerPrefs.SetString("CartData", JsonUtility.ToJson(data));
    }

    void LoadCart()
    {
        if (!PlayerPrefs.HasKey("CartData")) return;
        var data = JsonUtility.FromJson<CartSaveData>(PlayerPrefs.GetString("CartData"));
        if (data != null && data.items != null)
            items = data.items;
    }

    [Serializable]
    public class CartSaveData
    {
        public List<CartItem> items;
    }

    void OnProceedToCheckout()
    {
        if ((UIManagerAR.instance != null && CategoryManager.Instance.isDebugMode) || (CategoriesUI.Instance != null && CategoriesUI.Instance.isDebug)) Debug.Log("Proceed to checkout clicked");
    }
}
