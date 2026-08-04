using System;
using UnityEngine;

[Serializable]
public class FavoriteItem
{
    public string productId;
    public string productName;
    public string slug;
    public string colorName;
    public string colorCode;
    public string imageUrl;
    [NonSerialized] public Sprite productImage;
}
