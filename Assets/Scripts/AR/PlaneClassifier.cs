using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlaneManager))]
public class PlaneClassifier : MonoBehaviour
{
    private ARPlaneManager planeManager;

    public Material floorMat;
    public Material wallMat;
    public Material ceilMat;
    public Material doorMat;
    public Material couchMat;
    public Material SeatMat;
    public Material tableMat;
    //public Material wallArtMat;
    public Material windowMat;
    public Material unknownMaterial;

    void Awake()
    {
        planeManager = GetComponent<ARPlaneManager>();
    }

    void OnEnable()
    {
        planeManager.trackablesChanged.AddListener(OnPlanesChanged);
    }

    void OnDisable()
    {
        planeManager.trackablesChanged.RemoveListener(OnPlanesChanged);
    }

    void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> args)
    {
        foreach (ARPlane plane in args.added)
        {
            UpdatePlaneMaterial(plane);
        }

        foreach (ARPlane plane in args.updated)
        {
            UpdatePlaneMaterial(plane);
        }
    }

    void UpdatePlaneMaterial(ARPlane plane)
    {
#if UNITY_IOS
        var classification = plane.classifications;

        if (classification == PlaneClassifications.Floor)
        {
            plane.GetComponent<MeshRenderer>().material = floorMat;
        }
        else if (classification == PlaneClassifications.Table)
        {
            plane.GetComponent<MeshRenderer>().material = tableMat;
        }
        else if (classification == PlaneClassifications.DoorFrame)
        {
            plane.GetComponent<MeshRenderer>().material = doorMat;
        }
        else if (classification == PlaneClassifications.Couch)
        {
            plane.GetComponent<MeshRenderer>().material = couchMat;
        }
        else if (classification == PlaneClassifications.Ceiling)
        {
            plane.GetComponent<MeshRenderer>().material = ceilMat;
        }
        else if (classification == PlaneClassifications.Seat)
        {
            plane.GetComponent<MeshRenderer>().material = SeatMat;
        }
        else if (classification == PlaneClassifications.WallFace)
        {
            plane.GetComponent<MeshRenderer>().material = wallMat;
        }
        else if (classification == PlaneClassifications.WindowFrame)
        {
            plane.GetComponent<MeshRenderer>().material = windowMat;
        }
        else
        {
            plane.GetComponent<MeshRenderer>().material = unknownMaterial;
        }
#else
        // Fallback for platforms without classification
        plane.GetComponent<MeshRenderer>().material = unknownMaterial;
#endif
    }
}
