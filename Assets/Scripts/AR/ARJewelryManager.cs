using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.UI;
using TMPro;
public enum CategoryType
{
    Necklace,
    Earing,
    LeftEarring,
    RightEarring,
    NosePin,
    Cap,
    Glasses,
    HeadPin
}

public class ARJewelryManager : MonoBehaviour
{
    
    [Header("References")]
    public ARFaceManager faceManager;

    public List<JewelryItem> jewelryItems = new List<JewelryItem>();
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

    private void Start()
    {
        if(ProductSelection.ProductName != null)
        {
            foreach(var jewelry in  jewelryItems)
            {
                if (jewelry.category == ProductSelection.SelectedObjectType && jewelry.prefab.name == ProductSelection.ProductName)
                {
                    jewelry.isSpawn = true;
                }
                else if (ProductSelection.SelectedObjectType == CategoryType.Earing && (jewelry.category == CategoryType.LeftEarring || jewelry.category == CategoryType.RightEarring))
                {
                    jewelry.isSpawn = true;
                }
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
        foreach (var face in args.added)
        {
            //if (currentFace == null)
            //{
                currentFace = face;
                Invoke(nameof(InitializeJewelryItems), 1f);
            //}
        }

        foreach (var face in args.updated)
        {
            //if(currentFace == null)
            //{
            currentFace = face;
            UpdateItems();
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
        foreach (var item in jewelryItems)
        {
            if (item.prefab != null && item.isSpawn && item.instance == null)
            {
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
                    case CategoryType.Necklace:
                        localPosition = GetLandmarkWorldPosition(ARKitFaceRegion.ChinIndex, item.localOffset);
                        break;
                    case CategoryType.NosePin:
                        localPosition = GetLandmarkWorldPosition(ARKitFaceRegion.NoseTip, item.localOffset);
                        break;
                }
                // Instantiate as child of currentFace.transform
                item.instance = Instantiate(item.prefab, currentFace.transform);
                item.instance.transform.localPosition = localPosition;
                item.instance.transform.localRotation = Quaternion.identity; // optional
            }
        }
    }
    private void RemoveJewelryItems()
    {
        foreach (var item in jewelryItems)
        {
            if (item.instance != null)
            {
                Destroy(item.instance);
                item.instance = null;
            }
        }
    }

    public void JewelrySelected(string name, string categoryName)
    {
        CategoryType category;
        ProductSelection.TryParseObjectType(categoryName, out category);
        foreach (var item in jewelryItems)
        {
            Debug.Log("item.Category: " + item.category + ", Category: " + category);

            if((category == CategoryType.Earing) && (item.category == CategoryType.LeftEarring || item.category == CategoryType.RightEarring))
            {
                Debug.Log("item.category == category");
                if (item.instance != null)
                {
                    item.isSpawn = false;
                    Debug.Log("Item Destroyed : " + item.prefab.name);
                    Destroy(item.instance);
                    item.instance = null;
                }
            }
            else if(category == item.category)
            {
                Debug.Log("item.category == category");
                if(item.instance != null)
                {
                    item.isSpawn = false;
                    Debug.Log("Item Destroyed : " + item.instance.name);
                    Destroy(item.instance);
                    item.instance = null;
                }
            }
        }

        int index = 0;
        foreach (var item in jewelryItems)
        {
            if (category == CategoryType.Earing)
            {
                if(item.prefab.name == name)
                    if (item.category == CategoryType.LeftEarring || item.category == CategoryType.RightEarring)
                    {
                        //currentItemSelected = index;
                        item.isSpawn = true;
                        Debug.Log("jewelry : " + item.prefab.name + ", " + item.isSpawn);
                    }
            }
            else if (item.prefab.name == name)
            {
                currentItemSelected = index;
                item.isSpawn = true;
                Debug.Log("jewelry Name: " + item.prefab.name + ", " + item.isSpawn);
            }
        }
    }

    private void UpdateItems()
    {
        if (currentFace == null) return;

        foreach (var item in jewelryItems)
        {
            if (item.instance != null && item.isSpawn)
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
            else if(item.instance == null && item.isSpawn)
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
        xText.text = "X : " + xSlider.value.ToString();
        yText.text = "Y : " + ySlider.value.ToString();
        zText.text = "Z : " + zSlider.value.ToString();

        if(currentItemSelected < jewelryItems.Count && currentItemSelected >= 0)
        {
            //jewelryItems[currentItemSelected].localOffset.x = xSlider.value;
            //jewelryItems[currentItemSelected].localOffset.y = ySlider.value;
            //jewelryItems[currentItemSelected].localOffset.z = zSlider.value;
        }
    }

    void Update()
    {
        //UpdateItems();
        /*if (currentFace == null) return;
        foreach (var item in jewelryItems)
        {
            if (item.instance == null) continue;
            Vector3 targetPosition = GetWorldPosition(item.category, item.localOffset);
            item.instance.transform.position = Vector3.SmoothDamp(
                item.instance.transform.position,
                targetPosition,
                ref item.velocity,
                item.smoothTime
            );
            // Apply rotation conditionally
            if (item.category == CategoryName.Necklace)
            {
                // Keep the necklace upright in world space (or align to neck if needed)
                item.instance.transform.rotation = Quaternion.identity;
            }
            else if (item.category == CategoryName.Glasses)
            {
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
            }
            else
            {
                // Rotate with face
                item.instance.transform.rotation = currentFace.transform.rotation;
            }
        }*/
    }
    private Vector3 GetWorldPosition(CategoryType category, Vector3 localOffset)
    {
        switch (category)
        {
            case CategoryType.Necklace:
#if UNITY_IOS
                return GetLandmarkWorldPosition(ARKitFaceRegion.ChinIndex, localOffset);
#else
                return currentFace.transform.TransformPoint(localOffset);
#endif
            case CategoryType.LeftEarring:
                return GetLandmarkWorldPosition(ARKitFaceRegion.LeftEar, localOffset);
            case CategoryType.RightEarring:
                return GetLandmarkWorldPosition(ARKitFaceRegion.RightEar, localOffset);
            case CategoryType.NosePin:
                return GetLandmarkWorldPosition(ARKitFaceRegion.NoseTip, localOffset);
            //case CategoryName.Cap:
            //    return GetLandmarkWorldPosition(ARKitFaceRegion.ForeheadCenter) + localOffset;
            //case CategoryName.HeadPin:
            //    return GetLandmarkWorldPosition(ARKitFaceRegion.HeadTop) + localOffset;
            case CategoryType.Glasses:
                Vector3 leftEye = currentFace.leftEye.position;
                Vector3 rightEye = currentFace.rightEye.position;
                Vector3 eyeCenter = (leftEye + rightEye) / 2f;
                return eyeCenter + localOffset;
            default:
                return currentFace.transform.TransformPoint(localOffset);
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
        public const int LeftEar = 234;         // Sample index
        public const int RightEar = 454;        // Sample index
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
