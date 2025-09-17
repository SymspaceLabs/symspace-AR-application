using UnityEngine.XR.ARFoundation;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlane))]
[RequireComponent(typeof(MeshCollider))]
public class ARPlaneCollider : MonoBehaviour
{
    private ARPlane _arPlane;
    private MeshCollider _meshCollider;

    void Awake()
    {
        _arPlane = GetComponent<ARPlane>();
        _meshCollider = GetComponent<MeshCollider>();

        // Set layer based on plane type
        gameObject.layer = _arPlane.alignment.IsVertical() ?
            LayerMask.NameToLayer("ARPlane_Vertical") :
            LayerMask.NameToLayer("ARPlane_Horizontal");
    }

    void Update()
    {
        if (_arPlane.subsumedBy == null) // Only update for active planes
        {
            if(_arPlane.classifications == PlaneClassifications.WallFace || _arPlane.classifications == PlaneClassifications.Floor || _arPlane.classifications == PlaneClassifications.Ceiling || _arPlane.classifications == PlaneClassifications.DoorFrame || _arPlane.classifications == PlaneClassifications.WindowFrame)
            _meshCollider.sharedMesh = _arPlane.GetComponent<ARPlaneMeshVisualizer>().mesh;
        }
    }
}