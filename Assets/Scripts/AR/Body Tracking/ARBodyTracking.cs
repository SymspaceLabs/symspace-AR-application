using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;
using System;


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

    [Header("Offsets")]
    public Vector3 shirtOffset = new Vector3(0, 0, 0);
    public Vector3 pantOffset = new Vector3(0, 0, 0);
    public Vector3 watchOffset = new Vector3(0, 0, 0);
    public Vector3 tieOffset = new Vector3(0, 0, 0);
    public Vector3 glassesOffset = new Vector3(0, 0, 0);
    public Vector3 ringOffset = new Vector3(0, 0, 0);

    private GameObject shirtInstance;
    private GameObject pantInstance;
    private GameObject watchInstance;
    private GameObject tieInstance;
    private GameObject glassesInstance;
    private GameObject ringInstance;

    public GameObject m_SkeletonPrefab;
    GameObject skeletonSpawned;

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
                boneController.ApplyBodyPose(humanBody); 
                
                //if (watchInstance != null)
                //    watchInstance.transform.localPosition = watchOffset;
                //if (ringInstance != null)
                //    ringInstance.transform.localPosition = ringOffset;

                //boneController.ApplyObjectPosition(watchInstance.transform, humanBody, BoneController.JointIndices.RightHand, watchOffset);
                //boneController.ApplyObjectPosition(ringInstance.transform, humanBody, BoneController.JointIndices.RightHandMid1, ringOffset);
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
        foreach(var body in eventArgs.updated)
        {
            var joints = body.joints;

            var chest = joints[(int)CustomBones.Chest];
            var hips = joints[(int)CustomBones.Hips];
            var leftWrist = joints[(int)CustomBones.LeftHand];
            var neck = joints[(int)CustomBones.Neck2];
            var leftEye = joints[(int)CustomBones.LeftEye];
            var rightEye = joints[(int)CustomBones.RightEye];

            if (shirtPrefab && chest.tracked)
            {
                if (!shirtInstance) shirtInstance = Instantiate(shirtPrefab, skeletonSpawned.GetComponent<BoneController>().GetJointTransform(BoneController.JointIndices.Spine6));
                shirtInstance.transform.SetLocalPositionAndRotation(
                    chest.localPose.position + /*chest.anchorPose.rotation * */shirtOffset,
                    chest.localPose.rotation);
                Debug.Log("shirt spawned");
            }

            if (pantPrefab && hips.tracked)
            {
                if (!pantInstance) pantInstance = Instantiate(pantPrefab);
                pantInstance.transform.SetPositionAndRotation(
                    hips.anchorPose.position + hips.anchorPose.rotation * pantOffset,
                    hips.anchorPose.rotation);
            }
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
}
