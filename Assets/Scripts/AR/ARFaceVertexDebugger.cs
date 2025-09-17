using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARFaceVertexDebugger : MonoBehaviour
{
    public ARFaceManager faceManager;

    private ARFace trackedFace;
    private bool logged = false;

    void Update()
    {
        // Assign tracked face once
        if (trackedFace == null && faceManager.trackables.count > 0)
        {
            foreach (var face in faceManager.trackables)
            {
                trackedFace = face;
                break;
            }
        }

        if (trackedFace == null || logged)
            return;

        var meshFilter = trackedFace.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.mesh == null)
            return;

        Vector3[] vertices = meshFilter.mesh.vertices;
        if (vertices.Length == 0) return;

        // Find extreme points
        int leftEar = -1;
        int rightEar = -1;
        int chin = -1;
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 localPos = vertices[i];

            if (localPos.x < minX)
            {
                minX = localPos.x;
                leftEar = i;
            }

            if (localPos.x > maxX)
            {
                maxX = localPos.x;
                rightEar = i;
            }

            if (localPos.y < minY)
            {
                minY = localPos.y;
                chin = i;
            }
        }

        // Defensive check
        if (leftEar >= 0 && leftEar < vertices.Length)
        {
            Vector3 worldPos = trackedFace.transform.TransformPoint(vertices[leftEar]);
            Debug.Log($"🟢 Left Ear Index: {leftEar}, World Pos: {worldPos}");
        }

        if (rightEar >= 0 && rightEar < vertices.Length)
        {
            Vector3 worldPos = trackedFace.transform.TransformPoint(vertices[rightEar]);
            Debug.Log($"🔵 Right Ear Index: {rightEar}, World Pos: {worldPos}");
        }

        if (chin >= 0 && chin < vertices.Length)
        {
            Vector3 worldPos = trackedFace.transform.TransformPoint(vertices[chin]);
            Debug.Log($"🔻 Chin Index: {chin}, World Pos: {worldPos}");
        }

        logged = true; // Only log once
    }
}
