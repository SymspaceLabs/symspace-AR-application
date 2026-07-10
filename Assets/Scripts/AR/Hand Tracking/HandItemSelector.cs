using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityGLTF;

public class HandItemSelector : MonoBehaviour
{
    public static HandItemSelector Instance;

    #region Inspector

    public List<GameObject> watches;
    public List<GameObject> rings;

    public Transform watchSpawnParent;
    public Transform ringSpawnParent;

    public GameObject watchesParent;
    public GameObject ringsParent;

    public GameObject plusBtnCanvas;

    #endregion


    #region Runtime Containers

    Dictionary<CategoryType, List<GameObject>> preloadedItems;
    Dictionary<CategoryType, Transform> spawnParents;
    Dictionary<CategoryType, GameObject> activeItems = new();
    Dictionary<CategoryType, GameObject> uiParents;

    Dictionary<CategoryType, Vector3> initialPos = new();
    Dictionary<CategoryType, Quaternion> initialRot = new();
    Dictionary<CategoryType, Vector3> initialScale = new();

    //Dictionary<string, GameObject> downloadedModels = new();

    #endregion


    #region Unity Lifecycle

    private void Awake()
    {
        Instance = this;

        preloadedItems = new()
        {
            { CategoryType.Watches, watches },
            { CategoryType.Rings, rings }
        };

        spawnParents = new()
        {
            { CategoryType.Watches, watchSpawnParent },
            { CategoryType.Rings, ringSpawnParent }
        };

        uiParents = new()
        {
            { CategoryType.Watches, watchesParent },
            { CategoryType.Rings, ringsParent }
        };

        CacheInitialTransforms();
    }

    private void Start()
    {
        DisableAllObjects();

        if (ProductSelection.productData != null)
            SelectItem(ProductSelection.productData.name, ProductSelection.categoryName);
            //HandleProductSelection();
    }

    #endregion


    #region Initialization

    void CacheInitialTransforms()
    {
        foreach (var kv in spawnParents)
        {
            initialPos[kv.Key] = kv.Value.localPosition;
            initialRot[kv.Key] = kv.Value.localRotation;
            initialScale[kv.Key] = kv.Value.localScale;
        }
    }

    void ResetSpawnParent(CategoryType category)
    {
        Transform parent = spawnParents[category];

        parent.localPosition = initialPos[category];
        parent.localRotation = initialRot[category];
        parent.localScale = initialScale[category];
    }

    #endregion


    #region Selection

    void HandleProductSelection(ProductItemData pid = null)
    {
        CategoryType category = ProductSelection.SelectedObjectType;
        string itemName = ProductSelection.productData.name;


        //if (downloadedModels.ContainsKey(name))
        //{
        //    GameObject existing = downloadedModels[name];

        //    var state = existing.GetComponent<DownloadState>();

        //    if (state != null)
        //    {
        //        if (state.isDownloading)
        //        {
        //            if(CategoryManager.Instance.isDebugMode)Debug.Log("Model already downloading.");
        //            //yield break;
        //        }

        //        if (state.isReady)
        //        {
        //            SelectItem(name, category.ToString());
        //            //yield break;
        //        }
        //    }
        //}

        //var item = GetPreloadedItem(category, itemName);

        /* if (item != null)
             SelectItem(item.name, category.ToString(), pid);
         else */
        if (ShouldDownloadModel())
        {
            StartCoroutine(DownloadAndSpawnModel(category, pid ? pid.ProductProgress : null, pid ? pid.DownloadFailed : null));
            return;
        }

    }

    GameObject GetPreloadedItem(CategoryType category, string name)
    {
        return preloadedItems[category].FirstOrDefault(x => x.name == name);
    }

