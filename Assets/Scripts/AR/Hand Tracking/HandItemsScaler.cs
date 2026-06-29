using UnityEngine;

public static class HandItemsScaler
{
    public static float ringTargetInnerDiameter = 0.018f;
    public static float watchTargetWidth = 0.04f; // ~4 cm typical watch size
    public static float scaleFactor;
    public static float innerDiameter;
    public static float outerDiameter;
    public static float thickness;

    [ContextMenu("Scale Ring From Bounds")]
    public static float RingScaleFromBounds(Renderer ringRenderer)
    {
        if (ringRenderer == null)
        {
            Debug.LogError("Ring Renderer missing!");
            return 1;
        }

        Bounds b = ringRenderer.bounds;

        // Get all 3 axes
        float x = b.size.x;
        float y = b.size.y;
        float z = b.size.z;

        // Sort axes to find correct dimensions
        float[] axes = new float[] { x, y, z };
        System.Array.Sort(axes);

        // axes[2] = largest → outer diameter
        // axes[0] = smallest → thickness
        outerDiameter = axes[2];
        thickness = axes[0];

        // Calculate inner diameter
        innerDiameter = outerDiameter - (2f * thickness);

        if (innerDiameter <= 0f)
        {
            Debug.LogError("Invalid ring dimensions!");
            return 1;
        }

        // Compute scale factor
        scaleFactor = ringTargetInnerDiameter / innerDiameter;

        Debug.Log($"Outer: {outerDiameter}");
        Debug.Log($"Thickness: {thickness}");
        Debug.Log($"Inner: {innerDiameter}");
        Debug.Log($"Scale Factor: {scaleFactor}");

        return scaleFactor;
    }

    public static float WatchScaleFromBounds(Renderer watchRenderer)
    {
        if (watchRenderer == null)
        {
            Debug.LogError("Watch Renderer missing!");
            return 1;
        }

        Bounds b = watchRenderer.bounds;

        float x = b.size.x;
        float y = b.size.y;
        float z = b.size.z;

        float[] axes = new float[] { x, y, z };
        System.Array.Sort(axes);

        // For watches:
        // axes[2] = largest → likely strap length
        // axes[1] = middle → watch face width (what we want)
        float watchWidth = axes[1];

        if (watchWidth <= 0f)
        {
            Debug.LogError("Invalid watch dimensions!");
            return 1;
        }

        scaleFactor = watchTargetWidth / watchWidth;

        Debug.Log($"Watch Width: {watchWidth}");
        Debug.Log($"Scale Factor: {scaleFactor}");

        return scaleFactor;
    }
}