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
using UnityEngine.XR.ARFoundation;
using UnityGLTF;

public class CategoriesUI : MonoBehaviour
{
    public static CategoriesUI Instance;

    #region API Endpoints
    private string getAllProductsURL = "/products";
    private string getProductsURL = "/products?subcategory";
    private string getProductBySlug = "/products/slug/";
    #endregion


    public enum UIScreenType_Shop
    {
        None,
        itemDetail
    }

    public List<UIScreen> screens;


    #region Main UI References

    [Header("Global UI")]
    public GameObject whiteBG;

    public GameObject supplyPanel;
    public GameObject shopPanel;
    public GameObject loadingPanel;

    #endregion


    #region Product Listing

    [Header("Product List")]
    public Transform contentParent;
    public GameObject productPrefab;

    public Button refreshButton;
    public TextMeshProUGUI statusText;

    #endregion


    #region Category Navigation

    [Header("Category Tabs")]
    public Image[] categoriesTabs;
    public ScrollRect categorySlideView;

    public TextMeshProUGUI categoryTitle;

    [Header("Category Colors")]
    public Color selectedColor;
    public Color unSelectedColor;

    #endregion


    #region Product Detail Panel

    [Header("Detail Panel")]
    public GameObject itemDetailPanel;
    public GameObject itemDetailPanelBG;

    [Header("Product Information")]
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemType;
    public TextMeshProUGUI itemPrice;
    public TextMeshProUGUI itemDiscountPrice;
    public TextMeshProUGUI itemDiscription;

    [Header("Company Information")]
    public TextMeshProUGUI companyDiscription;

    [Header("Actions")]
    public Button arRoomBtn;
    public Button addToCartBtn;

    #endregion


    #region 3D Model Viewer

    [Header("3D Model")]
    public GameObject targetModel;
    public GameObject modelPrefab;
    public GameObject UI_3D_Models_Parent;

    public ModelViewer mv;

    [Header("Camera Settings")]
    public float cameraOffset = 1.2f;

    [Header("Downloaded Models")]
    public List<GameObject> downloadedModels;

    #endregion


    #region Product Variants

    [Header("Colors")]
    public Transform colorsParent;
    public GameObject colorPrefab;

    [Header("Sizes")]
    public TMP_Dropdown sizes;

    #endregion


    #region Item Viewer UI

    [Header("360 View")]
    public RawImage modelViewer3D;
    public GameObject txt360View;
    public Image modelViewerImage;

    public GameObject itemViewBG;
    public GameObject itemViewPanel;

    #endregion


    #region Product Data

    [Header("Selected Product")]
    public ProductDetails selectedProduct;
    public int selectedMatIndex;

    public CategoryManager.ProductResponse allProductsData;

    #endregion


    #region Selection State

    [Header("Selection")]
    public bool isSizeSelected = false;
    public int selectedSizeIndex = 0;
    public int maxStocks = 0;

    public TextMeshProUGUI stocksSelected;

    public string currentProductID;

    private string lastResolvedVariantId;

    #endregion


    #region Search

    [Header("Search")]
    public TMP_InputField searchInputField;

    #endregion


    #region Runtime

    [Header("Runtime References")]
    public Coroutine currentCoroutine;
    public Coroutine tempCoroutine;

    public UnityWebRequest currentRequest;

    #endregion


    #region Debug

    [Header("Debug")]
    public bool isDebug = false;

    #endregion

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        statusText.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (refreshButton != null)
        {
            // Reserved for future refresh functionality
        }

        if (addToCartBtn != null)
            addToCartBtn.onClick.AddListener(AddCurrentToCart);

        if (searchInputField != null)
            searchInputField.onValueChanged.AddListener(OnSearchTextChanged);

