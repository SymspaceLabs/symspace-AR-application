using GLTF.Schema;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


public class ARBodyTracking : MonoBehaviour
{
    public ARHumanBodyManager aRHumanBodyManager;

    [Header("Prefabs")]
    public GameObject shirtPrefab;
    public GameObject pantPrefab;
    public GameObject watchPrefab;
    public GameObject tiePrefab;
    public GameObject glassesPrefab;
    public GameObject ringPrefab;
    public GameObject hatPrefab;

    [Header("Offsets")]
    public Vector3 shirtOffset = new Vector3(0, 0, 0);
    public Vector3 pantOffset = new Vector3(0, 0, 0);
    public Vector3 watchOffset = new Vector3(0, 0, 0);
    public Vector3 tieOffset = new Vector3(0, 0, 0);
    public Vector3 glassesOffset = new Vector3(0, 0, 0);
    public Vector3 ringOffset = new Vector3(0, 0, 0);
    public Vector3 hatOffset = new Vector3(0, 0.1f, 0);

    private GameObject shirtInstance;
    private GameObject pantInstance;
    private GameObject watchInstance;
    private GameObject tieInstance;
    private GameObject glassesInstance;
    private GameObject ringInstance;
    private GameObject hatInstance;

    public GameObject m_SkeletonPrefab;
    GameObject skeletonSpawned;

    public float robotModelHeight = 1.857f;

    [Header("Accessories Configuration")]
    public List<TrackedItem> accessories = new List<TrackedItem>();

    [Header("Target Real World Dimensions (Meters)")]
    public float targetShirtHeight = 0.75f; // 75cm
    public float targetShirtWidth = 0.55f;  // 55cm
    public float targetShirtDepth = 0.20f;  // 20cm

    private Vector3 shirtBaseScale = Vector3.one;

    [Header("Smoothing Settings")]
    [Range(0, 1)]
    public float positionSmoothing = 0.2f; // Lower = smoother/slower, Higher = faster/snappier
    [Range(0, 1)]
    public float rotationSmoothing = 0.15f;

    Dictionary<TrackableId, BoneController> m_SkeletonTracker = new Dictionary<TrackableId, BoneController>();

    private void OnEnable()
    {
        aRHumanBodyManager.trackablesChanged.AddListener(UpdateTrackedBody);
    }
    
    private void OnDisable()
    {
        aRHumanBodyManager.trackablesChanged.RemoveListener(UpdateTrackedBody);
    }

    private void OnBodyChange(ARTrackablesChangedEventArgs<ARHumanBody> args)
    {
        UpdateTrackedBody(args);
        //foreach (var body in args.added)
        //{
        //    UpdateTrackedBody(body);
        //}

        //foreach (var body in args.updated)
        //{
        //    UpdateTrackedBody(body);
        //}

        //foreach (var body in args.removed)
        //{
        //    DestroyAllInstances();
        //}
    }

