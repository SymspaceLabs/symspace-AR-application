using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using UnityGLTF;

public class CategoryManager : MonoBehaviour
{
    #region API URLs
    private string getAllProductsURL = "/products";
    private string getAllCategoriesURL = "/categories/mobile";
    #endregion

    #region Inspector Variables - Main Categories
    [Header("Main Categories System (Legacy)")]
    public List<MainCategory> mainCategories;
    public List<UnityEngine.UI.Image> mainCategoriesImages;
    public Transform subcategoryButtonContainer;
    public Button subcategoryButtonPrefab;
    public List<UnityEngine.UI.Image> subCategoriesImages;
    public Transform productContainer;
    public GameObject productCardPrefab;
    public UnityEngine.Color selectedBgColor;
    public UnityEngine.Color unselectedBgColor;
    private MainCategory currentCategory;
    #endregion

    #region AR References
    [Header("AR References")]
    public ObjectSpawner spawner;
    public ARJewelryManager arJewelryManager;
    public GameObject[] prefabs;
    public GameObject glbPrafabHorizontal;
    public GameObject glbPrafabVertical;
    #endregion

    #region Categories UI (New System)
    [Header("Categories UI (New System)")]
    public Transform topLevelCategoryParent;
    public GameObject categoryButtonPrefab;
    public Transform subCategoryParent;
    public Transform leafCategoryParent;
    public RootData cachedCategories;
    public ProductResponse allProductsData;
    #endregion

    #region Private Variables
    private string localPath;
    #endregion

    #region Unity Lifecycle Methods
    private void Start()
    {
        localPath = Path.Combine(Application.persistentDataPath, "tempModel.glb");
        GetAllCategories();
        StartCoroutine(InitialProductSelection());
    }
    #endregion

    #region Initialization Coroutines
    IEnumerator InitialProductSelection()
    {
        yield return null;
        if (ProductSelection.productData != null)
        {
            if (SceneManager.GetActiveScene().name == "AR Scene")
                yield return StartCoroutine(DownloadImage(ProductSelection.productData.images[1].url));
            StartCoroutine(ProductSelectedFunction(ProductSelection.productData,
                ProductSelection.productData.threeDModels[0].url,
                ProductSelection.productData.ar_type,
                ProductSelection.fetchedSprite));
        }
    }
    #endregion

    #region API Calls
    public void GetAllCategories()
    {
        StartCoroutine(AuthAPI.PostRequest(getAllCategoriesURL, "",
        (response) =>
        {
            string fixedJson = "{\"categories\":" + response + "}";
            RootData responseData = JsonUtility.FromJson<RootData>(fixedJson);
            Debug.Log("Categories loaded");
            LoadCategories(responseData);
        },
        (error) =>
        {
            Debug.LogError("Failed to load categories: " + error);
        }, "GET"));
    }

    void GetAllProducts(string type)
    {
        string formattedType = type.ToLower().Replace("'", "").Replace(" ", "-");
        Debug.Log("corrected formate : " + formattedType);
        StartCoroutine(AuthAPI.PostRequest(getAllProductsURL + "?subcategoryItem=" + formattedType, "",
        (response) =>
        {
            ProductResponse responseData = JsonUtility.FromJson<ProductResponse>(response);
            allProductsData = responseData;
            Debug.Log("Products loaded");
            PopulateProducts(responseData);
        },
        (error) =>
        {
            Debug.LogError("Failed to load categories: " + error);
        }, "GET"));
    }
    #endregion

    #region Image Download Methods
    public IEnumerator DownloadImage(string url, UnityEngine.UI.Image imageComponent = null)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            byte[] imageData = request.downloadHandler.data;

            // Check what kind of data we actually got
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
                if (imageComponent != null)
                    imageComponent.sprite = sprite;

