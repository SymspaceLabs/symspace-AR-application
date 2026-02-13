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
    Glasses,
    HeadPin,
    Watches,
    Rings
}

public class ARJewelryManager : MonoBehaviour
{
    [Header("References")]
    public ARFaceManager faceManager;

    public List<JewelryItem> jewelryItems = new List<JewelryItem>();
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

    private void Start()
    {
        DisableOcclusion();

        //if(ProductSelection.ProductName != null)
        //{
        //    foreach(var jewelry in  jewelryItems)
        //    {
        //        if (jewelry.category == ProductSelection.SelectedObjectType && jewelry.prefab.name == ProductSelection.ProductName)
        //        {
        //            jewelry.isSpawn = true;
        //        }
        //        else if (ProductSelection.SelectedObjectType == CategoryType.Earring && (jewelry.category == CategoryType.LeftEarring || jewelry.category == CategoryType.RightEarring))
        //        {
        //            jewelry.isSpawn = true;
        //        }
        //    }
        //}

        //if(ProductSelection.ProductName != null)
        //{
        //    JewelrySelected(ProductSelection.ProductName, ProductSelection.modelURL, ProductSelection.ProductName.subcategoryItemChild.name);
        //}
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
        Debug.Log("Face Detected");
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
                    item.instance.transform.parent = currentFace.transform;
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
                            Vector3 leftEye = currentFace.leftEye.localPosition;
                            Vector3 rightEye = currentFace.rightEye.localPosition;
                            localPosition = (leftEye + rightEye) / 2f + item.localOffset;
                        }
                        break;
                    case CategoryType.LeftEarring:
                        localPosition = GetLandmarkWorldPosition(ARKitFaceRegion.LeftEar, item.localOffset);
                        break;
                    case CategoryType.RightEarring:
                        localPosition = GetLandmarkWorldPosition(ARKitFaceRegion.RightEar, item.localOffset);
                        break;
                    case CategoryType.Necklaces:
                        localPosition = GetLandmarkWorldPosition(ARKitFaceRegion.ChinIndex, item.localOffset);
                        break;
                    case CategoryType.NosePin:
                        localPosition = GetLandmarkWorldPosition(ARKitFaceRegion.NoseTip, item.localOffset);
                        break;
                }
                // Instantiate as child of currentFace.transform
                //Debug.Log("Local Position " + localPosition);
                if (localPosition == Vector3.zero)
                    continue;

                //item.instance = Instantiate(item.prefab, currentFace.transform);
                item.instance.transform.localPosition = localPosition;
                Debug.Log($"New Position for {item.category} is: {item.instance.transform.localPosition}");
                //item.instance.transform.localRotation = Quaternion.identity; // optional

                // --- Add dynamic scale normalization ---
                float targetSize = 0.05f; // Default target size in meters; adjust per jewelry type if needed
                switch (item.category)
                {
                    case CategoryType.Necklaces: targetSize = 0.1f; break;
                    case CategoryType.Rings: targetSize = 0.02f; break;
                    case CategoryType.LeftEarring:
                    case CategoryType.RightEarring: targetSize = 0.03f; break;
                    case CategoryType.NosePin: targetSize = 0.015f; break;
                    case CategoryType.Glasses: targetSize = 0.12f; break;
                        // Add more types if needed
                }
                Debug.Log("Face scale: " + currentFace.transform.localScale);
                if (item.allowScale)
                {
                    NormalizeJewelryScale(item.instance, targetSize);
                    item.allowScale = true;
                }
                Debug.Log("item Scale : " + item.instance.transform.localScale);
            }
            else
                Debug.Log("Item instance = null : " + item.category);
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

    public void JewelrySelected(Products p, string url, string categoryName)
    {
        CategoryType category;
        ProductSelection.TryParseObjectType(categoryName, out category);

        GameObject newObject;
        if (category == CategoryType.Necklaces)
            newObject = Instantiate(necklaceHolder);
        else
            newObject = Instantiate(jewelryHolder);

        StartCoroutine(DownloadAndAssign(url, newObject, p));
    }

    IEnumerator DownloadAndAssign(string url, GameObject targetPrefab, Products p)
    {
        if (targetPrefab == null)
        {
            Debug.LogError("Target prefab is null!");
            yield break;
        }

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            string localPath = Path.Combine(Application.persistentDataPath, Path.GetFileName(url));
            www.downloadHandler = new DownloadHandlerFile(localPath);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Download failed: " + www.error);
                yield break;
            }

            using (FileStream stream = new FileStream(localPath, FileMode.Open, FileAccess.Read))
            {
                var importOptions = new ImportOptions();
                var importer = new GLTFSceneImporter(stream, importOptions);

                yield return importer.LoadSceneAsync(); // Loads the GLB into a new GameObject
                GameObject loadedGLB = importer.LastLoadedScene;

                if (loadedGLB == null)
                {
                    Debug.LogError("Failed to load GLB model!");
                    yield break;
                }
                Transform actualObject = loadedGLB.transform.childCount > 0
                         ? loadedGLB.transform.GetChild(0)
                         : loadedGLB.transform;
                actualObject.localScale = Vector3.one;

                loadedGLB.transform.parent = targetPrefab.transform;
                loadedGLB.transform.localPosition = Vector3.zero;
                loadedGLB.transform.localRotation = Quaternion.identity;

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
                                    Destroy(jewelries[i].instance);
                                    jewelries[i].instance = null;
                                }

                                // Assign new instance
                                if (jewelries[i].category == CategoryType.LeftEarring)
                                    jewelries[i].instance = newJewelry.instance;
                                else // RightEarring
                                    jewelries[i].instance = newRightEaring;

                                currentItemSelected = i;
                                break;
                            }

                        default:
                            {
                                // All other jewelry = 1:1 category mapping
                                if (jewelries[i].category != newJewelry.category)
                                    break;

                                // Remove old instance
                                if (jewelries[i].instance != null)
                                {
                                    Destroy(jewelries[i].instance);
                                    jewelries[i].instance = null;
                                }

                                // Apply offset & assign
                                //Vector3 tempRotation = newJewelry.instance.transform.localEulerAngles;
                                //tempRotation.x = -20f;
                                //newJewelry.instance.transform.localEulerAngles = tempRotation;

                                jewelries[i].instance = newJewelry.instance;
                                currentItemSelected = i;

                                if (jewelries[i].category == CategoryType.Necklaces)
                                {
                                    Vector3 tempRot = jewelries[i].instance.transform.GetChild(1).transform.localEulerAngles;
                                    tempRot.x = -20f;
                                    tempRot.y = 180;
                                    jewelries[i].instance.transform.GetChild(1).transform.localEulerAngles = tempRot;
                                }
                                else if (jewelries[i].category == CategoryType.Glasses)
                                {
                                    Vector3 tempRot = jewelries[i].instance.transform.GetChild(0).transform.localEulerAngles;
                                    tempRot.y = 180;
                                    jewelries[i].instance.transform.GetChild(0).transform.localEulerAngles = tempRot;
                                }

                                    break;
                            }
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
                        else
                            jewelry.instance.transform.localScale *= 1.3f;
                    }
                }

                Debug.Log("✅ GLB model downloaded, instantiated, and scaled successfully.");
            }
        }
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



    private void UpdateItems()
    {
        if (currentFace == null) return;

        foreach (var item in jewelries)
        {
            if (item.instance != null)
            {
                //Vector3 localPosition = Vector3.zero;
                //switch (item.category)
                //{
                //    case CategoryType.Glasses:
                //        if (currentFace.leftEye != null && currentFace.rightEye != null)
                //        {
                //            Vector3 leftEye = currentFace.leftEye.localPosition;
                //            Vector3 rightEye = currentFace.rightEye.localPosition;
                //            localPosition = (leftEye + rightEye) / 2f + item.localOffset;
                //        }
                //        break;
                //    case CategoryType.LeftEarring:
                //        localPosition = GetLandmarkWorldPosition(ARKitFaceRegion.LeftEar, item.localOffset);
                //        break;
                //    case CategoryType.RightEarring:
                //        localPosition = GetLandmarkWorldPosition(ARKitFaceRegion.RightEar, item.localOffset);
                //        break;
                //    case CategoryType.Necklace:
                //        localPosition = GetLandmarkWorldPosition(ARKitFaceRegion.ChinIndex, item.localOffset);
                //        break;
                //    case CategoryType.NosePin:
                //        localPosition = GetLandmarkWorldPosition(ARKitFaceRegion.NoseTip, item.localOffset);
                //        break;
                //}

                //item.instance.transform.localPosition = localPosition;
                //item.instance.transform.localRotation = currentFace.transform.rotation;

                //if (item.category == CategoryType.Necklace)
                //{
                //    Vector3 worldPos = GetLandmarkWorldPosition(ARKitFaceRegion.ChinIndex, item.localOffset);
                //    item.instance.transform.position = worldPos;

                //    // Optional: keep it upright
                //    item.instance.transform.rotation = Quaternion.identity;
                //}
            }
            else
            {
                InitializeJewelryItems();
            }
        }

        /*foreach (var item in jewelryItems)
        {
            if (item.instance == null) continue;

            Vector3 targetPosition = GetWorldPosition(item.category, item.localOffset);
            item.instance.transform.position = targetPosition;
            //item.instance.transform.position = Vector3.SmoothDamp(
            //    item.instance.transform.position,
            //    targetPosition,
            //    ref item.velocity,
            //    item.smoothTime
            //);
            // Apply rotation conditionally
            if (item.category == CategoryName.Necklace)
            {
                // Keep the necklace upright in world space (or align to neck if needed)
                item.instance.transform.rotation = Quaternion.identity;
            }
            else if (item.category == CategoryName.Glasses)
            {
#if UNITY_ANDROID
                // Use inverse for left/right and up/down, but preserve the original tilt
                Quaternion inverseRotation = Quaternion.Inverse(currentFace.transform.rotation);
                Vector3 inverseEuler = inverseRotation.eulerAngles;
                Vector3 originalEuler = currentFace.transform.rotation.eulerAngles;
                // Keep the original Z rotation (tilt) from the face
                item.instance.transform.rotation = Quaternion.Euler(
                    inverseEuler.x,     // Mirrored pitch (up/down)
                    inverseEuler.y,     // Mirrored yaw (left/right)
                    originalEuler.z     // Original roll (tilt) - NOT mirrored
                );
#elif UNITY_IOS
                item.instance.transform.rotation = currentFace.transform.rotation;
#endif
            }
            else
            {
                // Rotate with face
                item.instance.transform.rotation = currentFace.transform.rotation;
            }
        }*/
    }

    public void UpdateText()
    {
        //xText.text = "X : " + xSlider.value.ToString();
        //yText.text = "Y : " + ySlider.value.ToString();
        //zText.text = "Z : " + zSlider.value.ToString();
        Debug.Log("current Selected Item : " + currentItemSelected);
        if(currentItemSelected < jewelryItems.Count && currentItemSelected >= 0)
        {
            jewelries[currentItemSelected].localOffset.x = xSlider.value;
            jewelries[currentItemSelected].localOffset.y = ySlider.value;
            jewelries[currentItemSelected].localOffset.z = zSlider.value;
        }
    }

