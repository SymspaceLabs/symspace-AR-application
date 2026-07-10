using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityGLTF;
using static CategoryManager;
using static UnityEngine.GraphicsBuffer;
public enum CategoryType
{
    Necklaces,
    Earrings,
    LeftEarring,
    RightEarring,
    NosePin,
    Cap,
    Hat,
    Glasses,
    HeadPin,
    Watches,
    Rings
}

public class ARJewelryManager : MonoBehaviour
{
    public static ARJewelryManager Instance;

    [Header("References")]
    public ARFaceManager faceManager;

    //public List<JewelryItem> jewelryItems = new List<JewelryItem>();
    public List<Jewelries> jewelries = new List<Jewelries>();
    public List<GameObject> spawnedjewelries = new List<GameObject>();

    private ARFace currentFace;

    public Slider xSlider;
    public Slider ySlider;
    public Slider zSlider;

    public TextMeshProUGUI xText;
    public TextMeshProUGUI yText;
    public TextMeshProUGUI zText;

    public Slider leftEaringX;
    public Slider leftEaringY;
    public Slider leftEaringZ;

    public int currentItemSelected;

    public GameObject jewelryHolder;
    public GameObject necklaceHolder;

    private string localPath;


    //public List<GameObject> downloadedModels;

    public GameObject plusBtnCanvas;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private IEnumerator Start()
    {
        localPath = Path.Combine(Application.persistentDataPath, "tempModel.glb");

        yield return null;

        DisableOcclusion();
    }

    void DisableOcclusion()
    {
        //Camera.main.depthTextureMode = DepthTextureMode.None;
        List<XROcclusionSubsystem> subsystems = new List<XROcclusionSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);
        if (subsystems.Count < 1)
            if(CategoryManager.Instance.isDebugMode)Debug.Log("no subsytem found");

