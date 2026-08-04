using System;
using UnityEngine;

[Serializable]
public class CartItem
{
    public string productId;
    public string variantId;
    public string productName;
    public string colorName;
    public string colorCode;
    public string sizeName;
    public int colorIndex;
    public int sizeIndex;
    public int quantity;
    public float price;
    public float salePrice;
    public int maxStock;
    public string imageUrl;
    [NonSerialized] public Sprite productImage;

    public float EffectivePrice => salePrice > 0 && salePrice < price ? salePrice : price;
    public float LineTotal => EffectivePrice * quantity;
}
