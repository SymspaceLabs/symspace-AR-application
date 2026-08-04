using GLTF.Schema;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.EventSystems;

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

    //public SizeInInches[] objectsSize; // Array storing sizes for each object prefab

    public List<Color> objectColors; // Colors corresponding to objects

    public System.Action<Vector3, Quaternion> OnPlacementConfirmed;

    [SerializeField] ARPlaneManager planeManager;

    public Material transparentMat;
    public GameObject objectToSpawn;
    public Vector3 offset;

    public List<GameObject> spawnedObjects;

    public int spawnObjectCount = 0;
    private Material tempMaterial;

    public GameObject selectedAreaBottom;

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
        //HologramPreview hp = ghostInstance.AddComponent<HologramPreview>();
        MeshRenderer renderer = ghostInstance.GetComponentInChildren<MeshRenderer>();

        Material mat = new Material(transparentMat);

        GltfMaterialCopier.CopyAllTextures(renderer.material, mat);

        // assign
        renderer.material = mat;

        Outline outliner = ghostInstance.AddComponent<Outline>();
        outliner.OutlineMode = Outline.Mode.OutlineAll;
        outliner.OutlineColor = Color.skyBlue;
        outliner.OutlineWidth = 3f;

        UIManagerAR.instance.SelectModel(ghostInstance.GetComponent<ProductDetails>());
        //UIManagerAR.instance.smallDetail.SetActive(true);

        //hp.transparentMat = mat;
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
                spawnPoint = spawnPoint + offset;
            }

            ghostInstance.transform.SetPositionAndRotation(spawnPoint, rotation);
        }
        else
        {
            ghostInstance.SetActive(false);
        }

        //Tap to confirm placement (ignore UI clicks)
        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButton(0))
        {
            if (EventSystem.current != null)
            {
                bool isUI = false;
                if (Input.touchCount > 0)
                    isUI = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
                else
                    isUI = EventSystem.current.IsPointerOverGameObject();
                if (isUI) return;
            }

            if (ghostInstance.activeSelf)
            {
                isPlaced = true;
                Vector3 pos = ghostInstance.transform.position;
                Quaternion rot = ghostInstance.transform.rotation;

                ProductDetails ghostPd = UIManagerAR.instance.selectedModelDetails;
                int colorIndex = ghostPd != null ? ghostPd.selectedColorIndex : 0;

                Destroy(ghostInstance);
                ghostInstance = null;

                if (tapToPlaceHint != null)
                    tapToPlaceHint.SetActive(false);

                objectToSpawn.transform.SetPositionAndRotation(pos, rot);
                objectToSpawn.SetActive(true);
                spawnedObjects.Add(objectToSpawn);
                CategoryManager.Instance.tempModels.Remove(objectToSpawn);

                ProductDetails placedPd = objectToSpawn.GetComponent<ProductDetails>();
                if (placedPd != null)
                {
                    placedPd.selectedColorIndex = colorIndex;
                    UIManagerAR.instance.selectedModelDetails = placedPd;
                    if (placedPd.textures.Count > colorIndex)
                    {
                        MeshRenderer placedRenderer = objectToSpawn.GetComponentInChildren<MeshRenderer>();
                        if (placedRenderer != null)
                        {
                            Material mat = placedRenderer.material;
                            mat.mainTexture = placedPd.textures[colorIndex];
                            placedRenderer.material = mat;
                            placedRenderer.UpdateGIMaterials();
                        }
                    }
                }

                objectToSpawn = null;
                OnPlacementConfirmed?.Invoke(pos, rot);
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
        UIManagerAR.instance.smallDetail.SetActive(false);
    }

    public void ChangeTextureByIndex(int index)
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
        if (targetPd != null)
        {
            targetPd.selectedColorIndex = index;
            bool isGhost = ghostInstance != null && targetPd.gameObject == ghostInstance;

            if (targetPd.textures.Count > index)
            {
                Texture2D selectedTex = targetPd.textures[index];

                if (!isGhost)
                {
                    MeshRenderer targetRenderer = targetPd.GetComponentInChildren<MeshRenderer>();
                    if (targetRenderer != null)
                    {
                        tempMaterial = targetRenderer.material;
                        tempMaterial.mainTexture = selectedTex;
                        targetRenderer.material = tempMaterial;
                        targetRenderer.UpdateGIMaterials();
                    }

                    var modelView = UIManagerAR.instance.UI_3D_Models.Find(m =>
                        m.GetComponent<ProductDetails>().product.id == targetPd.product.id);
                    if (modelView != null)
                    {
                        MeshRenderer mvRenderer = modelView.GetComponentInChildren<MeshRenderer>();
                        if (mvRenderer != null)
                            mvRenderer.material = tempMaterial;
                    }

                    foreach (var spawned in spawnedObjects)
                    {
                        if (spawned != null && spawned != targetPd.gameObject)
                        {
                            var spawnedPd = spawned.GetComponent<ProductDetails>();
                            if (spawnedPd != null && spawnedPd.product.id == targetPd.product.id)
                            {
                                spawnedPd.selectedColorIndex = index;
                                MeshRenderer spawnedRenderer = spawned.GetComponentInChildren<MeshRenderer>();
                                if (spawnedRenderer != null)
                                {
                                    Material mat = spawnedRenderer.material;
                                    mat.mainTexture = selectedTex;
                                    spawnedRenderer.material = mat;
                                    spawnedRenderer.UpdateGIMaterials();
                                }
                            }
                        }
                    }
                }

                if (ghostInstance != null && ghostInstance.activeSelf)
                {
                    MeshRenderer ghostRenderer = ghostInstance.GetComponentInChildren<MeshRenderer>();
                    if (ghostRenderer != null)
                        ghostRenderer.material.mainTexture = selectedTex;
                }
            }

            UIManagerAR.instance.UpdateDetailData(targetPd);
        }

        if (CategoryManager.Instance.isDebugMode) Debug.Log("index : " + index);
        Canvas.ForceUpdateCanvases();
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