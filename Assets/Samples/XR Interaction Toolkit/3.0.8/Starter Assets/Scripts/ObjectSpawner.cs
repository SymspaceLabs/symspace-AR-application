using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

//namespace UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets
//{
/// <summary>
/// Behavior with an API for spawning objects from a given set of prefabs.
/// </summary>
public class ObjectSpawner : MonoBehaviour
{
    #region Serialized Fields and Properties

    [SerializeField]
    [Tooltip("The camera that objects will face when spawned. If not set, defaults to the main camera.")]
    Camera m_CameraToFace;

    /// <summary>
    /// The camera that objects will face when spawned. If not set, defaults to the <see cref="Camera.main"/> camera.
    /// </summary>
    public Camera cameraToFace
    {
        get
        {
            EnsureFacingCamera();
            return m_CameraToFace;
        }
        set => m_CameraToFace = value;
    }

    [SerializeField]
    [Tooltip("The list of prefabs available to spawn.")]
    List<GameObject> m_ObjectPrefabs = new List<GameObject>();

    /// <summary>
    /// The list of prefabs available to spawn.
    /// </summary>
    public List<GameObject> objectPrefabs
    {
        get => m_ObjectPrefabs;
        set => m_ObjectPrefabs = value;
    }

    [SerializeField]
    [Tooltip("Optional prefab to spawn for each spawned object. Use a prefab with the Destroy Self component to make " +
        "sure the visualization only lives temporarily.")]
    GameObject m_SpawnVisualizationPrefab;

    /// <summary>
    /// Optional prefab to spawn for each spawned object.
    /// </summary>
    /// <remarks>Use a prefab with <see cref="DestroySelf"/> to make sure the visualization only lives temporarily.</remarks>
    public GameObject spawnVisualizationPrefab
    {
        get => m_SpawnVisualizationPrefab;
        set => m_SpawnVisualizationPrefab = value;
    }

    [SerializeField]
    [Tooltip("The index of the prefab to spawn. If outside the range of the list, this behavior will select " +
        "a random object each time it spawns.")]
    int m_SpawnOptionIndex = -1;

    /// <summary>
    /// The index of the prefab to spawn. If outside the range of <see cref="objectPrefabs"/>, this behavior will
    /// select a random object each time it spawns.
    /// </summary>
    /// <seealso cref="isSpawnOptionRandomized"/>
    public int spawnOptionIndex
    {
        get => m_SpawnOptionIndex;
        set => m_SpawnOptionIndex = value;
    }

    [SerializeField]
    [Tooltip("Whether to only spawn an object if the spawn point is within view of the camera.")]
    bool m_OnlySpawnInView = true;

    /// <summary>
    /// Whether to only spawn an object if the spawn point is within view of the <see cref="cameraToFace"/>.
    /// </summary>
    public bool onlySpawnInView
    {
        get => m_OnlySpawnInView;
        set => m_OnlySpawnInView = value;
    }

    [SerializeField]
    [Tooltip("The size, in viewport units, of the periphery inside the viewport that will not be considered in view.")]
    float m_ViewportPeriphery = 0.15f;

    /// <summary>
    /// The size, in viewport units, of the periphery inside the viewport that will not be considered in view.
    /// </summary>
    public float viewportPeriphery
    {
        get => m_ViewportPeriphery;
        set => m_ViewportPeriphery = value;
    }

    [SerializeField]
    [Tooltip("When enabled, the object will be rotated about the y-axis when spawned by Spawn Angle Range, " +
        "in relation to the direction of the spawn point to the camera.")]
    bool m_ApplyRandomAngleAtSpawn = true;

    /// <summary>
    /// When enabled, the object will be rotated about the y-axis when spawned by <see cref="spawnAngleRange"/>
    /// in relation to the direction of the spawn point to the camera.
    /// </summary>
    public bool applyRandomAngleAtSpawn
    {
        get => m_ApplyRandomAngleAtSpawn;
        set => m_ApplyRandomAngleAtSpawn = value;
    }

