using UnityEngine;
using System.Collections.Generic;

public static class PlaneFitting
{
    public static Plane FitPlaneToPoints(Vector3[] points)
    {
        if (points.Length < 3)
        {
            return new Plane(Vector3.up, Vector3.zero);
        }

        // Calculate centroid
        Vector3 centroid = Vector3.zero;
        foreach (Vector3 point in points)
        {
            centroid += point;
        }
        centroid /= points.Length;

        // Calculate covariance matrix
        float xx = 0, xy = 0, xz = 0, yy = 0, yz = 0, zz = 0;

        foreach (Vector3 point in points)
        {
            Vector3 diff = point - centroid;
            xx += diff.x * diff.x;
            xy += diff.x * diff.y;
            xz += diff.x * diff.z;
            yy += diff.y * diff.y;
            yz += diff.y * diff.z;
            zz += diff.z * diff.z;
        }

        // Find principal component (smallest eigenvector = normal)
        Matrix4x4 covariance = new Matrix4x4(
            new Vector4(xx, xy, xz, 0),
            new Vector4(xy, yy, yz, 0),
            new Vector4(xz, yz, zz, 0),
            new Vector4(0, 0, 0, 0)
        );

        // Simple approximation for normal (more robust than full eigen decomposition)
        Vector3 normal = ApproximateNormal(covariance);

        return new Plane(normal, centroid);
    }

    private static Vector3 ApproximateNormal(Matrix4x4 covariance)
    {
        // Use cross products of principal directions as approximation
        Vector3 row1 = new Vector3(covariance.m00, covariance.m01, covariance.m02);
        Vector3 row2 = new Vector3(covariance.m10, covariance.m11, covariance.m12);
        Vector3 row3 = new Vector3(covariance.m20, covariance.m21, covariance.m22);

        // Cross products give normal direction approximation
        Vector3 normal1 = Vector3.Cross(row1, row2).normalized;
        Vector3 normal2 = Vector3.Cross(row2, row3).normalized;
        Vector3 normal3 = Vector3.Cross(row3, row1).normalized;

        // Average the normals
        return (normal1 + normal2 + normal3).normalized;
    }
}