using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CartItemUI : MonoBehaviour
{
    public TextMeshProUGUI productNameText;
    public TextMeshProUGUI colorText;
    public TextMeshProUGUI sizeText;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI salePriceText;
    public Image productImage;
    public GameObject loadingOverlay;
    public Image colorSwatch;
    public Button minusBtn;
    public Button plusBtn;
    public Button removeBtn;

    private CartItem cartItem;
    private CartManager cartManager;
    public string pendingImageUrl;

    public void Setup(CartItem ci, CartManager cm)
    {
        cartItem = ci;
        cartManager = cm;

        if (productNameText != null) productNameText.text = ci.productName;
        if (colorText != null) colorText.text = ci.colorName;
        if (sizeText != null) sizeText.text = ci.sizeName;
        if (quantityText != null) quantityText.text = ci.quantity.ToString();

        if (salePriceText != null)
        {
            if (ci.salePrice > 0 && ci.salePrice < ci.price)
            {
                salePriceText.text = "<s>$" + ci.price.ToString("F2") + "</s>";
                if (priceText != null) priceText.text = "$" + ci.salePrice.ToString("F2");
            }
            else
            {
                salePriceText.gameObject.SetActive(false);
                if (priceText != null) priceText.text = "$" + ci.price.ToString("F2");
            }
        }
        else if (priceText != null)
        {
            priceText.text = "$" + ci.EffectivePrice.ToString("F2");
        }

        if (colorSwatch != null && !string.IsNullOrEmpty(ci.colorCode))
        {
            Color col;
            if (ColorUtility.TryParseHtmlString(ci.colorCode, out col))
                colorSwatch.color = col;
        }

        //if (productImage != null && ci.productImage != null)
        //    productImage.sprite = ci.productImage;
        if ((UIManagerAR.instance != null && CategoryManager.Instance.isDebugMode) || (CategoriesUI.Instance != null && CategoriesUI.Instance.isDebug)) Debug.Log("CartItemUI Setup called for: " + ci.productName + ", imageUrl: " + ci.imageUrl);
        pendingImageUrl = ci.imageUrl;

        if (loadingOverlay != null)
            loadingOverlay.SetActive(!string.IsNullOrEmpty(pendingImageUrl));


        if (minusBtn != null)
            minusBtn.onClick.AddListener(() => cartManager.ChangeQuantity(ci.variantId, -1));
        if (plusBtn != null)
            plusBtn.onClick.AddListener(() => cartManager.ChangeQuantity(ci.variantId, 1));
        if (removeBtn != null)
            removeBtn.onClick.AddListener(() => cartManager.RemoveItem(ci.variantId));
    }

    void OnEnable()
    {
        if ((UIManagerAR.instance != null && CategoryManager.Instance.isDebugMode) || (CategoriesUI.Instance != null && CategoriesUI.Instance.isDebug)) Debug.Log("CartItemUI OnEnable called for: " + gameObject.name);
        if (productImage != null && productImage.sprite == null)
            StartCoroutine(DownloadImage());
    }

    public void RefreshQuantity(int newQty)
    {
        if (quantityText != null)
            quantityText.text = newQty.ToString();
    }

    IEnumerator DownloadImage()
    {
        yield return new WaitForSeconds(1f);
        if ((UIManagerAR.instance != null && CategoryManager.Instance.isDebugMode) || (CategoriesUI.Instance != null && CategoriesUI.Instance.isDebug)) Debug.Log("Downloading cart image: " + pendingImageUrl);
        if(string.IsNullOrEmpty(pendingImageUrl))
        {
            if ((UIManagerAR.instance != null && CategoryManager.Instance.isDebugMode) || (CategoriesUI.Instance != null && CategoriesUI.Instance.isDebug)) Debug.LogWarning("Cart image URL is null or empty");
            if (loadingOverlay != null)
                loadingOverlay.SetActive(false);
            yield break;
        }

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(pendingImageUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            if ((UIManagerAR.instance != null && CategoryManager.Instance.isDebugMode) || (CategoriesUI.Instance != null && CategoriesUI.Instance.isDebug)) Debug.Log("Cart image downloaded successfully");
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            if (productImage != null)
            {
                productImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                if ((UIManagerAR.instance != null && CategoryManager.Instance.isDebugMode) || (CategoriesUI.Instance != null && CategoriesUI.Instance.isDebug)) Debug.Log("Cart image set successfully");
                if (loadingOverlay != null)
                    loadingOverlay.SetActive(false);
            }
        }
        else
        {
            if ((UIManagerAR.instance != null && CategoryManager.Instance.isDebugMode) || (CategoriesUI.Instance != null && CategoriesUI.Instance.isDebug)) Debug.LogError("Cart image load failed: " + pendingImageUrl);
            if (loadingOverlay != null)
                loadingOverlay.SetActive(false);
        }
    }
}
