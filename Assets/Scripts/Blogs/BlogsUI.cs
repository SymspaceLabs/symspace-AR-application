using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.Networking;

public class BlogsUI : MonoBehaviour
{
    #region Parameters
    [Header("UI Elements")]
    public Transform contentParent;
    public GameObject blogItemPrefab;
    public Button refreshButton;
    public TextMeshProUGUI statusText;

    private string blogsURL = "/blogs";
    List<BlogsData> currentBlogsData;

    public GameObject blogPage;

    public Image blogImg;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI authorText;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI contentText;

    public GameObject loadingPanel;
    #endregion

    private void OnEnable()
    {
        currentBlogsData = new List<BlogsData>();
        statusText.gameObject.SetActive(false);
        LoadBlogs();
    }

    void Start()
    {
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(LoadBlogs);
        }
    }

    #region API Call
    public void LoadBlogs()
    {
        loadingPanel.SetActive(true);
        ClearBlogs();

        StartCoroutine(AuthAPI.PostRequest(blogsURL, "", // Empty string for no body
            (response) =>
            {
                //Debug.Log("Blogs loaded: " + response);

                // Parse the response
                string fixedJson = "{\"blogs\":" + response + "}";
                BlogsResponse responseData = JsonUtility.FromJson<BlogsResponse>(fixedJson);
                Debug.Log("Blogs loaded: " + responseData);

                if (responseData.blogs != null && responseData.blogs.Count > 0)
                {
                    if(currentBlogsData != null)
                        currentBlogsData.Clear();

                    foreach (BlogsData blogsData in responseData.blogs)
                        currentBlogsData.Add(blogsData);

                    PopulateBlogs(currentBlogsData);
                    statusText.gameObject.SetActive(false);
                }
                else
                {
                    ShowStatus("No categories found", false);
                }

                loadingPanel.SetActive(false);
            },
            (error) =>
            {
                Debug.LogError("Failed to load categories: " + error);
                ShowStatus("Failed to load categories", true);
                loadingPanel.SetActive(false);
            }, "GET"));
    }
    #endregion

    #region UI Methods
    private void PopulateBlogs(List<BlogsData> blogs)
    {
        foreach (BlogsData blog in blogs)
        {
            GameObject item = Instantiate(blogItemPrefab, contentParent);
            BlogsItemUI itemUI = item.GetComponent<BlogsItemUI>();

            if (itemUI != null)
            {
                itemUI.Initialize(blog.nickname, blog.image);

                itemUI.blogBtn.onClick.AddListener(() =>
                {
                    blogPage.SetActive(true);
                    titleText.text = blog.title;
                    authorText.text = blog.author;
                    dateText.text = blog.createdAt;
                    contentText.text = blog.content;
                    StartCoroutine(LoadImageFromURL(blog.image, blogImg));
                });
            }
            else
            {
                // Fallback if the prefab doesn't have the CategoryItemUI component
                TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = $"{blog.nickname} (ID: {blog.image})";
                }
            }
        }
    }

    private IEnumerator LoadImageFromURL(string url, Image img)
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
            img.sprite = sprite;
        }
    }

    private void ClearBlogs()
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
    private class BlogsResponse
    {
        public List<BlogsData> blogs;
    }

    [System.Serializable]
    private class BlogsData
    {
        public string id;
        public string title;
        public string nickname;
        public string content;
        public string image;
        public int newsType;
        public string author;
        public string slug;
        public string handle_url;
        public string handle_url_title;
        public string article_source_url;
        public string author_url;
        public string tag;
        public string createdAt;
        public string updatedAt;
    }
    #endregion
}