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


    public List<GameObject> downloadedModels;

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
            Debug.Log("no subsytem found");

        foreach (var subsystem in subsystems)
        {
            if (subsystem != null && subsystem.running)
            {
                Debug.Log("Stopping XROcclusionSubsystem in this scene");
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
        //Debug.Log("Face Detected");
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

                Debug.Log($"item.instance parent : {item.instance.transform.parent.name} parent Pos : {item.instance.transform.parent.localPosition}, item Pos: {item.instance.transform.localPosition}");

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

    GameObject FindExistingModel(string productId, CategoryType cat)
    {
        GameObject downloaded;
        if (cat == CategoryType.LeftEarring)
            downloaded = downloadedModels.Find(m =>
            m.GetComponent<ProductDetails>().product.id == productId && m.GetComponent<ProductDetails>().category == CategoryType.LeftEarring);
        else if (cat == CategoryType.RightEarring)
            downloaded = downloadedModels.Find(m =>
            m.GetComponent<ProductDetails>().product.id == productId && m.GetComponent<ProductDetails>().category == CategoryType.RightEarring);
        else
            downloaded = downloadedModels.Find(m =>
                m.GetComponent<ProductDetails>().product.id == productId);

        if (downloaded != null)
            return downloaded;

        return null;
    }

    public IEnumerator JewelrySelected(Products p, string url, string categoryName, ProductItemData pid)
    {
        CategoryType category;
        ProductSelection.TryParseObjectType(categoryName, out category);
        Debug.Log("name: " + categoryName + ", result: " + category);
        GameObject newObject = null;
        GameObject downloadedModel = null;
        GameObject leftEarringInstance = null;
        GameObject rightEarringInstance = null;
        DownloadState stateCheck = null;

        if (category == CategoryType.Earrings)
        {
            leftEarringInstance = FindExistingModel(p.id, CategoryType.LeftEarring);
            rightEarringInstance = FindExistingModel(p.id, CategoryType.RightEarring);
        }
        else
        {
            downloadedModel = FindExistingModel(p.id, category);
        }

        if (leftEarringInstance == null)
        {
            if (downloadedModel != null)
                newObject = downloadedModel;
            else
            {
                if (category == CategoryType.Necklaces)
                    newObject = Instantiate(necklaceHolder);
                else
                    newObject = Instantiate(jewelryHolder);

                var state = newObject.GetComponent<DownloadState>() ?? newObject.AddComponent<DownloadState>();
                state.isDownloading = false;
                state.isReady = false;

                newObject.GetComponent<ProductDetails>().product.id = p.id;
            }

            stateCheck = newObject.GetComponent<DownloadState>();

            if (!stateCheck.isReady && !stateCheck.isDownloading)
            {
                if (pid != null)
                {
                    Debug.Log("PID is not empty");
                    yield return StartCoroutine(DownloadAndAssign(url, newObject, p, pid.ProductProgress, pid.DownloadFailed));

                }
                else
                    yield return StartCoroutine(DownloadAndAssign(url, newObject, p));
            }
        }
        else
            stateCheck = leftEarringInstance.GetComponent<DownloadState>();

        var newJewelry = new Jewelries
        {
            category = category,
            instance = newObject
        };

        Jewelries newLeftJewelry = null;
        Jewelries newRightJewelry = null;

        if (leftEarringInstance != null)
            newLeftJewelry = new Jewelries
            {
                category = CategoryType.LeftEarring,
                instance = leftEarringInstance
            };

        if (rightEarringInstance != null)
            newRightJewelry = new Jewelries
            {
                category = CategoryType.RightEarring,
                instance = rightEarringInstance
            };

        if (downloadedModel != null)
        {
            if (!downloadedModel.activeInHierarchy)
            {
                for (int i = 0; i < jewelries.Count; i++)
                {
                    if (jewelries[i].category != newJewelry.category)
                        continue;

                    // Remove old instance
                    if (jewelries[i].instance != null)
                    {
                        //Destroy(jewelries[i].instance);
                        jewelries[i].instance.gameObject.SetActive(false);
                    }
                        jewelries[i].instance = newJewelry.instance;
                        jewelries[i].instance.gameObject.SetActive(true);
                }
            }
        }

        if (leftEarringInstance != null)
        {
            if (!leftEarringInstance.activeInHierarchy)
            {
                for (int i = 0; i < jewelries.Count; i++)
                {
                    if (jewelries[i].category == CategoryType.LeftEarring)
                    {
                        if (jewelries[i].instance != null)
                            jewelries[i].instance.gameObject.SetActive(false);

                        jewelries[i].instance = newLeftJewelry.instance;
                        jewelries[i].instance.gameObject.SetActive(true);
                    }
                    else
                    {
                        if (jewelries[i].instance != null)
                            jewelries[i].instance.gameObject.SetActive(false);

                        jewelries[i].instance = newRightJewelry.instance;
                        jewelries[i].instance.gameObject.SetActive(true);
                    }
                }
            }
        }


        if (stateCheck.isReady)
            CategoryManager.Instance.GetComponent<SlideUpPanel>().HidePanel();
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

    IEnumerator DownloadAndAssign(string url, GameObject targetPrefab, Products p, Action<float> onProgress = null, Action onFailed = null)
    {
        if (targetPrefab == null)
        {
            Debug.LogError("Target prefab is null!");
            yield break;
        }

        yield return null;

        var state = targetPrefab.GetComponent<DownloadState>();
        state.isDownloading = true;
        state.isReady = false;
        GameObject loadedGLB = null;
        yield return StartCoroutine(ModelLoaderService.DownloadAndLoad(url, (model) => { loadedGLB = model; }, onProgress, onFailed));

        if (loadedGLB == null)
            yield break;

            Transform actualObject = loadedGLB.transform.childCount > 0
                        ? loadedGLB.transform.GetChild(0)
                        : loadedGLB.transform;
            actualObject.localScale = Vector3.one;

            loadedGLB.transform.parent = targetPrefab.transform;
            loadedGLB.transform.localPosition = Vector3.zero;
            loadedGLB.transform.localRotation = Quaternion.identity;


            targetPrefab.name = GetUniqueName(p.name, downloadedModels);

            CategoryType cat;
            ProductSelection.TryParseObjectType(p.category.name, out cat);

            var newJewelry = new Jewelries
            {
                category = cat,
                instance = targetPrefab
            };

            Debug.Log("Name : " + p.name + ", tag : " + p.category.name);
            Debug.Log("category : " + cat);
            //if(Equip(newJewelry))
            GameObject newRightEaring = new GameObject();


            if (newJewelry.category == CategoryType.Earrings)
            {
                newRightEaring = Instantiate(newJewelry.instance);
                Quaternion tempRotation = newRightEaring.transform.GetChild(0).transform.rotation;
                tempRotation.y = 180f;
                newRightEaring.transform.GetChild(0).transform.rotation = tempRotation;
            }


            //Vector3 camPos = Camera.main.transform.position;
            //camPos.y = newJewelry.instance.transform.position.y;
            //newJewelry.instance.transform.LookAt(camPos);

            for (int i = 0; i < jewelries.Count; i++)
            {
                switch (newJewelry.category)
                {
                    case CategoryType.Earrings:
                        {
                            // Earrings map to TWO slots
                            if (jewelries[i].category != CategoryType.LeftEarring &&
                                jewelries[i].category != CategoryType.RightEarring)
                                break;

                            // Remove old instance
                            if (jewelries[i].instance != null)
                            {
                                //Destroy(jewelries[i].instance);
                                jewelries[i].instance.gameObject.SetActive(false);
                                jewelries[i].instance = null;
                            }

                            // Assign new instance
                            if (jewelries[i].category == CategoryType.LeftEarring)
                            {
                                newJewelry.instance.GetComponent<ProductDetails>().category = CategoryType.LeftEarring;
                                jewelries[i].instance = newJewelry.instance;
                            }
                            else // RightEarring
                            {
                                newRightEaring.GetComponent<ProductDetails>().category = CategoryType.RightEarring;
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
                    if (jewelry.category == CategoryType.LeftEarring || jewelry.category == CategoryType.RightEarring)
                        jewelry.instance.transform.localScale *= 2.5f;
                    else if (jewelry.category == CategoryType.Glasses)
                        jewelry.instance.transform.localScale *= 1.2f;

                    // ✅ NEW
                    else if (jewelry.category == CategoryType.Cap)
                        jewelry.instance.transform.localScale *= 0.7f;

                    else
                        jewelry.instance.transform.localScale *= 1f;
                }
            }

            state.isDownloading = false;
            state.isReady = true;

            if(newJewelry.category == CategoryType.Earrings)
            {
                if (!downloadedModels.Contains(newJewelry.instance))
                    downloadedModels.Add(newJewelry.instance);
                if (!downloadedModels.Contains(newRightEaring))
                    downloadedModels.Add(newRightEaring);
            }
            else if (!downloadedModels.Contains(targetPrefab))
                downloadedModels.Add(targetPrefab);

            Debug.Log("✅ GLB model downloaded, instantiated, and scaled successfully.");
        //}
    }

    void HandleSingleItem(int i, Jewelries newJewelry)
    {
        if (jewelries[i].category != newJewelry.category)
            return;

        // Remove old
        if (jewelries[i].instance != null)
        {
            jewelries[i].instance.gameObject.SetActive(false);
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
        else */if (newJewelry.category == CategoryType.Glasses)
        {
            Vector3 rot = jewelries[i].instance.transform.GetChild(0).localEulerAngles;
            rot.y = 180;
            jewelries[i].instance.transform.GetChild(0).localEulerAngles = rot;
            //Debug.Log("Glasses Rotated");
        }
        else if(newJewelry.category == CategoryType.LeftEarring || newJewelry.category == CategoryType.RightEarring)
        {
            Vector3 rot = jewelries[i].instance.transform.GetChild(0).localEulerAngles;
            rot.y = 180;
            jewelries[i].instance.transform.GetChild(0).localEulerAngles = rot;
        }
            //Debug.Log("New Jewelry : " + newJewelry.category);
    }

    public bool Equip(Jewelries newItem)
    {
        // Look for another item of same category that is equipped
        foreach (var j in jewelries)
        {
            if (j.category == newItem.category && j != newItem)
            {
                // remove old instance
                if (j.instance != null)
                    Destroy(j.instance);

                j.instance = null;
            }
        }

        // If item is already equipped, do nothing
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
                //Destroy(jewelry.instance.gameObject);
                jewelry.instance.gameObject.SetActive(false);
                jewelry.instance = null;
            }
        }
    }

#if UNITY_ANDROID
#elif UNITY_IOS
#endif
    private Vector3 GetLandmarkWorldPosition(int vertexIndex, Vector3 offset)
    {
        var vertices = GetFaceVertices();
        Debug.Log("vertex index " + vertexIndex);
        if (vertices != null && vertexIndex < vertices.Length)
        {
            return vertices[vertexIndex] + offset;
        }
        if(vertices != null)
            Debug.Log("vertices " + vertices.Length);

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
        Debug.Log("Mesh Filter is null");
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
        Debug.Log("Mesh true size: " + trueSize);

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
