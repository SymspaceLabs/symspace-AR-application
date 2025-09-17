using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SubCategoryManager : MonoBehaviour
{
    public Transform productContainer;
    public GameObject productCardPrefab;

    [System.Serializable]
    public class Product
    {
        public string itemName;
        public string itemType;
        public string price;
        public Sprite image;
    }

    public List<Product> products;

    public void LoadProducts()
    {
        foreach (Transform child in productContainer)
            Destroy(child.gameObject);

        foreach (Product p in products)
        {
            GameObject card = Instantiate(productCardPrefab, productContainer);
            card.transform.Find("ItemName").GetComponent<Text>().text = p.itemName;
            card.transform.Find("ItemType").GetComponent<Text>().text = p.itemType;
            card.transform.Find("Price").GetComponent<Text>().text = p.price;
            card.transform.Find("ProductImage").GetComponent<Image>().sprite = p.image;
        }
    }
}
