using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityGLTF;
using static OnBoardingUI;

public class CategoriesUI : MonoBehaviour
{
    private string getProductsURL = "/products?subcategory";

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

    GameObject targetModel;
    public GameObject modelPrefab;
    public GameObject UI_3D_Models_Parent;
    private string localPath;

    public Transform colorsParent;
    public GameObject colorPrefab;

    public GameObject colorVariant1Parent;
    public GameObject colorVariant2Parent;
    public GameObject colorVariant3Parent;

    public Image colorVariant1;
    public Image colorVariant2;
    public Image colorVariant3;

    public ModelViewer mv;

    public float cameraOffset = 1.2f;

    private void OnEnable()
    {
        statusText.gameObject.SetActive(false);
        //LoadCategories();
    }

    void Start()
    {
        localPath = Path.Combine(Application.persistentDataPath, "tempModel.glb");
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

        // --- Step 2: Load GLB with UnityGLTF ---
        using (FileStream stream = new FileStream(localPath, FileMode.Open, FileAccess.Read))
        {
            var importOptions = new ImportOptions();

            var importer = new GLTFSceneImporter(stream, importOptions);
            yield return importer.LoadSceneAsync();     // This does NOT replace your Unity scene!

            GameObject loadedRoot = importer.LastLoadedScene;

            if (loadedRoot == null)
            {
                Debug.LogError("❌ Failed to load GLB: no root GameObject returned");
                yield return null;
            }

            // --- Step 3: Extract mesh + material ---
            MeshFilter srcMF = loadedRoot.GetComponentInChildren<MeshFilter>();
            MeshRenderer srcMR = loadedRoot.GetComponentInChildren<MeshRenderer>();

            if (srcMF == null || srcMR == null)
            {
                Debug.LogError("Loaded model has no MeshFilter or MeshRenderer!");
                yield return null;
            }

            // --- Step 4: Assign to your target GameObject ---
            MeshFilter targetMF = targetObject.transform.Find("Visual").GetComponent<MeshFilter>() ?? targetObject.transform.Find("Visual").gameObject.AddComponent<MeshFilter>();
            MeshRenderer targetMR = targetObject.transform.Find("Visual").GetComponent<MeshRenderer>() ?? targetObject.transform.Find("Visual").gameObject.AddComponent<MeshRenderer>();
           
            // ✅ CREATE INSTANCES instead of using shared references
            targetMF.mesh = Instantiate(srcMF.sharedMesh); // Creates a copy
            targetMR.materials = srcMR.materials.Clone() as Material[]; // Creates material copies

            // For materials, we need to instantiate each one
            Material[] newMaterials = new Material[srcMR.materials.Length];
            for (int i = 0; i < srcMR.materials.Length; i++)
            {
                newMaterials[i] = new Material(srcMR.materials[i]); // Create material instance
            }
            targetMR.materials = newMaterials;
            targetModel.GetComponentInChildren<MeshRenderer>().material.color = colorVariant1.color;
            //targetModel.GetComponentInChildren<MeshRenderer>().material.SetFloat("metallicFactor", 1f);

            Debug.Log("✅ Mesh and materials assigned (instantiated copies)!");

            targetObject.SetActive(true);

            //if (p.ar_type.Equals("horizontal-plane detection"))
            //{
            //    UpdateObjectScale(p, targetObject, false);
            //}
            //else
            //{
            //    UpdateObjectScale(p, targetObject, true);
            //}

            mv.FrameObject(targetObject/*, cameraOffset*/);

            // Optional cleanup
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

            if(product.images.Count > 0)
                StartCoroutine(DownloadImage(product.images[0].url, newItem.transform.Find("Item Icon").GetComponent<Image>(), newItem.transform.Find("Loading Icon").gameObject));
    
            newItem.GetComponent<Button>().onClick.AddListener(() => {
                //whiteBG.SetActive(false);
                //ShowScreen(UIScreenType_Shop.itemDetail);
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
        itemDetailPanel.SetActive(true);
        itemDetailPanelBG.SetActive(true);
        shopPanel.SetActive(false);

        foreach (Transform obj in UI_3D_Models_Parent.transform)
            Destroy(obj.gameObject);

        
        targetModel = Instantiate(modelPrefab);
        targetModel.transform.parent = UI_3D_Models_Parent.transform;
        targetModel.transform.localPosition = Vector3.zero;
        targetModel.SetActive(false);

        foreach (Transform obj in colorsParent)
            Destroy(obj.gameObject);

        for (int i = 0; i < product.threeDModels.Count; i++)
        {
            Color newColor1;
            ColorUtility.TryParseHtmlString(product.threeDModels[i].colorCode, out newColor1);
            ModelVariant mv = Instantiate(colorPrefab, colorsParent).GetComponent<ModelVariant>();
            mv.img.color = newColor1;
        }
        
        //colorVariant1Parent.SetActive(false);
        //colorVariant2Parent.SetActive(false);
        //colorVariant3Parent.SetActive(false);

        //Color newColor;

        //if (product.threeDModels.Count > 0 && ColorUtility.TryParseHtmlString(product.threeDModels[0].colorCode, out newColor))
        //{
        //    colorVariant1.color = newColor;
        //    colorVariant1Parent.SetActive(true);
        //}

        //if (product.threeDModels.Count > 1 && ColorUtility.TryParseHtmlString(product.threeDModels[1].colorCode, out newColor))
        //{
        //    colorVariant2.color = newColor;
        //    colorVariant2Parent.SetActive(true);
        //}

        //if (product.threeDModels.Count > 2 && ColorUtility.TryParseHtmlString(product.threeDModels[2].colorCode, out newColor))
        //{
        //    colorVariant3.color = newColor;
        //    colorVariant3Parent.SetActive(true);
        //}


        StartCoroutine(DownloadAndAssign(product.threeDModels[0].url, targetModel, product));
        itemName.text = product.name;
        itemType.text = product.company.entityName;

        if (product.displayPrice.hasSale)
        {
            if (product.displayPrice.salePrice < product.displayPrice.price)
            {
               itemPrice.text = "$" + "<s>" + product.displayPrice.price + "</s>";
               itemPrice.color = new Color(0.7f, 0.7f, 0.7f);
            }
        }
        else
        {
            itemPrice.text = "$" + product.displayPrice.price.ToString();
            itemPrice.color = new Color(0, 0, 0);
        }

        if (product.displayPrice.hasSale)
            if (product.displayPrice.salePrice > 0)
                itemDiscountPrice.text = "$" + product.displayPrice.salePrice;

        itemDiscription.text = product.description;
        companyDiscription.text = product.company.description;

        if (arRoomBtn != null)
        {
            arRoomBtn.onClick.RemoveAllListeners();
            arRoomBtn.onClick.AddListener(() => CheckObjectScene(product));
        }
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
}