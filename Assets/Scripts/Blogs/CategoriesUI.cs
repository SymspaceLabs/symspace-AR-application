using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityGLTF;

public class CategoriesUI : MonoBehaviour
{
    public static CategoriesUI Instance;

    private string getProductsURL = "/products?subcategory";
    private string getProductBySlug = "/products/slug/";

    public enum UIScreenType_Shop
    {
        None,
        itemDetail
    }

    public List<UIScreen> screens;
    public GameObject whiteBG;

    #region Parameters
    [Header("UI Elements")]
    public Transform contentParent;
    public GameObject productPrefab;
    public Button refreshButton;
    public TextMeshProUGUI statusText;
    #endregion

    public GameObject supplyPanel;
    public GameObject shopPanel;
    public GameObject itemDetailPanel;
    public GameObject itemDetailPanelBG;
    public GameObject loadingPanel;

    public Image[] categoriesTabs;

    public TextMeshProUGUI categoryTitle;

    public Color selectedColor;
    public Color unSelectedColor;

    public GameObject loadingIcon;

    [Header("Detail Item Panel")]
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemType;
    public TextMeshProUGUI itemPrice;
    public TextMeshProUGUI itemDiscountPrice;
    public TextMeshProUGUI itemDiscription;
    public TextMeshProUGUI companyDiscription;
    public Button arRoomBtn;

    public GameObject targetModel;
    public GameObject modelPrefab;
    public GameObject UI_3D_Models_Parent;
    //private string localPath;

    public Transform colorsParent;
    public GameObject colorPrefab;

    //public GameObject colorVariant1Parent;
    //public GameObject colorVariant2Parent;
    //public GameObject colorVariant3Parent;

    //public Image colorVariant1;
    //public Image colorVariant2;
    //public Image colorVariant3;

    public TMP_Dropdown sizes;

    public ModelViewer mv;

    public float cameraOffset = 1.2f;

    public ProductDetails selectedProduct;
    public int selectedMatIndex;

    public RawImage modelViewer3D;
    public GameObject txt360View;
    public Image modelViewerImage;

    public List<GameObject> downloadedModels;
    Coroutine currentCoroutine;
    UnityWebRequest currentRequest;

    public GameObject itemViewBG;
    public GameObject itemViewPanel;

    public bool isSizeSelected = false;
    public int selectedSizeIndex = 0;
    public int maxStocks = 0;

