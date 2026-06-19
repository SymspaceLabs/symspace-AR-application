using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralDashedRing : MonoBehaviour
{
    [Header("Ring Dimensions")]
    public float outerRadius = 1.0f;
    public float innerRadius = 0.85f;

    [Header("Dash Settings")]
    public int totalSegments = 60; // Smoothness of the overall circle
    [Range(0f, 1f)] public float dashRatio = 0.2f; // Length of dash vs space (0.5 = equal dash/space)

    void Start()
    {
        Generate3DDiskMesh();
    }

    void Generate3DDiskMesh()
    {
        Mesh filterMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = filterMesh;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        float angleStep = 360f / totalSegments;
        int vertexIndex = 0;

        for (int i = 0; i < totalSegments; i++)
        {
            // Only generate the geometry for the "dash" part, skip the gap part
            float startAngle = i * angleStep;
            float endAngle = startAngle + (angleStep * dashRatio);

            // Convert angles to radians
            float radStart = startAngle * Mathf.Deg2Rad;
            float radEnd = endAngle * Mathf.Deg2Rad;

            // Define the 4 3D corner vertices for this single dash segment
            Vector3 outerStart = new Vector3(Mathf.Cos(radStart) * outerRadius, 0, Mathf.Sin(radStart) * outerRadius);
            Vector3 innerStart = new Vector3(Mathf.Cos(radStart) * innerRadius, 0, Mathf.Sin(radStart) * innerRadius);
            Vector3 outerEnd = new Vector3(Mathf.Cos(radEnd) * outerRadius, 0, Mathf.Sin(radEnd) * outerRadius);
            Vector3 innerEnd = new Vector3(Mathf.Cos(radEnd) * innerRadius, 0, Mathf.Sin(radEnd) * innerRadius);

            // Add vertices to the mesh geometry list
            vertices.Add(outerStart); // index + 0
            vertices.Add(innerStart); // index + 1
            vertices.Add(outerEnd);   // index + 2
            vertices.Add(innerEnd);   // index + 3

            // First Triangle for this dash segment (Clockwise face rendering)
            triangles.Add(vertexIndex + 0);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 1);

            // Second Triangle for this dash segment
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 3);

            vertexIndex += 4;
        }

        // Apply generated math arrays to the actual 3D engine mesh components
        filterMesh.vertices = vertices.ToArray();
        filterMesh.triangles = triangles.ToArray();
        filterMesh.RecalculateNormals();
        filterMesh.RecalculateBounds();
    }
}