    [SerializeField]
    [Tooltip("The range in degrees that the object will randomly be rotated about the y axis when spawned, " +
        "in relation to the direction of the spawn point to the camera.")]
    float m_SpawnAngleRange = 45f;

    /// <summary>
    /// The range in degrees that the object will randomly be rotated about the y axis when spawned, in relation
    /// to the direction of the spawn point to the camera.
    /// </summary>
    public float spawnAngleRange
    {
        get => m_SpawnAngleRange;
        set => m_SpawnAngleRange = value;
    }

    [SerializeField]
    [Tooltip("Whether to spawn each object as a child of this object.")]
    bool m_SpawnAsChildren;

    /// <summary>
    /// Whether to spawn each object as a child of this object.
    /// </summary>
    public bool spawnAsChildren
    {
        get => m_SpawnAsChildren;
        set => m_SpawnAsChildren = value;
    }

    [SerializeField]
    XRInteractionGroup m_InteractionGroup;

    [SerializeField]
    [Tooltip("Button that deletes a selected object.")]
    //Button m_DeleteButton;

    #endregion

    #region Public Fields

    public int objectIndex = 0; // Index to select which prefab to spawn
    public bool object1Spawned = false; // Track if object 1 is spawned
    public bool object2Spawned = false; // Track if object 2 is spawned
    public bool object3Spawned = false; // Track if object 3 is spawned

    [Header("Size in Inches")]
    [Tooltip("Real-world dimensions")]
    public float widthInches = 12f;
    public float heightInches = 6f;
    public float depthInches = 3f;
    public string unit = "cm"; // Units used for scaling

    public SizeInInches[] objectsSize; // Array storing sizes for each object prefab

    public List<Color> objectColors; // Colors corresponding to objects

    public List<GameObject> objectsSpawned; // List of currently spawned objects
    public int spawnObjectCount = 0; // Counter for spawned objects

    #endregion

    #region Nested Classes

    [Serializable]
    public class SizeInInches
    {
        public float length;
        public float height;
        public float width;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Accessor for the XRInteractionGroup.
    /// </summary>
    public XRInteractionGroup interactionGroup
    {
        get => m_InteractionGroup;
        set => m_InteractionGroup = value;
    }

    /// <summary>
    /// Whether this behavior will select a random object from <see cref="objectPrefabs"/> each time it spawns.
    /// </summary>
    public bool isSpawnOptionRandomized => m_SpawnOptionIndex < 0 || m_SpawnOptionIndex >= m_ObjectPrefabs.Count;

    #endregion

    #region Unity Callbacks

    void Awake()
    {
        EnsureFacingCamera(); // Ensure camera is assigned on awake
    }

    //private void OnEnable()
    //{
    //    ARObjectTouchDetector.OnObjectTouched += ObjectSelected;
    //    //m_DeleteButton.onClick.AddListener(DeleteFocusedObject);
    //}

    //private void OnDisable()
    //{
    //    ARObjectTouchDetector.OnObjectTouched -= ObjectSelected;
    //    //m_DeleteButton.onClick.RemoveListener(DeleteFocusedObject);
    //}

    //private void Update()
    //{
    //    if (m_DeleteButton != null)
    //    {
    //        m_DeleteButton.gameObject.SetActive(m_InteractionGroup?.focusInteractable != null);
    //    }
    //}

    #endregion

    #region Public Methods

    /// <summary>
    /// Called when an object is selected by touch or other input.
    /// </summary>
    /// <param name="obj">The selected GameObject.</param>
    public void ObjectSelected(int index)
    {
        UIManagerAR.instance.objectSelectedIndex = index;
    }

    /// <summary>
    /// Deletes the currently focused object in the interaction group.
    /// </summary>
    void DeleteFocusedObject()
    {
        var currentFocusedObject = m_InteractionGroup.focusInteractable;
        if (currentFocusedObject != null)
        {
            Destroy(currentFocusedObject.transform.gameObject);
        }
    }