        GetAllProducts();
    }

    private void AddCurrentToCart()
    {
        if (CartManager.Instance != null)
            CartManager.Instance.AddCurrentToCart();
    }


    #region Screen Management

    public void ShowScreen(UIScreenType_Shop type)
    {
        HideAll();

        foreach (var screen in screens)
        {
            if (screen.type != type)
                continue;

            if (screen.mainPanel)
                screen.mainPanel.SetActive(true);

            if (screen.blurPanel)
                screen.blurPanel.SetActive(true);

            if (screen.backPanel)
                screen.backPanel.SetActive(true);

            break;
        }
    }

    public void HideAll()
    {
        foreach (var screen in screens)
        {
            if (screen.mainPanel)
                screen.mainPanel.SetActive(false);

            if (screen.blurPanel)
                screen.blurPanel.SetActive(false);

            if (screen.backPanel)
                screen.backPanel.SetActive(false);
        }
    }

    #endregion


    #region Category & Search

    public void SelectCategory(string url)
    {
        supplyPanel.SetActive(false);
        shopPanel.SetActive(true);

        LoadCategories(url);
    }

    public void OnSearchTextChanged(string text)
    {
        CategoryManager.ProductResponse response = SearchProducts(text);

        ClearProducts();
        PopulateItems(response.products);

        UnselectedAllCategories();
    }

    public CategoryManager.ProductResponse SearchProducts(string query)
    {
        if (string.IsNullOrEmpty(query))
            return allProductsData;

        string[] searchWords = query
            .ToLower()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);


        var filteredProducts = allProductsData.products
            .Where(product =>
            {
                string searchableText =
                    $"{product.name} " +
                    $"{product.company.entityName} " +
                    $"{product.category.name} " +
                    $"{product.material} " +
                    $"{product.category.parent.name} " +
                    $"{product.category.parent.parent.name} " +
                    $"{product.category.parent.parent.parent.name}"
                    .ToLower();


                return searchWords.Any(word =>
                    searchableText.Contains(word));
            })
            .ToList();


        return new CategoryManager.ProductResponse
        {
            products = filteredProducts
        };
    }

    #endregion

    #region API Call

    private void GetAllProducts()
    {
        StartCoroutine(AuthAPI.PostRequest(
            getAllProductsURL,
            "",
            (response) =>
            {
                CategoryManager.ProductResponse responseData =
                    JsonUtility.FromJson<CategoryManager.ProductResponse>(response);

                allProductsData = responseData;
            },
            (error) =>
            {
                if (isDebug)
                    Debug.LogError("Failed to load all products: " + error);
            },
            "GET"));
    }


    public void UnselectedAllCategories()
    {
        foreach (var category in categoriesTabs)
        {
            category.color = unSelectedColor;
        }
    }


    public void SelectedCategoryIndex(int index)
    {
        UnselectedAllCategories();

        if (index < 0 || index >= categoriesTabs.Length)
            return;

        categoriesTabs[index].color = selectedColor;
        categoryTitle.text = categoriesTabs[index].name;
    }


    public void ScrollToSelectedHorizontal(int selectedIndex)
    {
        if (categorySlideView == null || categorySlideView.content == null)
            return;

        int totalItems = categorySlideView.content.childCount;

        if (totalItems <= 1)
            return;


        selectedIndex = Mathf.Clamp(
            selectedIndex,
            0,
            totalItems - 1);


        float normalizedPosition =
            (float)selectedIndex / (totalItems - 1);


        categorySlideView.horizontalNormalizedPosition =
            Mathf.Clamp01(normalizedPosition);
    }


    public void LoadCategories(string categoryURL)
    {
        ClearProducts();


        if (tempCoroutine != null)
            StopCoroutine(tempCoroutine);


        tempCoroutine = StartCoroutine(
            AuthAPI.PostRequest(
                getProductsURL + categoryURL,
                "",
                (response) =>
                {
                    if (isDebug)
                        Debug.Log("Categories loaded: " + response);


                    CategoryManager.ProductResponse responseData =
                        JsonUtility.FromJson<CategoryManager.ProductResponse>(response);


                    if (responseData.products != null &&
                        responseData.products.Count > 0)
                    {
                        PopulateItems(responseData.products);
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
                    loadingPanel.SetActive(false);

                    if (isDebug)
                        Debug.LogError("Failed to load categories: " + error);

                    ShowStatus("Failed to load categories", true);
                },
                "GET"));
    }

    #endregion
    #region Model Loading

    private IEnumerator DownloadAndAssign(
        string url,
        GameObject targetObject,
        CategoryManager.Products p)
    {
        yield return null;


        DownloadState state = targetObject.GetComponent<DownloadState>();

        if (state == null)
            state = targetObject.AddComponent<DownloadState>();


        state.isDownloading = true;
        state.isReady = false;


        GameObject loadedRoot = null;


        yield return StartCoroutine(
            ModelLoaderService.DownloadAndLoad(
                url,
                (model) =>
                {
                    loadedRoot = model;
                }));


        if (loadedRoot == null)
        {
            if (isDebug)
                Debug.LogError("Failed to load model");

            state.isDownloading = false;
            yield break;
        }


        MeshFilter sourceMeshFilter =
            loadedRoot.GetComponentInChildren<MeshFilter>();

        MeshRenderer sourceMeshRenderer =
            loadedRoot.GetComponentInChildren<MeshRenderer>();

        SkinnedMeshRenderer sourceSkinnedRenderer =
            loadedRoot.GetComponentInChildren<SkinnedMeshRenderer>();


        Mesh mesh = null;
        Material[] materials = null;


        if (sourceMeshFilter != null &&
            sourceMeshRenderer != null)
        {
            mesh = sourceMeshFilter.sharedMesh;
            materials = sourceMeshRenderer.materials;
        }
        else if (sourceSkinnedRenderer != null)
        {
            mesh = sourceSkinnedRenderer.sharedMesh;
            materials = sourceSkinnedRenderer.materials;
        }
        else
        {
            if (isDebug)
                Debug.LogError("Loaded model has no renderable mesh!");

            state.isDownloading = false;

            Destroy(loadedRoot);
            yield break;
        }


        Transform visual =
            targetObject.transform.Find("Visual");


        if (visual == null)
        {
            visual = new GameObject("Visual").transform;

            visual.SetParent(targetObject.transform);
            visual.localPosition = Vector3.zero;
            visual.localRotation = Quaternion.identity;
            visual.localScale = Vector3.one;
        }


        MeshFilter targetMeshFilter =
            visual.GetComponent<MeshFilter>();

        if (targetMeshFilter == null)
            targetMeshFilter = visual.gameObject.AddComponent<MeshFilter>();


        MeshRenderer targetMeshRenderer =
            visual.GetComponent<MeshRenderer>();

        if (targetMeshRenderer == null)
            targetMeshRenderer = visual.gameObject.AddComponent<MeshRenderer>();


        targetMeshFilter.mesh = Instantiate(mesh);
        targetMeshRenderer.materials = materials;


        if (isDebug)
            Debug.Log("Mesh and materials assigned!");


        targetObject.SetActive(currentProductID == p.id);


        if (!downloadedModels.Contains(targetObject))
            downloadedModels.Add(targetObject);


        state.isDownloading = false;
        state.isReady = true;


        mv.FrameObject(targetObject);


        Destroy(loadedRoot);
    }


    public void UpdateObjectScale(
        CategoryManager.Products p,
        GameObject newObject,
        bool isVertical = false)
    {
        Vector3 modelOriginalHeight =
            newObject.GetComponentInChildren<MeshRenderer>()
            .bounds.size;


        if (isVertical)
        {
            float temp = p.sizes[0].dimensions.length;

            p.sizes[0].dimensions.length =
                p.sizes[0].dimensions.height;

            p.sizes[0].dimensions.height =
                p.sizes[0].dimensions.width;

            p.sizes[0].dimensions.width = temp;
        }


        Vector3 finalScale = new Vector3(
            ConvertToUnityScale(
                p.sizes[0].dimensions.length,
                p.sizes[0].dimensions.unit)
                / modelOriginalHeight.x,

            ConvertToUnityScale(
                p.sizes[0].dimensions.height,
                p.sizes[0].dimensions.unit)
                / modelOriginalHeight.y,

            ConvertToUnityScale(
                p.sizes[0].dimensions.width,
                p.sizes[0].dimensions.unit)
                / modelOriginalHeight.z
        );


        newObject.transform.localScale = finalScale;
    }


    private float ConvertToUnityScale(
        float inputSize,
        string unit)
    {
        switch (unit.ToLower())
        {
            case "cm":
                return inputSize / 100f;

            case "in":
                return inputSize * 0.0254f;

            case "m":
                return inputSize;

            default:
                if (isDebug)
                    Debug.LogWarning(
                        "Unknown unit, defaulting to meters");

                return inputSize;
        }
    }

    #endregion

    #region UI Methods

    private void PopulateItems(List<CategoryManager.Products> products)
    {
        foreach (CategoryManager.Products product in products)
        {
            GameObject newItem = Instantiate(
                productPrefab,
                contentParent);


            TextMeshProUGUI itemNameText =
                newItem.transform
                .Find("Item Name")
                .GetComponent<TextMeshProUGUI>();

            TextMeshProUGUI itemTypeText =
                newItem.transform
                .Find("Item Type")
                .GetComponent<TextMeshProUGUI>();

            TextMeshProUGUI originalPriceText =
                newItem.transform
                .Find("Original Price")
                .GetComponent<TextMeshProUGUI>();

            TextMeshProUGUI discountPriceText =
                newItem.transform
                .Find("Discounted Price")
                .GetComponent<TextMeshProUGUI>();


            itemNameText.text = product.company.entityName;
            itemNameText.enabled = true;


            itemTypeText.text = product.name;
            itemTypeText.enabled = true;


            if (product.displayPrice.hasSale &&
                product.displayPrice.salePrice <
                product.displayPrice.price)
            {
                originalPriceText.text =
                    "<s>$" + product.displayPrice.price + "</s>";

                originalPriceText.color =
                    new Color(0.7f, 0.7f, 0.7f);
            }
            else
            {
                originalPriceText.text =
                    "$" + product.displayPrice.price;

                originalPriceText.color =
                    Color.black;
            }


            originalPriceText.enabled = true;


            if (product.displayPrice.salePrice > 0)
            {
                discountPriceText.text =
                    "$" + product.displayPrice.salePrice;

                discountPriceText.enabled = true;
            }


            if (product.thumbnail.Length > 0)
            {
                Image iconImage =
                    newItem
                    .GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(t => t.name == "Item Icon")
                    ?.GetComponent<Image>();


                GameObject loadingIcon =
                    newItem.transform
                    .Find("Loading Icon")
                    ?.gameObject;


                if (iconImage != null)
                {
                    StartCoroutine(
                        DownloadImage(
                            product.thumbnail,
                            iconImage,
                            loadingIcon));
                }
            }


            Button button =
                newItem.GetComponent<Button>();

            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    ShowItemDetail(product);
                });
            }
        }
    }


    public IEnumerator DownloadImage(
        string url,
        Image imageComponent,
        GameObject loadingIcon)
    {
        UnityWebRequest request =
            UnityWebRequest.Get(url);


        yield return request.SendWebRequest();


        if (request.result == UnityWebRequest.Result.Success)
        {
            byte[] imageData =
                request.downloadHandler.data;


            if (isDebug)
            {
                string textPreview =
                    System.Text.Encoding.UTF8
                    .GetString(imageData);

                Debug.Log(
                    "Response preview: " +
                    textPreview.Substring(
                        0,
                        Mathf.Min(
                            200,
                            textPreview.Length)));

                Debug.Log(
                    "Content-Type: " +
                    request.GetResponseHeader(
                        "Content-Type"));

                Debug.Log(
                    "Data length: " +
                    imageData.Length);
            }


            Texture2D texture =
                new Texture2D(2, 2);


            if (texture.LoadImage(imageData))
            {
                Sprite sprite =
                    Sprite.Create(
                        texture,
                        new Rect(
                            0,
                            0,
                            texture.width,
                            texture.height),
                        new Vector2(
                            0.5f,
                            0.5f));


                if (imageComponent != null)
                {
                    imageComponent.sprite = sprite;
                    imageComponent.enabled = true;
                }
            }
            else
            {
                if (isDebug)
                {
                    Debug.LogError(
                        url +
                        "\nFailed to decode image bytes.");
                }
            }
        }
        else
        {
            if (isDebug)
            {
                Debug.LogError(
                    "Failed to download image: " +
                    request.error);
            }
        }


        if (loadingIcon != null)
            loadingIcon.SetActive(false);
    }

    #endregion

    #region Product Detail

    public void ShowItemDetail(CategoryManager.Products product)
    {
        StartCoroutine(
            AuthAPI.PostRequest(
                getProductBySlug + product.slug,
                "",
                (response) =>
                {
                    if (isDebug)
                        Debug.Log("Product loaded: " + response);


                    CategoryManager.Products products =
                        JsonUtility.FromJson<CategoryManager.Products>(response);


                    if (products == null)
                    {
                        ShowStatus("No product found", false);
                        loadingPanel.SetActive(false);
                        return;
                    }


                    OpenProductDetailPanel();

                    SetupProductModel(products);

                    SetupProductData(products);

                    SetupProductVariants(products);

                    SetupProductImages(products);

                    SetupProductUI(products);


                    loadingPanel.SetActive(false);


                    if (FavoritesManager.Instance != null)
                        FavoritesManager.Instance.RefreshCurrentToggleIcon();
                },
                (error) =>
                {
                    loadingPanel.SetActive(false);

                    if (isDebug)
                        Debug.LogError(
                            "Failed to load product: " + error);

                    ShowStatus(
                        "Failed to load product",
                        true);
                },
                "GET"));
    }



    private void OpenProductDetailPanel()
    {
        targetModel = null;

        itemDetailPanel.SetActive(true);
        itemDetailPanelBG.SetActive(true);

        shopPanel.SetActive(false);


        foreach (Transform obj in UI_3D_Models_Parent.transform)
        {
            obj.gameObject.SetActive(false);
        }
    }



    private void SetupProductModel(
        CategoryManager.Products products)
    {
        currentProductID = products.id;


        GameObject downloaded =
            downloadedModels.Find(m =>
                m.GetComponent<ProductDetails>() != null &&
                m.GetComponent<ProductDetails>()
                .product.id == products.id);



        if (downloaded != null)
        {
            targetModel = downloaded;
        }
        else
        {
            foreach (Transform obj in UI_3D_Models_Parent.transform)
            {
                ProductDetails details =
                    obj.GetComponent<ProductDetails>();

                if (details != null &&
                    details.product.id == products.id)
                {
                    targetModel = obj.gameObject;
                    break;
                }
            }
        }


        if (targetModel == null)
        {
            targetModel = Instantiate(modelPrefab);
        }


        targetModel.transform.SetParent(
            UI_3D_Models_Parent.transform);

        targetModel.transform.localPosition =
            Vector3.zero;

        targetModel.transform.localRotation =
            Quaternion.identity;


        targetModel.SetActive(true);
    }

    #endregion

    #region Product Setup
    private void SetupProductVariants(
    CategoryManager.Products products)
    {
        products.colors.Sort((a, b) =>
            a.sortOrder.CompareTo(b.sortOrder));


        products.images.Sort((a, b) =>
            a.sortOrder.CompareTo(b.sortOrder));


        products.sizes.Sort((a, b) =>
            a.sortOrder.CompareTo(b.sortOrder));


        products.threeDModels.Sort((a, b) =>
        {
            int indexA =
                products.colors.FindIndex(
                    c => c.code == a.colorCode);

            int indexB =
                products.colors.FindIndex(
                    c => c.code == b.colorCode);


            return indexA.CompareTo(indexB);
        });


        products.variants.Sort((a, b) =>
        {
            int indexA =
                products.sizes.FindIndex(
                    c => c.sortOrder == a.size.sortOrder);

            int indexB =
                products.sizes.FindIndex(
                    c => c.sortOrder == b.size.sortOrder);


            return indexA.CompareTo(indexB);
        });



        foreach (Transform obj in colorsParent)
        {
            Destroy(obj.gameObject);
        }


        for (int i = 0; i < products.colors.Count; i++)
        {
            Color color;

            ColorUtility.TryParseHtmlString(
                products.colors[i].code,
                out color);


            ModelVariant variant =
                Instantiate(
                    colorPrefab,
                    colorsParent)
                .GetComponent<ModelVariant>();


            variant.img.color = color;
            variant.index = i;
        }



        SetupSizeDropdown(products);
    }



    private void SetupSizeDropdown(
        CategoryManager.Products products)
    {
        isSizeSelected = false;


        SizeDropdownHandler handler =
            sizes.GetComponent<SizeDropdownHandler>();

        if (handler != null)
            handler.Reset();


        sizes.ClearOptions();



        if (products.sizes.Count == 1)
        {
            isSizeSelected = true;
            sizes.interactable = false;

            sizes.options.Add(
                new TMP_Dropdown.OptionData(
                    products.sizes[0].size));
        }
        else
        {
            sizes.interactable = true;


            sizes.options.Add(
                new TMP_Dropdown.OptionData(
                    "Size"));


            foreach (var option in products.sizes)
            {
                sizes.options.Add(
                    new TMP_Dropdown.OptionData(
                        option.size));
            }
        }


        sizes.value = 0;
        sizes.RefreshShownValue();


        SizeDropdownHandler sizeHandler =
            sizes.GetComponent<SizeDropdownHandler>();

        if (sizeHandler != null)
            sizeHandler.RefreshButtonSprite();
    }



    private void SetupProductData(
        CategoryManager.Products products)
    {
        DownloadState state =
            targetModel.GetComponent<DownloadState>();


        if (state == null)
        {
            state =
                targetModel.AddComponent<DownloadState>();
        }


        if (isDebug)
        {
            Debug.Log(
                "Model Ready: " +
                state.isReady +
                " Downloading: " +
                state.isDownloading);
        }



        if (!state.isReady &&
            !state.isDownloading)
        {
            currentCoroutine =
                StartCoroutine(
                    DownloadAndAssign(
                        products.threeDModels[0].url,
                        targetModel,
                        products));
        }
        else if (state.isReady)
        {
            mv.FrameObject(targetModel);
        }



        selectedProduct =
            targetModel.GetComponent<ProductDetails>();


        if (selectedProduct == null)
        {
            selectedProduct =
                targetModel.AddComponent<ProductDetails>();
        }



        selectedProduct.imagesUrl.Clear();


        foreach (var image in products.images)
        {
            selectedProduct.imagesUrl.Add(image.url);
        }



        selectedProduct.texturesUrl.Clear();


        foreach (var model in products.threeDModels)
        {
            selectedProduct.texturesUrl.Add(
                model.texture);
        }


        selectedProduct.product = products;
    }
    private void SetupProductImages(
        CategoryManager.Products products)
    {
        if (selectedProduct == null)
            return;


        if (selectedProduct.sprites.Count == 0)
        {
            StartCoroutine(SetProductImages(selectedProduct));
        }
    }

    private void SetupProductUI(
        CategoryManager.Products products)
    {
        itemName.text =
            products.company.entityName;


        itemType.text =
            products.name;


        stocksSelected.text = "1";


        itemPrice.text =
            products.displayPrice.range;


        itemPrice.color =
            Color.black;


        itemDiscountPrice.gameObject.SetActive(false);


        itemDiscription.text =
            products.description;


        companyDiscription.text =
            products.company.description;



        if (arRoomBtn != null)
        {
            arRoomBtn.onClick.RemoveAllListeners();

            arRoomBtn.onClick.AddListener(() =>
            {
                CheckObjectScene(products);
            });
        }



        modelViewerImage.enabled = false;

        modelViewer3D.enabled = true;


        currentImage = 0;


        txt360View.SetActive(true);


        if (IsInvoking(nameof(Disable360Hand)))
            CancelInvoke(nameof(Disable360Hand));


        Invoke(
            nameof(Disable360Hand),
            3f);


        ChangeModelTexture(0);
    }

