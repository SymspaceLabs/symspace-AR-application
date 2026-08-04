using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BlogsUI : MonoBehaviour
{
    #region Parameters
    [Header("UI Elements")]
    public Transform contentParent;
    public Transform blurContentTransform;
    public GameObject blurBlogsPanel;
    public GameObject blogItemPrefab;
    public Button refreshButton;
    public TextMeshProUGUI statusText;

    [Space(10)]
    private string blogsURL = "/blogs";
    List<BlogsData> currentBlogsData;
    private BlogsResponse cachedResponse;

    public GameObject blogDetailPage;
    public GameObject blurBlogDetailPage;

    [Space(10)]
    [Header("Blog Detail UI Elements")]
    public Image blogImg;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI authorText;
    public Button originalPostBtn;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI contentText;
    public GameObject detailLoadingIcon;
    public Sprite blurImage;

    [Space(10)]
    [Header("Blur Blog Detail UI Elements")]
    public Image blurBlogImg;
    public TextMeshProUGUI blurTitleText;
    public TextMeshProUGUI blurAuthorText;
    public Button blurOriginalPostBtn;
    public TextMeshProUGUI blurDateText;
    public TextMeshProUGUI blurContentText;

    [Space(10)]
    public TMP_InputField searchText;
    public List<GameObject> bottomBtns;

    public Image homeBtn;
    public Image shopBtn;

    public Sprite homeBlack;
    public Sprite homeBlue;

    public Sprite shopBlack;
    public Sprite shopBlue;

    public GameObject loadingPanel;

    private bool wasBlurBlogsPanelActive;
    private bool wasBlurBlogDetailActive;
    #endregion

    public void HideBlurPanels()
    {
        wasBlurBlogsPanelActive = blurBlogsPanel != null && blurBlogsPanel.activeSelf;
        wasBlurBlogDetailActive = blurBlogDetailPage != null && blurBlogDetailPage.activeSelf;
        if (blurBlogsPanel != null) blurBlogsPanel.SetActive(false);
        if (blurBlogDetailPage != null) blurBlogDetailPage.SetActive(false);
    }

    public void RestoreBlurPanels()
    {
        if (blurBlogsPanel != null) blurBlogsPanel.SetActive(wasBlurBlogsPanelActive);
        if (blurBlogDetailPage != null) blurBlogDetailPage.SetActive(wasBlurBlogDetailActive);
    }

    private void OnEnable()
    {
        currentBlogsData = new List<BlogsData>();
        statusText.gameObject.SetActive(false);

        LoadBlogs();

        homeBtn.sprite = homeBlue;
        shopBtn.sprite = shopBlack;

        homeBtn.transform.GetChild(0).gameObject.SetActive(true);
        shopBtn.transform.GetChild(0).gameObject.SetActive(false);

        searchText.text = ""; // Clear the search text when the UI is enabled

        searchText.onValueChanged.AddListener(OnSearchValueChanged);
    }

    void Start()
    {
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(LoadBlogs);
        }

        foreach (GameObject obj in bottomBtns)
            obj.SetActive(true);
        GetComponentInParent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
    }

    public void OnSearchValueChanged(string value)
    {
        BlogsResponse filteredResponse = SearchBlogs(value);
        ClearBlogs();
        PopulateBlogs(filteredResponse.blogs);
    }

    public BlogsResponse SearchBlogs(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return cachedResponse;

        string search = query.Trim().ToLower();

        var filteredBlogs = cachedResponse.blogs.Where(blog =>
        {
            string search = Regex.Replace(query.ToLower(), @"\s+", " ").Trim();

            string searchableText = Regex.Replace(
                $"{blog.title} {blog.author} {blog.nickname}".ToLower(),
                @"\s+",
                " "
            ).Trim();

            return searchableText.Contains(search);
        }).ToList();

        return new BlogsResponse
        {
            blogs = filteredBlogs
        };
    }


    #region API Call
    public void LoadBlogs()
    {
        loadingPanel.SetActive(true);
        ClearBlogs();

        StartCoroutine(AuthAPI.PostRequest(blogsURL, "", // Empty string for no body
            (response) =>
            {
                //if (CategoriesUI.Instance.isDebug) Debug.Log("Blogs loaded: " + response);

                // Parse the response
                string fixedJson = "{\"blogs\":" + response + "}";
                BlogsResponse responseData = JsonUtility.FromJson<BlogsResponse>(fixedJson);
                if (CategoriesUI.Instance.isDebug) Debug.Log("Blogs loaded: " + responseData);

                cachedResponse = responseData; // Cache the response

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
                FirebaseAuthManager.ErrorResponse response = JsonUtility.FromJson<FirebaseAuthManager.ErrorResponse>(error);
                if (CategoriesUI.Instance.isDebug) Debug.LogError("Failed to load categories: " + error);
                if (CategoriesUI.Instance.isDebug) Debug.LogError("Message: " + response.message);
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

            GameObject blurItem = Instantiate(blogItemPrefab, blurContentTransform);
            BlogsItemUI blurItemUI = blurItem.GetComponent<BlogsItemUI>();
            if (blurItemUI != null)
            {
                blurItemUI.Initialize(blog.nickname, blog.image);
                //blurItemUI.blogImage.raycastTarget = false; // Disable raycast for the blur item
                blurItemUI.blogImage.color = new Color(1f, 1f, 1f, 0.01f); // Set the alpha to 0.01 for blur effect

                blurItemUI.transform.Find("Spinner 6").gameObject.SetActive(false); // Disable the loading spinner for the blur item
                //blurItemUI.blogBtn.interactable = false; // Disable the button for the blur item
                blurItemUI.blogBtn.onClick.AddListener(() =>
                {
                    //authorText.GetComponent<Button>().onClick.RemoveAllListeners();
                    //originalPostBtn.onClick.RemoveAllListeners();
                    if (CategoriesUI.Instance.isDebug) Debug.Log("Button Pressed");
                    blogDetailPage.SetActive(true);
                    blurBlogsPanel.SetActive(true);
                    titleText.text = blog.title;
                    authorText.text = blog.author;
                    //authorText.GetComponent<Button>().onClick.AddListener(() =>
                    //{
                    //    Application.OpenURL(blog.author_url);
                    //});
                    //originalPostBtn.onClick.AddListener(() =>
                    //{
                    //    Application.OpenURL(blog.article_source_url);
                    //});
                    dateText.text = ConvertToReadableDate(blog.createdAt);
                    contentText.text = blog.content;
                    StartCoroutine(LoadImageFromURL(blog.image, blogImg));

                    blurAuthorText.GetComponent<Button>().onClick.RemoveAllListeners();
                    blurOriginalPostBtn.onClick.RemoveAllListeners();
                    blurBlogDetailPage.SetActive(true);
                    blurBlogsPanel.SetActive(false);
                    blurTitleText.text = blog.title;
                    blurAuthorText.text = blog.author;
                    blurAuthorText.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        Application.OpenURL(blog.author_url);
                    });
                    blurOriginalPostBtn.onClick.AddListener(() =>
                    {
                        Application.OpenURL(blog.article_source_url);
                    });
                    blurDateText.text = ConvertToReadableDate(blog.createdAt);
                    blurContentText.text = blog.content;
                    StartCoroutine(LoadImageFromURL(blog.image, blurBlogImg));
                });
            }
            if (itemUI != null)
            {
                itemUI.Initialize("", blog.image);
                itemUI.transform.Find("Title BG").GetComponent<Image>().enabled = false; // Disable the title background image

                if (CategoriesUI.Instance.isDebug) Debug.Log("Before button Add click " + itemUI.blogBtn.name);
                
                
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
        img.sprite = blurImage;
        img.preserveAspect = false;
        //loadingPanel.SetActive(true);
        detailLoadingIcon.SetActive(true);
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            if (CategoriesUI.Instance.isDebug) Debug.LogError("Failed to load image: " + request.error + ", Bytes");
            if (CategoriesUI.Instance.isDebug) Debug.LogError("Message: " + request.downloadHandler.text);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;

            // Convert texture to sprite
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            Sprite sprite = Sprite.Create(texture, rect, pivot);
            img.sprite = sprite;
            FitImageToContainer(img);
        }
        detailLoadingIcon.SetActive(false);
        //loadingPanel.SetActive(false);
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

        rectTransform.sizeDelta = new Vector2(imgWidth * multiplier, imgHeight * multiplier);
    }

    private void ClearBlogs()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        foreach(Transform child in blurContentTransform)
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

    public static string ConvertToReadableDate(string isoDate)
    {
        // Parse the ISO 8601 date string
        DateTime date = DateTime.Parse(isoDate, null, DateTimeStyles.RoundtripKind);

        // Format it as "22 July 2025"
        string formattedDate = date.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture);
        return formattedDate;
    }

    public void SignOut()
    {
        PlayerPrefs.SetInt("RememberMe", 0);
        PlayerPrefs.SetInt("OnBoarding", 0);
        PlayerPrefs.DeleteKey("id");
        SceneManager.LoadScene(SceneNames.Home);
    }

    private void OnDisable()
    {
        //homeBtn.sprite = homeBlack;
        //shopBtn.sprite = shopBlue;

        //homeBtn.transform.GetChild(0).gameObject.SetActive(false);
        //shopBtn.transform.GetChild(0).gameObject.SetActive(true);

        searchText.onValueChanged.RemoveListener(OnSearchValueChanged);
    }
    #endregion

    #region Structure Classes
    [System.Serializable]
    public class BlogsResponse
    {
        public List<BlogsData> blogs;
    }

    [System.Serializable]
    public class BlogsData
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