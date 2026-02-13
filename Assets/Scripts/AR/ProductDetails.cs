using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class ProductDetails : MonoBehaviour
{
    public List<string> imagesUrl  = new List<string>();
    public List<string> texturesUrl  = new List<string>();
    public List<Sprite> sprites = new List<Sprite>();
    public CategoryManager.Products product;

    public int selectedColorIndex = 0;

    public List<Color> colors = new List<Color>();

    public List<Texture2D> textures;

    public bool isSizeSelected = false;
}
