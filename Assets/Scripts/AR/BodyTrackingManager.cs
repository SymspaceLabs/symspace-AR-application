using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class BodyTrackingManager : MonoBehaviour
{
    [SerializeField] private ARHumanBodyManager humanBodyManager;

    // Clothing prefabs
    [SerializeField] private GameObject shirtPrefab;
    [SerializeField] private GameObject pantsPrefab;
    [SerializeField] private GameObject shoesPrefab;

    // Active clothing instances
    private GameObject currentShirt;
    private GameObject currentPants;
    private GameObject currentShoes;

    // Tracked body reference
    private ARHumanBody trackedBody;

    void Start()
    {
        if (humanBodyManager == null)
            humanBodyManager = FindFirstObjectByType<ARHumanBodyManager>();
    }

    void OnEnable()
    {
        humanBodyManager.trackablesChanged.AddListener(OnHumanBodiesChanged);
    }

    void OnDisable()
    {
        humanBodyManager.trackablesChanged.RemoveListener(OnHumanBodiesChanged);
    }

    private void OnHumanBodiesChanged(ARTrackablesChangedEventArgs<ARHumanBody> args)
    {
        // Get the first detected body
        if (args.added.Count > 0 || args.updated.Count > 0)
        {
            trackedBody = args.added.Count > 0 ? args.added[0] : args.updated[0];

            //UpdateClothingPosition();
        }
    }

    //void Update()
    //{
    //    if (trackedBody != null)
    //    {
    //        UpdateClothingPosition();
    //    }
    //}

    //private void UpdateClothingPosition()
    //{
    //    // Update shirt position (attach to chest/hips)
    //    if (currentShirt != null)
    //    {
    //        PlaceShirtOnBody();
    //    }

    //    // Update pants position (attach to hips)
    //    if (currentPants != null)
    //    {
    //        PlacePantsOnBody();
    //    }

    //    // Update shoes position
    //    if (currentShoes != null)
    //    {
    //        PlaceShoesOnBody();
    //    }
    //}

    //private void PlaceShirtOnBody()
    //{
    //    // Use chest as main anchor, fallback to hips
    //    if (trackedBody.joints.Length > (int)XRHumanBodyJoint.Chest)
    //    {
    //        Pose chestPose = trackedBody.joints[(int)XRHumanBodyJoint.Chest].pose;
    //        currentShirt.transform.position = chestPose.position;
    //        currentShirt.transform.rotation = chestPose.rotation;
    //    }
    //    else if (trackedBody.joints.Length > (int)XRHumanBodyJoint.Hips)
    //    {
    //        Pose hipsPose = trackedBody.joints[(int)XRHumanBodyJoint.Hips].pose;
    //        currentShirt.transform.position = hipsPose.position;
    //        currentShirt.transform.rotation = hipsPose.rotation;
    //    }
    //}

    //private void PlacePantsOnBody()
    //{
    //    // Attach pants to hips
    //    if (trackedBody.joints.Length > (int)XRHumanBodyJoint.Hips)
    //    {
    //        Pose hipsPose = trackedBody.joints[(int)XRHumanBodyJoint.Hips].pose;
    //        currentPants.transform.position = hipsPose.position;
    //        currentPants.transform.rotation = hipsPose.rotation;
    //    }
    //}

    //private void PlaceShoesOnBody()
    //{
    //    // Place shoes at feet positions
    //    if (trackedBody.joints.Length > (int)XRHumanBodyJoint.LeftFoot)
    //    {
    //        Pose leftFootPose = trackedBody.joints[(int)XRHumanBodyJoint.LeftFoot].pose;
    //        Pose rightFootPose = trackedBody.joints[(int)XRHumanBodyJoint.RightFoot].pose;

    //        // If shoes is a single object, place it between feet
    //        currentShoes.transform.position = (leftFootPose.position + rightFootPose.position) / 2f;
    //    }
    //}
}