    private void UpdateTrackedBody(ARTrackablesChangedEventArgs<ARHumanBody> eventArgs)
    {
        //if (body == null || body.joints == null || body.trackingState != TrackingState.Tracking)
        //    return;

        BoneController boneController;

        foreach (var humanBody in eventArgs.added)
        {
            if (!m_SkeletonTracker.TryGetValue(humanBody.trackableId, out boneController))
            {
                Debug.Log($"Adding a new skeleton [{humanBody.trackableId}].");

                skeletonSpawned = Instantiate(m_SkeletonPrefab, humanBody.transform);
                skeletonSpawned.transform.localPosition = Vector3.zero;
                skeletonSpawned.transform.localRotation = Quaternion.identity;
                skeletonSpawned.transform.localScale = Vector3.one;

                boneController = skeletonSpawned.GetComponent<BoneController>();
                m_SkeletonTracker.Add(humanBody.trackableId, boneController);


            }

            boneController.InitializeSkeletonJoints();
            boneController.ApplyBodyPose(humanBody);

            if(watchPrefab != null)
                watchInstance = Instantiate(watchPrefab, boneController.GetJointTransform(BoneController.JointIndices.RightForearm));

            if (ringPrefab != null)
                ringInstance = Instantiate(ringPrefab, boneController.GetJointTransform(BoneController.JointIndices.RightHandMid1));

            //if(watchInstance != null)
            //    watchInstance.transform.localPosition = watchOffset;
            //if (ringInstance != null)
            //    ringInstance.transform.localPosition = ringOffset;

            //boneController.ApplyObjectPosition(ringInstance.transform, humanBody, BoneController.JointIndices.RightHandMid1, ringOffset);

            //boneController.ApplyObjectPosition(watchInstance.transform, humanBody, BoneController.JointIndices.RightForearm, watchOffset);
        }

        foreach (var humanBody in eventArgs.updated)
        {
            if (m_SkeletonTracker.TryGetValue(humanBody.trackableId, out boneController))
            {

                SmoothApplyBodyPose(boneController, humanBody);
                //boneController.ApplyBodyPose(humanBody);
                float humanScale = humanBody.estimatedHeightScaleFactor;

                //boneController.transform.localScale = new Vector3(humanScale, humanScale, humanScale);
                // Smoothly scale the root too
                boneController.transform.localScale = Vector3.Lerp(
                    boneController.transform.localScale,
                    Vector3.one * humanScale,
                    positionSmoothing
                );

                foreach (var item in accessories)
                {
                    SyncItem(item, humanBody, boneController, humanScale);
                }
            }
        }

        foreach (var /*(trackableId, _)*/humanBody in eventArgs.removed)
        {
            //Debug.Log($"Removing a skeleton [{trackableId}].");
            //if (m_SkeletonTracker.TryGetValue(trackableId, out boneController))
            //{
                //Destroy(boneController.gameObject);
                //Destroy(watchInstance.gameObject);
                //Destroy(ringInstance.gameObject);
                //m_SkeletonTracker.Remove(trackableId);
            //}
        }

        //if (watchPrefab && leftWrist.tracked)
        //{
        //    if (!watchInstance) watchInstance = Instantiate(watchPrefab);
        //    watchInstance.transform.SetPositionAndRotation(
        //        leftWrist.anchorPose.position + leftWrist.anchorPose.rotation * watchOffset,
        //        leftWrist.anchorPose.rotation);
        //}

        //if (tiePrefab && neck.tracked)
        //{
        //    if (!tieInstance) tieInstance = Instantiate(tiePrefab);
        //    tieInstance.transform.SetPositionAndRotation(
        //        neck.anchorPose.position + neck.anchorPose.rotation * tieOffset,
        //        neck.anchorPose.rotation);
        //}

        //if (glassesPrefab && leftEye.tracked && rightEye.tracked)
        //{
        //    if (!glassesInstance) glassesInstance = Instantiate(glassesPrefab);
        //    Vector3 midPos = (leftEye.anchorPose.position + rightEye.anchorPose.position) / 2f;
        //    Quaternion rot = Quaternion.Slerp(leftEye.anchorPose.rotation, rightEye.anchorPose.rotation, 0.5f);
        //    glassesInstance.transform.SetPositionAndRotation(
        //        midPos + rot * glassesOffset,
        //        rot);
        //}
    }

