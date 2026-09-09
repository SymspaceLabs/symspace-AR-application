using System;
using UnityEngine;

[Serializable]
public class FavoriteItem
{
    public string productId;
    public string productName;
    public int originalPrice;
    public int salePrice;
    public string slug;
    public string colorName;
    public string colorCode;
    public string imageUrl;
    [NonSerialized] public Sprite productImage;

    public string variantId;
    public string sizeName;
    public int colorIndex;
    public int sizeIndex;
    public float price;
    public float salePriceFloat;
    public int maxStock;
    public bool hasVariant;
}
