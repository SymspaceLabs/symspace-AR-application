using GLTF.Schema;
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
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using UnityGLTF;

public class CategoryManager : MonoBehaviour
{
    public static CategoryManager Instance;

    #region API URLs
    private string getAllProductsURL = "/products";
    private string getAllCategoriesURL = "/categories/mobile";
    private string getProductBySlug = "/products/slug/";
    #endregion

    #region Inspector Variables - Main Categories
    [Header("Main Categories System (Legacy)")]
    //public List<MainCategory> mainCategories;
    public List<UnityEngine.UI.Image> mainCategoriesImages;
    public Transform subcategoryButtonContainer;
    public Button subcategoryButtonPrefab;
    public List<UnityEngine.UI.Image> subCategoriesImages;
    public Transform productContainer;
    public GameObject productCardPrefab;
    public UnityEngine.Color selectedBgColor;
    public UnityEngine.Color unselectedBgColor;
    //private MainCategory currentCategory;
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

    public GameObject modelVariantPrefab;
    public Transform modelVariantParent;

    public ModelViewer mv;

    public List<GameObject> downloadedModels;
    public List<GameObject> tempModels;

    public Vector3 finalScale;
    public bool firstTime = false;

    public GameObject plusBtnCanvas;

    public bool isDebugMode = false;

    #region Private Variables
    private string localPath;
    #endregion

    #region Unity Lifecycle Methods

