using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class BlogsItemUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI blogNickName;
    public RectTransform maskParent;
    public Image blogImage;
    public Button blogBtn;
    public GameObject loadingIcon;
    /// <summary>
    /// Initialize the blog UI with nickname and image URL
    /// </summary>
    /// <param name="nickName">Blog nickname</param>
    /// <param name="imageUrl">URL to the blog image</param>
    public void Initialize(string nickName, string imageUrl)
    {
        if (blogNickName != null)
            blogNickName.text = nickName;

        if (!string.IsNullOrEmpty(imageUrl))
            StartCoroutine(LoadImageFromURL(imageUrl));
    }

    private IEnumerator LoadImageFromURL(string url)
    {
        loadingIcon.SetActive(true);
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load image: " + request.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;

            // Convert texture to sprite
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            Sprite sprite = Sprite.Create(texture, rect, pivot);

            Debug.Log("width : " + sprite.rect.width + ", Height: " + sprite.rect.height);

            //maskParent.sizeDelta = new Vector2(maskParent.sizeDelta.x, sprite.rect.height);
            if (blogImage != null)
                blogImage.sprite = sprite;
            FitImageToContainer(blogImage);
        }
        loadingIcon.SetActive(false);
    }

    public void FitImageToContainer(Image image)
    {
        if (image.sprite == null) return;

        Sprite newSprite = image.sprite;
        RectTransform rectTransform = image.rectTransform;

        image.preserveAspect = true;

        float imgWidth = newSprite.rect.width;
        float imgHeight = newSprite.rect.height;
        float containerWidth = 1000f;  // Or parent.sizeDelta.x
        float containerHeight = 1000f; // Or parent.sizeDelta.y

        float scaleX = containerWidth / imgWidth;
        float scaleY = containerHeight / imgHeight;
        float multiplier = Mathf.Max(scaleX, scaleY);
        //float multiplier = scaleY;

        Debug.Log("image width: " + imgWidth + ", Height : " + imgHeight + ", multiplayer : " + multiplier, image.gameObject);
        rectTransform.sizeDelta = new Vector2(imgWidth * multiplier, imgHeight * multiplier);
        Debug.Log("rectTransform: " + rectTransform.sizeDelta.x + ", " + rectTransform.sizeDelta.y, image.gameObject);
    }
}
