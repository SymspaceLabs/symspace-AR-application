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

    // Lists for pre-loaded objects (remove these if you're only using downloaded models)
    public List<GameObject> watches;
    public List<GameObject> rings;

    // Spawn parents for downloaded models
    public Transform watchSpawnParent;
    public Transform ringSpawnParent;

    Vector3 initialWatchPos;
    Quaternion initialWatchRot;
    Vector3 initialWatchScale;

    public Vector3 initialRingPos;
    public Quaternion initialRingRot;
    public Vector3 initialRingScale;

    // Parent containers for UI organization
    public GameObject watchesParent;
    public GameObject ringsParent;

    // Active spawned models
    private GameObject activeWatch;
    private GameObject activeRing;

    // Cache for downloaded models to avoid re-downloading
    private Dictionary<string, GameObject> downloadedModels = new Dictionary<string, GameObject>();

    private void Awake()
    {
        Instance = this;

        initialWatchPos = watchSpawnParent.localPosition;
        initialWatchRot = watchSpawnParent.localRotation;
        initialWatchScale = watchSpawnParent.localScale;

        initialRingPos = ringSpawnParent.localPosition;
        initialRingRot = ringSpawnParent.localRotation;
        initialRingScale = ringSpawnParent.localScale;
    }

    private void Start()
    {
        // Disable pre-loaded objects initially
        DisableAllObjects();

        if (ProductSelection.productData != null)
        {
            HandleProductSelection();
        }
    }

    private void HandleProductSelection()
    {
        if (ProductSelection.SelectedObjectType == CategoryType.Watches)
        {
            // Check if we should download or use pre-loaded
            if (ShouldDownloadModel())
            {
                StartCoroutine(DownloadAndSpawnModel(CategoryType.Watches));
            }
            else
            {
                // Use pre-loaded watch
                foreach (GameObject w in watches)
                {
                    if (w.name == ProductSelection.productData.name)
                    {
                        SelectItem(w.name, CategoryType.Watches.ToString());
                    }
                }
            }
        }
        else if (ProductSelection.SelectedObjectType == CategoryType.Rings)
        {
            // Check if we should download or use pre-loaded
            if (ShouldDownloadModel())
            {
                StartCoroutine(DownloadAndSpawnModel(CategoryType.Rings));
            }
            else
            {
                // Use pre-loaded ring
                foreach (GameObject r in rings)
                {
                    if (r.name == ProductSelection.productData.name)
                    {
                        SelectItem(r.name, CategoryType.Rings.ToString());
                    }
                }
            }
        }
    }

    private bool ShouldDownloadModel()
    {
        // Check if product has a model URL or if it's not in pre-loaded lists
        bool hasUrl = !string.IsNullOrEmpty(ProductSelection.modelURL);
        bool isPreLoaded = watches.Any(w => w.name == ProductSelection.productData.name) ||
                          rings.Any(r => r.name == ProductSelection.productData.name);

        return hasUrl && !isPreLoaded;
    }

    public void DisableAllObjects()
    {
        // Disable pre-loaded objects
        //foreach (var item in watches)
        //    item.SetActive(false);

        //foreach (var item in rings)
        //    item.SetActive(false);

        //Disable downloaded models
        DisableDownloadedModels();
    }

    private void DisableDownloadedModels()
    {
        if (activeWatch != null && !watches.Any(w => w == activeWatch))
            activeWatch.SetActive(false);

        if (activeRing != null && !rings.Any(r => r == activeRing))
            activeRing.SetActive(false);
    }

    private GameObject GetItemByName(List<GameObject> list, string name) =>
        list.FirstOrDefault(obj => obj.name == name);

    public void SelectItem(string itemName, string categoryName)
    {
        CategoryType category;
        ProductSelection.TryParseObjectType(categoryName, out category);

        ProductSelection.SelectedObjectType = category;

        switch (category)
        {
            case CategoryType.Watches:
                HandleWatchSelection(itemName);

                break;

            case CategoryType.Rings:
                HandleRingSelection(itemName);
                break;
        }
    }

    private void HandleWatchSelection(string itemName)
    {
        // First check downloaded models
        if (downloadedModels.ContainsKey(itemName) && downloadedModels[itemName] != null)
        {
            if (activeWatch != null && activeWatch != downloadedModels[itemName])
                activeWatch.SetActive(false);

            activeWatch = downloadedModels[itemName];
            activeWatch.SetActive(true);
            watchesParent.SetActive(true);
        }
        else
        {
            HandleProductSelection();
        }
        
        // Then check pre-loaded watches
        //else
        //{
        //    if (activeWatch != null && activeWatch.name != itemName)
        //        activeWatch.SetActive(false);

        //    activeWatch = GetItemByName(watches, itemName);
        //    if (activeWatch != null)
        //    {
        //        activeWatch.SetActive(true);
        //        watchesParent.SetActive(true);
        //    }
        //}
    }

    private void HandleRingSelection(string itemName)
    {
        // First check downloaded models
        if (downloadedModels.ContainsKey(itemName) && downloadedModels[itemName] != null)
        {
            if (activeRing != null && activeRing != downloadedModels[itemName])
                activeRing.SetActive(false);

            activeRing = downloadedModels[itemName];
            activeRing.SetActive(true);
            ringsParent.SetActive(true);
        }
        else
        {
            HandleProductSelection();
        }

        // Then check pre-loaded rings
        //else
        //{
        //    if (activeRing != null && activeRing.name != itemName)
        //        activeRing.SetActive(false);

        //    activeRing = GetItemByName(rings, itemName);
        //    if (activeRing != null)
        //    {
        //        activeRing.SetActive(true);
        //        ringsParent.SetActive(true);
        //    }
        //}
    }

    public IEnumerator DownloadAndSpawnModel(CategoryType category)
    {
        if (string.IsNullOrEmpty(ProductSelection.modelURL))
        {
            Debug.LogError($"No model URL for product: {ProductSelection.productData.name}");
            yield break;
        }

        // Check if already downloaded
        if (downloadedModels.ContainsKey(ProductSelection.productData.name) && downloadedModels[ProductSelection.productData.name] != null)
        {
            Debug.Log($"Model already downloaded: {ProductSelection.productData.name}");
            SelectItem(ProductSelection.productData.name, category.ToString());
            yield break;
        }

        string url = ProductSelection.modelURL;
        string localPath = Path.Combine(Application.persistentDataPath, $"{ProductSelection.productData.name}.glb");

        Debug.Log($"Downloading model from: {url}");
        Debug.Log($"Saving to: {localPath}");

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.downloadHandler = new DownloadHandlerFile(localPath);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Download failed for {ProductSelection.productData.name}: {www.error}");
                yield break;
            }

            Debug.Log($"Download completed: {localPath}");

            // Load and spawn the GLB model
            yield return StartCoroutine(LoadAndSpawnGLB(localPath, ProductSelection.productData, category));
        }
    }

    private IEnumerator LoadAndSpawnGLB(string filePath, CategoryManager.Products productData, CategoryType category)
    {
        using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            var importOptions = new ImportOptions();
            var importer = new GLTFSceneImporter(stream, importOptions);

            yield return importer.LoadSceneAsync();
            GameObject loadedGLB = importer.LastLoadedScene;

            if (loadedGLB == null)
            {
                Debug.LogError($"Failed to load GLB model: {productData.name}");
                yield break;
            }

            // Set up the loaded model
            loadedGLB.name = productData.name;

            // Get the actual mesh (often a child of the root)
            Transform modelTransform = loadedGLB.transform;
            if (loadedGLB.transform.childCount > 0)
            {
                modelTransform = loadedGLB.transform.GetChild(0);
            }

            if(category == CategoryType.Watches)
            {
                watchSpawnParent.localPosition = initialWatchPos;
                watchSpawnParent.localRotation = initialWatchRot;
                watchSpawnParent.localScale = initialWatchScale;
            }
            else
            {
                ringSpawnParent.localPosition = initialRingPos;
                ringSpawnParent.localRotation = initialRingRot;
                ringSpawnParent.localScale = initialRingScale;
            }

            // Set parent based on category
            Transform spawnParent = category == CategoryType.Watches ? watchSpawnParent : ringSpawnParent;

            // Reset transform
            loadedGLB.transform.localRotation = Quaternion.identity;
            loadedGLB.transform.localScale = Vector3.one;
            loadedGLB.transform.localEulerAngles = new Vector3(0,180,0);

            if(category == CategoryType.Rings)
                spawnParent.localEulerAngles = new Vector3(90,0,0);

            // watches[0].transform.localEulerAngles = new Vector3(0, 0, 0);

            foreach (Transform obj in spawnParent)
                obj.gameObject.SetActive(false);    
            loadedGLB.transform.SetParent(spawnParent);

            loadedGLB.transform.localPosition = Vector3.zero;
            // yield return new WaitForSeconds(1f);
            // watches[0].transform.localEulerAngles = new Vector3(90, 0, 0);
            // Debug.Log("Watch : " + watches[0].transform.localEulerAngles);

            if (category == CategoryType.Watches)
            {
                //GetComponent<HandTrackingVisualizer>().currentWristAnchor = loadedGLB;
                GetComponent<HandTrackingVisualizer>().watchWidth = loadedGLB.GetComponentInChildren<MeshRenderer>().bounds.size.x;
            }

            if(category == CategoryType.Rings)
                spawnParent.localEulerAngles = new Vector3(0,0,0);
                
            //else if (category == CategoryType.Rings)
            //    GetComponent<RingPlacer>().currentRing = loadedGLB;

            Debug.Log("cateogry : " + category);

            // Add to downloaded models cache
            downloadedModels[ProductSelection.productData.name] = loadedGLB;

            // Initially disable the model
            loadedGLB.SetActive(false);

            Debug.Log($"✅ Model loaded and cached: {ProductSelection.productData.name}");

            // Automatically select the downloaded model
            SelectItem(ProductSelection.productData.name, category.ToString());
        }
    }

    // Cleanup method to remove downloaded models
    public void ClearDownloadedModels()
    {
        foreach (var model in downloadedModels.Values)
        {
            if (model != null)
                Destroy(model);
        }
        downloadedModels.Clear();

        // Reset active references
        activeWatch = null;
        activeRing = null;
    }

    // Method to pre-download a model (optional)
    public void PreDownloadModel(CategoryType category)
    {
        if (!downloadedModels.ContainsKey(ProductSelection.productData.name))
        {
            StartCoroutine(DownloadAndSpawnModel(category));
        }
    }
}