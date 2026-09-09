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
    public Button clickBtn;
    public GameObject selectedMark;

    private FavoriteItem currentItem;
    private FavoritesManager currentManager;
    private bool isSelected;
    private bool listenersWired;

    public void Setup(FavoriteItem fi, FavoritesManager fm)
    {
        currentItem = fi;
        currentManager = fm;

        if (selectedMark == null)
        {
            Transform mark = transform.Find("Selected Mark");
            if (mark != null) selectedMark = mark.gameObject;
        }

        if (clickBtn == null)
            clickBtn = GetComponent<Button>();

        isSelected = fm != null && fm.IsProductSelected(fi.productId);
        SetSelectedVisual(isSelected);

        if (!listenersWired)
        {
            if (clickBtn != null)
                clickBtn.onClick.AddListener(ToggleSelection);
            if (removeBtn != null)
                removeBtn.onClick.AddListener(() => fm.Remove(fi.productId));
            listenersWired = true;
        }

        if (productNameText != null) productNameText.text = fi.productName;
        if (colorText != null) colorText.text = fi.colorName;

        if (productImage != null)
        {
            productImage.enabled = true;
            if (fi.productImage != null)
                productImage.sprite = fi.productImage;
        }

        if (originalPriceText != null && fi.price > 0)
            originalPriceText.text = "$" + fi.price.ToString("F2");

        if (salePriceText != null)
        {
            if (fi.salePriceFloat > 0 && fi.salePriceFloat < fi.price)
            {
                if (originalPriceText != null)
                {
                    originalPriceText.text = "<s>" + originalPriceText.text + "</s>";
                    originalPriceText.color = new Color(0.7f, 0.7f, 0.7f);
                }
                salePriceText.text = "$" + fi.salePriceFloat.ToString("F2");
            }
            else
            {
                salePriceText.gameObject.SetActive(false);
            }
        }

        if (loadingOverlay != null)
            loadingOverlay.SetActive(productImage == null || productImage.sprite == null);

        if (!string.IsNullOrEmpty(fi.imageUrl) && (productImage == null || productImage.sprite == null))
            StartCoroutine(DownloadImage(fi.imageUrl));

        if (removeBtn != null)
            removeBtn.onClick.AddListener(() => fm.Remove(fi.productId));
    }

    void OnEnable()
    {
        if (productImage != null && productImage.sprite == null &&
            currentItem != null && !string.IsNullOrEmpty(currentItem.imageUrl))
        {
            StartCoroutine(DownloadImage(currentItem.imageUrl));
        }
    }

    void ToggleSelection()
    {
        isSelected = !isSelected;
        SetSelectedVisual(isSelected);

        if (currentManager != null && currentItem != null)
            currentManager.ToggleSelected(currentItem.productId, isSelected);
    }

    public void RefreshSelectedState()
    {
        if (currentManager == null || currentItem == null)
            return;

        isSelected = currentManager.IsProductSelected(currentItem.productId);
        SetSelectedVisual(isSelected);
    }

    void SetSelectedVisual(bool selected)
    {
        if (selectedMark != null)
            selectedMark.SetActive(selected);
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
