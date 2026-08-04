using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FavoriteItemUI : MonoBehaviour
{
    public TextMeshProUGUI productNameText;
    public TextMeshProUGUI originalPriceText;
    public TextMeshProUGUI salePriceText;
    public TextMeshProUGUI colorText;
    public Image productImage;
    public GameObject loadingOverlay;
    public Button removeBtn;

    public void Setup(FavoriteItem fi, FavoritesManager fm)
    {
        if (productNameText != null) productNameText.text = fi.productName;
        if (colorText != null) colorText.text = fi.colorName;

        if (productImage != null && fi.productImage != null)
            productImage.sprite = fi.productImage;

        if(originalPriceText != null)
            originalPriceText.text = fi.originalPrice > 0 ? $"${fi.originalPrice:F2}" : "";

        if (salePriceText != null)
            salePriceText.text = fi.salePrice > 0 ? $"${fi.salePrice:F2}" : "";

        if (loadingOverlay != null)
            loadingOverlay.SetActive(productImage == null || productImage.sprite == null);

        if (!string.IsNullOrEmpty(fi.imageUrl) && (productImage == null || productImage.sprite == null))
            StartCoroutine(DownloadImage(fi.imageUrl));

        if (removeBtn != null)
            removeBtn.onClick.AddListener(() => fm.Remove(fi.productId));
    }

    IEnumerator DownloadImage(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            if (productImage != null)
            {
                productImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                if (loadingOverlay != null)
                    loadingOverlay.SetActive(false);
            }
        }
        else
        {
            if ((UIManagerAR.instance != null && CategoryManager.Instance.isDebugMode) || (CategoriesUI.Instance != null && CategoriesUI.Instance.isDebug)) Debug.LogWarning("Favorite image load failed: " + url);
            if (loadingOverlay != null)
                loadingOverlay.SetActive(false);
        }
    }
}