    private void SyncItem(TrackedItem item, ARHumanBody body, BoneController controller, float scale)
    {
        if (item.prefab == null) return;

        // 1. Check if the specific joint is tracked
        var joint = body.joints[(int)item.bone];
        if (!joint.tracked) return;

        // 2. Get the bone transform from the robot skeleton
        Transform boneTransform = controller.GetJointTransform((BoneController.JointIndices)item.bone);

        if (item.instance == null)
        {
            item.instance = Instantiate(item.prefab);
            item.baseScale = CalculateBaseScale(item.instance, item.realWorldDimensions);
        }

        // 3. Sync Position (World space + Offset)
        item.instance.transform.position = boneTransform.position + (boneTransform.rotation * (item.offset * scale));

        // 4. Sync Rotation (Using the "Green is Front" logic)
        // Adjust these vectors if specific items (like shoes) face a different way
        Vector3 forward = boneTransform.up;    // Green
        Vector3 up = boneTransform.right;      // Red
        if (forward != Vector3.zero)
        {
            item.instance.transform.rotation = Quaternion.LookRotation(forward, up);
        }

        // 5. Sync Scale (Real world size * AR height factor)
        item.instance.transform.localScale = Vector3.Scale(item.baseScale, Vector3.one * scale);
    }

    private Vector3 CalculateBaseScale(GameObject obj, Vector3 targetDimensions)
    {
        MeshFilter mesh = obj.GetComponentInChildren<MeshFilter>();
        if (mesh == null) return Vector3.one;

        Vector3 meshSize = mesh.sharedMesh.bounds.size;
        return new Vector3(
            targetDimensions.x / meshSize.x,
            targetDimensions.y / meshSize.y,
            targetDimensions.z / meshSize.z
        );
    }

    public Slider xSlider;
    public Slider ySlider;
    public Slider zSlider;

    // New method to handle the "Lag" effect
    private void SmoothApplyBodyPose(BoneController controller, ARHumanBody body)
    {
        // We iterate through the joints and Lerp their local positions/rotations
        foreach (var joint in body.joints)
        {
            if (!joint.tracked) continue;

            Transform boneTransform = controller.GetJointTransform((BoneController.JointIndices)joint.index);
            if (boneTransform == null) continue;

            // Smooth Position
            boneTransform.localPosition = Vector3.Lerp(
                boneTransform.localPosition,
                joint.localPose.position,
                positionSmoothing
            );

            // Smooth Rotation
            boneTransform.localRotation = Quaternion.Slerp(
                boneTransform.localRotation,
                joint.localPose.rotation,
                rotationSmoothing
            );
        }
    }

    //private void Update()
    //{
    //    if(accessories.Count == 2)
    //    {
    //        accessories[1].realWorldDimensions = new Vector3(xSlider.value, ySlider.value, zSlider.value);
    //    }
    //}

    private void DestroyAllInstances()
    {
        if (shirtInstance) Destroy(shirtInstance);
        if (pantInstance) Destroy(pantInstance);
        if (watchInstance) Destroy(watchInstance);
        if (tieInstance) Destroy(tieInstance);
        if (glassesInstance) Destroy(glassesInstance);

        shirtInstance = null;
        pantInstance = null;
        watchInstance = null;
        tieInstance = null;
        glassesInstance = null;
    }

    public enum CustomBones
    {
        Hips = 1,
        LeftLegUp = 2,
        LeftLeg = 3,
        LeftFoot = 4,
        LeftToes = 5,
        LeftToesEnd = 6,
        RightUpLeg = 7,
        RightLeg = 8,
        RightFoot = 9,
        RightToes = 10,
        RightToesEnd = 11,
        Chest = 17,
        LeftShoulder = 19,
        LeftArm = 20,
        LeftForeArm = 21,
        LeftHand = 22,
        Neck1 = 47,
        Neck2 = 48,
        Neck3 = 49,
        Neck4 = 50,
        Head = 51,
        Jaw = 52,
        Chin = 53,
        LeftEye = 54,
        RightEye = 59
    }

    [System.Serializable]
    public class TrackedItem
    {
        public string label;
        public GameObject prefab;
        public CustomBones bone;
        public Vector3 offset;
        public Vector3 realWorldDimensions = new Vector3(0.5f, 0.5f, 0.1f); // Width, Height, Depth in meters
        [HideInInspector] public GameObject instance;
        [HideInInspector] public Vector3 baseScale;
    }
}
