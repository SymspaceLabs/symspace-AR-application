using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using static ObjectSpawner;

public class GhostPlacementController : MonoBehaviour
{
    public static GhostPlacementController Instance;

    [Header("References")]
    [SerializeField] private ARRaycastManager raycastManager;

    [Header("UI")]
    [SerializeField] private GameObject tapToPlaceHint; // "Tap to place" text overlay

    private GameObject ghostInstance;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool isPlaced = false;
    private bool isVerticalProduct = false;

    [Tooltip("Real-world dimensions")]
    public float widthInches = 12f;
    public float heightInches = 6f;
    public float depthInches = 3f;
    public string unit = "cm"; // Units used for scaling

    public SizeInInches[] objectsSize; // Array storing sizes for each object prefab

    public List<Color> objectColors; // Colors corresponding to objects

    public System.Action<Vector3, Quaternion> OnPlacementConfirmed;

    [SerializeField] ARPlaneManager planeManager;

    public Material transparentMat;
    public GameObject objectToSpawn;

    public List<GameObject> spawnedObjects;

    public int spawnObjectCount = 0;
    private Material tempMaterial;

    /// <summary>
    /// Call this to start showing the ghost preview for a product.
    /// </summary>
    /// <param name="prefab">The product's 3D model prefab/loaded GLB</param>
    /// <param name="vertical">True for wall-mounted products (TVs), false for floor/table (furniture)</param>
    /// 

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        //Invoke(nameof(BeginPlacement), 3f);
    }
    [ContextMenu("Begin Placement")]
    public void BeginPlacement(/*GameObject prefab = null, */bool vertical = false)
    {
        CancelPlacement();
        //if(prefab != null )
        //    productPrefab = prefab;
        isVerticalProduct = vertical;
        isPlaced = false;

        ghostInstance = Instantiate(objectToSpawn);
        HologramPreview hp = ghostInstance.AddComponent<HologramPreview>();
        hp.transparentMat = transparentMat;
        ghostInstance.SetActive(false);
        if(CategoryManager.Instance.isDebugMode)Debug.Log("Ghost item available", ghostInstance);
        if (tapToPlaceHint != null)
            tapToPlaceHint.SetActive(true);
    }

    void Update()
    {
        if (isPlaced || ghostInstance == null) return;

        // Raycast from screen center
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            ARPlane validPlane = null;
            Pose validPose = default;

            foreach (var hit in hits)
            {
                ARPlane plane = planeManager.GetPlane(hit.trackableId);
                if (plane == null) continue;

                bool isVerticalPlane = plane.alignment == PlaneAlignment.Vertical;
                bool isHorizontalPlane =
                    plane.alignment == PlaneAlignment.HorizontalUp ||
                    plane.alignment == PlaneAlignment.HorizontalDown;

                // ✅ STRICT FILTERING
                if (isVerticalProduct && !isVerticalPlane)
                    continue;

                if (!isVerticalProduct && !isHorizontalPlane)
                    continue;

                // first valid plane wins
                validPlane = plane;
                validPose = hit.pose;
                break;
            }

            if (validPlane == null)
            {
                ghostInstance.SetActive(false);
                return;
            }

            ghostInstance.SetActive(true);

            Vector3 spawnPoint = validPose.position;
            Vector3 spawnNormal = validPlane.normal;
            //if(CategoryManager.Instance.isDebugMode)Debug.Log("Spawn rotation: " + spawnPoint);
            Vector3 projectedForward;

            Quaternion rotation;

            if (!isVerticalProduct)
            {
                // HORIZONTAL — face camera
                Vector3 facePosition = Camera.main.transform.position;
                Vector3 forward = facePosition - spawnPoint;

                BurstMathUtility.ProjectOnPlane(forward, spawnNormal, out projectedForward);

                rotation = Quaternion.LookRotation(projectedForward, spawnNormal);
            }
            else
            {
                // VERTICAL — wall alignment
                BurstMathUtility.ProjectOnPlane(spawnPoint, spawnNormal, out projectedForward);

                rotation = Quaternion.LookRotation(projectedForward, spawnNormal);

                Vector3 forward = projectedForward.normalized;

                // 🔑 Stabilize "up" so it doesn't tilt
                Vector3 up = spawnNormal;

                // Remove any accidental roll by re-orthogonalizing
                Vector3 right = Vector3.Cross(up, forward).normalized;
                up = Vector3.Cross(forward, right).normalized;

                rotation = Quaternion.LookRotation(forward, up);
                if (Vector3.Dot(rotation * Vector3.up, Vector3.up) < 0f)
                {
                    rotation = Quaternion.AngleAxis(180f, spawnNormal) * rotation;
                }

                Vector3 rightAngle = rotation * Vector3.right;

                float dot = Vector3.Dot(rightAngle, Vector3.down);
                if (dot < 0)
                {
                    rotation = rotation * Quaternion.Euler(0f, 180f, 0f);
                }

                Vector3 euler = rotation.eulerAngles;
                euler.x = 0f;

                rotation = Quaternion.Euler(euler);
            }

            ghostInstance.transform.SetPositionAndRotation(spawnPoint, rotation);
        }
        else
        {
            ghostInstance.SetActive(false);
        }

        //Tap to confirm placement
        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButton(0))
        {
            if (ghostInstance.activeSelf)
            {
                isPlaced = true;
                Vector3 pos = ghostInstance.transform.position;
                Quaternion rot = ghostInstance.transform.rotation;

                Destroy(ghostInstance);
                ghostInstance = null;

                if (tapToPlaceHint != null)
                    tapToPlaceHint.SetActive(false);

                //GameObject obj = Instantiate(objectToSpawn, pos, rot);
                objectToSpawn.transform.SetPositionAndRotation(pos, rot);
                objectToSpawn.SetActive(true);
                spawnedObjects.Add(objectToSpawn);
                CategoryManager.Instance.tempModels.Remove(objectToSpawn);
                objectToSpawn = null;
                OnPlacementConfirmed?.Invoke(pos, rot);

                //Invoke(nameof(BeginPlacement), 5f);
            }
        }
    }

    public void CancelPlacement()
    {
        if (ghostInstance != null)
            Destroy(ghostInstance);
        ghostInstance = null;
        isPlaced = false;
        if (tapToPlaceHint != null)
            tapToPlaceHint.SetActive(false);
    }

    public void ChangeTextureByIndex(int index)
    {
        //objectIndex = index; // commented out to preserve original logic

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


        if (spawnedObjects.Count > 0)
        {
            GameObject obj = spawnedObjects[UIManagerAR.instance.objectSelectedIndex];
            /*Material*/
            tempMaterial = obj.GetComponentInChildren<MeshRenderer>().material;

            var modelView = UIManagerAR.instance.UI_3D_Models.Find(m =>
            m.GetComponent<ProductDetails>().product.id == obj.GetComponent<ProductDetails>().product.id);

            if(CategoryManager.Instance.isDebugMode)Debug.Log($"obj count {obj.GetComponent<ProductDetails>().textures.Count}, index {index}");
            if (obj.GetComponent<ProductDetails>().textures.Count > index)
                tempMaterial.mainTexture = obj.GetComponent<ProductDetails>().textures[index];


            obj.GetComponent<ProductDetails>().selectedColorIndex = index;
            if (UIManagerAR.instance.objectSelectedIndex >= 0 && UIManagerAR.instance.objectSelectedIndex < spawnedObjects.Count)
            {
                obj.GetComponentInChildren<MeshRenderer>().material = tempMaterial;
                obj.GetComponentInChildren<MeshRenderer>().UpdateGIMaterials();
            }


            modelView.GetComponentInChildren<MeshRenderer>().material = tempMaterial;
            Canvas.ForceUpdateCanvases();

            UIManagerAR.instance.UpdateDetailData();

            //return true;
        }


        if(CategoryManager.Instance.isDebugMode)Debug.Log("index : " + index);

        Canvas.ForceUpdateCanvases();
        //return false;
    }

    public void DeleteAllSpawnedObjects()
    {
        foreach (var obj in spawnedObjects)
            if (obj != null)
                Destroy(obj);

        spawnedObjects.Clear();

        spawnObjectCount = 0;

        objectToSpawn = null;
        ghostInstance = null;
        UIManagerAR.instance.OnResetClick?.Invoke();
    }

    public void DeleteSpawnedObject(GameObject obj)
    {
        if (obj != null)
        {
            var pd = obj.GetComponent<ProductDetails>();
            if(pd != null && pd.plusCanvas != null)
                Destroy(pd.plusCanvas);

            spawnedObjects.Remove(obj);
            Destroy(obj);
            if(spawnObjectCount > 0)
                spawnObjectCount--;
        }
    }
}