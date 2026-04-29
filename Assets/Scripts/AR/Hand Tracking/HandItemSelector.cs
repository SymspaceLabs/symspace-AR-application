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

    #endregion


    #region Runtime Containers

    Dictionary<CategoryType, List<GameObject>> preloadedItems;
    Dictionary<CategoryType, Transform> spawnParents;
    Dictionary<CategoryType, GameObject> activeItems = new();
    Dictionary<CategoryType, GameObject> uiParents;

    Dictionary<CategoryType, Vector3> initialPos = new();
    Dictionary<CategoryType, Quaternion> initialRot = new();
    Dictionary<CategoryType, Vector3> initialScale = new();

    Dictionary<string, GameObject> downloadedModels = new();

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
        //            Debug.Log("Model already downloading.");
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
            //HandleProductSelection(pid? pid: null);
            if (ShouldDownloadModel())
            {
                StartCoroutine(DownloadAndSpawnModel(category, pid ? pid.ProductProgress : null, pid ? pid.DownloadFailed : null));
                return;
            }
        }

        var state = item.GetComponent<DownloadState>();

        if (state != null && !state.isReady)
        {
            Debug.Log("Model not ready yet.");
            return;
        }

        SetActiveItem(category, item);


        CategoryManager.Instance.GetComponent<SlideUpPanel>().HidePanel();
    }

    GameObject GetItem(string itemName, CategoryType category)
    {
        if (downloadedModels.ContainsKey(itemName))
            return downloadedModels[itemName];

        return GetPreloadedItem(category, itemName);
    }

    void SetActiveItem(CategoryType category, GameObject obj)
    {
        if (activeItems.ContainsKey(category) && activeItems[category] != obj)
            activeItems[category].SetActive(false);

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

    public IEnumerator DownloadAndSpawnModel(
        CategoryType category,
        Action<float> onProgress = null,
        Action onFailed = null)
    {
        string name = ProductSelection.productData.name;

        string url = ProductSelection.modelURL;
        string localPath = Path.Combine(Application.persistentDataPath, $"{name}.glb");

        GameObject model = null;

        yield return StartCoroutine(ModelLoaderService.DownloadAndLoad(url, (glbModel) => { model = glbModel; }, onProgress, onFailed));

        //using (UnityWebRequest www = UnityWebRequest.Get(url))
        //{
        //    www.downloadHandler = new DownloadHandlerFile(localPath);
        //    www.SendWebRequest();


        //    while (!www.isDone)
        //    {
        //        onProgress?.Invoke(www.downloadProgress);
        //        yield return null;
        //    }

        //    if (www.result != UnityWebRequest.Result.Success)
        //    {
        //        Debug.LogError("Download failed: " + www.error);
        //        onFailed?.Invoke();
        //        yield break;
        //    }

        //    onProgress?.Invoke(1f);
        //    Debug.Log("total size : " + www.downloadedBytes);
        //}

        if (model == null)
            yield break;

        model.name = name;

        var state = model.GetComponent<DownloadState>();
        if (state == null)
            state = model.AddComponent<DownloadState>();

        state.isDownloading = true;
        state.isReady = false;

        Transform parent = spawnParents[category];

        ResetSpawnParent(category);

        // Reset transform
        model.transform.localRotation = Quaternion.identity;

        if (category == CategoryType.Rings)
        {
            float scaleFactor = HandItemsScaler.RingScaleFromBounds(model.GetComponentInChildren<Renderer>());
            Debug.Log("Ring Mesh true size: " + scaleFactor);

            model.transform.localScale = Vector3.one * scaleFactor;
        }
        else
        {
            float scaleFactor = HandItemsScaler.WatchScaleFromBounds(model.GetComponentInChildren<Renderer>());
            Debug.Log("Watch Mesh true size: " + scaleFactor);

            model.transform.localScale = Vector3.one * scaleFactor;
            //model.transform.localScale = Vector3.one;
        }

        model.transform.localEulerAngles = new Vector3(0, 180, 0);


        if (category == CategoryType.Rings)
            parent.localEulerAngles = new Vector3(90, 0, 0);

        // watches[0].transform.localEulerAngles = new Vector3(0, 0, 0);

        foreach (Transform obj in parent)
            obj.gameObject.SetActive(false);
        model.transform.SetParent(parent);

        model.transform.localPosition = Vector3.zero;
        // yield return new WaitForSeconds(1f);
        // watches[0].transform.localEulerAngles = new Vector3(90, 0, 0);
        // Debug.Log("Watch : " + watches[0].transform.localEulerAngles);

        if (category == CategoryType.Watches)
        {
            //GetComponent<HandTrackingVisualizer>().currentWristAnchor = loadedGLB;
            GetComponent<HandTrackingVisualizer>().watchWidth = model.GetComponentInChildren<MeshRenderer>().bounds.size.x;
        }

        if (category == CategoryType.Rings)
            parent.localEulerAngles = new Vector3(0, 0, 0);

        //else if (category == CategoryType.Rings)
        //    GetComponent<RingPlacer>().currentRing = loadedGLB;

        Debug.Log("cateogry : " + category);

        // Add to downloaded models cache
        downloadedModels[ProductSelection.productData.name] = model;

        // Initially disable the model
        //model.SetActive(false);

        Debug.Log($"✅ Model loaded and cached: {ProductSelection.productData.name}");

        //model.transform.localScale = Vector3.one;
        //model.transform.SetParent(parent);
        //model.transform.localPosition = Vector3.zero;
        //model.transform.localRotation = Quaternion.identity;
        //model.transform.localEulerAngles = new Vector3(0, 180, 0);

        //foreach (Transform t in parent)
        //    t.gameObject.SetActive(false);

        //if (category == CategoryType.Watches)
        //{
        //    GetComponent<HandTrackingVisualizer>().watchWidth =
        //        model.GetComponentInChildren<MeshRenderer>().bounds.size.x;
        //}

        //downloadedModels[name] = model;

        state.isDownloading = false;
        state.isReady = true;

        //Debug.Log($"Model downloaded and cached: {name}");

        //SelectItem(name, category.ToString());

        CategoryManager.Instance.GetComponent<SlideUpPanel>().HidePanel();

        //yield return StartCoroutine(LoadAndSpawnGLB(localPath, category));

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
    //    //    Debug.LogError("GLB failed to load.");
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
        if (!downloadedModels.ContainsKey(name))
            return false;

        var state = downloadedModels[name].GetComponent<DownloadState>();

        return state != null && state.isReady;
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
        foreach (var model in downloadedModels.Values)
        {
            if (model != null)
                Destroy(model);
        }

        downloadedModels.Clear();
        activeItems.Clear();
    }

    public void DeleteAllHandItems()
    {
        foreach (var parent in spawnParents.Values)
        {
            foreach (Transform t in parent)
                t.gameObject.SetActive(false);
                //Destroy(t.gameObject);
        }

        //ClearDownloadedModels();
    }

    public void PreDownloadModel(CategoryType category)
    {
        if (!downloadedModels.ContainsKey(ProductSelection.productData.name))
            StartCoroutine(DownloadAndSpawnModel(category));
    }

    #endregion
}