                ProductSelection.fetchedSprite = sprite;
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
    }

    public IEnumerator DownloadSpriteCoroutine(string url, Action<Sprite> callback)
    {
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(req.error);
            callback?.Invoke(null);
            yield break;
        }

        byte[] imageData = req.downloadHandler.data;
        Texture2D texture = new Texture2D(2, 2);

        if (texture.LoadImage(imageData))
        {
            Sprite sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            callback?.Invoke(sprite);
        }
        else
        {
            Debug.LogError(url + "\n❌ Failed to decode image bytes. Response is not a valid PNG/JPG.");
            callback?.Invoke(null);
        }
    }
    #endregion

    #region Product Selection & Scene Management
    IEnumerator ProductSelectedFunction(Products p, string url, string arType, Sprite sprite)
    {
        if (CheckObjectScene(p))
        {
            if (SceneManager.GetActiveScene().name == "AR Face")
            {
                arJewelryManager.JewelrySelected(p, p.threeDModels[0].url, p.category.name);
            }
            else if (SceneManager.GetActiveScene().name == "AR Scene")
            {
                #region plane Detection Scene Code
                string type = null;

                if (p.category.id != null)
                    type = p.ar_type;
                else
                    type = p.ar_type;

                GameObject newObject;

                if (type.Equals("horizontal-plane detection"))
                    newObject = Instantiate(glbPrafabHorizontal);
                else
                    newObject = Instantiate(glbPrafabVertical);

                newObject.SetActive(false);
                newObject.name = GetUniqueName(p.name, spawner.objectsSpawned);

                spawner.objectPrefabs.Clear();
                spawner.objectPrefabs.Add(newObject);

                UIManagerAR.instance.eventSystem.gameObject.SetActive(false);
                yield return StartCoroutine(DownloadAndAssign(url, newObject, p));
                UIManagerAR.instance.eventSystem.gameObject.SetActive(true);

                EventSystem.current.gameObject.SetActive(true);
                spawner.object1Spawned = false;
                spawner.objectIndex = 0;
                spawner.objectsSize[0].length = p.sizes[0].dimensions.length;
                spawner.objectsSize[0].width = p.sizes[0].dimensions.width;
                spawner.objectsSize[0].height = p.sizes[0].dimensions.height;
                spawner.unit = p.sizes[0].dimensions.unit;
                UIManagerAR.instance.itemsToPlaceParent.SetActive(true);

                UnityEngine.Color newColor;
                if (p.threeDModels.Count > 0 && ColorUtility.TryParseHtmlString(p.threeDModels[0].colorCode, out newColor))
                {
                    spawner.objectColors[0] = newColor;
                    UIManagerAR.instance.item1.color = newColor;
                    UIManagerAR.instance.item1.sprite = sprite;
                }

                if (p.threeDModels.Count > 1 && ColorUtility.TryParseHtmlString(p.threeDModels[1].colorCode, out newColor))
                {
                    spawner.objectColors[1] = newColor;
                    UIManagerAR.instance.item2.color = newColor;
                    UIManagerAR.instance.item2.sprite = sprite;
                }

                if (p.threeDModels.Count > 2 && ColorUtility.TryParseHtmlString(p.threeDModels[2].colorCode, out newColor))
                {
                    spawner.objectColors[2] = newColor;
                    UIManagerAR.instance.item3.color = newColor;
                    UIManagerAR.instance.item3.sprite = sprite;
                }

                if (type.Equals("horizontal-plane detection"))
                {
                    newObject.GetComponent<ARTransformer>().objectPlaneTranslationMode = ARTransformer.PlaneTranslationMode.Horizontal;
                    newObject.GetComponent<ARObjectManipulator>().orientation = ARObjectManipulator.Orientation.Horizontal;
                }
                else if (type.Equals("vertical-plane detection"))
                {
                    newObject.GetComponent<ARTransformer>().objectPlaneTranslationMode = ARTransformer.PlaneTranslationMode.Vertical;
                    newObject.GetComponent<ARObjectManipulator>().orientation = ARObjectManipulator.Orientation.Vertical;
                }

                UIManagerAR.instance.TogglePlaneVisuals(true);
                #endregion
            }

            GetComponent<SlideUpPanel>().HidePanel();
        }
        yield return null;
    }

    bool CheckObjectScene(Products p)
    {
        if (p.ar_type.Equals("vertical-plane detection") || p.ar_type.Equals("horizontal-plane detection"))
        {
            if (SceneManager.GetActiveScene().name != "AR Scene")
            {
                ProductSelection.ClearSelection();
                ProductSelection.SetSelection(p, false, "", p.ar_type.Equals("horizontal-plane detection"), p.threeDModels[0].url);
                UIManagerAR.instance.ChangeARScene("AR Scene");
                return false;
            }
            return true;
        }
        else if (p.ar_type.Equals("face-tracking"))
        {
            if (SceneManager.GetActiveScene().name != "AR Face")
            {
                CategoryType category;
                ProductSelection.TryParseObjectType(p.name, out category);
                ProductSelection.ClearSelection();
                ProductSelection.SetSelection(p, true, p.category.name, false, p.threeDModels[0].url);
                UIManagerAR.instance.ChangeARScene("AR Face");
                return false;
            }
            return true;
        }
        else if (p.ar_type.Equals("hand-tracking"))
        {
            if (SceneManager.GetActiveScene().name != "Hand Tracking")
            {
                CategoryType category;
                ProductSelection.TryParseObjectType(p.name, out category);
                ProductSelection.ClearSelection();
                ProductSelection.SetSelection(p, false, p.category.name, false, p.threeDModels[0].url);
                UIManagerAR.instance.ChangeARScene("Hand Tracking");
                return false;
            }
            else if (SceneManager.GetActiveScene().name == "Hand Tracking")
            {
                if (HandItemSelector.Instance != null)
                {
                    ProductSelection.ClearSelection();
                    ProductSelection.SetSelection(p, false, p.category.name, false, p.threeDModels[0].url);
                    HandItemSelector.Instance.SelectItem(p.name, p.category.name);
                }
                return false;
            }
        }
        return true;
    }

    public string GetUniqueName(string baseName, List<GameObject> existingObjects)
    {
        string newName = baseName;
        int counter = 1;

        while (existingObjects.Exists(obj => obj.name == newName))
        {
            newName = $"{baseName}_{counter}";
            counter++;
        }

        return newName;
    }
    #endregion

    #region 3D Model Download & Assignment
    IEnumerator DownloadAndAssign(string url, GameObject targetObject, Products p)
    {
        yield return null;
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.downloadHandler = new DownloadHandlerFile(localPath);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Download failed: " + www.error);
                yield break;
            }
        }

        // Load GLB with UnityGLTF
        using (FileStream stream = new FileStream(localPath, FileMode.Open, FileAccess.Read))
        {
            var importOptions = new ImportOptions();
            var importer = new GLTFSceneImporter(stream, importOptions);
            yield return importer.LoadSceneAsync();

            GameObject loadedRoot = importer.LastLoadedScene;

            if (loadedRoot == null)
            {
                Debug.LogError("❌ Failed to load GLB: no root GameObject returned");
                yield return null;
            }

            MeshFilter srcMF = loadedRoot.GetComponentInChildren<MeshFilter>();
            MeshRenderer srcMR = loadedRoot.GetComponentInChildren<MeshRenderer>();

            if (srcMF == null || srcMR == null)
            {
                Debug.LogError("Loaded model has no MeshFilter or MeshRenderer!");
                yield return null;
            }

            MeshFilter targetMF = targetObject.transform.Find("Visual").GetComponent<MeshFilter>() ?? targetObject.transform.Find("Visual").gameObject.AddComponent<MeshFilter>();
            MeshRenderer targetMR = targetObject.transform.Find("Visual").GetComponent<MeshRenderer>() ?? targetObject.transform.Find("Visual").gameObject.AddComponent<MeshRenderer>();
            MeshCollider targetMC = targetObject.transform.Find("Visual").GetComponent<MeshCollider>() ?? targetObject.transform.Find("Visual").gameObject.AddComponent<MeshCollider>();

            targetMF.mesh = Instantiate(srcMF.sharedMesh);
            targetMR.materials = srcMR.materials.Clone() as Material[];

            Material[] newMaterials = new Material[srcMR.materials.Length];
            for (int i = 0; i < srcMR.materials.Length; i++)
            {
                newMaterials[i] = new Material(srcMR.materials[i]);
            }
            targetMR.materials = newMaterials;
            targetMC.sharedMesh = targetMF.mesh;

            targetMF.GetComponent<ARDimensionVisualizer>().enabled = true;

            GameObject modelToView = Instantiate(UIManagerAR.instance.modelPrefab);
            modelToView.transform.parent = UIManagerAR.instance.UI_3D_Models_Parent.transform;
            modelToView.transform.localPosition = Vector3.zero;
            UIManagerAR.instance.UI_3D_Models.Add(modelToView);

            ProductDetails pd = modelToView.AddComponent<ProductDetails>();
            pd.imagesUrl.Clear();
            foreach (var img in p.images)
                pd.imagesUrl.Add(img.url);

            pd.product = p;

            modelToView.transform.Find("Visual").GetComponent<MeshFilter>().mesh = targetMF.mesh;
            modelToView.transform.Find("Visual").GetComponent<MeshRenderer>().materials = targetMR.materials;
            modelToView.name = targetObject.name;

            Debug.Log("✅ Mesh and materials assigned (instantiated copies)!");
            Destroy(loadedRoot);
        }
    }
    #endregion

    #region Categories Population (New System)
    public void LoadCategories(RootData data)
    {
        cachedCategories = data;
        PopulateTopLevelCategories(data.categories);
    }

    private void PopulateTopLevelCategories(List<CategoryData> Categories)
    {
        ClearAllUI();

        Debug.Log("Categories : " + Categories.Count);
        foreach (var category in Categories)
        {
            GameObject buttonObj = Instantiate(categoryButtonPrefab, topLevelCategoryParent);
            buttonObj.name = category.name;

            //mainCategoriesImages.Add(buttonObj.GetComponent<UnityEngine.UI.Image>());

            TextMeshProUGUI label = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = category.name;

            Button btn = buttonObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() =>
                {
                    PopulateSubCategories(category.items);
                    UnSelectAllImages(topLevelCategoryParent);
                    SelectedImage(buttonObj.GetComponent<UnityEngine.UI.Image>());
                });
            }
        }
    }

    private void PopulateSubCategories(List<Items> subCategories)
    {
        ClearSubCategoriesUI();

        foreach (var sub in subCategories)
        {
            GameObject buttonObj = Instantiate(categoryButtonPrefab, subCategoryParent);
            buttonObj.name = sub.name;

            TextMeshProUGUI label = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = sub.name;

            bool productsSpawned = false;

            Button btn = buttonObj.GetComponent<Button>();
            Debug.Log("Button found on " + buttonObj.name);
            if (btn != null)
            {
                btn.onClick.AddListener(() =>
                {
                    string formattedType = sub.name.Replace(" ", "-");
                    Debug.Log("corrected formate : " + formattedType);
                    StartCoroutine(AuthAPI.PostRequest(getAllProductsURL + "?subcategoryItem=" + formattedType, "",
                    (response) =>
                    {
                        ProductResponse responseData = JsonUtility.FromJson<ProductResponse>(response);
                        productsSpawned = PopulateProducts(responseData);
                        
                        if (sub.items != null && sub.items.Count > 0)
                        {
                            PopulateLeafCategories(sub.items);
                            Debug.Log("Product Spawned " + productsSpawned);
                            if (productsSpawned)
                                leafCategoryParent.gameObject.SetActive(false);
                            else
                                leafCategoryParent.gameObject.SetActive(true);


                        }
                    },
                    (error) =>
                    {
                        Debug.LogError("Failed to load categories: " + error);
                    }, "GET"));


                    UnSelectAllImages(subCategoryParent);
                    SelectedImage(buttonObj.GetComponent<UnityEngine.UI.Image>());
                });
            }
        }
    }

    private void PopulateLeafCategories(List<string> leafNodes)
    {
        foreach (Transform child in leafCategoryParent)
            Destroy(child.gameObject);

        foreach (var leaf in leafNodes)
        {
            GameObject buttonObj = Instantiate(categoryButtonPrefab, leafCategoryParent);
            TextMeshProUGUI label = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = leaf;

            Button btn = buttonObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() =>
                {
                    if (leaf != null)
                    {
                        GetAllProducts(leaf);
                        UnSelectAllImages(leafCategoryParent);
                        SelectedImage(buttonObj.GetComponent<UnityEngine.UI.Image>());
                    }
                });
            }
        }
    }

    private void ClearAllUI()
    {
        ClearTransform(topLevelCategoryParent);
        mainCategoriesImages.Clear();

        ClearTransform(subCategoryParent);
        subCategoriesImages.Clear();

        ClearTransform(leafCategoryParent);
        ClearTransform(productContainer);
    }

    private void ClearSubCategoriesUI()
    {
        ClearTransform(subCategoryParent);
        ClearTransform(leafCategoryParent);
        ClearTransform(productContainer);
    }

    private void ClearTransform(Transform parent)
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);
    }
    #endregion

    #region Products Population
    bool PopulateProducts(ProductResponse response)
    {
        ClearTransform(productContainer);

        Debug.Log("Products count : " + response.products.Count);
        if (response.products.Count == 0)
            return false;

        foreach (var p in response.products)
        {
            GameObject card = Instantiate(productCardPrefab, productContainer);
            card.transform.Find("Border/ItemName").GetComponent<TextMeshProUGUI>().text = p.company.entityName;
            card.transform.Find("Border/ItemType").GetComponent<TextMeshProUGUI>().text = p.name;

            if (p.displayPrice.salePrice < p.displayPrice.price)
            {
                card.transform.Find("Border/Price").GetComponent<TextMeshProUGUI>().text = "<s>$" + p.displayPrice.price + "</s>";
                card.transform.Find("Border/Price").GetComponent<TextMeshProUGUI>().color = new UnityEngine.Color(0.7f, 0.7f, 0.7f);

                card.transform.Find("Border/DiscountPrice").GetComponent<TextMeshProUGUI>().enabled = true;
            }
            else
            {
                card.transform.Find("Border/Price").GetComponent<TextMeshProUGUI>().text = "$" + p.displayPrice.price;
                card.transform.Find("Border/Price").GetComponent<TextMeshProUGUI>().color = new UnityEngine.Color(1, 1, 1);

                card.transform.Find("Border/DiscountPrice").GetComponent<TextMeshProUGUI>().enabled = false;
            }

            card.transform.Find("Border/DiscountPrice").GetComponent<TextMeshProUGUI>().text = "$" + p.displayPrice.salePrice;
            if(p.images.Count > 0)
                StartCoroutine(DownloadImage(p.images[0].url, card.transform.Find("Border/ProductImage").GetComponent<UnityEngine.UI.Image>()));
            card.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(ProductSelectedFunction(p, p.threeDModels[0].url, p.ar_type, card.transform.Find("Border/ProductImage").GetComponent<UnityEngine.UI.Image>().sprite)));
        }

        return true;
    }
    #endregion

    #region Legacy Main Categories System (Button Callbacks)
    public void SelectedImage(UnityEngine.UI.Image img)
    {
        img.color = selectedBgColor;
    }

    public void UnSelectAllImages(Transform parent)
    {
        if (parent != null)
        {
            foreach(Transform obj in parent)
            {
                obj.GetComponent<UnityEngine.UI.Image>().color = unselectedBgColor;
            }
        }
    }

    public void OnMainCategorySelected(string categoryName)
    {
        foreach (var image in mainCategoriesImages)
        {
            image.color = unselectedBgColor;
        }

        currentCategory = mainCategories.Find(c => c.name == categoryName);

        ClearTransform(subcategoryButtonContainer);
        ClearTransform(productContainer);

        if (currentCategory == null) return;

        subCategoriesImages.Clear();

        // Create new subcategory buttons
        foreach (Subcategory sub in currentCategory.subcategories)
        {
            Button btn = Instantiate(subcategoryButtonPrefab, subcategoryButtonContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = sub.name;
            btn.onClick.AddListener(() => OnSubcategorySelected(sub));
            btn.onClick.AddListener(() => SelectedImage(btn.GetComponent<UnityEngine.UI.Image>()));
            subCategoriesImages.Add(btn.GetComponent<UnityEngine.UI.Image>());
        }

        // Auto-load first subcategory
        if (currentCategory.subcategories.Count > 0)
        {
            OnSubcategorySelected(currentCategory.subcategories[0]);
            subCategoriesImages[0].color = selectedBgColor;
        }
    }

    public void OnSubcategorySelected(Subcategory subcategory)
    {
        ClearTransform(productContainer);

        foreach (var img in subCategoriesImages)
        {
            img.color = unselectedBgColor;
        }

        // Load new products
        foreach (Product p in subcategory.products)
        {
            GameObject card = Instantiate(productCardPrefab, productContainer);
            card.transform.Find("Border/ItemName").GetComponent<TextMeshProUGUI>().text = p.itemName;
            card.transform.Find("Border/ItemType").GetComponent<TextMeshProUGUI>().text = p.itemType;

            if (p.discountPrice.Length > 0)
            {
                card.transform.Find("Border/Price").GetComponent<TextMeshProUGUI>().text = "<s>" + p.price + "</s>";
                card.transform.Find("Border/Price").GetComponent<TextMeshProUGUI>().color = new UnityEngine.Color(0.7f, 0.7f, 0.7f);
            }
            else
            {
                card.transform.Find("Border/Price").GetComponent<TextMeshProUGUI>().text = p.price;
                card.transform.Find("Border/Price").GetComponent<TextMeshProUGUI>().color = new UnityEngine.Color(1, 1, 1);
            }

            card.transform.Find("Border/DiscountPrice").GetComponent<TextMeshProUGUI>().text = p.discountPrice;
            card.transform.Find("Border/ProductImage").GetComponent<UnityEngine.UI.Image>().sprite = p.image;
        }
    }
    #endregion

    #region Helper Methods
    public void SetProductImages(ProductDetails pd)
    {
        try
        {
            pd.sprites.Clear();
            foreach (var url in pd.imagesUrl)
            {
                StartCoroutine(DownloadSpriteCoroutine(url, sprite =>
                {
                    pd.sprites.Add(sprite);
                }));
            }
        }
        catch (Exception ex)
        {
            Debug.Log("Sprites Download failed: " + ex.Message);
        }
    }
    #endregion

    #region Data Classes - Legacy System
    [System.Serializable]
    public class Product
    {
        public string itemName;
        public string itemType;
        public string price;
        public string discountPrice;
        public Sprite image;

        public float width;
        public float depth;
        public float height;

        public List<Texture> texture;

        public string unit;

        public bool horizontal;
        public bool isFaceObject = false;
        public bool isFurniture = false;
        public bool isHandObject = false;
        public bool isBodyObject = false;

        public string categoryType;
    }

    [System.Serializable]
    public class Subcategory
    {
        public string name;
        public List<Product> products;
    }

    [System.Serializable]
    public class MainCategory
    {
        public string name;
        public List<Subcategory> subcategories;
    }
    #endregion

    #region Data Classes - API Structure
    #region Product API Structure
    [System.Serializable]
    public class ProductResponse
    {
        public List<Products> products;
        public List<Brand> brands;
        public PriceRange priceRange;
        public List<Category> category;
        public List<string> genders;
        public List<string> availabilities;
        public List<ColorInfo> colors;
    }

    [System.Serializable]
    public class Products
    {
        public string id;
        public string name;
        public string slug;
        public string description;
        public string material;
        public string gender;
        public string ar_type;

        public Company company;
        public Category category;

        public List<ImageData> images;
        public List<ThreeDModel> threeDModels;
        public List<ColorData> colors;
        public List<SizeData> sizes;

        public DisplayPrice displayPrice;
        public string availability;

    }

    [System.Serializable]
    public class ImageData
    {
        public string url;
        public string colorCode;
        public string colorId;
        public int sortOrder;
    }

    [System.Serializable]
    public class ColorData
    {
        public string id;
        public string name;
        public string code;
    }

    [System.Serializable]
    public class SizeData
    {
        public string id;
        public string size;
        public int sortOrder;
        public string sizeChartUrl;
        public Dimensions dimensions;
        public string productWeight;
    }

    [System.Serializable]
    public class DisplayPrice
    {
        public float price;
        public float salePrice;
        public bool hasSale;
    }


    [System.Serializable]
    public class ProductWeight
    {
        public string unit;
        public float? value;
    }

    [System.Serializable]
    public class Dimensions
    {
        public string unit;
        public float width;
        public float height;
        public float length;
    }

    [System.Serializable]
    public class Company
    {
        public string id;
        public string entityName;
        public string website;
        public string location;
        public string ein;
        public string userId;
        public string slug;
        public string legalName;
        public string address1;
        public string address2;
        public string city;
        public string state;
        public string country;
        public string zip;
        public float? gmv;
        public string category;
        public string businessPhone;
        public string emailSupport;
        public string phoneSupport;
        public string description;
        public string tagLine;
        public string logo;
        public string banner;
        public string web;
        public string instagram;
        public string twitter;
        public string youtube;
        public string facebook;
        public bool isOnboardingFormFilled;
    }

    [System.Serializable]
    public class Image
    {
        public string id;
        public string url;
        public string altText;
        public int sortOrder;
        public string colorCode;
    }

    [System.Serializable]
    public class Color
    {
        public string id;
        public string name;
        public string code;
        public string createdAt;
        public string updatedAt;
    }

    [System.Serializable]
    public class Size
    {
        public string id;
        public string size;
        public int sortOrder;
    }

    [System.Serializable]
    public class SubcategoryItem
    {
        public string id;
        public string name;
        public string slug;
        public string subcategoryId;
        public List<string> tags_required;
        public List<string> optional_tags;
        public TagDefaults tag_defaults;
        public string mobileLevel2;
        public string mobileLevel2Name;
        public Subcategorys subcategory;
    }

    [System.Serializable]
    public class TagDefaults
    {
        public string ar_type;
        public string indoor_outdoor;
        public bool? accessible;
    }

    [System.Serializable]
    public class Subcategorys
    {
        public string id;
        public string name;
        public string categoryId;
        public string slug;
        public string mobileLevel1;
        public Category category;
    }

    [System.Serializable]
    public class Category
    {
        public string id;
        public string name;
        public string slug;
    }

    [System.Serializable]
    public class SubcategoryItemChild
    {
        public string id;
        public string name;
        public string slug;
        public List<string> tags_required;
        public List<string> optional_tags;
        public TagDefaultsChild tag_defaults;
        public string subCategoryItemId;
        public string mobileLevel3;
        public string mobileLevel3Name;
    }

    [System.Serializable]
    public class TagDefaultsChild
    {
        public string ar_type;
    }

    [System.Serializable]
    public class Variant
    {
        public string id;
        public float price;
        public int stock;
        public bool isActive;
        public string sku;
        public float? salePrice;
        public ProductWeight productWeight;
        public Dimensions dimensions;
        public string sizeChart;
        public string sizeFit;
        public Color color;
        public Size size;
    }

    [System.Serializable]
    public class ThreeDModel
    {
        public string id;
        public string url;
        public string colorCode;
        public List<float> pivot;
        public string format;
        public BoundingBox boundingBox;
    }

    [System.Serializable]
    public class BoundingBox
    {
        public List<float> max;
        public List<float> min;
    }

    [System.Serializable]
    public class Brand
    {
        public string id;
        public string entityName;
    }

    [System.Serializable]
    public class PriceRange
    {
        public float min;
        public float max;
    }

    [System.Serializable]
    public class CategoryWithChild
    {
        public string title;
        public string slug;
        public List<SubcategoryChild> child;
    }

    [System.Serializable]
    public class SubcategoryChild
    {
        public string id;
        public string name;
        public string slug;
    }

    [System.Serializable]
    public class ColorInfo
    {
        public string name;
        public string code;
    }
    #endregion

    #region Categories API Structure
    [Serializable]
    public class RootData
    {
        public List<CategoryData> categories;
    }

    [Serializable]
    public class CategoryData
    {
        public string name;
        public List<Items> items;
    }

    [Serializable]
    public class Items
    {
        public string name;
        public List<string> items;
    }
    #endregion
    #endregion
}