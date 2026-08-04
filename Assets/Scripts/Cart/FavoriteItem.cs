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
}