    private void Awake()
    {
        Instance = this;
    }

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
            if (SceneManager.GetActiveScene().name == SceneNames.ARScene)
                yield return StartCoroutine(DownloadImage(ProductSelection.productData.images[0].url));
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
            if (isDebugMode)
                Debug.Log("Categories loaded");
            LoadCategories(responseData);
        },
        (error) =>
        {
            if (isDebugMode)
                Debug.LogError("Failed to load categories: " + error);
        }, "GET"));
    }

    void GetAllProducts(string type)
    {
        //string formattedType = type.ToLower().Replace("'", "").Replace(" ", "-");
        if (isDebugMode)
            Debug.Log("corrected formate : " + type);
        StartCoroutine(AuthAPI.PostRequest(getAllProductsURL + "?" + type, "",
        (response) =>
        {
            ProductResponse responseData = JsonUtility.FromJson<ProductResponse>(response);
            allProductsData = responseData;
            if (isDebugMode)
                Debug.Log("Products loaded");
            PopulateProducts(responseData);
        },
        (error) =>
        {
            if (isDebugMode)
                Debug.LogError("Failed to load categories: " + error);
        }, "GET"));
    }
    #endregion

    #region Image Download Methods
    public IEnumerator DownloadImage(string url, UnityEngine.UI.Image imageComponent = null, GameObject loadingIcon = null)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            byte[] imageData = request.downloadHandler.data;

            // Check what kind of data we actually got
            string textPreview = System.Text.Encoding.UTF8.GetString(imageData);
            if (isDebugMode)
                Debug.Log("Response preview: " + textPreview.Substring(0, Mathf.Min(200, textPreview.Length)));

            if (isDebugMode)
                Debug.Log("Content-Type: " + request.GetResponseHeader("Content-Type"));
            if (isDebugMode)
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
     
                if(imageComponent != null)
                    imageComponent.enabled = true;
            }
            else
            {
                if (isDebugMode)
                    Debug.LogError(url + "\n❌ Failed to decode image bytes. Response is not a valid PNG/JPG.");
            }
        }
        else
        {
            if (isDebugMode)
                Debug.LogError($"Failed to download image: {request.error}");
        }

        if(loadingIcon != null)
            loadingIcon.SetActive(false);
    }

    public IEnumerator DownloadSpriteCoroutine(string url, List<Sprite> spritesList, int index)
    {
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            if (isDebugMode)
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
            if(spritesList != null)
                spritesList[index] = sprite;
        }
        else
        {
            if (isDebugMode)
                Debug.LogError(url + "\n❌ Failed to decode image bytes. Response is not a valid PNG/JPG.");
        }
    }

    public IEnumerator DownloadTextureCoroutine(string url, List<Texture2D> texturesList, int index)
    {
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            if (isDebugMode)
                Debug.LogError(req.error);

            yield break;
        }

        byte[] imageData = req.downloadHandler.data;
        Texture2D texture = new Texture2D(2, 2);

        if (texture.LoadImage(imageData))
        {
            if(texturesList != null)
                texturesList[index] = texture;
        }
        else
        {
            if (isDebugMode)
                Debug.LogError(url + "\n❌ Failed to decode image bytes. Response is not a valid PNG/JPG.");
        }
    }
    #endregion

    #region Product Selection & Scene Management
    IEnumerator ProductSelectedFunction(Products p, string url, string arType, Sprite sprite, ProductItemData pid = null)
    {
        if (CheckObjectScene(p))
        {
            if (SceneManager.GetActiveScene().name == SceneNames.ARFace)
            {
                StartCoroutine(arJewelryManager.JewelrySelected(p, p.threeDModels[0].url, p.category.name, pid ? pid : null));
            }
            else if (SceneManager.GetActiveScene().name == SceneNames.ARScene)
            {
                yield return null;
                #region plane Detection Scene Code
                string type = null;

                if (p.category.id != null)
                    type = p.ar_type;
                else
                    type = p.ar_type;

                GameObject newObject = null;

                var downloaded = FindExistingModel(p.id);
                //var downloaded = downloadedModels.Find(m =>
                //    m.GetComponent<ProductDetails>().product.id == p.id);

                if(downloaded == null)
                {
                    foreach (var obj in FindObjectsByType<DownloadState>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    {
                        var tempPd = obj.GetComponent<ProductDetails>();
                        if (isDebugMode)
                            Debug.Log("download state found");
                        if (tempPd != null && tempPd.product.id == p.id && obj.isDownloading)
                        {
                            yield break;
                        }
                    }

                }

                if (downloaded == null)
                {
                    // ALWAYS instantiate from prefab first
                    if (type.Equals("horizontal-plane detection"))
                        newObject = Instantiate(glbPrafabHorizontal);
                    else
                        newObject = Instantiate(glbPrafabVertical);
                }
                else
                    newObject = downloaded;

                var state = newObject.GetComponent<DownloadState>() ?? newObject.AddComponent<DownloadState>();
/*                state.isDownloading = false;
                state.isReady = false;

                // ONLY set ID if prefab instance (safe)
                var pd = newObject.GetComponent<ProductDetails>();
                pd.product.id = p.id;

                // apply cached downloaded data (safe copy)
                if (downloaded != null)
                {
                    var dlState = downloaded.GetComponent<DownloadState>();
                    if (dlState != null && dlState.isReady)
                    {
                        CopyDownloadedData(downloaded, newObject);
                    }
                }*/

                newObject.transform.localPosition = Vector3.zero;
                newObject.SetActive(false);

                newObject.name = GetUniqueName(p.name, GhostPlacementController.Instance.spawnedObjects);

                GhostPlacementController.Instance.objectToSpawn = newObject;

                SpawnCanvas(newObject);

                if (state == null)
                {
                    if (isDebugMode)
                        Debug.Log("statecheck is null");
                }
                else
                    if (isDebugMode)
                        Debug.Log("statecheck is not null");
                if (state.isReady)
                {
                    GhostPlacementController.Instance.objectToSpawn = newObject;
                    if (isDebugMode)
                        Debug.Log("product is already downloading or ready : " + state.isReady);

                    GhostPlacementController.Instance.unit =
                                (p.sizes != null && p.sizes.Count > 0)
                                    ? p.sizes[0].dimensions.unit
                                    : GhostPlacementController.Instance.unit;

                    GhostPlacementController.Instance.BeginPlacement(
                        type.Equals("horizontal-plane detection") ? false : true
                    );

                    UIManagerAR.instance.TogglePlaneVisuals(true);

                    
                    GetComponent<SlideUpPanel>().HidePanel();

                    foreach (var model in UIManagerAR.instance.UI_3D_Models)
                    {
                        if(model.GetComponent<ProductDetails>().product.id == p.id)
                            yield break;
                    }
                    GameObject modelToView = Instantiate(UIManagerAR.instance.modelPrefab);
                    modelToView.transform.parent = UIManagerAR.instance.UI_3D_Models_Parent.transform;
                    modelToView.transform.localPosition = Vector3.zero;
                    modelToView.GetComponent<ProductDetails>().product.id = p.id;

                    modelToView.transform.Find("Visual").GetComponent<MeshFilter>().mesh = newObject.GetComponentInChildren<MeshFilter>().mesh;

                    CopyDownloadedData(newObject, modelToView);
                    mv.FrameObject(modelToView);
                    UIManagerAR.instance.UI_3D_Models.Add(modelToView);

                    yield break;
                }

                if (!state.isReady && !state.isDownloading)
                {
                    if (pid != null)
                        yield return StartCoroutine(DownloadAndAssign(url, newObject, p, pid.ProductProgress, pid.DownloadFailed));
                    else
                        yield return StartCoroutine(DownloadAndAssign(url, newObject, p));
                }
                else
                {
                    if (!tempModels.Contains(newObject))
                        tempModels.Add(newObject);
                }

                // ---------------- WORLD CANVAS ----------------

                

                // ---------------- UI ENABLE ----------------

                UIManagerAR.instance.eventSystem.gameObject.SetActive(true);
                UIManagerAR.instance.itemsToPlaceParent.SetActive(true);

                foreach (Transform obj in modelVariantParent)
                    Destroy(obj.gameObject);

                // ---------------- VARIANTS ----------------

                GhostPlacementController.Instance.unit =
                    (p.sizes != null && p.sizes.Count > 0)
                        ? p.sizes[0].dimensions.unit
                        : GhostPlacementController.Instance.unit;

                GhostPlacementController.Instance.objectColors.Clear();

                for (int i = 0; i < p.colors.Count; i++)
                {
                    UnityEngine.Color newColor1;

                    if (ColorUtility.TryParseHtmlString(p.colors[i].code, out newColor1))
                    {
                        GhostPlacementController.Instance.objectColors.Add(newColor1);

                        ModelVariant mv =
                            Instantiate(modelVariantPrefab, modelVariantParent)
                            .GetComponent<ModelVariant>();

                        mv.index = i;
                        mv.colorImg.color = newColor1;
                        mv.colorName.text = p.colors[i].name;
                    }
                }

                GhostPlacementController.Instance.BeginPlacement(
                    type.Equals("horizontal-plane detection") ? false : true
                );

                GhostPlacementController.Instance.ChangeTextureByIndex(0);
/*
                // ---------------- APPLY PREVIEW TEXTURE ----------------

                if (downloaded != null)
                {
                    var downloadedPd = downloaded.GetComponent<ProductDetails>();

                    if (downloadedPd != null && downloadedPd.textures != null && downloadedPd.textures.Count > 0)
                    {
                        foreach (var obj in UIManagerAR.instance.UI_3D_Models)
                        {
                            var objPd = obj.GetComponent<ProductDetails>();

                            if (objPd != null && objPd.product.id == newPd.product.id)
                            {
                                var renderer = newObject.GetComponentInChildren<MeshRenderer>();

                                if (renderer != null)
                                    renderer.material.mainTexture = downloadedPd.textures[0];

                                newPd.selectedColorIndex = 0;
                            }
                        }
                    }
                }

                // ---------------- CLEAN VARIANTS UI ----------------
*/
                foreach (Transform obj in modelVariantParent)
                    obj.gameObject.SetActive(false);

                // ---------------- ORIENTATION ----------------

                var transformer = newObject.GetComponent<ARTransformer>();
                var manipulator = newObject.GetComponent<ARObjectManipulator>();

                if (type.Equals("horizontal-plane detection"))
                {
                    transformer.objectPlaneTranslationMode = ARTransformer.PlaneTranslationMode.Horizontal;
                    manipulator.orientation = ARObjectManipulator.Orientation.Horizontal;
                }
                else if (type.Equals("vertical-plane detection"))
                {
                    transformer.objectPlaneTranslationMode = ARTransformer.PlaneTranslationMode.Vertical;
                    manipulator.orientation = ARObjectManipulator.Orientation.Vertical;
                }

                // ---------------- PLANE VISUAL ----------------

                UIManagerAR.instance.TogglePlaneVisuals(true);

                if (state.isReady)
                    GetComponent<SlideUpPanel>().HidePanel();
                #endregion
            }
            else if (SceneManager.GetActiveScene().name == SceneNames.HandTracking)
            {
                if (HandItemSelector.Instance != null)
                {
                    ProductSelection.ClearSelection();
                    ProductSelection.SetSelection(p, false, p.category.name, false, p.threeDModels[0].url);
                    HandItemSelector.Instance.SelectItem(p.name, p.category.name, pid);
                }
            }
            else if (SceneManager.GetActiveScene().name.Equals(SceneNames.ARBodyTrackingMars))
            {
                if (isDebugMode)
                    Debug.Log("Mars 1");
                BodyTrackingWithMars.Instance.BodyModelSelected(p, url, p.category.name, pid);
            }
        }
        yield return null;
    }

    void SpawnCanvas(GameObject newObject)
    {
        WorldCanvasFaceCamera[] allWorldCanvases =
                    FindObjectsByType<WorldCanvasFaceCamera>(FindObjectsSortMode.None);

        bool spawnCanvas = true;

        var newPd = newObject.GetComponent<ProductDetails>();

        foreach (var c in allWorldCanvases)
        {
            if (c.pd == newPd)
            {
                spawnCanvas = false;
                break;
            }
        }

        if (spawnCanvas)
        {
            WorldCanvasFaceCamera btnCanvas =
                Instantiate(plusBtnCanvas).GetComponent<WorldCanvasFaceCamera>();

            if (!btnCanvas.GetComponent<ObjectDetail>())
                btnCanvas.gameObject.AddComponent<ObjectDetail>();
            btnCanvas.GetComponent<ObjectDetail>().index = GhostPlacementController.Instance.spawnObjectCount;
            GhostPlacementController.Instance.spawnObjectCount++;

            btnCanvas.canvasPosition = WorldCanvasFaceCamera.CanvasPosition.Top;
            btnCanvas.targetModel = newObject.GetComponentInChildren<MeshRenderer>()?.transform;
            newPd.plusCanvas = btnCanvas.gameObject;
            btnCanvas.pd = newPd;
            btnCanvas.objDetail = newObject.GetComponent<ObjectDetail>();
        }
    }

    void CopyDownloadedData(GameObject source, GameObject target)
    {
        var srcMR = source.GetComponentInChildren<MeshRenderer>();
        var tgtMR = target.GetComponentInChildren<MeshRenderer>();

        if (srcMR != null && tgtMR != null)
        {
            Material[] mats = new Material[srcMR.sharedMaterials.Length];

            for (int i = 0; i < mats.Length; i++)
                mats[i] = new Material(srcMR.sharedMaterials[i]);

            tgtMR.materials = mats;
        }
    }

    GameObject FindExistingModel(string productId)
    {
        // 1. Check downloaded templates
        var downloaded = downloadedModels.Find(m =>
            m.GetComponent<ProductDetails>().product.id == productId);

        if (downloaded != null)
        {
            var temp = tempModels.Find(m =>
                m.GetComponent<ProductDetails>().product.id == productId);

            if (temp != null)
                return temp;

            // IMPORTANT: clone safely (break material sharing)
            GameObject go = Instantiate(downloaded);

            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                Material[] mats = r.sharedMaterials;

                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = new Material(mats[i]);
                }

                r.materials = mats;
            }

            return go;
        }

        // 2. Already created temp model
        var existingTemp = tempModels.Find(m =>
            m.GetComponent<ProductDetails>().product.id == productId);

        if (existingTemp != null)
            return existingTemp;

        // 3. Downloading in progress
        
        return null;
    }


    public void UpdateObjectScale(ProductDetails pd, GameObject newObject, bool isVertical = false, int dimensionIndex = 0)
    {
        if (isDebugMode)
            Debug.Log("dimension Index : " + dimensionIndex);
        Vector3 modelOriginalHeight;


        Physics.SyncTransforms();
        Canvas.ForceUpdateCanvases();

        StartCoroutine(DelayFunction());
        
        IEnumerator DelayFunction()
        {

            // Get original bounds size of the mesh renderer

            //Transform plusBtnCanvas = newObject.GetComponentInChildren<Canvas>().transform;

            //plusBtnCanvas.SetParent(null, true);
            if (isDebugMode)
                Debug.Log("parent scale : " + newObject.transform.localScale);
            //UnparentSafely(plusBtnCanvas, newObject.transform);
            yield return null;
            //newObject.transform.localScale = Vector3.one;

            modelOriginalHeight = newObject.GetComponentInChildren<MeshFilter>().sharedMesh.bounds.size;

            //obj.transform.parent = realParent;

            // Calculate scale ratios for each axis according to desired size
            ARDimensionVisualizer arDimension = newObject.GetComponentInChildren<ARDimensionVisualizer>();


            float axis_L =  pd.product.sizes[dimensionIndex].dimensions.length;
            float axis_H =  pd.product.sizes[dimensionIndex].dimensions.height;
            float axis_W = pd.product.sizes[dimensionIndex].dimensions.width;

            arDimension.textLength = axis_L;
            arDimension.textHeight = axis_H;
            arDimension.textDepth = axis_W;

            arDimension.UpdateTexts();

            if (isVertical)
            {
                //if (!pd.product.isSorted)
                //{
                    //float temp = axis_L;
                    //axis_L = axis_H;
                    //axis_H = axis_W;
                    ////axis_H = temp;
                    //axis_W = temp;
                //}

                finalScale = new Vector3(ConvertToUnityScale(axis_H, pd.product.sizes[dimensionIndex].dimensions.unit) / modelOriginalHeight.y,
                ConvertToUnityScale(axis_W, pd.product.sizes[dimensionIndex].dimensions.unit) / modelOriginalHeight.z,
                ConvertToUnityScale(axis_L, pd.product.sizes[dimensionIndex].dimensions.unit) / modelOriginalHeight.x
                );
                newObject.transform.localScale = finalScale;
            }
            else
            {
                finalScale = new Vector3(ConvertToUnityScale(axis_L, pd.product.sizes[dimensionIndex].dimensions.unit) / modelOriginalHeight.x,
                ConvertToUnityScale(axis_H, pd.product.sizes[dimensionIndex].dimensions.unit) / modelOriginalHeight.y,
                ConvertToUnityScale(axis_W, pd.product.sizes[dimensionIndex].dimensions.unit) / modelOriginalHeight.z
                );
                newObject.transform.localScale = finalScale;
            }
            //finalScale = new Vector3(ConvertToUnityScale(axis_L, pd.product.sizes[dimensionIndex].dimensions.unit) / modelOriginalHeight.x,
            //    ConvertToUnityScale(axis_H, pd.product.sizes[dimensionIndex].dimensions.unit) / modelOriginalHeight.y,
            //    ConvertToUnityScale(axis_W, pd.product.sizes[dimensionIndex].dimensions.unit) / modelOriginalHeight.z
            //    );
            //    newObject.transform.localScale = finalScale;

            if (isDebugMode)
            {
                Debug.Log("model Original Size : " + modelOriginalHeight);
                Debug.Log("Length: " + axis_L + ", Width: " + axis_W + ", Height: " + axis_H);
            }

            Physics.SyncTransforms();
            Canvas.ForceUpdateCanvases();

            //plusBtnCanvas.SetParent(newObject.transform, true);
            yield return new WaitForSeconds(1f);
            //ReparentSafely(plusBtnCanvas, newObject.transform);
        }
    }

    public void ReparentSafely(Transform child, Transform parent)
    {
        if (child == null || parent == null)
        {
            if (isDebugMode)
                Debug.LogWarning("Assign both child and parent!");
            return;
        }

        // 1. Store child's world matrix
        Matrix4x4 childWorldMatrix = child.localToWorldMatrix;

        // 2. Parent the child
        child.SetParent(parent, worldPositionStays: false); // we handle world matrix manually

        // 3. Compute local matrix relative to new parent
        Matrix4x4 parentWorldToLocal = parent.worldToLocalMatrix;
        Matrix4x4 childLocalMatrix = parentWorldToLocal * childWorldMatrix;

        // 4. Extract position, rotation, and scale from matrix
        child.localPosition = childLocalMatrix.GetColumn(3);
        child.localRotation = Quaternion.LookRotation(
            childLocalMatrix.GetColumn(2),
            childLocalMatrix.GetColumn(1)
        );
        child.localScale = new Vector3(
            childLocalMatrix.GetColumn(0).magnitude,
            childLocalMatrix.GetColumn(1).magnitude,
            childLocalMatrix.GetColumn(2).magnitude
        );

        if (isDebugMode)
            Debug.Log($"Child {child.name} safely reparented under {parent.name} with matrix. {child.localScale.ToString("F9")}");
    }

    public void UnparentSafely(Transform child, Transform parent)
    {
        if (child == null)
        {
            if (isDebugMode)
                Debug.LogWarning("Assign a child!");
            return;
        }

        if (isDebugMode)
            Debug.Log("Child local scale: " + child.localScale.ToString("F9"));
        // Store world matrix
        Matrix4x4 childWorldMatrix = child.localToWorldMatrix;

        // Remove parent
        child.SetParent(null, worldPositionStays: false);

        // Set local transform from world matrix (no parent now)
        child.localPosition = childWorldMatrix.GetColumn(3);
        child.localRotation = Quaternion.LookRotation(
            childWorldMatrix.GetColumn(2),
            childWorldMatrix.GetColumn(1)
        );
        child.localScale = new Vector3(
            childWorldMatrix.GetColumn(0).magnitude,
            childWorldMatrix.GetColumn(1).magnitude,
            childWorldMatrix.GetColumn(2).magnitude
        );

        child.localScale = new Vector3(0.005f, 0.005f, 0.005f);

        if (isDebugMode)
            Debug.Log($"Child {child.name} safely unparented using matrix. {child.lossyScale.ToString("F9")}");
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
                if (isDebugMode)
                    Debug.LogWarning("Unknown unit, defaulting to meters");
                return inputSize;
        }
    }

    bool CheckObjectScene(Products p)
    {
        if (p.ar_type.Equals("vertical-plane detection") || p.ar_type.Equals("horizontal-plane detection"))
        {
            if (SceneManager.GetActiveScene().name != "AR Scene")
            {
                ProductSelection.ClearSelection();
                ProductSelection.SetSelection(p, false, "", p.ar_type.Equals("horizontal-plane detection"), p.threeDModels[0].url);
                UIManagerAR.instance.ChangeARScene(SceneNames.ARScene);
                return false;
            }
            return true;
        }
        else if (p.ar_type.Equals("face-tracking"))
        {
            if (SceneManager.GetActiveScene().name != SceneNames.ARFace)
            {
                CategoryType category;
                ProductSelection.TryParseObjectType(p.name, out category);
                ProductSelection.ClearSelection();
                ProductSelection.SetSelection(p, true, p.category.name, false, p.threeDModels[0].url);
                UIManagerAR.instance.ChangeARScene(SceneNames.ARFace);
                return false;
            }
            return true;
        }
        else if (p.ar_type.Equals("hand-tracking"))
        {
            if (SceneManager.GetActiveScene().name != SceneNames.HandTracking)
            {
                CategoryType category;
                ProductSelection.TryParseObjectType(p.name, out category);
                ProductSelection.ClearSelection();
                ProductSelection.SetSelection(p, false, p.category.name, false, p.threeDModels[0].url);
                UIManagerAR.instance.ChangeARScene(SceneNames.HandTracking);
                return false;
            }
            return true;
        }
        else if (p.ar_type.Equals("body-tracking"))
        {
            if (!SceneManager.GetActiveScene().name.Equals(SceneNames.ARBodyTrackingMars))
            {
                if (isDebugMode)
                    Debug.Log("not Mars");
                CategoryType category;
                ProductSelection.TryParseObjectType(p.name, out category);
                ProductSelection.ClearSelection();
                ProductSelection.SetSelection(p, false, p.category.name, false, p.threeDModels[0].url);
                UIManagerAR.instance.ChangeARScene(SceneNames.ARBodyTrackingMars);
                return false;
            }
            if (isDebugMode)
                Debug.Log("Mars");
            return true;
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
    IEnumerator DownloadAndAssign(string url, GameObject targetObject, Products p,
    Action<float> onProgress = null, Action onFailed = null)
    {
        yield return null;

        var state = targetObject.GetComponent<DownloadState>();
        state.isDownloading = true;
        state.isReady = false;


        GameObject loadedRoot = null;
        yield return StartCoroutine(ModelLoaderService.DownloadAndLoad(url, (model) => { loadedRoot = model; } ,onProgress, onFailed));
        if (loadedRoot == null)
        {
            Debug.LogError("download failed");
            yield break;
        }

        MeshFilter srcMF = loadedRoot.GetComponentInChildren<MeshFilter>();
        MeshRenderer srcMR = loadedRoot.GetComponentInChildren<MeshRenderer>();

        if (srcMF == null || srcMR == null)
        {
            if (isDebugMode)
                Debug.LogError("GLB missing mesh");
            yield break;
        }

        Transform visual = targetObject.transform.Find("Visual");

        MeshFilter targetMF = visual.GetComponent<MeshFilter>() ?? visual.gameObject.AddComponent<MeshFilter>();
        MeshRenderer targetMR = visual.GetComponent<MeshRenderer>() ?? visual.gameObject.AddComponent<MeshRenderer>();
        MeshCollider targetMC = visual.GetComponent<MeshCollider>() ?? visual.gameObject.AddComponent<MeshCollider>();

        // ---------------- MESH ----------------
        targetMF.mesh = Instantiate(srcMF.sharedMesh);

        targetMR.materials = srcMR.materials.Clone() as Material[];

        //Material[] newMaterials = new Material[srcMR.materials.Length];
        for (int matIdx = 0; matIdx < targetMR.materials.Length; matIdx++)
        {
            Material mat = targetMR.materials[matIdx];
            Material srcMat = srcMR.materials[matIdx];
            mat.SetFloat("normalScale", 0);

            foreach (string prop in srcMat.GetTexturePropertyNames())

            {
                Texture tex = srcMat.GetTexture(prop);
                if (tex == null) continue;

                if (tex is Texture2D srcTex)
                {
                    Texture2D copy = new Texture2D(srcTex.width, srcTex.height, srcTex.format, srcTex.mipmapCount > 1);
                    Graphics.CopyTexture(srcTex, copy);     // fast GPU copy

                    // or slower but more compatible:
                    // copy.LoadImage(srcTex.EncodeToPNG());

                    copy.wrapMode = srcTex.wrapMode;
                    copy.filterMode = srcTex.filterMode;
                    copy.anisoLevel = srcTex.anisoLevel;
                    copy.Apply();

                    mat.SetTexture(prop, copy);
                }
            }
        }

        //targetMR.materials = newMats;
        targetMC.sharedMesh = targetMF.mesh;

        targetMF.GetComponent<ARDimensionVisualizer>().enabled = true;

        ProductDetails pd = targetObject.GetComponent<ProductDetails>();

        pd.product = p;

        pd.colors.Clear();
        foreach (var c in p.colors)
        {
            UnityEngine.Color newColor;
            ColorUtility.TryParseHtmlString(c.code, out newColor);
            pd.colors.Add(newColor);
        }

        UpdateObjectScale(pd, targetObject,
            !p.ar_type.Equals("horizontal-plane detection"));

        yield return new WaitForSeconds(0.2f);

        state.isDownloading = false;
        state.isReady = true;

        pd.imagesUrl.Clear();

        foreach (var img in pd.product.images)
            pd.imagesUrl.Add(img.url);

        pd.texturesUrl.Clear();

        foreach (var t in pd.product.threeDModels)
            if (t.texture.Length > 0)
                pd.texturesUrl.Add(t.texture);

        // ---------------- CACHE (UNCHANGED LOGIC) ----------------

        GameObject downloadedModel = Instantiate(targetObject);

        CopyDownloadedData(targetObject, downloadedModel);

        if (!downloadedModels.Contains(downloadedModel))
            downloadedModels.Add(downloadedModel);

        if (!tempModels.Contains(targetObject))
            tempModels.Add(targetObject);

        // ---------------- UI MODEL PREVIEW (UNCHANGED) ----------------
        GameObject modelToView = Instantiate(UIManagerAR.instance.modelPrefab);
        modelToView.transform.parent = UIManagerAR.instance.UI_3D_Models_Parent.transform;
        modelToView.transform.localPosition = Vector3.zero;

        UIManagerAR.instance.UI_3D_Models.Add(modelToView);

        StartCoroutine(SetProductImages(downloadedModel.GetComponent<ProductDetails>()));
        StartCoroutine(SetProductImages(targetObject.GetComponent<ProductDetails>()));

        modelToView.transform.Find("Visual").GetComponent<MeshFilter>().mesh = targetMF.mesh;
        modelToView.transform.Find("Visual").GetComponent<MeshRenderer>().materials = targetMR.materials;
        modelToView.name = targetObject.name;

        modelToView.GetComponent<ProductDetails>().product.id = pd.product.id;

        mv.FrameObject(modelToView);

        GetComponent<SlideUpPanel>().HidePanel();

        Destroy(loadedRoot);

        if (isDebugMode)
            Debug.Log("✅ Download + Assign complete");
        
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
        if(isDebugMode)
            Debug.Log("Categories : " + Categories.Count);
        foreach (var category in Categories)
        {
            GameObject buttonObj = Instantiate(categoryButtonPrefab, topLevelCategoryParent);
            buttonObj.name = category.name;

            //mainCategoriesImages.Add(buttonObj.GetComponent<UnityEngine.UI.Image>());

            TextMeshProUGUI label = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = category.name;

            bool productsSpawned = false;

            Button btn = buttonObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() =>
                {
                    string query = string.Join("&", category.slugs.Select(s => $"{category.queryParam}={s}"));

                    ProductSelection.categoryName = category.name;
                    //string formattedType = category.name.Replace(" ", "-");
                    if (isDebugMode)
                        Debug.Log("corrected query : " + query);
                    StartCoroutine(AuthAPI.PostRequest(getAllProductsURL + "?" + query, "",
                    (response) =>
                    {
                        ProductResponse responseData = JsonUtility.FromJson<ProductResponse>(response);
                        productsSpawned = PopulateProducts(responseData);

                        if (category.items != null && category.items.Count > 0)
                        {
                            PopulateSubCategories(category.items);
                            if (isDebugMode)
                                Debug.Log("Product Spawned " + productsSpawned);
                            //if (productsSpawned)
                            //    subCategoryParent.gameObject.SetActive(false);
                            //else
                            //    subCategoryParent.gameObject.SetActive(true);
                        }
                    },
                    (error) =>
                    {
                        if (isDebugMode)
                            Debug.LogError("Failed to load categories: " + error);
                    }, "GET"));



                    //PopulateSubCategories(category.items);
                    UnSelectAllImages(topLevelCategoryParent);
                    SelectedImage(buttonObj.GetComponent<UnityEngine.UI.Image>());
                });
            }
        }

        if (!firstTime)
        {
            if (Categories[0] != null)
            {
                firstTime = true;
                Button btn = topLevelCategoryParent.GetChild(0).GetComponent<Button>();
                if (isDebugMode)
                    Debug.Log("Button listener count: " + btn.onClick.GetPersistentEventCount());

                StartCoroutine(somefunction());
                IEnumerator somefunction()
                {
                    bool categoryFound = false;

                    yield return new WaitForSeconds(1f);
                    foreach(Transform btn in topLevelCategoryParent)
                    {
                        if (btn.name == ProductSelection.categoryName)
                        {
                            categoryFound = true;
                            btn.GetComponent<Button>()?.onClick?.Invoke();
                        }
                    }

                    if (!categoryFound)
                        topLevelCategoryParent.GetChild(0).GetComponent<Button>()?.onClick?.Invoke();
                }
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
            if (isDebugMode)
                Debug.Log("Button found on " + buttonObj.name);
            if (btn != null)
            {
                btn.onClick.AddListener(() =>
                {
                    string query = sub.queryParam + "=" + sub.slug;
                    //string formattedType = sub.name.Replace(" ", "-");
                    if (isDebugMode)
                        Debug.Log("corrected formate : " + query);
                    StartCoroutine(AuthAPI.PostRequest(getAllProductsURL + "?" + query, "",
                    (response) =>
                    {
                        ProductResponse responseData = JsonUtility.FromJson<ProductResponse>(response);
                        productsSpawned = PopulateProducts(responseData);
                        
                        if (sub.items != null && sub.items.Count > 0)
                        {
                            PopulateLeafCategories(sub.items);
                            if (isDebugMode)
                                Debug.Log("Product Spawned " + productsSpawned);
                            //if (productsSpawned)
                            //    leafCategoryParent.gameObject.SetActive(false);
                            //else
                            //    leafCategoryParent.gameObject.SetActive(true);


                        }
                    },
                    (error) =>
                    {
                        if (isDebugMode)
                            Debug.LogError("Failed to load categories: " + error);
                    }, "GET"));


                    UnSelectAllImages(subCategoryParent);
                    SelectedImage(buttonObj.GetComponent<UnityEngine.UI.Image>());
                });
            }
        }
    }

    private void PopulateLeafCategories(List<SubItems> leafNodes)
    {
        foreach (Transform child in leafCategoryParent)
            Destroy(child.gameObject);

        foreach (var leaf in leafNodes)
        {
            GameObject buttonObj = Instantiate(categoryButtonPrefab, leafCategoryParent);
            TextMeshProUGUI label = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = leaf.name;

            Button btn = buttonObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() =>
                {
                    if (leaf != null)
                    {
                        string query = leaf.queryParam + "=" + leaf.slug;
                        GetAllProducts(query);
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
        //ClearTransform(productContainer);
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

        if (isDebugMode)
            Debug.Log("Products count : " + response.products.Count);
        if (response.products.Count == 0)
            return false;

        foreach (var p in response.products)
        {
            GameObject card = Instantiate(productCardPrefab, productContainer);
            ProductItemData pid = card.GetComponent<ProductItemData>();
            pid.name.text = p.company.entityName;
            pid.type.text = p.name;

            if (p.displayPrice.salePrice < p.displayPrice.price)
            {
                pid.price.text = "<s>$" + p.displayPrice.price + "</s>";
                pid.price.color = new UnityEngine.Color(0.7f, 0.7f, 0.7f);

                pid.salePrice.enabled = true;
            }
            else
            {
                pid.price.text = "$" + p.displayPrice.price;
                pid.price.color = new UnityEngine.Color(1, 1, 1);

                pid.salePrice.enabled = false;
            }

            pid.salePrice.text = "$" + p.displayPrice.salePrice;
            if (p.thumbnail.Length > 0)
            {
                StartCoroutine(DownloadImage(p.thumbnail, pid.productImage, pid.loadingIcon));
            }

            StartCoroutine(AuthAPI.PostRequest(getProductBySlug + p.slug, "", // Empty string for no body
            (response) =>
            {
                Products product = JsonUtility.FromJson<Products>(response);
                if (product != null)
                {
                    product.colors.Sort((a, b) => a.sortOrder.CompareTo(b.sortOrder));
                    product.images.Sort((a, b) => a.sortOrder.CompareTo(b.sortOrder));
                    product.sizes.Sort((a, b) => a.sortOrder.CompareTo(b.sortOrder));
                    product.threeDModels.Sort((a, b) =>
                    {
                        int indexA = product.colors.FindIndex(c => c.code == a.colorCode);
                        int indexB = product.colors.FindIndex(c => c.code == b.colorCode);

                        return indexA.CompareTo(indexB);
                    });

                    product.variants.Sort((a, b) =>
                    {
                        int indexA = product.sizes.FindIndex(c => c.sortOrder == a.size.sortOrder);
                        int indexB = product.sizes.FindIndex(c => c.sortOrder == b.size.sortOrder);

                        return indexA.CompareTo(indexB);
                    });

                    //product.isSorted = true;
                    if(pid != null && pid.productImage != null)
                        card.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(ProductSelectedFunction(product, product.threeDModels[0].url, product.ar_type, pid.productImage.sprite, pid)));
                }
            },
            (error) =>
            {
                //loadingIcon.SetActive(false);
                //loadingPanel.SetActive(false);
                //Debug.LogError("Failed to load categories: " + error);
                //ShowStatus("Failed to load categories", true);
                //MenuManager.Instance.loadingPanel.SetActive(false);
            }, "GET"));
        }
        return true;
    }

    public void FilterProducts()
    {
        if (subCategoryParent.gameObject.activeInHierarchy)
            leafCategoryParent.gameObject.SetActive(true);
        else
            subCategoryParent.gameObject.SetActive(true);
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

    //public void OnMainCategorySelected(string categoryName)
    //{
    //    foreach (var image in mainCategoriesImages)
    //    {
    //        image.color = unselectedBgColor;
    //    }

    //    currentCategory = mainCategories.Find(c => c.name == categoryName);

    //    ClearTransform(subcategoryButtonContainer);
    //    ClearTransform(productContainer);

    //    if (currentCategory == null) return;

    //    subCategoriesImages.Clear();

    //    // Create new subcategory buttons
    //    foreach (Subcategory sub in currentCategory.subcategories)
    //    {
    //        Button btn = Instantiate(subcategoryButtonPrefab, subcategoryButtonContainer);
    //        btn.GetComponentInChildren<TextMeshProUGUI>().text = sub.name;
    //        btn.onClick.AddListener(() => OnSubcategorySelected(sub));
    //        btn.onClick.AddListener(() => SelectedImage(btn.GetComponent<UnityEngine.UI.Image>()));
    //        subCategoriesImages.Add(btn.GetComponent<UnityEngine.UI.Image>());
    //    }

    //    // Auto-load first subcategory
    //    if (currentCategory.subcategories.Count > 0)
    //    {
    //        OnSubcategorySelected(currentCategory.subcategories[0]);
    //        subCategoriesImages[0].color = selectedBgColor;
    //    }
    //}

    //public void OnSubcategorySelected(Subcategory subcategory)
    //{
    //    ClearTransform(productContainer);

    //    foreach (var img in subCategoriesImages)
    //    {
    //        img.color = unselectedBgColor;
    //    }

    //    // Load new products
    //    foreach (Product p in subcategory.products)
    //    {
    //        GameObject card = Instantiate(productCardPrefab, productContainer);
    //        card.transform.Find("Border/ItemName").GetComponent<TextMeshProUGUI>().text = p.itemName;
    //        card.transform.Find("Border/ItemType").GetComponent<TextMeshProUGUI>().text = p.itemType;

    //        if (p.discountPrice.Length > 0)
    //        {
    //            card.transform.Find("Border/Price").GetComponent<TextMeshProUGUI>().text = "<s>" + p.price + "</s>";
    //            card.transform.Find("Border/Price").GetComponent<TextMeshProUGUI>().color = new UnityEngine.Color(0.7f, 0.7f, 0.7f);
    //        }
    //        else
    //        {
    //            card.transform.Find("Border/Price").GetComponent<TextMeshProUGUI>().text = p.price;
    //            card.transform.Find("Border/Price").GetComponent<TextMeshProUGUI>().color = new UnityEngine.Color(1, 1, 1);
    //        }

    //        card.transform.Find("Border/DiscountPrice").GetComponent<TextMeshProUGUI>().text = p.discountPrice;
    //        card.transform.Find("Border/ProductImage").GetComponent<UnityEngine.UI.Image>().sprite = p.image;
    //    }
    //}
    #endregion

    #region Helper Methods
    public IEnumerator SetProductImages(ProductDetails pd)
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
            if (isDebugMode)
                Debug.Log("texture downloading");
        }
        if (isDebugMode)
            Debug.Log("texture Finish");
        
    }

    #endregion

    //#region Data Classes - Legacy System
    //[System.Serializable]
    //public class Product
    //{
    //    public string itemName;
    //    public string itemType;
    //    public string price;
    //    public string discountPrice;
    //    public Sprite image;

    //    public float width;
    //    public float depth;
    //    public float height;

    //    public List<Texture> texture;

    //    public string unit;

    //    public bool horizontal;
    //    public bool isFaceObject = false;
    //    public bool isFurniture = false;
    //    public bool isHandObject = false;
    //    public bool isBodyObject = false;

    //    public string categoryType;
    //}

    //[System.Serializable]
    //public class Subcategory
    //{
    //    public string name;
    //    public List<Product> products;
    //}

    //[System.Serializable]
    //public class MainCategory
    //{
    //    public string name;
    //    public List<Subcategory> subcategories;
    //}
    //#endregion

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
        public string thumbnail;
        public List<ThreeDModel> threeDModels;
        public List<ColorData> colors;
        public List<SizeData> sizes;

        public DisplayPrice displayPrice;
        public string availability;

        public List<Variants> variants;
        public string status;
        public bool isSorted = false;
    }

    [System.Serializable]
    public class Variants
    {
        public string id;
        public string sku;
        public int stock;
        public float price;
        public float salePrice;
        public float cost;
        public ColorData color;
        public SizeData size;
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
        public string createdAt;
        public string updatedAt;
        public int sortOrder;
    }

    [System.Serializable]
    public class SizeData
    {
        public string id;
        public string size;
        public int sortOrder;
        public string sizeChartUrl;
        public Dimensions dimensions;
        public ProductWeight productWeight;
    }

    [System.Serializable]
    public class DisplayPrice
    {
        public float price;
        public float salePrice;
        public bool hasSale;
        public string range;
    }


    [System.Serializable]
    public class ProductWeight
    {
        public string unit;
        public float value;
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
        public int sortOrder;
    }

    [System.Serializable]
    public class SizeSingleData
    {
        public string id;
        public string size;
        public int sortOrder;
        public string sizeChartUrl;
        public Dimensions dimensions;
        public ProductWeight productWeight;
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
        public Parent parent;
    }

    [System.Serializable]
    public class Parent
    {
        public string id;
        public string name;
        public string slug;
        public LesserParent parent;
    }

    [System.Serializable]
    public class LesserParent
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

    //[System.Serializable]
    //public class Variant
    //{
    //    public string id;
    //    public float price;
    //    public int stock;
    //    public bool isActive;
    //    public string sku;
    //    public float? salePrice;
    //    public ProductWeight productWeight;
    //    public Dimensions dimensions;
    //    public string sizeChart;
    //    public string sizeFit;
    //    public Color color;
    //    public SizeData size;
    //}

    [System.Serializable]
    public class ThreeDModel
    {
        public string id;
        public string url;
        public string colorCode;
        public List<float> pivot;
        public string format;
        public BoundingBox boundingBox;
        public string texture;
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
        public List<string> slugs;
        public string queryParam;
        public List<Items> items;
    }

    [Serializable]
    public class Items
    {
        public string name;
        public string slug;
        public string queryParam;
        public List<SubItems> items;
    }

    [Serializable]
    public class SubItems
    {
        public string name;
        public string slug;
        public string queryParam;
    }
    #endregion

    #endregion
}