using UnityEngine;

public class SafeParentMatrix : MonoBehaviour
{
    public Transform child;  // Cube1
    public Transform parent; // Cube2

    [ContextMenu("Reparent Safely (Matrix)")]
    public void ReparentSafely()
    {
        if (child == null || parent == null)
        {
            Debug.LogWarning("Assign both child and parent!");
            return;
        }

        // 1. Store child's world matrix
        Matrix4x4 childWorldMatrix = child.localToWorldMatrix;

        // 2. Parent the child
        child.SetParent(parent, worldPositionStays: false); // we handle world matrix manually

        // 3. Compute local matrix relative to new parent
        Matrix4x4 parentWorldToLocal = parent.worldToLocalMatrix;
        Matrix4x4 childLocalMatrix = parentWorldToLocal * childWorldMatrix;

        // 4. Extract position, rotation, and scale from matrix
        child.localPosition = childLocalMatrix.GetColumn(3);
        child.localRotation = Quaternion.LookRotation(
            childLocalMatrix.GetColumn(2),
            childLocalMatrix.GetColumn(1)
        );
        child.localScale = new Vector3(
            childLocalMatrix.GetColumn(0).magnitude,
            childLocalMatrix.GetColumn(1).magnitude,
            childLocalMatrix.GetColumn(2).magnitude
        );

        Debug.Log($"Child {child.name} safely reparented under {parent.name} with matrix.");
    }

    [ContextMenu("Unparent Safely (Matrix)")]
    public void UnparentSafely()
    {
        if (child == null)
        {
            Debug.LogWarning("Assign a child!");
            return;
        }

        // Store world matrix
        Matrix4x4 childWorldMatrix = child.localToWorldMatrix;

        // Remove parent
        child.SetParent(null, worldPositionStays: false);

        // Set local transform from world matrix (no parent now)
        child.localPosition = childWorldMatrix.GetColumn(3);
        child.localRotation = Quaternion.LookRotation(
            childWorldMatrix.GetColumn(2),
            childWorldMatrix.GetColumn(1)
        );
        child.localScale = new Vector3(
            childWorldMatrix.GetColumn(0).magnitude,
            childWorldMatrix.GetColumn(1).magnitude,
            childWorldMatrix.GetColumn(2).magnitude
        );

        Debug.Log($"Child {child.name} safely unparented using matrix.");
    }
}
