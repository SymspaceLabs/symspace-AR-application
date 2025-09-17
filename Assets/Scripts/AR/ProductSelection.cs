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
    public static string ProductName { get; private set; }
    public static bool IsForFace { get; private set; }
    public static CategoryType SelectedObjectType { get; private set; }
    public static bool isHorizontalPlane = false;
    //public static PlaneType SelectedPlaneType { get; private set; }

    // Call this method when the user selects a product
    public static void SetSelection(string productName, bool isForFace, string objectType = null, bool isHorizontal = false)
    {
        ProductName = productName;
        IsForFace = isForFace;

        TryParseObjectType(objectType, out CategoryType SelectedObjectType);

        isHorizontalPlane = isHorizontal;

        //TryParsePlaneType(planeType, out PlaneType SelectedPlanType);
    }

    public static void ClearSelection()
    {
        ProductName = null;
        IsForFace = false;
        SelectedObjectType = default;
        //SelectedPlaneType = PlaneType.None;
    }

    public static bool TryParseObjectType(string value, out CategoryType objectType)
    {
        return Enum.TryParse(value, ignoreCase: true, out objectType) && Enum.IsDefined(typeof(CategoryType), objectType);
    }

    public static bool TryParsePlaneType(string value, out PlaneType planeType)
    {
        return Enum.TryParse(value, ignoreCase: true, out planeType) && Enum.IsDefined(typeof(PlaneType), planeType);
    }
}