#endregion

    #region Product Selection


    public void ChangeSize()
    {
        isSizeSelected = true;

        selectedSizeIndex = sizes.value;


        UpdateVariantForSelection();
    }



    public void ChangeItemAmount(int value)
    {
        int currentStocks =
            int.Parse(stocksSelected.text);


        currentStocks += value;


        int minStocks = maxStocks > 0 ? 1 : 0;


        if (currentStocks < minStocks)
            currentStocks = minStocks;


        if (currentStocks > maxStocks)
            currentStocks = maxStocks;


        if (currentStocks < minStocks)
            currentStocks = minStocks;


        stocksSelected.text =
            currentStocks.ToString();
    }



    private void DisableFirstItem(
        TMP_Dropdown dropdown)
    {
        Canvas canvas =
            dropdown.GetComponentInChildren<Canvas>();


        if (!canvas)
            return;


        Toggle[] toggles =
            canvas.GetComponentsInChildren<Toggle>();


        if (toggles.Length > 0)
            toggles[0].interactable = false;
    }


    #endregion

    #region Image Viewer


    private int currentImage = 0;


    public void NextPreviousImage(int index)
    {
        string code =
            selectedProduct.product
            .colors[selectedMatIndex]
            .code;


        currentImage += index;


        int count =
            selectedProduct.sprites.Count;


        if (currentImage < -1)
            currentImage = count - 1;


        if (currentImage >= count)
            currentImage = -1;



        if (currentImage != -1)
        {
            int startIndex = currentImage;


            while (
                selectedProduct.product.images[currentImage]
                .colorCode != code)
            {
                currentImage += index;


                if (currentImage < -1)
                    currentImage = count - 1;


                if (currentImage >= count)
                    currentImage = -1;


                if (currentImage == -1 ||
                    currentImage == startIndex)
                    break;
            }
        }



        if (currentImage == -1)
        {
            modelViewerImage.enabled = false;

            modelViewer3D.enabled = true;


            txt360View.SetActive(true);


            if (IsInvoking(nameof(Disable360Hand)))
                CancelInvoke(nameof(Disable360Hand));


            Invoke(
                nameof(Disable360Hand),
                3f);
        }
        else
        {
            modelViewer3D.enabled = false;

            modelViewerImage.enabled = true;


            txt360View.SetActive(false);


            if (selectedProduct.sprites[currentImage] != null)
            {
                modelViewerImage.sprite =
                    selectedProduct.sprites[currentImage];
            }
        }
    }



    private void Disable360Hand()
    {
        txt360View.SetActive(false);
    }


    #endregion



    #region Product Images


    private IEnumerator SetProductImages(
        ProductDetails pd)
    {
        yield return null;


        pd.sprites.Clear();


        for (int i = 0; i < pd.imagesUrl.Count; i++)
        {
            pd.sprites.Add(null);
        }


        for (int i = 0; i < pd.imagesUrl.Count; i++)
        {
            yield return StartCoroutine(
                DownloadSpriteCoroutine(
                    pd.imagesUrl[i],
                    pd.sprites,
                    i));
        }



        pd.textures.Clear();


        for (int i = 0; i < pd.texturesUrl.Count; i++)
        {
            pd.textures.Add(null);
        }


        for (int i = 0; i < pd.texturesUrl.Count; i++)
        {
            yield return StartCoroutine(
                DownloadTextureCoroutine(
                    pd.texturesUrl[i],
                    pd.textures,
                    i));
        }


        ChangeModelTexture(0);
    }



    public IEnumerator DownloadSpriteCoroutine(
        string url,
        List<Sprite> spritesList,
        int index)
    {
        UnityWebRequest request =
            UnityWebRequest.Get(url);


        yield return request.SendWebRequest();


        if (request.result != UnityWebRequest.Result.Success)
        {
            if (isDebug)
                Debug.LogError(request.error);

            yield break;
        }



        Texture2D texture =
            new Texture2D(2, 2);



        if (texture.LoadImage(
            request.downloadHandler.data))
        {
            spritesList[index] =
                Sprite.Create(
                    texture,
                    new Rect(
                        0,
                        0,
                        texture.width,
                        texture.height),
                    new Vector2(
                        0.5f,
                        0.5f));
        }
        else if (isDebug)
        {
            Debug.LogError(
                url +
                "\nFailed to decode image bytes.");
        }
    }



    public IEnumerator DownloadTextureCoroutine(
        string url,
        List<Texture2D> texturesList,
        int index)
    {
        UnityWebRequest request =
            UnityWebRequest.Get(url);


        yield return request.SendWebRequest();



        if (request.result != UnityWebRequest.Result.Success)
        {
            if (isDebug)
                Debug.LogError(request.error);

            yield break;
        }



        Texture2D texture =
            new Texture2D(2, 2);



        if (texture.LoadImage(
            request.downloadHandler.data))
        {
            texturesList[index] = texture;
        }
        else if (isDebug)
        {
            Debug.LogError(
                url +
                "\nFailed to decode texture bytes.");
        }
    }


    #endregion

    #region Model Variants


    public void ChangeModelTexture(int index)
    {
        foreach (Transform child in colorsParent)
        {
            ModelVariant variant =
                child.GetComponent<ModelVariant>();

            if (variant == null)
                continue;


            variant.selectedImg.SetActive(
                variant.index == index);
        }



        if (selectedProduct == null)
            return;


        selectedMatIndex = index;


        UpdateVariantForSelection();



        if (index >= selectedProduct.textures.Count)
            return;


        Transform visual =
            targetModel.transform.Find("Visual");


        if (visual == null)
            return;


        MeshRenderer renderer =
            visual.GetComponent<MeshRenderer>();


        if (renderer == null)
            return;



        Material material =
            renderer.material;


        material.mainTexture =
            selectedProduct.textures[index];


        renderer.material =
            material;



        if (FavoritesManager.Instance != null)
            FavoritesManager.Instance.RefreshCurrentToggleIcon();
    }



    public void ChangeModelVariant()
    {
        modelViewerImage.enabled = false;

        modelViewer3D.enabled = true;

        currentImage = 0;
    }


    private void UpdateVariantForSelection()
    {
        if (selectedProduct == null ||
            selectedProduct.product == null)
            return;

        if (selectedMatIndex < 0 ||
            selectedMatIndex >= selectedProduct.product.colors.Count)
            return;

        bool multiSize =
            selectedProduct.product.sizes.Count > 1;

        string colorId =
            selectedProduct.product.colors[selectedMatIndex].id;

        if (multiSize && !isSizeSelected)
        {
            maxStocks = 0;
            lastResolvedVariantId = null;
            stocksSelected.text = "0";
            itemPrice.text =
                selectedProduct.product.displayPrice.range;
            itemPrice.color =
                Color.black;
            itemDiscountPrice.gameObject.SetActive(false);
            return;
        }

        string sizeId = null;

        if (multiSize)
        {
            int realIdx = selectedSizeIndex - 1;

            if (realIdx < 0 ||
                realIdx >= selectedProduct.product.sizes.Count)
            {
                maxStocks = 0;
                lastResolvedVariantId = null;
                stocksSelected.text = "0";
                return;
            }

            sizeId =
                selectedProduct.product.sizes[realIdx].id;
        }
        else if (selectedProduct.product.sizes.Count == 1)
        {
            sizeId =
                selectedProduct.product.sizes[0].id;
        }

        CategoryManager.Variants variant = null;

        foreach (var v in selectedProduct.product.variants)
        {
            if (v.color.id != colorId)
                continue;

            if (string.IsNullOrEmpty(sizeId))
            {
                if (v.size == null)
                {
                    variant = v;
                    break;
                }
            }
            else if (v.size != null &&
                     v.size.id == sizeId)
            {
                variant = v;
                break;
            }
        }

        if (variant == null)
        {
            maxStocks = 0;
            lastResolvedVariantId = null;
            stocksSelected.text = "0";
            return;
        }

        maxStocks = variant.stock;

        if (variant.id != lastResolvedVariantId)
        {
            lastResolvedVariantId = variant.id;
            stocksSelected.text = maxStocks > 0 ? "1" : "0";
        }

        if (variant.salePrice > 0 &&
            variant.price > variant.salePrice)
        {
            itemPrice.text =
                "<s>$" + variant.price + "</s>";

            itemPrice.color =
                new Color(
                    0.7f,
                    0.7f,
                    0.7f);

            itemDiscountPrice.text =
                "$" + variant.salePrice;

            itemDiscountPrice.gameObject.SetActive(true);
        }
        else
        {
            itemPrice.text =
                "$" + variant.price;

            itemPrice.color =
                Color.black;

            itemDiscountPrice.gameObject.SetActive(false);
        }
    }


    #endregion

    #region Navigation
    public void BackToShop()
    {
        shopPanel.SetActive(true);
        itemViewBG.SetActive(false);
        itemViewPanel.SetActive(false);

        foreach (Transform obj in UI_3D_Models_Parent.transform)
        {
            var state = obj.GetComponent<DownloadState>();

            if (state != null)
            {
                if (state.isReady)
                {
                    obj.gameObject.SetActive(false);
                }
                else if (!state.isDownloading)
                {
                    Destroy(obj.gameObject);
                }
                else
                {
                    obj.gameObject.SetActive(false);
                }
            }
            else
            {
                Destroy(obj.gameObject);
            }
        }
    }

    #endregion

    #region AR
    private void CheckObjectScene(
        CategoryManager.Products p)
    {
        if (p.ar_type.Equals(
            "vertical-plane detection") ||
            p.ar_type.Equals(
            "horizontal-plane detection"))
        {
            ProductSelection.ClearSelection();


            ProductSelection.SetSelection(
                p,
                false,
                "",
                p.ar_type.Equals(
                    "horizontal-plane detection"),
                p.threeDModels[0].url);


            SceneManager.LoadScene(
                SceneNames.ARScene);
        }
        else if (p.ar_type.Equals("face-tracking"))
        {
            CategoryType category;

            ProductSelection.TryParseObjectType(
                p.name,
                out category);


            ProductSelection.ClearSelection();


            ProductSelection.SetSelection(
                p,
                true,
                p.category.name,
                false,
                p.threeDModels[0].url);


            SceneManager.LoadScene(
                SceneNames.ARFace);
        }

#if UNITY_IOS

    else if (p.ar_type.Equals("hand-tracking"))
    {
        CategoryType category;

        ProductSelection.TryParseObjectType(
            p.name,
            out category);


        ProductSelection.ClearSelection();


        ProductSelection.SetSelection(
            p,
            true,
            p.category.name,
            false,
            p.threeDModels[0].url);


        SceneManager.LoadScene(
            SceneNames.HandTracking);
    }


    else if (p.ar_type.Equals("body-tracking"))
    {
        if (!SceneManager
            .GetActiveScene()
            .name
            .Equals(SceneNames.ARBodyTrackingMars))
        {
            CategoryType category;

            ProductSelection.TryParseObjectType(
                p.name,
                out category);


            ProductSelection.ClearSelection();


            ProductSelection.SetSelection(
                p,
                false,
                p.category.name,
                false,
                p.threeDModels[0].url);


            LoaderUtility.Deinitialize();


            SceneManager.LoadScene(
                SceneNames.ARBodyTrackingMars);
        }
    }

#endif
    }

    public void LoadARScene()
    {
        SceneManager.LoadScene(SceneNames.ARScene);
    }

    #endregion



    #region Cleanup


    public void ChangeModelColor(Image img)
    {
        MeshRenderer renderer =
            targetModel
            .GetComponentInChildren<MeshRenderer>();


        if (renderer != null)
            renderer.material.color = img.color;
    }



    private void ClearProducts()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }



    private void ShowStatus(
        string message,
        bool isError)
    {
        statusText.gameObject.SetActive(true);

        statusText.text =
            message;


        statusText.color =
            isError
                ? Color.red
                : Color.white;
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
        public bool isDownloading = false;

        public bool isReady = false;
    }


    #endregion
}