    public void DeleteAllSpawnedObjects()
    {
        if (objectsSpawned == null || objectsSpawned.Count == 0) return;

        foreach (var obj in objectsSpawned)
        {
            if (obj != null)
                Destroy(obj);
        }
        objectsSpawned.Clear();

        object1Spawned = false;
        object2Spawned = false;
        object3Spawned = false;

        spawnObjectCount = 0;
        objectIndex = -1;
    }
    public Material mat;
    /// <summary>
    /// Select the color of a spawned object by index.
    /// </summary>
    /// <param name="index">Index of the color to apply.</param>
    public bool ChangeTextureByIndex(int index)
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

        
        if (object1Spawned)
        {
            GameObject obj = objectsSpawned[UIManagerAR.instance.objectSelectedIndex];
            /*Material*/ mat = obj.GetComponentInChildren<MeshRenderer>().material;
            
            var modelView = UIManagerAR.instance.UI_3D_Models.Find(m =>
            m.GetComponent<ProductDetails>().product.id == obj.GetComponent<ProductDetails>().product.id);

            mat.mainTexture = modelView.GetComponent<ProductDetails>().textures[index];

            obj.GetComponent<ProductDetails>().selectedColorIndex = index;
            if (UIManagerAR.instance.objectSelectedIndex >= 0 && UIManagerAR.instance.objectSelectedIndex < objectsSpawned.Count)
            {
                obj.GetComponentInChildren<MeshRenderer>().material = mat;
                obj.GetComponentInChildren<MeshRenderer>().UpdateGIMaterials();
            }


            modelView.GetComponentInChildren<MeshRenderer>().material = mat;
            Canvas.ForceUpdateCanvases();

            UIManagerAR.instance.UpdateDetailData();

            return true;
        }


        Debug.Log("index : " + index);

        Canvas.ForceUpdateCanvases();
        return false;
    }

    /// <summary>
    /// Sets this behavior to select a random object from <see cref="objectPrefabs"/> each time it spawns.
    /// </summary>
    public void RandomizeSpawnOption()
    {
        m_SpawnOptionIndex = -1; // Set index to -1 to indicate random selection
    }

