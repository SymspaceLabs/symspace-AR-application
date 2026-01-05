using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARBodyToHumanoid : MonoBehaviour
{
    [SerializeField] private ARHumanBodyManager bodyManager;
    [SerializeField] private Animator characterAnimator;

    private Dictionary<HumanBodyBones, XRHumanBodyJoint> jointMap =
        new Dictionary<HumanBodyBones, XRHumanBodyJoint>();

    void Update()
    {
        if (bodyManager.trackables.count == 0)
            return;

        foreach (var body in bodyManager.trackables)
        {
            UpdateBody(body);
            break; // use first detected body only
        }
    }

    void UpdateBody(ARHumanBody body)
    {
        if (!body.joints.IsCreated)
            return;

        MapJoints(body);

        foreach (var pair in jointMap)
        {
            Transform bone = characterAnimator.GetBoneTransform(pair.Key);
            if (bone == null || !pair.Value.tracked)
                continue;

            bone.localPosition = pair.Value.anchorPose.position;
            bone.localRotation = pair.Value.anchorPose.rotation;
        }
    }

    void MapJoints(ARHumanBody body)
    {
        //XRHumanBodyJoint joint = new XRHumanBodyJoint();
        //jointMap[HumanBodyBones.Hips] = body.joints[(int)XRHumanBodyJoint.Hips];
        //jointMap[HumanBodyBones.Spine] = body.joints[(int)XRHumanBodyJointIndex.Spine];
        //jointMap[HumanBodyBones.Chest] = body.joints[(int)XRHumanBodyJointIndex.Chest];
        //jointMap[HumanBodyBones.Head] = body.joints[(int)XRHumanBodyJointIndex.Head];

        //jointMap[HumanBodyBones.LeftUpperArm] = body.joints[(int)XRHumanBodyJointIndex.LeftUpperArm];
        //jointMap[HumanBodyBones.LeftLowerArm] = body.joints[(int)XRHumanBodyJointIndex.LeftLowerArm];
        //jointMap[HumanBodyBones.LeftHand] = body.joints[(int)XRHumanBodyJointIndex.LeftHand];

        //jointMap[HumanBodyBones.RightUpperArm] = body.joints[(int)XRHumanBodyJointIndex.RightUpperArm];
        //jointMap[HumanBodyBones.RightLowerArm] = body.joints[(int)XRHumanBodyJointIndex.RightLowerArm];
        //jointMap[HumanBodyBones.RightHand] = body.joints[(int)XRHumanBodyJointIndex.RightHand];

        //jointMap[HumanBodyBones.LeftUpperLeg] = body.joints[(int)XRHumanBodyJointIndex.LeftUpperLeg];
        //jointMap[HumanBodyBones.LeftLowerLeg] = body.joints[(int)XRHumanBodyJointIndex.LeftLowerLeg];
        //jointMap[HumanBodyBones.LeftFoot] = body.joints[(int)XRHumanBodyJointIndex.LeftFoot];

        //jointMap[HumanBodyBones.RightUpperLeg] = body.joints[(int)XRHumanBodyJointIndex.RightUpperLeg];
        //jointMap[HumanBodyBones.RightLowerLeg] = body.joints[(int)XRHumanBodyJointIndex.RightLowerLeg];
        //jointMap[HumanBodyBones.RightFoot] = body.joints[(int)XRHumanBodyJointIndex.RightFoot];
    }
}
