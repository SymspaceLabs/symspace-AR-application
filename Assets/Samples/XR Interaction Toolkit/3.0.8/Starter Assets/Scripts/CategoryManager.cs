using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using UnityEngine.SceneManagement;

public class CategoryManager : MonoBehaviour
{
    public List<MainCategory> mainCategories;
    public List<Image> mainCategoriesImages;

    public Transform subcategoryButtonContainer; // e.g., HorizontalLayoutGroup
    public Button subcategoryButtonPrefab;
    public List<Image> subCategoriesImages;

    public Transform productContainer; // Scroll view content
    public GameObject productCardPrefab;

    // UI colors
    public Color selectedBgColor;
    public Color unselectedBgColor;

    private MainCategory currentCategory;

    public ObjectSpawner spawner;

    public ARJewelryManager arJewelryManager;

    public GameObject[] prefabs;

    private void Start()
    {
        if (ProductSelection.ProductName != null)
        {
            foreach(MainCategory mainCat in mainCategories)
            {
                foreach(Subcategory subCat in mainCat.subcategories)
                {
                    foreach(var p in subCat.products)
                    {
                        if(p.itemName == ProductSelection.ProductName)
                        {
                            OnMainCategorySelected(mainCat.name);
                            OnSubcategorySelected(subCat);
                            OnProductSelected(p);
                        }
                    }
                }
            }
        }
    }

    public void SelectedImage(Image img)
    {
        img.color = selectedBgColor;
    }

    public void OnMainCategorySelected(string categoryName)
    {
        foreach (var image in mainCategoriesImages)
        {
            image.color = unselectedBgColor;
        }

        currentCategory = mainCategories.Find(c => c.name == categoryName);
        
        // Clear old subcategory buttons
        foreach (Transform child in subcategoryButtonContainer)
        {
            child.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(child.gameObject);
        }

        foreach (Transform child in productContainer)
            Destroy(child.gameObject);

        if (currentCategory == null) return;


        subCategoriesImages.Clear();

        // Create new subcategory buttons
        foreach (Subcategory sub in currentCategory.subcategories)
        {
            Button btn = Instantiate(subcategoryButtonPrefab, subcategoryButtonContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = sub.name;
            btn.onClick.AddListener(() => OnSubcategorySelected(sub));
            btn.onClick.AddListener(() => SelectedImage(btn.GetComponent<Image>()));
            subCategoriesImages.Add(btn.GetComponent<Image>());
        }

        // Auto-load first subcategory
        if (currentCategory.subcategories.Count > 0)
        {
            OnSubcategorySelected(currentCategory.subcategories[0]);
            subCategoriesImages[0].color = selectedBgColor;
        }
    }


    public void OnSubcategorySelected(Subcategory subcategory)
    {
        // Clear old products
        foreach (Transform child in productContainer)
            Destroy(child.gameObject);

        foreach(var img in subCategoriesImages)
        {
            img.color = unselectedBgColor;
        }

        // Load new products
        foreach (Product p in subcategory.products)
        {
            GameObject card = Instantiate(productCardPrefab, productContainer);
            card.transform.Find("Border/ItemName").GetComponent<TextMeshProUGUI>().text = p.itemName;
            card.transform.Find("Border/ItemType").GetComponent<TextMeshProUGUI>().text = p.itemType;
            if (p.discountPrice.Length > 0)
            {
                card.transform.Find("Border/Price").GetComponent<TextMeshProUGUI>().text = "<s>" + p.price + "</s>";
                card.transform.Find("Border/Price").GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.7f, 0.7f);
            }
            else
            {
                card.transform.Find("Border/Price").GetComponent<TextMeshProUGUI>().text = p.price;
                card.transform.Find("Border/Price").GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1);
            }

            card.transform.Find("Border/DiscountPrice").GetComponent<TextMeshProUGUI>().text = p.discountPrice;
            card.transform.Find("Border/ProductImage").GetComponent<Image>().sprite = p.image;

            card.GetComponent<Button>().onClick.AddListener(() => OnProductSelected(p));
        }
    }

    public void OnProductSelected(Product p)
    {
        //var visual = spawner.objectPrefabs[0].transform.Find("Visual");
        //visual.GetComponent<MeshFilter>().mesh = 

        if (p.isFaceObject)
        {
            if (SceneManager.GetActiveScene().name != "AR Face")
            {
                ProductSelection.ClearSelection();
                ProductSelection.SetSelection(p.itemName, true, p.categoryType);
                UIManagerAR.instance.ChangeARScene("AR Face");
            }
            else
            {
                arJewelryManager.JewelrySelected(p.itemName, p.categoryType);
                GetComponent<SlideUpPanel>().HidePanel();
            }
            //SceneManager.LoadScene("AR Face");
        }
        else
        {

            if (SceneManager.GetActiveScene().name != "AR Scene")
            {
                ProductSelection.ClearSelection();
                ProductSelection.SetSelection(p.itemName, false, null, p.horizontal);
                UIManagerAR.instance.ChangeARScene("AR Scene");
            }
            else
            {
                for (int i = 0; i < prefabs.Length; i++)
                {
                    if (prefabs[i].name == p.itemName)
                    {
                        //if (spawner.transform.childCount > 0)
                        //    Destroy(spawner.transform.GetChild(0).gameObject);

                        spawner.objectPrefabs.Insert(0, prefabs[i]);
                        spawner.object1Spawned = false;
                        spawner.objectIndex = 0;
                        spawner.objectsSize[0].width = p.width;
                        spawner.objectsSize[0].depth = p.depth;
                        spawner.objectsSize[0].height = p.height;
                        spawner.unit = p.unit;
                        UIManagerAR.instance.itemsToPlaceParent.SetActive(true);
                        UIManagerAR.instance.item1.sprite = p.image;
                        UIManagerAR.instance.item2.sprite = p.image;
                        UIManagerAR.instance.item3.sprite = p.image;
                        UIManagerAR.instance.TogglePlaneVisuals(true);
                        GetComponent<SlideUpPanel>().HidePanel();
                    }

                }
            }
        }
    }


    [System.Serializable]
    public class Product
    {
        public string itemName;
        public string itemType;
        public string price;
        public string discountPrice;
        public Sprite image;

        public float width;
        public float depth;
        public float height;

        public List<Texture> texture;

        public string unit;

        public bool horizontal;
        public bool isFaceObject = false;

        public string categoryType; 
    }

    [System.Serializable]
    public class Subcategory
    {
        public string name;
        public List<Product> products;
    }

    [System.Serializable]
    public class MainCategory
    {
        public string name;
        public List<Subcategory> subcategories;
    }
}