    /// <summary>
    /// Attempts to spawn an object at the given position and orientation.
    /// </summary>
    /// <param name="spawnPoint">The world space position at which to spawn the object.</param>
    /// <param name="spawnNormal">The world space normal of the spawn surface.</param>
    /// <returns>True if spawning succeeded, false otherwise.</returns>
    public bool TrySpawnObject(Vector3 spawnPoint, Vector3 spawnNormal)
    {
        // Check if spawn point is within camera view if onlySpawnInView is enabled
        if (m_OnlySpawnInView)
        {
            var inViewMin = m_ViewportPeriphery;
            var inViewMax = 1f - m_ViewportPeriphery;
            var pointInViewportSpace = cameraToFace.WorldToViewportPoint(spawnPoint);

            // If point is behind camera or outside viewport bounds with periphery considered, reject spawn
            if (pointInViewportSpace.z < 0f || pointInViewportSpace.x > inViewMax || pointInViewportSpace.x < inViewMin ||
                pointInViewportSpace.y > inViewMax || pointInViewportSpace.y < inViewMin)
            {
                return false;
            }
        }

        // Determine if object can spawn based on current object index and spawn flags
        bool spawnObject = false;
        switch (objectIndex)
        {
            case 0: if (object1Spawned) spawnObject = true; break;
            case 1: if (object2Spawned) spawnObject = true; break;
            case 2: if (object3Spawned) spawnObject = true; break;
        }
        if (spawnObject)
            return false; // Prevent spawning if object already spawned

        // Instantiate the prefab at the specified index
        //var newObject = Instantiate(m_ObjectPrefabs[objectIndex]);
        var newObject = m_ObjectPrefabs[objectIndex];
        if (!newObject.GetComponent<ObjectDetail>())
            newObject.AddComponent<ObjectDetail>().index = spawnObjectCount; // Assign unique index
        else
            newObject.GetComponent<ObjectDetail>().index = spawnObjectCount;

        newObject.SetActive(true);
        UIManagerAR.instance.objectSelectedIndex = spawnObjectCount;
        spawnObjectCount++;
        //objectsSpawned.Insert(0, newObject); // Add to the front of spawned list
        objectsSpawned.Add(newObject); // Add to the front of spawned list

        //ARDimensionVisualizer aRDimension = newObject.GetComponentInChildren<ARDimensionVisualizer>();

        // Check if the spawn surface is vertical (less than 0.5 Y normal)
        bool isVerticalSurface = Mathf.Abs(spawnNormal.y) < 0.5f;

        // Set the ARDimensionVisualizer's dimension texts
        //aRDimension.textLength = objectsSize[objectIndex].length;
        //aRDimension.textHeight = objectsSize[objectIndex].height;
        //aRDimension.textDepth = objectsSize[objectIndex].width;

        // Update the object's scale based on stored dimensions
        //UpdateObjectScale(newObject, isVerticalSurface);


        newObject.transform.position = spawnPoint; // Position the spawned object

        if (m_SpawnAsChildren)
            newObject.transform.parent = transform; // Parent to spawner if enabled

        EnsureFacingCamera(); // Confirm the facing camera is assigned

        //Transform plusBtnCanvas = newObject.GetComponentInChildren<Canvas>().transform;

        // Orient the spawned object based on surface type
        if (!isVerticalSurface)
        {
            var facePosition = m_CameraToFace.transform.position;
            var forward = facePosition - spawnPoint;
            BurstMathUtility.ProjectOnPlane(forward, spawnNormal, out var projectedForward);
            
            //CategoryManager.Instance.UnparentSafely(plusBtnCanvas, newObject.transform);

            newObject.transform.rotation = Quaternion.LookRotation(projectedForward, spawnNormal);
            
            Physics.SyncTransforms();
            Canvas.ForceUpdateCanvases();

            //CategoryManager.Instance.ReparentSafely(plusBtnCanvas, newObject.transform);
        }
        else
        {
            BurstMathUtility.ProjectOnPlane(spawnPoint, spawnNormal, out var projectedForward);

            //CategoryManager.Instance.UnparentSafely(plusBtnCanvas, newObject.transform);

            newObject.transform.rotation = Quaternion.LookRotation(projectedForward, spawnNormal);


            Vector3 euler = newObject.transform.eulerAngles;

            // If the surface is almost vertical (a wall)
            if (Mathf.Abs(spawnNormal.y) < 0.1f)
            {
                euler.x = 0f; // Remove tilt on X axis

                if (Vector3.Dot(-newObject.transform.right, Vector3.down) > 0.5f)
                {
                    euler.x += 180f; // Flip it if necessary
                }
            }

            newObject.transform.rotation = Quaternion.Euler(euler);
            
            Physics.SyncTransforms();
            Canvas.ForceUpdateCanvases();

            //CategoryManager.Instance.ReparentSafely(plusBtnCanvas, newObject.transform);
        }

        // Apply a random y-axis rotation if enabled
        if (m_ApplyRandomAngleAtSpawn)
        {
            var randomRotation = UnityEngine.Random.Range(-m_SpawnAngleRange, m_SpawnAngleRange);
            newObject.transform.Rotate(Vector3.up, randomRotation);
        }

        // Instantiate spawn visualization prefab if set
        if (m_SpawnVisualizationPrefab != null)
        {
            var visualizationTrans = Instantiate(m_SpawnVisualizationPrefab).transform;
            visualizationTrans.position = spawnPoint;
            visualizationTrans.rotation = newObject.transform.rotation;
        }

        

        //StartCoroutine(UIManagerAR.instance.ChangeMovementControllers(newObject)); // Change UI movement controllers

        objectSpawned?.Invoke(newObject); // Trigger event

        CategoryManager.Instance.tempModels.Remove(newObject);

        foreach (Transform obj in CategoryManager.Instance.modelVariantParent)
            obj.gameObject.SetActive(true);

        // Update spawn flags for the corresponding object index
        switch (objectIndex)
        {
            case 0: object1Spawned = true; break;
            case 1: object2Spawned = true; break;
            case 2: object3Spawned = true; break;
        }

        UIManagerAR.instance.TogglePlaneVisuals(false); // Hide plane visuals on spawn
        objectIndex = -1;
        return true; // Spawn successful
    }

