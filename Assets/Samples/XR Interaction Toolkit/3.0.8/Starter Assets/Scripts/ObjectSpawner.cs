using System;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.UI;
using UnityEngine;

//namespace UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets
//{
    /// <summary>
    /// Behavior with an API for spawning objects from a given set of prefabs.
    /// </summary>
    public class ObjectSpawner : MonoBehaviour
    {
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

        /// <summary>
        /// Whether this behavior will select a random object from <see cref="objectPrefabs"/> each time it spawns.
        /// </summary>
        /// <seealso cref="spawnOptionIndex"/>
        /// <seealso cref="RandomizeSpawnOption"/>
        public bool isSpawnOptionRandomized => m_SpawnOptionIndex < 0 || m_SpawnOptionIndex >= m_ObjectPrefabs.Count;

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

        /// <summary>
        /// Event invoked after an object is spawned.
        /// </summary>
        /// <seealso cref="TrySpawnObject"/>
        public event Action<GameObject> objectSpawned;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        /// 

        [SerializeField]
        XRInteractionGroup m_InteractionGroup;

        [SerializeField]
        [Tooltip("Button that deletes a selected object.")]
        //Button m_DeleteButton;

        public int objectIndex = 0;
        public bool object1Spawned = false;
        public bool object2Spawned = false;
        public bool object3Spawned = false;

        [Header("Size in Inches")]
        [Tooltip("Real-world dimensions")]
        public float widthInches = 12f;
        public float heightInches = 6f;
        public float depthInches = 3f;
        public string unit = "cm";
        public SizeInInches[] objectsSize;

        public Color[] objectColors;
        public List<GameObject> objectsSpawned;
        public int spawnObjectCount = 0;

        [Serializable]
        public class SizeInInches
        {
            public float width;
            public float height;
            public float depth;
        }

        public XRInteractionGroup interactionGroup
        {
            get => m_InteractionGroup;
            set => m_InteractionGroup = value;
        }

        void Awake()
        {
            EnsureFacingCamera();
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

        public void ObjectSelected(GameObject obj)
        {
            UIManagerAR.instance.objectSelectedIndex = obj.GetComponent<ObjectDetail>().index;
        }


        void DeleteFocusedObject()
        {
            var currentFocusedObject = m_InteractionGroup.focusInteractable;
            if (currentFocusedObject != null)
            {
                Destroy(currentFocusedObject.transform.gameObject);
            }
        }

        public void SelectObjectIndex(int index)
        {
            //objectIndex = index;
            if(object1Spawned)
                objectsSpawned[0].GetComponentInChildren<MeshRenderer>().material.color = objectColors[index];
        }

        //private void Update()
        //{
        //    if (m_DeleteButton != null)
        //    {
        //        m_DeleteButton.gameObject.SetActive(m_InteractionGroup?.focusInteractable != null);
        //    }
        //}

        void EnsureFacingCamera()
        {
            if (m_CameraToFace == null)
                m_CameraToFace = Camera.main;
        }

        /// <summary>
        /// Sets this behavior to select a random object from <see cref="objectPrefabs"/> each time it spawns.
        /// </summary>
        /// <seealso cref="spawnOptionIndex"/>
        /// <seealso cref="isSpawnOptionRandomized"/>
        public void RandomizeSpawnOption()
        {
            m_SpawnOptionIndex = -1;
        }

        /// <summary>
        /// Attempts to spawn an object from <see cref="objectPrefabs"/> at the given position. The object will have a
        /// yaw rotation that faces <see cref="cameraToFace"/>, plus or minus a random angle within <see cref="spawnAngleRange"/>.
        /// </summary>
        /// <param name="spawnPoint">The world space position at which to spawn the object.</param>
        /// <param name="spawnNormal">The world space normal of the spawn surface.</param>
        /// <returns>Returns <see langword="true"/> if the spawner successfully spawned an object. Otherwise returns
        /// <see langword="false"/>, for instance if the spawn point is out of view of the camera.</returns>
        /// <remarks>
        /// The object selected to spawn is based on <see cref="spawnOptionIndex"/>. If the index is outside
        /// the range of <see cref="objectPrefabs"/>, this method will select a random prefab from the list to spawn.
        /// Otherwise, it will spawn the prefab at the index.
        /// </remarks>
        /// <seealso cref="objectSpawned"/>
        public bool TrySpawnObject(Vector3 spawnPoint, Vector3 spawnNormal)
        {
            if (m_OnlySpawnInView)
            {
                var inViewMin = m_ViewportPeriphery;
                var inViewMax = 1f - m_ViewportPeriphery;
                var pointInViewportSpace = cameraToFace.WorldToViewportPoint(spawnPoint);
                if (pointInViewportSpace.z < 0f || pointInViewportSpace.x > inViewMax || pointInViewportSpace.x < inViewMin ||
                    pointInViewportSpace.y > inViewMax || pointInViewportSpace.y < inViewMin)
                {
                    return false;
                }
            }

            //var objectIndex = isSpawnOptionRandomized ? Random.Range(0, m_ObjectPrefabs.Count) : m_SpawnOptionIndex;
            bool spawnObject = false;
            switch(objectIndex)
            {
                case 0: if (object1Spawned) spawnObject = true; break;
                case 1: if(object2Spawned) spawnObject = true; break;
                case 2: if(object3Spawned) spawnObject = true; break;
            }
            if (spawnObject)
                return false;

            var newObject = Instantiate(m_ObjectPrefabs[objectIndex]);
            newObject.AddComponent<ObjectDetail>().index = spawnObjectCount;
            spawnObjectCount++;
            objectsSpawned.Insert(0, newObject);
            ARDimensionVisualizer aRDimension = newObject.GetComponentInChildren<ARDimensionVisualizer>();

            bool isVerticalSurface = Mathf.Abs(spawnNormal.y) < 0.5f;

            UpdateObjectScale(newObject, isVerticalSurface);
            aRDimension.textWidth = objectsSize[objectIndex].width;
            aRDimension.textHeight = objectsSize[objectIndex].height;
            aRDimension.textDepth = objectsSize[objectIndex].depth;

            //ARDimensionVisualizer visualizer = newObject.GetComponentInChildren<ARDimensionVisualizer>();
            //visualizer.targetObject = newObject.transform;
            //visualizer.transform.SetParent(newObject.transform, false);

            if (m_SpawnAsChildren)
                newObject.transform.parent = transform;

            newObject.transform.position = spawnPoint;
            EnsureFacingCamera();


            if (!isVerticalSurface)
            {
                var facePosition = m_CameraToFace.transform.position;
                var forward = facePosition - spawnPoint;
                BurstMathUtility.ProjectOnPlane(forward, spawnNormal, out var projectedForward);
                newObject.transform.rotation = Quaternion.LookRotation(projectedForward, spawnNormal);
            }
            else
            {
                BurstMathUtility.ProjectOnPlane(spawnPoint, spawnNormal, out var projectedForward);
                newObject.transform.rotation = Quaternion.LookRotation(projectedForward, spawnNormal);



                //Quaternion rot = Quaternion.LookRotation(-projectedForward, spawnNormal);
                Vector3 euler = newObject.transform.eulerAngles;// rot.eulerAngles;



                // If the surface is vertical, you may want to clamp the X angle
                if (Mathf.Abs(spawnNormal.y) < 0.1f) // means it's a wall
                {
                    //euler.x = Mathf.Round(euler.x / 90f) * 90f;
                    euler.x = 0f; // remove tilt

                    if (Vector3.Dot(-newObject.transform.right, Vector3.down) > 0.5f)
                    {
                        euler.x += 180f; // Flip it over
                    }
                }

                newObject.transform.rotation = Quaternion.Euler(euler);
            }

            if (m_ApplyRandomAngleAtSpawn)
            {
                var randomRotation = UnityEngine.Random.Range(-m_SpawnAngleRange, m_SpawnAngleRange);
                newObject.transform.Rotate(Vector3.up, randomRotation);
            }

            if (m_SpawnVisualizationPrefab != null)
            {
                var visualizationTrans = Instantiate(m_SpawnVisualizationPrefab).transform;
                visualizationTrans.position = spawnPoint;
                visualizationTrans.rotation = newObject.transform.rotation;
            }

            objectSpawned?.Invoke(newObject);

            switch (objectIndex)
            {
                case 0: object1Spawned = true; break;
                case 1: object2Spawned = true; break;
                case 2: object3Spawned = true; break;
            }

            UIManagerAR.instance.TogglePlaneVisuals(false);

            return true;
        }

        public void UpdateObjectScale(GameObject newObject, bool isVertical = false)
        {
            if (newObject != null)
            {
                Vector3 modelOriginalHeight;
                //float targetHeightInMeters = ConvertToUnityScale(objectsSize[objectIndex].height, unit);
                //if(!isVertical)
                //    modelOriginalHeight = newObject.GetComponentInChildren<MeshRenderer>().bounds.size;
                //else
                    modelOriginalHeight = newObject.GetComponentInChildren<MeshRenderer>().bounds.size;

                //float scaleFactor = targetHeightInMeters / modelOriginalHeight;

                // Calculate scale ratios for each axis
                float scaleX = ConvertToUnityScale(objectsSize[objectIndex].width, unit) / modelOriginalHeight.x;
                float scaleY = ConvertToUnityScale(objectsSize[objectIndex].height, unit) / modelOriginalHeight.y;
                float scaleZ = ConvertToUnityScale(objectsSize[objectIndex].depth, unit) / modelOriginalHeight.z;

                // Choose the smallest scale factor to fit the object within the target bounds
                float uniformScale = Mathf.Min(scaleX, Mathf.Min(scaleY, scaleZ));

                newObject.transform.localScale = Vector3.one * uniformScale;

                //newObject.transform.localScale = InchesToScale(
                //    objectsSize[objectIndex].width,
                //    objectsSize[objectIndex].height,
                //    objectsSize[objectIndex].depth
                //);
            }
        }

        public static Vector3 InchesToScale(float widthInches, float heightInches, float depthInches)
        {
            const float inchesToMeters = 0.0254f; // 1 inch = 0.0254 meters
            return new Vector3(
                widthInches * inchesToMeters,
                heightInches * inchesToMeters,
                depthInches * inchesToMeters
            );
        }

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
    }
//}