    public void SelectItem(string itemName, string categoryName, ProductItemData pid = null)
    {
        ProductSelection.TryParseObjectType(categoryName, out CategoryType category);

        ProductSelection.SelectedObjectType = category;

        GameObject item = GetItem(itemName, category);

        if (item == null)
        {
            item = new GameObject();

            if (ShouldDownloadModel())
            {
                StartCoroutine(
                    DownloadAndSpawnModel(
                        category,
                        pid ? pid.ProductProgress : null,
                        pid ? pid.DownloadFailed : null));

                return;
            }
        }

        if(CategoryManager.Instance.isDebugMode)Debug.Log("item Name : " + item.name);

        SetActiveItem(category, item);

        ProductDetails selectedPd = item.GetComponent<ProductDetails>();
        if (selectedPd != null && selectedPd.product != null)
            UIManagerAR.instance.SelectModel(selectedPd);

        CategoryManager.Instance.GetComponent<SlideUpPanel>().HidePanel();
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

            btnCanvas.canvasPosition = WorldCanvasFaceCamera.CanvasPosition.TopRight;
            btnCanvas.targetModel = newObject.GetComponentInChildren<MeshRenderer>()?.transform;
            newPd.plusCanvas = btnCanvas.gameObject;
            btnCanvas.pd = newPd;
            btnCanvas.objDetail = newObject.GetComponent<ObjectDetail>();

            btnCanvas.gameObject.layer = LayerMask.NameToLayer("top");
            foreach (Transform obj in btnCanvas.transform)
            {
                obj.gameObject.layer = LayerMask.NameToLayer("top");
            }
        }
    }

    GameObject GetItem(string itemName, CategoryType category)
    {
        return GetPreloadedItem(category, itemName);
    }

    void SetActiveItem(CategoryType category, GameObject obj)
    {
        if (activeItems.ContainsKey(category) && activeItems[category] != obj)
        {
            Destroy(activeItems[category]);
        }

        activeItems[category] = obj;

        obj.SetActive(true);
        uiParents[category].SetActive(true);
    }

    #endregion


    #region Download System

    bool ShouldDownloadModel()
    {
        bool hasUrl = !string.IsNullOrEmpty(ProductSelection.modelURL);

        bool isPreloaded =
            watches.Any(w => w.name == ProductSelection.productData.name) ||
            rings.Any(r => r.name == ProductSelection.productData.name);

        return hasUrl && !isPreloaded;
    }

    public IEnumerator DownloadAndSpawnModel(CategoryType category, Action<float> onProgress = null, Action onFailed = null)
    {
        string name = ProductSelection.productData.name;
        string url = ProductSelection.modelURL;

        GameObject model = null;

        yield return StartCoroutine(
            ModelLoaderService.DownloadAndLoad(
                url,
                (glbModel) => { model = glbModel; },
                onProgress,
                onFailed));

        if (model == null)
            yield break;

        model.name = name;

        var state = model.GetComponent<DownloadState>();
        if (state == null)
            state = model.AddComponent<DownloadState>();

        ProductDetails pd = model.GetComponent<ProductDetails>();
        if (pd == null)
            pd = model.AddComponent<ProductDetails>();

        pd.product = ProductSelection.productData;


        pd.colors.Clear();
        foreach (var c in pd.product.colors)
        {
            UnityEngine.Color newColor;
            ColorUtility.TryParseHtmlString(c.code, out newColor);
            pd.colors.Add(newColor);
        }

        pd.imagesUrl.Clear();

        foreach (var img in pd.product.images)
            pd.imagesUrl.Add(img.url);

        pd.texturesUrl.Clear();

        foreach (var t in pd.product.threeDModels)
            if (t.texture.Length > 0)
                pd.texturesUrl.Add(t.texture);

        // ---------------- UI MODEL PREVIEW (UNCHANGED) ----------------
        GameObject modelToView = Instantiate(UIManagerAR.instance.modelPrefab);
        modelToView.transform.parent = UIManagerAR.instance.UI_3D_Models_Parent.transform;
        modelToView.transform.localPosition = Vector3.zero;

        UIManagerAR.instance.UI_3D_Models.Add(modelToView);

        StartCoroutine(SetProductImages(pd));

        MeshFilter srcMF = model.GetComponentInChildren<MeshFilter>();
        MeshRenderer srcMR = model.GetComponentInChildren<MeshRenderer>();

        modelToView.transform.Find("Visual").GetComponent<MeshFilter>().mesh = srcMF.mesh;
        modelToView.transform.Find("Visual").GetComponent<MeshRenderer>().materials = srcMR.materials;
        modelToView.name = model.name;

        modelToView.GetComponent<ProductDetails>().product.id = pd.product.id;

        state.isDownloading = false;
        state.isReady = true;

        if (pd != null && pd.product != null)
            UIManagerAR.instance.SelectModel(pd);

        CategoryManager.Instance.mv.FrameObject(modelToView);

        Transform parent = spawnParents[category];

        //ResetSpawnParent(category);

        //model.transform.localRotation = Quaternion.identity;

        //if (category == CategoryType.Rings)
        //{
        //    float scaleFactor = HandItemsScaler.RingScaleFromBounds(
        //        model.GetComponentInChildren<Renderer>());

        //    model.transform.localScale = Vector3.one * scaleFactor;
        //}
        //else
        //{
        //    float scaleFactor = HandItemsScaler.WatchScaleFromBounds(
        //        model.GetComponentInChildren<Renderer>());

        //    model.transform.localScale = Vector3.one * scaleFactor;
        //}

        //model.transform.localEulerAngles = new Vector3(0, 180, 0);

        //if (category == CategoryType.Rings)
        //    parent.localEulerAngles = new Vector3(90, 0, 0);

        foreach (Transform obj in parent)
            obj.gameObject.SetActive(false);

        //model.transform.localScale = Vector3.one;
        model.transform.SetParent(parent);
        //if(category == CategoryType.Watches)
        //    model.transform.localPosition = Vector3.zero + GetComponent<HandTrackingVisualizer>().wristOffset;
        //else
        //{
            model.transform.localPosition = Vector3.zero;
            model.transform.localEulerAngles = new Vector3(0, 0, 0);
            model.transform.localScale = Vector3.one;
        //}
        //model.transform.localRotation = Quaternion.identity;

        if (category == CategoryType.Watches)
        {
            GetComponent<HandTrackingVisualizer>().watchWidth =
                model.GetComponentInChildren<MeshRenderer>().bounds.size.x;
        }

        //if (category == CategoryType.Rings)
        //    parent.localEulerAngles = new Vector3(0, 0, 0);

        if(CategoryManager.Instance.isDebugMode)Debug.Log("category : " + category);

        SpawnCanvas(model);

        model.GetComponentInChildren<MeshRenderer>().gameObject.layer = LayerMask.NameToLayer("top");

        CategoryManager.Instance.GetComponent<SlideUpPanel>().HidePanel();
    }

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
            if(CategoryManager.Instance.isDebugMode)Debug.Log("texture downloading");
        }
        if(CategoryManager.Instance.isDebugMode)Debug.Log("texture Finish");

    }

    public IEnumerator DownloadSpriteCoroutine(string url, List<Sprite> spritesList, int index)
    {
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            if(CategoryManager.Instance.isDebugMode)Debug.LogError(req.error);

            yield break;
        }

        byte[] imageData = req.downloadHandler.data;
        Texture2D texture = new Texture2D(2, 2);

        if (texture.LoadImage(imageData))
        {
            Sprite sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            if (spritesList != null)
                spritesList[index] = sprite;
        }
        else
        {
            if(CategoryManager.Instance.isDebugMode)Debug.LogError(url + "\n❌ Failed to decode image bytes. Response is not a valid PNG/JPG.");
        }
    }

    public IEnumerator DownloadTextureCoroutine(string url, List<Texture2D> texturesList, int index)
    {
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            if(CategoryManager.Instance.isDebugMode)Debug.LogError(req.error);

            yield break;
        }

        byte[] imageData = req.downloadHandler.data;
        Texture2D texture = new Texture2D(2, 2);

        if (texture.LoadImage(imageData))
        {
            if (texturesList != null)
                texturesList[index] = texture;
        }
        else
        {
            if(CategoryManager.Instance.isDebugMode)Debug.LogError(url + "\n❌ Failed to decode image bytes. Response is not a valid PNG/JPG.");
        }
    }

    public void ChangeModelTexture(int index)
    {
        foreach (Transform child in UIManagerAR.instance.colorParent_LD)
        {
            if (child.GetComponent<ModelVariant>()?.index == index)
                child.GetComponent<ModelVariant>().selectedImg.SetActive(true);
            else
                child.GetComponent<ModelVariant>()?.selectedImg.SetActive(false);
        }

        foreach (Transform child in UIManagerAR.instance.colorParent_SD)
        {
            if (child.GetComponent<ModelVariant>()?.index == index)
                child.GetComponent<ModelVariant>().selectedImg.SetActive(true);
            else
                child.GetComponent<ModelVariant>()?.selectedImg.SetActive(false);
        }

        foreach (Transform child in CategoryManager.Instance.modelVariantParent)
        {
            child.gameObject.SetActive(true);
            if (child.GetComponent<ModelVariant>()?.index == index)
                child.GetComponent<ModelVariant>().selectedImg.SetActive(true);
            else
                child.GetComponent<ModelVariant>()?.selectedImg.SetActive(false);
        }
        
        ProductDetails targetPd = UIManagerAR.instance.selectedModelDetails;
        if (targetPd == null) return;

        targetPd.selectedColorIndex = index;
        if (index >= 0 && index < targetPd.textures.Count)
        {
            Material mat = targetPd.GetComponentInChildren<MeshRenderer>().material;
            mat.mainTexture = targetPd.textures[index];
            targetPd.GetComponentInChildren<MeshRenderer>().material = mat;

            var modelView = UIManagerAR.instance.UI_3D_Models.Find(m =>
                m.GetComponent<ProductDetails>().product.id == targetPd.product.id);
            if (modelView != null)
            {
                MeshRenderer mvRenderer = modelView.GetComponentInChildren<MeshRenderer>();
                if (mvRenderer != null)
                    mvRenderer.material = mat;
            }
        }

        UIManagerAR.instance.UpdateDetailData(targetPd);
        Canvas.ForceUpdateCanvases();
    }

    #endregion


    #region GLB Loading

    //IEnumerator LoadAndSpawnGLB(string path, CategoryType category)
    //{
    //    //using FileStream stream = new(path, FileMode.Open, FileAccess.Read);

    //    //var importer = new GLTFSceneImporter(stream, new ImportOptions());

    //    //yield return importer.LoadSceneAsync();

    //    //GameObject model = importer.LastLoadedScene;

    //    //if (model == null)
    //    //{
    //    //    if(CategoryManager.Instance.isDebugMode)Debug.LogError("GLB failed to load.");
    //    //    yield break;
    //    //}


    //}


    private Vector3 GetTrueSize(GameObject obj)
    {
        MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>();
        if (meshFilters.Length == 0)
            return Vector3.zero;

        Bounds total = new Bounds();

        bool initialized = false;

        foreach (MeshFilter mf in meshFilters)
        {
            Mesh mesh = mf.sharedMesh;
            if (!mesh) continue;

            // Mesh.bounds is in local mesh space
            Bounds meshBounds = mesh.bounds;

            // Convert mesh bounds to world space
            Matrix4x4 localToWorld = mf.transform.localToWorldMatrix;

            // Transform the bounds
            Bounds worldBounds = TransformBounds(localToWorld, meshBounds);

            if (!initialized)
            {
                total = worldBounds;
                initialized = true;
            }
            else
            {
                total.Encapsulate(worldBounds);
            }
        }

        // Convert world bounds to the root object's LOCAL space
        Vector3 localSize = obj.transform.InverseTransformVector(total.size);

        return new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));
    }

    // Transform a bounds by a matrix (Unity safe method)
    private Bounds TransformBounds(Matrix4x4 matrix, Bounds bounds)
    {
        var center = matrix.MultiplyPoint(bounds.center);

        Vector3 extents = bounds.extents;
        Vector3 axisX = matrix.MultiplyVector(new Vector3(extents.x, 0, 0));
        Vector3 axisY = matrix.MultiplyVector(new Vector3(0, extents.y, 0));
        Vector3 axisZ = matrix.MultiplyVector(new Vector3(0, 0, extents.z));

        extents = new Vector3(
            Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x),
            Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y),
            Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z)
        );

        return new Bounds(center, extents * 2);
    }

    #endregion


    #region Utilities

    public bool IsModelReady(string name)
    {
        // Check currently active item first
        foreach (var item in activeItems.Values)
        {
            if (item != null && item.name == name)
            {
                var state = item.GetComponent<DownloadState>();
                return state != null && state.isReady;
            }
        }

        // Not loaded in scene yet → treat as not ready
        return false;
    }

    public void DisableAllObjects()
    {
        foreach (var item in activeItems.Values)
        {
            if (item != null)
                item.SetActive(false);
        }
    }

    public void ClearDownloadedModels()
    {
        foreach (var item in activeItems.Values)
        {
            if (item != null)
                Destroy(item);
        }

        activeItems.Clear();
    }

    public void DeleteAllHandItems()
    {
        foreach (var parent in spawnParents.Values)
        {
            foreach (Transform t in parent)
            {
                var details = t.GetComponent<ProductDetails>();
                if (details != null)
                {
                    var canvas = details.plusCanvas;

                    if (canvas != null)
                    {
                        Destroy(canvas);
                    }
                }

                Destroy(t.gameObject);
            }

        }

        activeItems.Clear();

        UIManagerAR.instance.DeleteAllSpawnedObjects();
    }

    public void DeleteSelectedItem(GameObject objToDelete)
    {
        foreach(var parent in spawnParents.Values)
        {
            foreach (Transform t in parent)
            {
                if (t.gameObject == objToDelete)
                {
                    Destroy(t.GetComponent<ProductDetails>().plusCanvas.gameObject);
                    Destroy(t.gameObject);
                    return;
                }
            }
        }
    }

    public void PreDownloadModel(CategoryType category)
    {
        if (ProductSelection.productData == null)
            return;

        // Already active in scene → no need to preload
        if (activeItems.ContainsKey(category) && activeItems[category] != null)
            return;

        if (ShouldDownloadModel())
        {
            StartCoroutine(DownloadAndSpawnModel(category));
        }
    }

    #endregion
}