    /// <summary>
    /// Updates the scale of the spawned object based on its target dimensions.
    /// </summary>
    /// <param name="newObject">The spawned GameObject to scale.</param>
    /// <param name="isVertical">If true, indicates object is on vertical surface.</param>
    //public void UpdateObjectScale(GameObject newObject, bool isVertical = false)
    //{
    //    Vector3 modelOriginalHeight;

    //    // Get original bounds size of the mesh renderer
    //    modelOriginalHeight = newObject.GetComponentInChildren<MeshRenderer>().bounds.size;

    //    // Calculate scale ratios for each axis according to desired size

    //    if(isVertical)
    //    {
    //        float temp = objectsSize[objectIndex].length;
    //        objectsSize[objectIndex].length = objectsSize[objectIndex].height;
    //        objectsSize[objectIndex].height = objectsSize[objectIndex].width;
    //        objectsSize[objectIndex].width = temp;
    //    }

    //    Vector3 finalScale = new Vector3(ConvertToUnityScale(objectsSize[objectIndex].length, unit) / modelOriginalHeight.x,
    //        ConvertToUnityScale(objectsSize[objectIndex].height, unit) / modelOriginalHeight.y,
    //        ConvertToUnityScale(objectsSize[objectIndex].width, unit) / modelOriginalHeight.z
    //        );

    //    //float scaleX = ConvertToUnityScale(objectsSize[objectIndex].length, unit) / modelOriginalHeight.x;
    //    //float scaleY = ConvertToUnityScale(objectsSize[objectIndex].height, unit) / modelOriginalHeight.y;
    //    //float scaleZ = ConvertToUnityScale(objectsSize[objectIndex].width, unit) / modelOriginalHeight.z;


    //    //// Use the smallest scale factor for uniform scaling to fit inside bounds
    //    //float uniformScale = Mathf.Min(scaleX, Mathf.Min(scaleY, scaleZ));
    //    //Debug.Log("Uniform Scale : " + uniformScale);
    //    //newObject.transform.localScale = Vector3.one * uniformScale;
    //    newObject.transform.localScale = finalScale;
    //}

    /// <summary>
    /// Converts dimensions from inches to Unity scale (meters).
    /// </summary>
    /// <param name="widthInches">Width in inches.</param>
    /// <param name="heightInches">Height in inches.</param>
    /// <param name="depthInches">Depth in inches.</param>
    /// <returns>Vector3 scale in meters.</returns>
    public static Vector3 InchesToScale(float widthInches, float heightInches, float depthInches)
    {
        const float inchesToMeters = 0.0254f; // 1 inch = 0.0254 meters
        return new Vector3(
            widthInches * inchesToMeters,
            heightInches * inchesToMeters,
            depthInches * inchesToMeters
        );
    }

    /// <summary>
    /// Converts input size based on unit string to meters (Unity scale).
    /// </summary>
    /// <param name="inputSize">Size value.</param>
    /// <param name="unit">Unit string, e.g., "cm", "in", or "m".</param>
    /// <returns>Size converted to meters.</returns>
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
                Debug.LogWarning("Unknown unit, defaulting to meters");
                return inputSize;
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Ensures the camera to face is assigned, defaults to Camera.main if null.
    /// </summary>
    void EnsureFacingCamera()
    {
        if (m_CameraToFace == null)
            m_CameraToFace = Camera.main;
    }

    #endregion

    #region Events

    /// <summary>
    /// Event invoked when an object is spawned.
    /// </summary>
    public event Action<GameObject> objectSpawned;

    #endregion
}
//}
