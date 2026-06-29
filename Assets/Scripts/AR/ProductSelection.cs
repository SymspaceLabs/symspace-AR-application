using System;
using UnityEngine;

public enum PlaneType
{
    Horizontal,
    Vertical,
    None // For face objects
}

// This static class holds the selected product info between scenes
public static class ProductSelection
{
    public static CategoryManager.Products productData { get; private set; }
    public static bool IsForFace { get; private set; }
    public static CategoryType SelectedObjectType { get; set; }
    public static bool isHorizontalPlane = false;

    public static string modelURL;

    public static Sprite fetchedSprite;

    public static string categoryName;

    //public static PlaneType SelectedPlaneType { get; private set; }

    // Call this method when the user selects a product
    public static void SetSelection(CategoryManager.Products productName, bool isForFace, string objectType = "", bool isHorizontal = false, string url = "")
    {
        modelURL = url;
        productData = productName;
        IsForFace = isForFace;
        if (objectType.Length > 0)
        {
            CategoryType parsed;
            if (TryParseObjectType(objectType, out parsed))
                SelectedObjectType = parsed;
        }

        isHorizontalPlane = isHorizontal;

        //TryParsePlaneType(planeType, out PlaneType SelectedPlanType);
    }

    public static void ClearSelection()
    {
        productData = null;
        IsForFace = false;
        SelectedObjectType = default;
        //SelectedPlaneType = PlaneType.None;
    }

    public static bool TryParseObjectType(string value, out CategoryType objectType)
    {
        objectType = default;

        if (string.IsNullOrEmpty(value))
            return false;

        foreach (CategoryType type in Enum.GetValues(typeof(CategoryType)))
        {
            if (value.IndexOf(type.ToString(), StringComparison.OrdinalIgnoreCase) >= 0)
            {
                objectType = type;
                return true;
            }
        }

        return false;
    }

    public static bool TryParsePlaneType(string value, out PlaneType planeType)
    {
        return Enum.TryParse(value, ignoreCase: true, out planeType) && Enum.IsDefined(typeof(PlaneType), planeType);
    }
}