//    private Vector3 GetWorldPosition(CategoryType category, Vector3 localOffset)
//    {
//        switch (category)
//        {
//            case CategoryType.Necklace:
//#if UNITY_IOS
//                return GetLandmarkWorldPosition(ARKitFaceRegion.ChinIndex, localOffset);
//#else
//                return currentFace.transform.TransformPoint(localOffset);
//#endif
//            case CategoryType.LeftEarring:
//                return GetLandmarkWorldPosition(ARKitFaceRegion.LeftEar, localOffset);
//            case CategoryType.RightEarring:
//                return GetLandmarkWorldPosition(ARKitFaceRegion.RightEar, localOffset);
//            case CategoryType.NosePin:
//                return GetLandmarkWorldPosition(ARKitFaceRegion.NoseTip, localOffset);
//            //case CategoryName.Cap:
//            //    return GetLandmarkWorldPosition(ARKitFaceRegion.ForeheadCenter) + localOffset;
//            //case CategoryName.HeadPin:
//            //    return GetLandmarkWorldPosition(ARKitFaceRegion.HeadTop) + localOffset;
//            case CategoryType.Glasses:
//                Vector3 leftEye = currentFace.leftEye.position;
//                Vector3 rightEye = currentFace.rightEye.position;
//                Vector3 eyeCenter = (leftEye + rightEye) / 2f;
//                return eyeCenter + localOffset;
//            default:
//                return currentFace.transform.TransformPoint(localOffset);
//        }
//    }

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
        public const int LeftEye = 130;         // Approx eye socket center
        public const int RightEye = 359;        // Approx eye socket center
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
    public CategoryType category;
    public GameObject instance;
    public Vector3 localOffset;
    public float smoothTime = 0.1f;
    [HideInInspector] public Vector3 velocity;

    public bool allowScale = false;
}
