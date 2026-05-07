using UnityEngine;

public static class ClothingFitController
{
    static SkinnedMeshRenderer meshRenderer;

    // Blendshape indices
    static int increaseKeyIndex = 0; // Bigger (L, XL)
    static int decreaseKeyIndex = 1; // Smaller (S, XS)

    // Example product size chart (chest in inches)
    static float chest_S = 36f;
    static float chest_M = 40f;
    static float chest_L = 44f;
    static float chest_XL = 48f;

    // Call this with user chest measurement
    public static void ApplyFit(float userChest, SkinnedMeshRenderer renderer)
    {
        meshRenderer = renderer;
        float sizeValue = GetSizeValue(userChest);
        ApplyBlendshape(sizeValue);
    }

    public static void ApplySizeFromLabel(string size, SkinnedMeshRenderer renderer)
    {
        meshRenderer = renderer;
        float value = 0f;

        switch (size.ToUpper())
        {
            case "XS": value = -1f; break;
            case "S": value = -0.5f; break;
            case "M": value = 0f; break;
            case "L": value = 0.5f; break;
            case "XL": value = 1f; break;
            default:
                Debug.LogWarning("Unknown size: " + size);
                return;
        }

        ApplyBlendshape(value);
    }

    // Convert measurement → size range (-1 to +1)
    static float GetSizeValue(float userChest)
    {
        if (userChest <= chest_S)
            return -1f; // Small
        else if (userChest <= chest_M)
            return Mathf.Lerp(-1f, 0f, InverseLerp(chest_S, chest_M, userChest));
        else if (userChest <= chest_L)
            return Mathf.Lerp(0f, 0.5f, InverseLerp(chest_M, chest_L, userChest));
        else if (userChest <= chest_XL)
            return Mathf.Lerp(0.5f, 1f, InverseLerp(chest_L, chest_XL, userChest));
        else
            return 1f; // Bigger than XL
    }

    // Apply to blendshapes
    static void ApplyBlendshape(float value)
    {
        if(meshRenderer == null)
        {
            Debug.LogWarning("MeshRenderer not assigned.");
            return;
        }

        value = Mathf.Clamp(value, -1f, 1f);

        float scaleFactor = 1f; // adjust this based on your model

        float increaseWeight = Mathf.Max(0f, value) * scaleFactor;
        float decreaseWeight = Mathf.Max(0f, -value) * scaleFactor;

        meshRenderer.SetBlendShapeWeight(increaseKeyIndex, increaseWeight);
        meshRenderer.SetBlendShapeWeight(decreaseKeyIndex, decreaseWeight);
    }

    // Custom inverse lerp (since Unity's is clamped)
    static float InverseLerp(float a, float b, float value)
    {
        if (a != b)
            return Mathf.Clamp01((value - a) / (b - a));
        return 0f;
    }
}