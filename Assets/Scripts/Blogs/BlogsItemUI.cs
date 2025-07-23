using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class BlogsItemUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI blogNickName;
    public Image blogImage;
    public Button blogBtn;

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

            if (blogImage != null)
                blogImage.sprite = sprite;
        }
    }
}