    public TextMeshProUGUI stocksSelected;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        statusText.gameObject.SetActive(false);
        //LoadCategories();
    }

    void Start()
    {
        //localPath = Path.Combine(Application.persistentDataPath, "tempModel.glb");
        if (refreshButton != null)
        {
            //refreshButton.onClick.AddListener(LoadCategories);
        }
    }

    public void ShowScreen(UIScreenType_Shop type)
    {
        HideAll();

        foreach (var screen in screens)
        {
            if (screen.type == type)
            {
                if (screen.mainPanel) screen.mainPanel.SetActive(true);
                if (screen.blurPanel) screen.blurPanel.SetActive(true);
                if (screen.backPanel) screen.backPanel.SetActive(true);
                break;
            }
        }
    }

    public void HideAll()
    {
        foreach (var screen in screens)
        {
            if (screen.mainPanel) screen.mainPanel.SetActive(false);
            if (screen.blurPanel) screen.blurPanel.SetActive(false);
            if (screen.backPanel) screen.backPanel.SetActive(false);
        }
    }

    public void SelectCategory(string url)
    {
        supplyPanel.SetActive(false);
        shopPanel.SetActive(true);

        LoadCategories(url);
    }

    #region API Call
    public void LoadCategories(string categoryURL)
    {
        foreach (var category in categoriesTabs)
            category.color = unSelectedColor;

        foreach(var category in categoriesTabs)
            if(categoryURL.Contains(category.name))
            {
                category.color = selectedColor;
                categoryTitle.text = category.name;
            }


        loadingIcon.SetActive(true);

        //MenuManager.Instance.loadingPanel.SetActive(true);
        ClearCategories();

        StartCoroutine(AuthAPI.PostRequest(getProductsURL + categoryURL, "", // Empty string for no body
            (response) =>
            {
                Debug.Log("Categories loaded: " + response);

                // Parse the response
                CategoryManager.ProductResponse responseData = JsonUtility.FromJson<CategoryManager.ProductResponse>(response);

                if (responseData.products != null && responseData.products.Count > 0)
                {
                    PopulateItems(responseData.products);
                    statusText.gameObject.SetActive(false);
                }
                else
                {
                    ShowStatus("No categories found", false);
                }

                loadingIcon.SetActive(false);
                loadingPanel.SetActive(false);
            },
            (error) =>
            {
                loadingIcon.SetActive(false);
                loadingPanel.SetActive(false);
                Debug.LogError("Failed to load categories: " + error);
                ShowStatus("Failed to load categories", true);
                //MenuManager.Instance.loadingPanel.SetActive(false);
            }, "GET"));
    }

    IEnumerator DownloadAndAssign(string url, GameObject targetObject, CategoryManager.Products p)
    {
        yield return null;

        var state = targetObject.GetComponent<DownloadState>();
        state.isDownloading = true;
        state.isReady = false;

        string localPath = Path.Combine(Application.persistentDataPath, p.id + ".glb");

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            currentRequest = www;
            Debug.Log("1");
            www.downloadHandler = new DownloadHandlerFile(localPath);
            Debug.Log("2");
            yield return www.SendWebRequest();
            Debug.Log("3");

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Download failed: " + www.error);
                state.isDownloading = false; // allow retry
                yield break;
            }
            Debug.Log("4");
        }

        // --- Step 2: Load GLB with UnityGLTF ---
        using (FileStream stream = new FileStream(localPath, FileMode.Open, FileAccess.Read))
        {
            var importOptions = new ImportOptions();
            var importer = new GLTFSceneImporter(stream, importOptions);
            yield return importer.LoadSceneAsync();

            GameObject loadedRoot = importer.LastLoadedScene;

            if (loadedRoot == null)
            {
                Debug.LogError("Failed to load GLB: no root GameObject returned");
                state.isDownloading = false;
                yield break;
            }

            MeshFilter srcMF = loadedRoot.GetComponentInChildren<MeshFilter>();
            MeshRenderer srcMR = loadedRoot.GetComponentInChildren<MeshRenderer>();

            if (srcMF == null || srcMR == null)
            {
                Debug.LogError("Loaded model has no MeshFilter or MeshRenderer!");
                state.isDownloading = false;
                yield break;
            }

            // Assign mesh/material to target
            Transform visual = targetObject.transform.Find("Visual");
            if (visual == null)
            {
                visual = new GameObject("Visual").transform;
                visual.parent = targetObject.transform;
                visual.localPosition = Vector3.zero;
            }

            MeshFilter targetMF = visual.GetComponent<MeshFilter>() ?? visual.gameObject.AddComponent<MeshFilter>();
            MeshRenderer targetMR = visual.GetComponent<MeshRenderer>() ?? visual.gameObject.AddComponent<MeshRenderer>();

            targetMF.mesh = Instantiate(srcMF.sharedMesh);
            targetMR.materials = srcMR.materials.Clone() as Material[];

            for (int matIdx = 0; matIdx < targetMR.materials.Length; matIdx++)
            {
                Material mat = targetMR.materials[matIdx];
                Material srcMat = srcMR.materials[matIdx];

                foreach (string prop in srcMat.GetTexturePropertyNames())
                {
                    Texture tex = srcMat.GetTexture(prop);
                    if (tex == null) continue;

                    if (tex is Texture2D srcTex)
                    {
                        Texture2D copy = new Texture2D(srcTex.width, srcTex.height, srcTex.format, srcTex.mipmapCount > 1);
                        Graphics.CopyTexture(srcTex, copy);
                        copy.wrapMode = srcTex.wrapMode;
                        copy.filterMode = srcTex.filterMode;
                        copy.anisoLevel = srcTex.anisoLevel;
                        copy.Apply();
                        mat.SetTexture(prop, copy);
                    }
                }
            }

            Debug.Log("Mesh and materials assigned!");

            targetObject.SetActive(true);

            if (!downloadedModels.Contains(targetObject))
                downloadedModels.Add(targetObject);

            state.isDownloading = false;
            state.isReady = true;

            mv.FrameObject(targetObject);

            Destroy(loadedRoot);
        }
    }


    public void UpdateObjectScale(CategoryManager.Products p, GameObject newObject, bool isVertical = false)
    {
        Vector3 modelOriginalHeight;

        // Get original bounds size of the mesh renderer
        modelOriginalHeight = newObject.GetComponentInChildren<MeshRenderer>().bounds.size;

        // Calculate scale ratios for each axis according to desired size

        if (isVertical)
        {
            float temp = p.sizes[0].dimensions.length;
            p.sizes[0].dimensions.length = p.sizes[0].dimensions.height;
            p.sizes[0].dimensions.height = p.sizes[0].dimensions.width;
            p.sizes[0].dimensions.width = temp;
        }

        Vector3 finalScale = new Vector3(ConvertToUnityScale(p.sizes[0].dimensions.length, p.sizes[0].dimensions.unit) / modelOriginalHeight.x,
            ConvertToUnityScale(p.sizes[0].dimensions.height, p.sizes[0].dimensions.unit) / modelOriginalHeight.y,
            ConvertToUnityScale(p.sizes[0].dimensions.width, p.sizes[0].dimensions.unit) / modelOriginalHeight.z
            );

        newObject.transform.localScale = finalScale;
    }
    float ConvertToUnityScale(float inputSize, string unit)
    {
        switch (unit.ToLower())
        {
            case "cm":
                return inputSize / 100f;  // 100 cm = 1 m
            case "in":
                return inputSize * 0.0254f; // 1 inch = 0.0254 m
            case "m":
                return inputSize; // already in meters
            default:
                Debug.LogWarning("Unknown unit, defaulting to meters");
                return inputSize;
        }
    }

    #endregion

    #region UI Methods
    private void PopulateItems(List<CategoryManager.Products> Products)
    {
        foreach (CategoryManager.Products product in Products)
        {
            GameObject newItem = Instantiate(productPrefab, contentParent);

            newItem.transform.Find("Item Name").GetComponent<TextMeshProUGUI>().text = product.company.legalName;
            newItem.transform.Find("Item Name").GetComponent<TextMeshProUGUI>().enabled = true;

            newItem.transform.Find("Item Type").GetComponent<TextMeshProUGUI>().text = product.name;
            newItem.transform.Find("Item Type").GetComponent<TextMeshProUGUI>().enabled = true;

            if (product.displayPrice.hasSale)
            {
                if (product.displayPrice.salePrice < product.displayPrice.price)
                {
                    newItem.transform.Find("Original Price").GetComponent<TextMeshProUGUI>().text = "<s>$" + product.displayPrice.price + "</s>";
                    newItem.transform.Find("Original Price").GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.7f, 0.7f);
                }
            }
            else
            {
                newItem.transform.Find("Original Price").GetComponent<TextMeshProUGUI>().text = "$" + product.displayPrice.price + "";
                newItem.transform.Find("Original Price").GetComponent<TextMeshProUGUI>().color = new Color(0, 0, 0);
            }

            newItem.transform.Find("Original Price").GetComponent<TextMeshProUGUI>().enabled = true;
            
            if (product.displayPrice.salePrice > 0)
            {
                newItem.transform.Find("Discounted Price").GetComponent<TextMeshProUGUI>().text = "$" + product.displayPrice.salePrice;
                newItem.transform.Find("Discounted Price").GetComponent<TextMeshProUGUI>().enabled = true;
            }

            if(product.thumbnail.Length > 0)
                StartCoroutine(DownloadImage(product.thumbnail, newItem.transform.Find("Item Icon").GetComponent<Image>(), newItem.transform.Find("Loading Icon").gameObject));
    
            newItem.GetComponent<Button>().onClick.AddListener(() => {
                ShowItemDetail(product);
            });
           
        }
    }

    public IEnumerator DownloadImage(string url, UnityEngine.UI.Image imageComponent, GameObject loadingIcon)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            byte[] imageData = request.downloadHandler.data;

            // 🔍 Check what kind of data we actually got
            string textPreview = System.Text.Encoding.UTF8.GetString(imageData);
            Debug.Log("Response preview: " + textPreview.Substring(0, Mathf.Min(200, textPreview.Length)));

            Debug.Log("Content-Type: " + request.GetResponseHeader("Content-Type"));
            Debug.Log("Data length: " + imageData.Length);

            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(imageData))
            {
                Sprite sprite = Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
                imageComponent.sprite = sprite;
                imageComponent.enabled = true;
            }
            else
            {
                Debug.LogError(url + "\n❌ Failed to decode image bytes. Response is not a valid PNG/JPG.");
            }
        }
        else
        {
            Debug.LogError($"Failed to download image: {request.error}");
        }

        loadingIcon.SetActive(false);
    }

    public void ShowItemDetail(CategoryManager.Products product)
    {
        StartCoroutine(AuthAPI.PostRequest(getProductBySlug + product.slug, "", // Empty string for no body
            (response) =>
            {
                Debug.Log("Categories loaded: " + response);

                // Parse the response
                CategoryManager.Products products = JsonUtility.FromJson<CategoryManager.Products>(response);

                Debug.Log("Products : " + products.variants.Count);
                if (products != null)
                {
                    targetModel = null;
                    itemDetailPanel.SetActive(true);
                    itemDetailPanelBG.SetActive(true);
                    shopPanel.SetActive(false);

                    foreach (Transform obj in UI_3D_Models_Parent.transform)
                        obj.gameObject.SetActive(false);
                    //Destroy(obj.gameObject);

                    GameObject downloaded = downloadedModels.Find(m => m.GetComponent<ProductDetails>().product.id == products.id);


                    //bool allowDownload = downloaded == null;


                    if (downloaded != null)
                    {
                        targetModel = downloaded;      // reuse downloaded prefab
                                                       //allowDownload = false;
                    }
                    else
                    {
                        // Check if a prefab for this product is currently downloading
                        foreach (Transform obj in UI_3D_Models_Parent.transform)
                        {
                            if (obj.GetComponent<ProductDetails>() && obj.GetComponent<ProductDetails>().product.id == products.id)
                            {
                                Debug.Log("target = " + targetModel == null);
                                targetModel = obj.gameObject;
                                Debug.Log("ID Found : " + obj.GetComponent<ProductDetails>().product.id);
                                //allowDownload = false;
                            }
                        }

                        //targetModel = UI_3D_Models_Parent.<DownloadState>()
                        //    .FirstOrDefault(s => s.GetComponent<ProductDetails>()?.product.id == product.id)
                        //    ?.gameObject;
                        Debug.Log("product ID: " + products.id);
                    }

                    // Decide if we need to instantiate
                    if (targetModel == null)
                    {
                        Debug.Log("target is null");
                        //allowDownload = true;
                        targetModel = Instantiate(modelPrefab);
                    }

                    products.colors.Sort((a, b) => a.sortOrder.CompareTo(b.sortOrder));
                    products.images.Sort((a, b) => a.sortOrder.CompareTo(b.sortOrder));
                    products.sizes.Sort((a, b) => a.sortOrder.CompareTo(b.sortOrder));
                    products.threeDModels.Sort((a, b) =>
                    {
                        int indexA = products.colors.FindIndex(c => c.code == a.colorCode);
                        int indexB = products.colors.FindIndex(c => c.code == b.colorCode);

                        return indexA.CompareTo(indexB);
                    });

                    products.variants.Sort((a, b) =>
                    {
                        int indexA = products.sizes.FindIndex(c => c.sortOrder == a.size.sortOrder);
                        int indexB = products.sizes.FindIndex(c => c.sortOrder == b.size.sortOrder);

                        return indexA.CompareTo(indexB);
                    });

                    //products.variants.Sort((a, b) =>
                    //{
                    //    int indexA = products.sizes.FindIndex(c => c.id == a.id);
                    //    int indexB = products.sizes.FindIndex(c => c.id == b.id);

                    //    return indexA.CompareTo(indexB);
                    //});


                    targetModel.transform.parent = UI_3D_Models_Parent.transform;
                    targetModel.transform.localPosition = Vector3.zero;
                    targetModel.SetActive(true);

                    foreach (Transform obj in colorsParent)
                        Destroy(obj.gameObject);

                    for (int i = 0; i < products.colors.Count; i++)
                    {
                        Color newColor1;
                        ColorUtility.TryParseHtmlString(products.colors[i].code, out newColor1);
                        ModelVariant mv = Instantiate(colorPrefab, colorsParent).GetComponent<ModelVariant>();
                        mv.img.color = newColor1;
                        mv.index = i;
                    }

                    isSizeSelected = false;
                    sizes.ClearOptions();

                    if (products.sizes.Count == 1)
                    {
                        sizes.options.Add(new TMP_Dropdown.OptionData(products.sizes[0].size));
                    }
                    else
                    {
                        sizes.options.Add(new TMP_Dropdown.OptionData("Size"));
                        foreach (var option in products.sizes)
                        {
                            sizes.options.Add(new TMP_Dropdown.OptionData(option.size));
                        }
                    }

                    sizes.value = 0;
                    sizes.RefreshShownValue();

                    var stateCheck = targetModel.GetComponent<DownloadState>() ?? targetModel.AddComponent<DownloadState>();
                    Debug.Log("stateCheck is Ready " + stateCheck.isReady + ", isdownloading : " + stateCheck.isDownloading, targetModel.gameObject);
                    if (!stateCheck.isReady && !stateCheck.isDownloading)
                    {
                        Debug.Log("inside stateCheck is Ready " + stateCheck.isReady + ", isdownloading : " + stateCheck.isDownloading, targetModel.gameObject);
                        currentCoroutine = StartCoroutine(DownloadAndAssign(products.threeDModels[0].url, targetModel, products));
                    }
                    else if (stateCheck.isReady)
                        mv.FrameObject(targetModel);

                    selectedProduct = targetModel.GetComponent<ProductDetails>() ?? targetModel.AddComponent<ProductDetails>();
                    selectedProduct.imagesUrl.Clear();
                    foreach (var img in products.images)
                        selectedProduct.imagesUrl.Add(img.url);

                    selectedProduct.texturesUrl.Clear();
                    foreach(var model in products.threeDModels)
                        selectedProduct.texturesUrl.Add(model.texture);

                    selectedProduct.product = products;
                    if(downloaded == null)
                        StartCoroutine(SetProductImages(selectedProduct));

                    ChangeModelTexture(0);

                    itemName.text = products.name;
                    itemType.text = products.company.entityName;
                    stocksSelected.text = "1";

                    //if (product.displayPrice.hasSale)
                    //{
                    //    if (product.displayPrice.salePrice < product.displayPrice.price)
                    //    {
                    //        itemPrice.text = "$" + "<s>" + product.displayPrice.price + "</s>";
                    //        itemPrice.color = new Color(0.7f, 0.7f, 0.7f);
                    //    }
                    //}
                    //else
                    //{
                        itemPrice.text = products.displayPrice.range;
                        itemPrice.color = new Color(0, 0, 0);

                    itemDiscountPrice.gameObject.SetActive(false);
                    //}

                    //if (product.displayPrice.hasSale)
                    //    if (product.displayPrice.salePrice > 0)
                    //        itemDiscountPrice.text = "$" + product.displayPrice.salePrice;

                    itemDiscription.text = products.description;
                    companyDiscription.text = products.company.description;

                    if (arRoomBtn != null)
                    {
                        arRoomBtn.onClick.RemoveAllListeners();
                        arRoomBtn.onClick.AddListener(() => CheckObjectScene(products));

                    }

                    modelViewerImage.enabled = false;
                    modelViewer3D.enabled = true;
                    currentImage = 0;
                }
                else
                {
                    ShowStatus("No categories found", false);
                }

                loadingIcon.SetActive(false);
                loadingPanel.SetActive(false);
            },
            (error) =>
            {
                loadingIcon.SetActive(false);
                loadingPanel.SetActive(false);
                Debug.LogError("Failed to load categories: " + error);
                ShowStatus("Failed to load categories", true);
                //MenuManager.Instance.loadingPanel.SetActive(false);
            }, "GET"));
    }

    public void ChangeSize()
    {
        isSizeSelected = true;
        selectedSizeIndex = sizes.value;

        if (selectedSizeIndex == 0)
            return;

        if (selectedProduct.product.variants[selectedSizeIndex - 1].salePrice > 0 && selectedProduct.product.variants[selectedSizeIndex - 1].price > selectedProduct.product.variants[selectedSizeIndex - 1].salePrice)
        {
            itemPrice.text = "<s>$" + selectedProduct.product.variants[selectedSizeIndex - 1].price + "<s>";
            itemPrice.color = new Color(0.7f, 0.7f, 0.7f);
            itemDiscountPrice.text = "$" + selectedProduct.product.variants[selectedSizeIndex - 1].salePrice;
            itemDiscountPrice.gameObject.SetActive(true);
        }
        else
        {
            itemPrice.text = "$" + selectedProduct.product.variants[selectedSizeIndex - 1].price;
            itemPrice.color = new Color(0f, 0f, 0f);
            itemDiscountPrice.gameObject.SetActive(false);
        }

        maxStocks = selectedProduct.product.variants[selectedSizeIndex - 1].stock;

        //if(maxStocks > 0)
        //    stocksSelected.text = "1";
        //else
            stocksSelected.text = "1";
    }

    public void ChangeItemAmount(int value)
    {
        int currentStocks = int.Parse(stocksSelected.text);

        currentStocks += value;

        if (currentStocks < 1)
            currentStocks = 1;
        if (currentStocks > maxStocks)
            currentStocks = maxStocks;

        if(currentStocks < 1)
            currentStocks = 1;

       stocksSelected.text = currentStocks.ToString();
    }

    void DisableFirstItem(TMP_Dropdown dropdown)
    {
        Canvas canvas = dropdown.GetComponentInChildren<Canvas>();
        if (!canvas) return;

        Toggle[] toggles = canvas.GetComponentsInChildren<Toggle>();
        if (toggles.Length > 0)
        {
            toggles[0].interactable = false;
        }
    }

    int currentImage = 0;

    public void NextPreviousImage(int index)
    {
        string id = selectedProduct.product.colors[selectedMatIndex].id;
        currentImage += index;
        Debug.Log("current Index Before : " + currentImage);

        if (currentImage < 0)
            currentImage = selectedProduct.sprites.Count - 1;
        if (currentImage >= selectedProduct.sprites.Count)
        {
            currentImage = 0;
        }

        Debug.Log("Selected sprites Count : " + selectedProduct.sprites.Count);

        if (currentImage != 0)
            while (selectedProduct.product.images[currentImage].colorId != id)
            {
                currentImage += index;

                if (currentImage == 0)
                    break;

                if (currentImage < 0)
                    currentImage = selectedProduct.sprites.Count - 1;
                if (currentImage >= selectedProduct.sprites.Count)
                {
                    currentImage = 0;
                    break;
                }
            }

        Debug.Log("current Index : " + currentImage);

        if (currentImage == 0)
        {
            modelViewerImage.enabled = false;
            modelViewer3D.enabled = true;
            //txt360View.SetActive(true);
        }
        else
        {
            modelViewer3D.enabled = false;
            //txt360View.SetActive(false);
            modelViewerImage.enabled = true;
            modelViewerImage.sprite = selectedProduct.sprites[currentImage];
        }

    }

    public void BackToShop()
    {
        shopPanel.SetActive(true);
        itemViewBG.SetActive(false);
        itemViewPanel.SetActive(false);

        // Hide or clean up 3D models
        foreach (Transform obj in UI_3D_Models_Parent.transform)
        {
            var state = obj.GetComponent<DownloadState>();
            if (state != null)
            {
                if (state.isReady)
                {
                    // Already downloaded → just hide
                    obj.gameObject.SetActive(false);
                }
                else if (!state.isDownloading)
                {
                    // Download failed → safe to destroy
                    Destroy(obj.gameObject);
                }
                else
                {
                    // Download in progress → keep it running, just hide
                    obj.gameObject.SetActive(false);
                }
            }
            else
            {
                // No state component → just destroy to be safe
                Destroy(obj.gameObject);
            }
        }

        //if (currentRequest != null && currentRequest.isDone)
        //    currentRequest.Abort();

        //if(currentCoroutine != null)
        //    StopCoroutine(currentCoroutine);

        //currentCoroutine = null;
        //currentRequest = null;
    }

    IEnumerator SetProductImages(ProductDetails pd)
    {
        yield return null;
        pd.sprites.Clear();
        for (int i = 0; i < pd.imagesUrl.Count; i++)
        {
            pd.sprites.Add(null);
        }

        for (int i = 0; i < pd.imagesUrl.Count; i++)
        {
            yield return StartCoroutine(DownloadSpriteCoroutine(pd.imagesUrl[i], pd.sprites, i));
        }

        pd.textures.Clear();
        for (int i = 0; i < pd.texturesUrl.Count; i++)
        {
            pd.textures.Add(null);
        }

        for (int i = 0; i < pd.texturesUrl.Count; i++)
        {
            yield return StartCoroutine(DownloadTextureCoroutine(pd.texturesUrl[i], pd.textures, i));
            Debug.Log("texture downloading");
        }
        Debug.Log("texture Finish");
        ChangeModelTexture(0);
    }

    public IEnumerator DownloadSpriteCoroutine(string url, List<Sprite> spritesList, int index)
    {
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(req.error);

            yield break;
        }

        byte[] imageData = req.downloadHandler.data;
        Texture2D texture = new Texture2D(2, 2);

        if (texture.LoadImage(imageData))
        {
            Sprite sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            spritesList[index] = sprite;
        }
        else
        {
            Debug.LogError(url + "\n❌ Failed to decode image bytes. Response is not a valid PNG/JPG.");
        }
    }

    public IEnumerator DownloadTextureCoroutine(string url, List<Texture2D> texturesList, int index)
    {
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(req.error);

            yield break;
        }

        byte[] imageData = req.downloadHandler.data;
        Texture2D texture = new Texture2D(2, 2);

        if (texture.LoadImage(imageData))
        {
            texturesList[index] = texture;
        }
        else
        {
            Debug.LogError(url + "\n❌ Failed to decode image bytes. Response is not a valid PNG/JPG.");
        }
    }

    public void ChangeModelTexture(int index)
    {
        foreach (Transform child in colorsParent)
        {
            if (child.GetComponent<ModelVariant>()?.index == index)
                child.GetComponent<ModelVariant>().selectedImg.SetActive(true);
            else
                child.GetComponent<ModelVariant>()?.selectedImg.SetActive(false);
        }

        if (index >= selectedProduct.textures.Count)
            return;

        selectedMatIndex = index;
        Material mat = targetModel.transform.Find("Visual").GetComponent<MeshRenderer>().material;

        //Color newColor1;
        //ColorUtility.TryParseHtmlString(selectedProduct.product.colors[index].code, out newColor1);
        //mat.color = newColor1;
        
        mat.mainTexture = selectedProduct.textures[index];

        targetModel.transform.Find("Visual").GetComponent<MeshRenderer>().material = mat;

        if(isSizeSelected)
        {
            isSizeSelected = true;
            selectedSizeIndex = sizes.value;

            if (selectedSizeIndex == 0)
                return;

            if (selectedProduct.product.variants[selectedSizeIndex - 1].salePrice > 0 && selectedProduct.product.variants[selectedSizeIndex - 1].price > selectedProduct.product.variants[selectedSizeIndex - 1].salePrice)
            {
                itemPrice.text = "<s>$" + selectedProduct.product.variants[selectedSizeIndex - 1].price + "<s>";
                itemPrice.color = new Color(0.7f, 0.7f, 0.7f);
                itemDiscountPrice.text = "$" + selectedProduct.product.variants[selectedSizeIndex - 1].salePrice;
                itemDiscountPrice.gameObject.SetActive(true);
            }
            else
            {
                itemPrice.text = "$" + selectedProduct.product.variants[selectedSizeIndex - 1].price;
                itemPrice.color = new Color(0f, 0f, 0f);
                itemDiscountPrice.gameObject.SetActive(false);
            }

            foreach (var v in selectedProduct.product.variants)
            {
                if (v.color.id == selectedProduct.product.colors[selectedMatIndex].id)
                {
                    maxStocks = v.stock;
                    
                    if(v.salePrice > 0 && v.salePrice < v.price)
                    {
                        itemPrice.text = "<s>$" + v.price + "<s>";
                        itemPrice.color = new Color(0.7f, 0.7f, 0.7f);
                        itemDiscountPrice.text = v.salePrice.ToString();
                        itemDiscountPrice.gameObject.SetActive(true);
                    }
                    else
                    {
                        itemPrice.text = v.price.ToString();
                        itemPrice.color = new Color(0, 0, 0);

                        itemDiscountPrice.gameObject.SetActive(false);
                    }
                }
            }

            //if (maxStocks > 0)
            //    stocksSelected.text = "1";
            //else
                stocksSelected.text = "1";
        }
        else
        {
            itemPrice.text = selectedProduct.product.displayPrice.range;
            itemPrice.color = new Color(0, 0, 0);

            itemDiscountPrice.gameObject.SetActive(false);

            stocksSelected.text = "1";
        }

    }

    public void ChangeModelVariant()
    {
        modelViewerImage.enabled = false;
        modelViewer3D.enabled = true;
        //txt360View.SetActive(true);
        currentImage = 0;
    }

    void CheckObjectScene(CategoryManager.Products p)
    {
        if (p.ar_type.Equals("vertical-plane detection") || p.ar_type.Equals("horizontal-plane detection"))
        {
            ProductSelection.ClearSelection();
            ProductSelection.SetSelection(p, false, "", p.ar_type.Equals("horizontal-plane detection"), p.threeDModels[0].url);
            SceneManager.LoadScene("AR Scene");
        }
        else if (p.ar_type.Equals("face-tracking"))
        {
            CategoryType category;
            ProductSelection.TryParseObjectType(p.name, out category);
            ProductSelection.ClearSelection();
            ProductSelection.SetSelection(p, true, p.category.name, false, p.threeDModels[0].url);
            SceneManager.LoadScene("AR Face");
        }
        else if(p.ar_type.Equals("hand-tracking"))
        {
            CategoryType category;
            ProductSelection.TryParseObjectType(p.name, out category);
            ProductSelection.ClearSelection();
            ProductSelection.SetSelection(p, true, p.category.name, false, p.threeDModels[0].url);
            SceneManager.LoadScene("Hand Tracking");
        }
    }

    public void ChangeModelColor(Image img)
    {
        targetModel.GetComponentInChildren<MeshRenderer>().material.color = img.color;
    }    

    private void ClearCategories()
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
    private class CategoriesResponse
    {
        public CategoryData[] categories;
    }

    [System.Serializable]
    private class CategoryData
    {
        public string id;
        public string name;
    }
    #endregion

    [System.Serializable]
    public class UIScreen
    {
        public UIScreenType_Shop type;
        public GameObject mainPanel;
        public GameObject blurPanel;
        public GameObject backPanel;
    }

    public class DownloadState : MonoBehaviour
    {
        public bool isDownloading = false;  // true while downloading
        public bool isReady = false;        // true if successfully downloaded
    }
}