        foreach (var subsystem in subsystems)
        {
            if (subsystem != null && subsystem.running)
            {
                if(CategoryManager.Instance.isDebugMode)Debug.Log("Stopping XROcclusionSubsystem in this scene");
                subsystem.Stop();
            }
        }
    }

    void OnEnable()
    {
        faceManager.trackablesChanged.AddListener(OnFacesChanged);
    }
    void OnDisable()
    {
        faceManager.trackablesChanged.RemoveListener(OnFacesChanged);
    }

    private void OnFacesChanged(ARTrackablesChangedEventArgs<ARFace> args)
    {
        //if(CategoryManager.Instance.isDebugMode)Debug.Log("Face Detected");
        foreach (var face in args.added)
        {
            //if (currentFace == null)
            //{
            currentFace = face;
            Invoke(nameof(InitializeJewelryItems), 3f);
            //}
        }

        foreach (var face in args.updated)
        {
            //if(currentFace == null)
            //{
            currentFace = face;
            //UpdateItems();
            InitializeJewelryItems();
            //}
        }

        foreach (var face in args.removed)
        {
            if (currentFace != null && face.Value.trackableId == currentFace.trackableId)
            {
                RemoveJewelryItems();
                currentFace = null;
            }
        }
    }
    private void InitializeJewelryItems()
    {
        foreach (var item in jewelries)
        {
            if (item.instance != null)
            {
                if (currentFace != null)
                {
                    item.instance.SetActive(true);
                    //item.instance.transform.parent = currentFace.transform;
                    item.instance.transform.localRotation = Quaternion.identity;
                }
                else
                    return;

                Vector3 localPosition = Vector3.zero;

                switch (item.category)
                {
                    case CategoryType.Glasses:
                        if (currentFace.leftEye != null && currentFace.rightEye != null)
                        {
#if UNITY_IOS
                            item.instance.transform.parent = currentFace.transform;
                            Vector3 leftEye = currentFace.leftEye.localPosition;
                            Vector3 rightEye = currentFace.rightEye.localPosition;
                            localPosition = (leftEye + rightEye) / 2f + item.localOffset;
#endif
                        }
#if UNITY_ANDROID

                        item.instance.transform.parent = currentFace.GetComponent<ARFaceLandMarks>().glassesPosition;
                            localPosition = Vector3.zero + item.localOffset;
                            //Vector3 leftEye = GetLandmarkWorldPosition(ARKitFaceRegion.LeftEye, Vector3.zero);
                            //Vector3 rightEye = GetLandmarkWorldPosition(ARKitFaceRegion.RightEye, Vector3.zero);
                            //localPosition = (leftEye + rightEye) / 2f + item.localOffset;
#endif
                        break;

                    case CategoryType.LeftEarring:
                        item.instance.transform.parent = currentFace.GetComponent<ARFaceLandMarks>().leftEaring;
                        localPosition = Vector3.zero/* + item.localOffset*/;
                        //localPosition = GetLandmarkWorldPosition(ARKitFaceRegion.LeftEar, item.localOffset);
                        break;

                    case CategoryType.RightEarring:
                        item.instance.transform.parent = currentFace.GetComponent<ARFaceLandMarks>().rightEaring;
                        localPosition = Vector3.zero/* + item.localOffset*/;
                        //localPosition = GetLandmarkWorldPosition(ARKitFaceRegion.RightEar, item.localOffset);
                        break;

                    case CategoryType.Necklaces:
                        item.instance.transform.parent = currentFace.GetComponent<ARFaceLandMarks>().necklacePoint;
                        localPosition = Vector3.zero/* + item.localOffset*/;
                        //localPosition = GetLandmarkWorldPosition(ARKitFaceRegion.ChinIndex, item.localOffset);
                        break;

                    case CategoryType.NosePin:
                        localPosition = GetLandmarkWorldPosition(ARKitFaceRegion.NoseTip, item.localOffset);
                        break;

                    // ✅ NEW: CAP SUPPORT
                    case CategoryType.Cap:
                        item.instance.transform.parent = currentFace.GetComponent<ARFaceLandMarks>().headTop;
                        localPosition = Vector3.zero/* + item.localOffset*/;
                        //localPosition = GetLandmarkWorldPosition(ARKitFaceRegion.HeadTop, item.localOffset);
                        break;

                    case CategoryType.Hat:
                        item.instance.transform.parent = currentFace.GetComponent<ARFaceLandMarks>().headTop;
                        localPosition = Vector3.zero/* + item.localOffset*/;
                        //localPosition = GetLandmarkWorldPosition(ARKitFaceRegion.HeadTop, item.localOffset);
                        break;
                }


                //if (localPosition == Vector3.zero)
                //    continue;

                item.instance.transform.localPosition = localPosition;

                if(CategoryManager.Instance.isDebugMode)Debug.Log($"item.instance parent : {item.instance.transform.parent.name} parent Pos : {item.instance.transform.parent.localPosition}, item Pos: {item.instance.transform.localPosition}");

                float targetSize = 0.05f;

                switch (item.category)
                {
                    case CategoryType.Necklaces: targetSize = 0.1f; break;
                    case CategoryType.Rings: targetSize = 0.02f; break;
                    case CategoryType.LeftEarring:
                    case CategoryType.RightEarring: targetSize = 0.03f; break;
                    case CategoryType.NosePin: targetSize = 0.015f; break;
                    case CategoryType.Glasses: targetSize = 0.12f; break;

                    // ✅ NEW
                    case CategoryType.Cap: targetSize = 0.18f; break;
                }

                if (item.allowScale)
                {
                    NormalizeJewelryScale(item.instance, targetSize);
                    item.allowScale = true;
                }
            }
        }
    }
    private void RemoveJewelryItems()
    {
        foreach (var item in jewelries)
        {
            if (item.instance != null)
            {
                item.instance.SetActive(false);
                item.instance.transform.SetParent(null);
                //item.instance = null;
            }
        }
    }

    //GameObject FindExistingModel(string productId, CategoryType cat)
    //{
    //    GameObject downloaded;
    //    if (cat == CategoryType.LeftEarring)
    //        downloaded = downloadedModels.Find(m =>
    //        m.GetComponent<ProductDetails>().product.id == productId && m.GetComponent<ProductDetails>().category == CategoryType.LeftEarring);
    //    else if (cat == CategoryType.RightEarring)
    //        downloaded = downloadedModels.Find(m =>
    //        m.GetComponent<ProductDetails>().product.id == productId && m.GetComponent<ProductDetails>().category == CategoryType.RightEarring);
    //    else
    //        downloaded = downloadedModels.Find(m =>
    //            m.GetComponent<ProductDetails>().product.id == productId);

    //    if (downloaded != null)
    //        return downloaded;

    //    return null;
    //}

    public IEnumerator JewelrySelected(Products p, string url, string categoryName, ProductItemData pid)
    {
        CategoryType category;
        ProductSelection.TryParseObjectType(categoryName, out category);
        if(CategoryManager.Instance.isDebugMode)Debug.Log("name: " + categoryName + ", result: " + category);

        GameObject newObject = null;
        DownloadState stateCheck = null;

        if (category == CategoryType.Necklaces)
            newObject = Instantiate(necklaceHolder);
        else
            newObject = Instantiate(jewelryHolder);

        var state = newObject.GetComponent<DownloadState>() ?? newObject.AddComponent<DownloadState>();
        state.isDownloading = false;
        state.isReady = false;

        newObject.GetComponent<ProductDetails>().product.id = p.id;

        stateCheck = newObject.GetComponent<DownloadState>();

        if (!stateCheck.isReady && !stateCheck.isDownloading)
        {
            if (pid != null)
            {
               if(CategoryManager.Instance.isDebugMode)Debug.Log("PID is not empty");
                yield return StartCoroutine(
                    DownloadAndAssign(
                        url,
                        newObject,
                        p,
                        pid.ProductProgress,
                        pid.DownloadFailed));
            }
            else
            {
                yield return StartCoroutine(
                    DownloadAndAssign(
                        url,
                        newObject,
                        p));
            }
        }

        SpawnCanvas(newObject, category);

        if (stateCheck.isReady)
            CategoryManager.Instance.GetComponent<SlideUpPanel>().HidePanel();

        ProductDetails pd = newObject.GetComponent<ProductDetails>();
        if (pd != null && pd.product != null)
        {
            UIManagerAR.instance.SelectModel(pd);
        }
    }

    void SpawnCanvas(GameObject newObject, CategoryType category)
    {
        WorldCanvasFaceCamera[] allWorldCanvases =
                    FindObjectsByType<WorldCanvasFaceCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        bool spawnCanvas = true;

        var newPd = newObject.GetComponent<ProductDetails>();

        foreach (var c in allWorldCanvases)
        {
            if (c.pd == newPd)
            {
                spawnCanvas = false;
                c.gameObject.SetActive(true);
                break;
            }
        }

        if (spawnCanvas)
        {
            WorldCanvasFaceCamera btnCanvas =
                Instantiate(plusBtnCanvas).GetComponent<WorldCanvasFaceCamera>();

            if (!btnCanvas.GetComponent<ObjectDetail>())
                btnCanvas.gameObject.AddComponent<ObjectDetail>();

            //btnCanvas.canvasPosition = WorldCanvasFaceCamera.CanvasPosition.TopLeft;
            btnCanvas.targetModel = newObject.GetComponentInChildren<MeshRenderer>()?.transform;
            btnCanvas.pd = newPd;
            newPd.plusCanvas = btnCanvas.gameObject;
            btnCanvas.objDetail = newObject.GetComponent<ObjectDetail>();

            switch (category)
            {
                case CategoryType.Necklaces:
                    btnCanvas.canvasPosition = WorldCanvasFaceCamera.CanvasPosition.Left;
                    break;
                case CategoryType.Earrings:
                    btnCanvas.canvasPosition = WorldCanvasFaceCamera.CanvasPosition.Left;
                    break;
                case CategoryType.Glasses:
                    btnCanvas.canvasPosition = WorldCanvasFaceCamera.CanvasPosition.Right;
                    break;
                case CategoryType.Hat:
                    btnCanvas.canvasPosition = WorldCanvasFaceCamera.CanvasPosition.Right;
                    break;
            }
        }
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

    IEnumerator DownloadAndAssign(string url, GameObject targetPrefab, Products p,
    Action<float> onProgress = null,
    Action onFailed = null)
    {
        if (targetPrefab == null)
        {
            if(CategoryManager.Instance.isDebugMode)Debug.LogError("Target prefab is null!");
            yield break;
        }

        yield return null;

        var state = targetPrefab.GetComponent<DownloadState>();
        state.isDownloading = true;
        state.isReady = false;

        GameObject loadedGLB = null;

        yield return StartCoroutine(
            ModelLoaderService.DownloadAndLoad(
                url,
                (model) => { loadedGLB = model; },
                onProgress,
                onFailed));

        if (loadedGLB == null)
            yield break;

        Transform actualObject = loadedGLB.transform.childCount > 0
            ? loadedGLB.transform.GetChild(0)
            : loadedGLB.transform;

        actualObject.localScale = Vector3.one;

        loadedGLB.transform.parent = targetPrefab.transform;
        loadedGLB.transform.localPosition = Vector3.zero;
        loadedGLB.transform.localRotation = Quaternion.identity;

        targetPrefab.name = p.name;

        CategoryType cat;
        ProductSelection.TryParseObjectType(p.category.name, out cat);

        var newJewelry = new Jewelries
        {
            category = cat,
            instance = targetPrefab
        };

        if(CategoryManager.Instance.isDebugMode)Debug.Log("Name : " + p.name + ", tag : " + p.category.name);
        if(CategoryManager.Instance.isDebugMode)Debug.Log("category : " + cat);

        GameObject newRightEaring = new GameObject();

        if (newJewelry.category == CategoryType.Earrings)
        {
            newRightEaring = Instantiate(newJewelry.instance);

            Quaternion tempRotation =
                newRightEaring.transform.GetChild(0).rotation;

            tempRotation.y = 180f;

            newRightEaring.transform.GetChild(0).rotation =
                tempRotation;
        }

        for (int i = 0; i < jewelries.Count; i++)
        {
            switch (newJewelry.category)
            {
                case CategoryType.Earrings:
                    {
                        if (jewelries[i].category != CategoryType.LeftEarring &&
                            jewelries[i].category != CategoryType.RightEarring)
                            break;

                        if (jewelries[i].instance != null)
                        {
                            Destroy(jewelries[i].instance.GetComponent<ProductDetails>().plusCanvas.gameObject);
                            Destroy(jewelries[i].instance.gameObject);
                            jewelries[i].instance = null;
                        }

                        if (jewelries[i].category == CategoryType.LeftEarring)
                        {
                            newJewelry.instance.GetComponent<ProductDetails>().category =
                                CategoryType.LeftEarring;

                            jewelries[i].instance = newJewelry.instance;
                        }
                        else
                        {
                            newRightEaring.GetComponent<ProductDetails>().category =
                                CategoryType.RightEarring;

                            jewelries[i].instance = newRightEaring;
                        }

                        currentItemSelected = i;
                        break;
                    }

                case CategoryType.Necklaces:
                    HandleSingleItem(i, newJewelry);
                    break;

                case CategoryType.Cap:
                    HandleSingleItem(i, newJewelry);
                    break;

                case CategoryType.Hat:
                    HandleSingleItem(i, newJewelry);
                    break;

                case CategoryType.Glasses:
                    HandleSingleItem(i, newJewelry);
                    break;

                case CategoryType.NosePin:
                    HandleSingleItem(i, newJewelry);
                    break;
            }
        }

        foreach (var jewelry in jewelries)
        {
            if (jewelry.instance != null)
            {
                jewelry.instance.transform.localScale = Vector3.one;

                if (jewelry.category == CategoryType.LeftEarring ||
                    jewelry.category == CategoryType.RightEarring)
                {
                    jewelry.instance.transform.localScale *= 2.5f;
                }
                else if (jewelry.category == CategoryType.Glasses)
                {
                    jewelry.instance.transform.localScale *= 1.2f;
                }
                else if (jewelry.category == CategoryType.Cap)
                {
                    jewelry.instance.transform.localScale *= 0.7f;
                }
                else
                {
                    jewelry.instance.transform.localScale *= 1f;
                }
            }
        }

        ProductDetails pd = targetPrefab.GetComponent<ProductDetails>();

        pd.product = p;

        pd.colors.Clear();
        foreach (var c in p.colors)
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

        StartCoroutine(SetProductImages(targetPrefab.GetComponent<ProductDetails>()));

        MeshFilter srcMF = loadedGLB.GetComponentInChildren<MeshFilter>();
        MeshRenderer srcMR = loadedGLB.GetComponentInChildren<MeshRenderer>();

        modelToView.transform.Find("Visual").GetComponent<MeshFilter>().mesh = srcMF.mesh;
        modelToView.transform.Find("Visual").GetComponent<MeshRenderer>().materials = srcMR.materials;
        modelToView.name = targetPrefab.name;

        modelToView.GetComponent<ProductDetails>().product.id = pd.product.id;

        state.isDownloading = false;
        state.isReady = true;

        CategoryManager.Instance.mv.FrameObject(modelToView);
        CategoryManager.Instance.GetComponent<SlideUpPanel>().ShowPanel();

        if(CategoryManager.Instance.isDebugMode)Debug.Log("✅ GLB model downloaded, instantiated, and scaled successfully.");
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

        if (pd.selectedColorIndex >= 0 && pd.selectedColorIndex < pd.textures.Count && pd.textures[pd.selectedColorIndex] != null)
        {
            MeshRenderer renderer = pd.GetComponentInChildren<MeshRenderer>();
            if (renderer != null)
            {
                Material mat = renderer.material;
                mat.mainTexture = pd.textures[pd.selectedColorIndex];
                renderer.material = mat;
            }
        }
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

    void HandleSingleItem(int i, Jewelries newJewelry)
    {
        if (jewelries[i].category != newJewelry.category)
            return;

        // Remove old
        if (jewelries[i].instance != null)
        {
            Destroy(jewelries[i].instance.GetComponent<ProductDetails>().plusCanvas.gameObject);
            Destroy(jewelries[i].instance.gameObject);
            jewelries[i].instance = null;
        }

        // Assign new
        newJewelry.instance.GetComponent<ProductDetails>().category = newJewelry.category;
        jewelries[i].instance = newJewelry.instance;
        currentItemSelected = i;

        // Category-specific tweaks
        /*if (newJewelry.category == CategoryType.Necklaces)
        {
            Vector3 rot = jewelries[i].instance.transform.GetChild(1).localEulerAngles;
            rot.x = -20f;
            rot.y = 180;
            jewelries[i].instance.transform.GetChild(1).localEulerAngles = rot;
        }
        else */
        if (newJewelry.category == CategoryType.Glasses)
        {
            Vector3 rot = jewelries[i].instance.transform.GetChild(0).localEulerAngles;
            rot.y = 180;
            jewelries[i].instance.transform.GetChild(0).localEulerAngles = rot;
        }
        else if (newJewelry.category == CategoryType.LeftEarring || newJewelry.category == CategoryType.RightEarring)
        {
            Vector3 rot = jewelries[i].instance.transform.GetChild(0).localEulerAngles;
            rot.y = 180;
            jewelries[i].instance.transform.GetChild(0).localEulerAngles = rot;
        }
    }

    public bool Equip(Jewelries newItem)
    {
        foreach (var j in jewelries)
        {
            if (j.category == newItem.category && j != newItem)
            {
                if (j.instance != null)
                    Destroy(j.instance.gameObject);

                j.instance = null;
            }
        }

        if (newItem.instance != null)
            return false;

        return true;
    }

    public void DeleteAllJewelries()
    {
        foreach(var jewelry in jewelries)
        {
            if (jewelry.instance != null)
            {
                var details = jewelry.instance.GetComponent<ProductDetails>();
                var canvas = details.plusCanvas;

                if (canvas != null)
                {
                    Destroy(canvas);
                }

                Destroy(jewelry.instance.gameObject);
                //jewelry.instance.gameObject.SetActive(false);
                jewelry.instance = null;
            }
        }

        UIManagerAR.instance.DeleteAllSpawnedObjects();
    }

    public void DeleteSelectedJewelry(GameObject objToDelete)
    {
        foreach (var j in jewelries)
        {
            if(j.instance != null && j.instance == objToDelete)
            {
                Destroy(j.instance.GetComponent<ProductDetails>().plusCanvas.gameObject);
                Destroy(j.instance.gameObject);
                j.instance = null;
                break;
            }
        }
    }

#if UNITY_ANDROID
#elif UNITY_IOS
#endif
    private Vector3 GetLandmarkWorldPosition(int vertexIndex, Vector3 offset)
    {
        var vertices = GetFaceVertices();
        if(CategoryManager.Instance.isDebugMode)Debug.Log("vertex index " + vertexIndex);
        if (vertices != null && vertexIndex < vertices.Length)
        {
            return vertices[vertexIndex] + offset;
        }
        if(vertices != null)
            if(CategoryManager.Instance.isDebugMode)Debug.Log("vertices " + vertices.Length);

        return Vector3.zero;
    }
    private Vector3[] GetFaceVertices()
    {
        MeshFilter meshFilter = null;
        if(currentFace != null)
            meshFilter = currentFace.GetComponent<MeshFilter>();

        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            return meshFilter.sharedMesh.vertices;
        }
        if(CategoryManager.Instance.isDebugMode)Debug.Log("Mesh Filter is null");
        return null;
    }
    // Replace these with actual ARKit vertex indices or custom mappings as per your AR SDK
    private static class ARKitFaceRegion
    {
#if UNITY_ANDROID
        public const int LeftEar = 215;         // Sample index
        public const int RightEar = 435;        // Sample index
        public const int NoseTip = 9;           // Sample index
        public const int ForeheadCenter = 10;   // Sample index
        public const int LeftEye = 133;         // Approx eye socket center
        public const int RightEye = 362;        // Approx eye socket center
        public const int HeadTop = 20;          // Approx top of head (for headpin)
        public const int ChinIndex = 152;       // Approx bottom of chin
#elif UNITY_IOS
        public const int LeftEar = 208;         // Sample index
        public const int RightEar = 1213;        // Sample index
        public const int NoseTip = 9;           // Sample index
        public const int ForeheadCenter = 10;   // Sample index
        public const int LeftEye = 1075;         // Approx eye socket center
        public const int RightEye = 1075;        // Approx eye socket center
        public const int HeadTop = 20;          // Approx top of head (for headpin)
        public const int ChinIndex = 1047;       // Approx bottom of chin
#endif
    }

    #region Dynamic Jewelry Scaling

    // Calculates world-space size of the model including all children
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


    // Normalizes the model scale based on a target size
    private void NormalizeJewelryScale(GameObject model, float targetSize)
    {
        if (model == null) return;

        model.transform.localScale = Vector3.one;

        Vector3 trueSize = GetTrueSize(model);
        if(CategoryManager.Instance.isDebugMode)Debug.Log("Mesh true size: " + trueSize);

        float maxDimension = Mathf.Max(trueSize.x, trueSize.y, trueSize.z);

        if (maxDimension > 0)
        {
            float scaleFactor = targetSize / maxDimension;
            model.transform.localScale = Vector3.one * scaleFactor;
        }
    }

    #endregion
}

[System.Serializable]
public class JewelryItem
{
    public CategoryType category;
    public GameObject prefab;
    public Vector3 localOffset;
    public float smoothTime = 0.1f;
    [HideInInspector] public GameObject instance;
    [HideInInspector] public Vector3 velocity;
    public bool isSpawn = false;
}

[System.Serializable]
public class Jewelries
{
    public string id;
    public CategoryType category;
    public GameObject instance;
    public Vector3 localOffset;
    public float smoothTime = 0.1f;
    [HideInInspector] public Vector3 velocity;

    public bool allowScale = false;
}
