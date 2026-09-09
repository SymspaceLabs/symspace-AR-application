using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CategoryFilterUI : MonoBehaviour
{
    public static CategoryFilterUI Instance { get; private set; }

    [Header("References (optional, auto-found if empty)")]
    public CategoriesUI categoriesUI;
    public CategoryManager categoryManager;
    public Transform categoriesContent;
    public Button filterToggleButton;
    public Transform brandsContent;
    public Transform colorsContent;
    public PriceRangeSlider priceRangeSlider;
    public CategoryRow inStockRow;
    public CategoryRow soldOutRow;

    [Header("Prefab")]
    public CategoryRow rowPrefab;
    public GameObject brandRowPrefab;
    public GameObject colorRowPrefab;

    [Header("Layout")]
    public int parentIndent = 0;
    public int childIndent = 35;

    [Header("Label Colors")]
    public Color selectedLabelColor = Color.black;
    public Color unselectedLabelColor = new Color(0.55f, 0.55f, 0.55f, 1f);
    public Color selectedColorLabelColor = new Color(0, 0.55f, 1, 1f);

    [Header("Filter Page")]
    public GameObject filterPage;
    public Button showButton;
    public Button resetButton;
    public Button crossButton;
    public TextMeshProUGUI showCountText;
    public SizeFilterUI sizeFilter;

    private bool useAR;
    private CategoryRow arRowPrefab;
    private GameObject arBrandPrefab;
    private GameObject arColorPrefab;

    private CategoryRow ActiveRowPrefab => useAR && arRowPrefab != null ? arRowPrefab : rowPrefab;
    private GameObject ActiveBrandPrefab => useAR && arBrandPrefab != null ? arBrandPrefab : brandRowPrefab;
    private GameObject ActiveColorPrefab => useAR && arColorPrefab != null ? arColorPrefab : colorRowPrefab;

    private CategoryManager.ProductResponse ActiveProductsData
    {
        get
        {
            if (categoriesUI != null) return categoriesUI.allProductsData;
            if (categoryManager != null) return categoryManager.allProductsData;
            return null;
        }
    }

    private readonly HashSet<string> selectedIds = new HashSet<string>();
    private readonly HashSet<string> selectedBrandIds = new HashSet<string>();
    private readonly HashSet<string> selectedColorCodes = new HashSet<string>();
    private readonly List<BrandRowEntry> brandRowEntries = new List<BrandRowEntry>();

    private CategoryManager.ProductResponse currentResponse;
    private int lastFilteredCount;
    private int lastDisplayedCount;
    private bool controlsWired;
    private bool availabilityWired;
    private bool filterInStock;
    private bool filterSoldOut;
    private Coroutine countRollRoutine;
    private System.Action filterBackAction;

    private class BrandRowEntry
    {
        public string brandId;
        public BrandRow row;
    }

    private class ChildRowEntry
    {
        public CategoryManager.SubcategoryChild data;
        public CategoryRow row;
    }

    #region Lifecycle

    private void Awake()
    {
        Instance = this;
        FindReferences();
        LoadPrefabs();
    }

    private void OnEnable()
    {
        if (priceRangeSlider != null)
            priceRangeSlider.OnRangeChanged += ApplyFilters;
        WireFilterControls();
    }

    private void OnDisable()
    {
        if (priceRangeSlider != null)
            priceRangeSlider.OnRangeChanged -= ApplyFilters;
        PopFilterBack();
    }

    private void Start()
    {
        StartCoroutine(WaitForDataAndBuild());
    }

    private IEnumerator WaitForDataAndBuild()
    {
        while (ActiveProductsData == null ||
               ActiveProductsData.category == null ||
               ActiveProductsData.category.Count == 0)
        {
            TryFindDataSources();
            yield return new WaitForSeconds(0.25f);
        }
        Build();
    }

    #endregion

    #region Setup

    private void FindReferences()
    {
        if (categoriesUI == null)
            categoriesUI = CategoriesUI.Instance;
        if (categoriesUI == null)
            categoriesUI = FindObjectOfType<CategoriesUI>();
        if (categoriesUI == null && categoryManager == null)
            categoryManager = FindObjectOfType<CategoryManager>();

        if (filterPage == null)
            filterPage = FindFilterPage();

        AutoFindScrollContents();
        if (priceRangeSlider == null)
            priceRangeSlider = FindObjectOfType<PriceRangeSlider>();
    }

    private void AutoFindScrollContents()
    {
        if (categoriesContent != null && brandsContent != null && colorsContent != null)
            return;

        foreach (Transform t in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (t.gameObject.scene != gameObject.scene) continue;

            if (categoriesContent == null && t.name == "Categories Scroll")
            {
                ScrollRect sr = t.GetComponent<ScrollRect>();
                if (sr != null) categoriesContent = sr.content;
            }
            else if (brandsContent == null && t.name == "Brands Scroll")
            {
                ScrollRect sr = t.GetComponent<ScrollRect>();
                if (sr != null) brandsContent = sr.content;
            }
            else if (colorsContent == null && t.name == "Colors Scroll")
            {
                ScrollRect sr = t.GetComponent<ScrollRect>();
                if (sr != null) colorsContent = sr.content;
            }
        }
    }

    private void LoadPrefabs()
    {
        if (rowPrefab == null)
            rowPrefab = Resources.Load<CategoryRow>("UI/CategoryRow");
        if (brandRowPrefab == null)
            brandRowPrefab = Resources.Load<GameObject>("UI/Brand Row");
        if (colorRowPrefab == null)
            colorRowPrefab = Resources.Load<GameObject>("UI/Color Filter Prefab");

        arRowPrefab = null;
        arBrandPrefab = null;
        arColorPrefab = null;
        useAR = false;

        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool isAR = scene.Contains("AR") || scene == "Hand Tracking";

        if (isAR)
        {
            arRowPrefab = Resources.Load<CategoryRow>("UI/CategoryRow (AR)");
            arBrandPrefab = Resources.Load<GameObject>("UI/Brand Row (AR)");
            arColorPrefab = Resources.Load<GameObject>("UI/Color Filter Prefab (AR)");
            useAR = arRowPrefab != null || arBrandPrefab != null || arColorPrefab != null;

            selectedLabelColor = Color.white;
            unselectedLabelColor = new Color(1f, 1f, 1f, 0.6f);
            selectedColorLabelColor = new Color(0.5f, 0.8f, 1f, 1f);
        }
    }

    private void TryFindDataSources()
    {
        if (categoriesUI == null)
            categoriesUI = CategoriesUI.Instance;
        if (categoriesUI == null)
            categoriesUI = FindObjectOfType<CategoriesUI>();
        if (categoriesUI == null && categoryManager == null)
            categoryManager = FindObjectOfType<CategoryManager>();
    }

    private GameObject FindFilterPage()
    {
        Transform t = transform;
        while (t != null)
        {
            if (t.name == "Filter Page")
                return t.gameObject;
            t = t.parent;
        }

        foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.name == "Filter Page" && go.scene == gameObject.scene)
                return go;
        }
        return null;
    }

    #endregion

    #region Build

    public void Build()
    {
        if (ActiveProductsData == null)
            return;

        currentResponse = ActiveProductsData;
        lastFilteredCount = currentResponse.products != null ? currentResponse.products.Count : 0;
        lastDisplayedCount = lastFilteredCount;
        RefreshShowButtonText();
        WireAvailabilityRows();

        if (priceRangeSlider != null && currentResponse.priceRange != null)
        {
            priceRangeSlider.minLimit = currentResponse.priceRange.min;
            priceRangeSlider.maxLimit = currentResponse.priceRange.max;
            priceRangeSlider.SetValues(currentResponse.priceRange.min, currentResponse.priceRange.max);
        }

        ClearContent();
        if (currentResponse.category == null)
            return;

        foreach (CategoryManager.CategoryWithChild category in currentResponse.category)
            CreateParentRow(category);

        CreateBrandRows();
        CreateColorRows();
    }

    #endregion

    #region Category Rows

    private void CreateParentRow(CategoryManager.CategoryWithChild category)
    {
        CategoryRow row = InstantiateRow(category.title, parentIndent, true);
        if (row == null || row.checkboxButton == null)
            return;

        row.SetLabelBold(true);
        row.SetLabelColor(selectedLabelColor);

        List<ChildRowEntry> children = new List<ChildRowEntry>();

        row.checkboxButton.onClick.AddListener(() =>
        {
            if (category.child == null) return;

            bool allSelected = category.child.All(c => selectedIds.Contains(c.id));

            if (allSelected)
                category.child.ForEach(c => selectedIds.Remove(c.id));
            else
                category.child.ForEach(c => selectedIds.Add(c.id));

            RefreshChildren(children);
            RefreshRowState(row, category.child);
            ApplyFilters();
        });

        RefreshRowState(row, category.child);

        if (category.child == null) return;

        foreach (CategoryManager.SubcategoryChild child in category.child)
        {
            CategoryRow childRow = CreateChildRow(child, row, category.child);
            if (childRow != null)
                children.Add(new ChildRowEntry { data = child, row = childRow });
        }
    }

    private CategoryRow CreateChildRow(
        CategoryManager.SubcategoryChild child,
        CategoryRow parentRow,
        List<CategoryManager.SubcategoryChild> siblings)
    {
        CategoryRow row = InstantiateRow(child.name, childIndent, true);
        if (row == null || row.checkboxButton == null)
            return null;

        row.SetLabelBold(false);
        SetRowSelected(row, selectedIds.Contains(child.id));

        row.checkboxButton.onClick.AddListener(() =>
        {
            ToggleId(selectedIds, child.id);
            SetRowSelected(row, selectedIds.Contains(child.id));
            RefreshRowState(parentRow, siblings);
            ApplyFilters();
        });

        return row;
    }

    private void RefreshRowState(CategoryRow row, List<CategoryManager.SubcategoryChild> children)
    {
        if (children == null || children.Count == 0)
        {
            row.SetState(false, false);
            return;
        }

        int selected = children.Count(c => selectedIds.Contains(c.id));
        row.SetState(selected == children.Count, selected > 0 && selected < children.Count);
    }

    private void RefreshChildren(List<ChildRowEntry> children)
    {
        foreach (ChildRowEntry entry in children)
        {
            bool selected = selectedIds.Contains(entry.data.id);
            entry.row.SetState(selected, false);
            entry.row.SetLabelColor(selected ? selectedLabelColor : unselectedLabelColor);
        }
    }

    #endregion

    #region Brand Rows

    private void CreateBrandRows()
    {
        if (currentResponse.brands == null) return;
        brandRowEntries.Clear();
        foreach (CategoryManager.Brand brand in currentResponse.brands)
            CreateBrandRow(brand);
    }

    private void CreateBrandRow(CategoryManager.Brand brand)
    {
        if (ActiveBrandPrefab == null || brandsContent == null) return;

        GameObject instance = Instantiate(ActiveBrandPrefab, brandsContent);
        instance.SetActive(true);

        BrandRow row = instance.GetComponent<BrandRow>();
        if (row == null) row = instance.AddComponent<BrandRow>();

        row.Setup(brand.entityName, CountProductsForBrand(brand.id));
        brandRowEntries.Add(new BrandRowEntry { brandId = brand.id, row = row });

        bool isSelected = selectedBrandIds.Contains(brand.id);
        row.SetState(isSelected, false);
        row.SetLabelColor(isSelected ? selectedLabelColor : unselectedLabelColor);

        if (row.checkboxButton == null) return;
        row.checkboxButton.onClick.AddListener(() =>
        {
            ToggleId(selectedBrandIds, brand.id);
            bool nowSelected = selectedBrandIds.Contains(brand.id);
            row.SetState(nowSelected, false);
            row.SetLabelColor(nowSelected ? selectedLabelColor : unselectedLabelColor);
            ApplyFilters();
        });
    }

    #endregion

    #region Color Rows

    private void CreateColorRows()
    {
        if (currentResponse.colors == null) return;
        foreach (CategoryManager.ColorInfo color in currentResponse.colors)
            CreateColorRow(color);
    }

    private void CreateColorRow(CategoryManager.ColorInfo color)
    {
        if (ActiveColorPrefab == null || colorsContent == null) return;

        GameObject instance = Instantiate(ActiveColorPrefab, colorsContent);
        instance.SetActive(true);

        ColorRow row = instance.GetComponent<ColorRow>();
        if (row == null) row = instance.AddComponent<ColorRow>();

        row.Setup(color.name, color.code);
        bool isSelected = IsColorSelected(color.code);
        row.SetState(isSelected, false);
        row.SetLabelColor(isSelected ? selectedColorLabelColor : unselectedLabelColor);

        if (row.checkboxButton == null) return;
        row.checkboxButton.onClick.AddListener(() =>
        {
            string normalized = NormalizeColorCode(color.code);
            ToggleId(selectedColorCodes, normalized);
            bool nowSelected = selectedColorCodes.Contains(normalized);
            row.SetState(nowSelected, false);
            row.SetLabelColor(nowSelected ? selectedColorLabelColor : unselectedLabelColor);
            ApplyFilters();
        });
    }

    #endregion

    #region Availability

    private void WireAvailabilityRows()
    {
        if (availabilityWired) return;

        if (inStockRow == null || soldOutRow == null)
            FindAvailabilityRows();

        if (inStockRow != null) WireAvailabilityRow(inStockRow, true);
        if (soldOutRow != null) WireAvailabilityRow(soldOutRow, false);
        availabilityWired = (inStockRow != null || soldOutRow != null);
    }

    private void FindAvailabilityRows()
    {
        foreach (CategoryRow row in FindObjectsOfType<CategoryRow>(true))
        {
            if (row == null || row.label == null || row.label.text == null) continue;
            if (IsInsideFilterContent(row.transform)) continue;

            string text = row.label.text.ToLowerInvariant();

            if (inStockRow == null && text.Contains("in stock"))
                inStockRow = row;
            else if (soldOutRow == null && text.Contains("sold out"))
                soldOutRow = row;

            if (inStockRow != null && soldOutRow != null) break;
        }
    }

    private bool IsInsideFilterContent(Transform t)
    {
        return (categoriesContent != null && t.IsChildOf(categoriesContent)) ||
               (brandsContent != null && t.IsChildOf(brandsContent)) ||
               (colorsContent != null && t.IsChildOf(colorsContent));
    }

    private void WireAvailabilityRow(CategoryRow row, bool isInStock)
    {
        if (row == null || row.checkboxButton == null) return;

        UpdateAvailabilityVisual(row, isInStock);
        row.checkboxButton.onClick.AddListener(() =>
        {
            if (isInStock) filterInStock = !filterInStock;
            else filterSoldOut = !filterSoldOut;
            UpdateAvailabilityVisual(row, isInStock);
            ApplyFilters();
        });
    }

    private void UpdateAvailabilityVisual(CategoryRow row, bool isInStock)
    {
        if (row == null) return;
        bool selected = isInStock ? filterInStock : filterSoldOut;
        row.SetState(selected, false);
        row.SetLabelColor(selected ? selectedLabelColor : unselectedLabelColor);
    }

    #endregion

    #region Filtering

    public void ApplyFilters()
    {
        if (ActiveProductsData == null || currentResponse?.products == null)
            return;

        float min = priceRangeSlider != null ? priceRangeSlider.MinPrice : float.MinValue;
        float max = priceRangeSlider != null ? priceRangeSlider.MaxPrice : float.MaxValue;

        List<CategoryManager.Products> filtered = currentResponse.products
            .Where(p => MatchesPrice(p, min, max))
            .Where(p => MatchesCategories(p))
            .Where(p => MatchesBrand(p))
            .Where(p => MatchesColor(p))
            .Where(p => MatchesAvailability(p))
            .ToList();

        var response = new CategoryManager.ProductResponse { products = filtered };

        if (categoriesUI != null)
            categoriesUI.ShowFilteredProducts(response);
        else if (categoryManager != null)
            categoryManager.PopulateProducts(response);

        lastFilteredCount = filtered.Count;
        RefreshShowButtonText();
        RefreshBrandCounts();
    }

    public void ClearFilters()
    {
        selectedIds.Clear();
        selectedBrandIds.Clear();
        selectedColorCodes.Clear();
        filterInStock = false;
        filterSoldOut = false;

        if (inStockRow != null) UpdateAvailabilityVisual(inStockRow, true);
        if (soldOutRow != null) UpdateAvailabilityVisual(soldOutRow, false);

        ApplyFilters();
    }

    private bool MatchesPrice(CategoryManager.Products p, float min, float max)
    {
        if (p?.displayPrice == null) return true;
        float effective = p.displayPrice.hasSale ? p.displayPrice.salePrice : p.displayPrice.price;
        return effective >= min && effective <= max;
    }

    private bool MatchesCategories(CategoryManager.Products p)
    {
        if (selectedIds.Count == 0 || p?.category == null)
            return selectedIds.Count == 0;

        if (IsIdSelected(p.category.id))
            return true;

        CategoryManager.Parent parent = p.category.parent;
        if (parent != null && IsIdSelected(parent.id))
            return true;

        CategoryManager.LesserParent lesser = parent?.parent;
        if (lesser != null && IsIdSelected(lesser.id))
            return true;

        CategoryManager.LesserLesserParent less = lesser?.parent;
        return less != null && IsIdSelected(less.id);
    }

    private bool MatchesBrand(CategoryManager.Products p)
    {
        return selectedBrandIds.Count == 0 ||
               (p?.company != null && !string.IsNullOrEmpty(p.company.id) && selectedBrandIds.Contains(p.company.id));
    }

    private bool MatchesColor(CategoryManager.Products p)
    {
        if (selectedColorCodes.Count == 0 || p?.colors == null)
            return selectedColorCodes.Count == 0;

        return p.colors.Any(c => IsColorSelected(c.code));
    }

    private bool MatchesAvailability(CategoryManager.Products p)
    {
        if (filterInStock == filterSoldOut) return true;
        if (p == null || string.IsNullOrEmpty(p.availability)) return false;

        string a = p.availability.ToLowerInvariant();
        return filterInStock ? a.Contains("in stock") : a.Contains("sold out");
    }

    private int CountProductsForBrand(string brandId)
    {
        if (currentResponse?.products == null || string.IsNullOrEmpty(brandId))
            return 0;

        float min = priceRangeSlider != null ? priceRangeSlider.MinPrice : float.MinValue;
        float max = priceRangeSlider != null ? priceRangeSlider.MaxPrice : float.MaxValue;

        return currentResponse.products.Count(p =>
            p?.company != null && !string.IsNullOrEmpty(p.company.id) &&
            p.company.id == brandId &&
            MatchesPrice(p, min, max) && MatchesCategories(p) &&
            MatchesColor(p) && MatchesAvailability(p));
    }

    private void RefreshBrandCounts()
    {
        foreach (BrandRowEntry entry in brandRowEntries)
        {
            if (entry.row?.countText != null)
                entry.row.countText.text = CountProductsForBrand(entry.brandId).ToString();
        }
    }

    #endregion

    #region Filter Page UI

    public void WireFilterControls()
    {
        if (controlsWired) return;
        controlsWired = true;

        if (filterPage == null)
            filterPage = FindFilterPage();
        if (filterPage == null) return;

        if (resetButton == null)
            resetButton = FindButtonDeep(filterPage.transform, "Reset Btn");
        if (showButton == null)
            showButton = FindButtonDeep(filterPage.transform, "Show Btn");
        if (crossButton == null)
            crossButton = FindButtonDeep(filterPage.transform, "Cross Btn");

        if (showCountText == null && showButton != null)
        {
            TextMeshProUGUI[] texts = showButton.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (texts.Length > 0) showCountText = texts[0];
        }

        if (sizeFilter == null)
            sizeFilter = FindObjectOfType<SizeFilterUI>();

        if (resetButton != null)
            resetButton.onClick.AddListener(() =>
            {
                ClearFilters();
                Build();
                if (sizeFilter != null) sizeFilter.ClearSelection();
                RefreshShowButtonText();
            });

        if (showButton != null)
            showButton.onClick.AddListener(OnShowClicked);
        if (crossButton != null)
            crossButton.onClick.AddListener(CloseFilterPage);

        WireFilterButtons();
    }

    private void WireFilterButtons()
    {
        if (filterToggleButton != null)
        {
            WireButton(filterToggleButton.gameObject, ToggleFilterPage);
            return;
        }

        if (filterPage?.transform.parent == null) return;

        List<Transform> buttons = new List<Transform>();
        CollectNamed(filterPage.transform.parent, "Filter Btn", buttons);

        if (buttons.Count == 0)
        {
            Canvas root = GetComponentInParent<Canvas>();
            if (root != null)
                CollectNamed(root.transform, "Filter Btn", buttons);
        }

        foreach (Transform t in buttons)
            WireButton(t.gameObject, ToggleFilterPage);
    }

    public void ToggleFilterPage()
    {
        SetFilterPageActive(filterPage != null && !filterPage.activeSelf);
    }

    public void CloseFilterPage()
    {
        SetFilterPageActive(false);
    }

    private void SetFilterPageActive(bool active)
    {
        if (filterPage == null || filterPage.activeSelf == active) return;

        filterPage.SetActive(active);
        SetBottomButtonsVisible(!active);

        if (active)
        {
            PushFilterBack();
            RefreshShowButtonText();
        }
    }

    public void OnShowClicked()
    {
        ApplyFilters();

        if (categoriesUI != null)
        {
            if (categoriesUI.supplyPanel != null) categoriesUI.supplyPanel.SetActive(false);
            if (categoriesUI.shopPanel != null) categoriesUI.shopPanel.SetActive(true);
        }
        else if (UIManagerAR.instance != null)
        {
            UIManagerAR.instance.ShowShop();
        }

        CloseFilterPage();
    }

    private void SetBottomButtonsVisible(bool visible)
    {
        BlogsUI[] blogs = FindObjectsByType<BlogsUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (blogs == null || blogs.Length == 0) return;

        foreach (GameObject btn in blogs[0].bottomBtns)
        {
            if (btn != null) btn.SetActive(visible);
        }
    }

    #endregion

    #region Show Count Animation

    private void RefreshShowButtonText()
    {
        if (showCountText == null) return;

        if (lastDisplayedCount == lastFilteredCount)
        {
            showCountText.text = "Show " + lastFilteredCount + " Items";
            return;
        }

        if (countRollRoutine != null) StopCoroutine(countRollRoutine);
        countRollRoutine = StartCoroutine(RollShowCount(lastDisplayedCount, lastFilteredCount));
    }

    private IEnumerator RollShowCount(int from, int to)
    {
        float t = 0f;
        const float duration = 0.35f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float roll = Mathf.Round(Mathf.Lerp(from, to, UIAnim.EaseOutCubic(Mathf.Clamp01(t))));
            showCountText.text = "Show " + roll + " Items";
            yield return null;
        }

        lastDisplayedCount = to;
        showCountText.text = "Show " + to + " Items";
        countRollRoutine = null;
    }

    #endregion

    #region Helpers

    private CategoryRow InstantiateRow(string text, int indent, bool enabled)
    {
        CategoryRow row = ActiveRowPrefab != null
            ? Instantiate(ActiveRowPrefab, categoriesContent)
            : FilterRowFactory.BuildRow(categoriesContent);

        if (row == null) return null;
        row.Setup(text, indent);
        row.SetEnabled(enabled);
        return row;
    }

    private void SetRowSelected(CategoryRow row, bool selected, bool isColor = false)
    {
        row.SetState(selected, false);
        row.SetLabelColor(selected
            ? (isColor ? selectedColorLabelColor : selectedLabelColor)
            : unselectedLabelColor);
    }

    private static void ToggleId(HashSet<string> set, string id)
    {
        if (set.Contains(id)) set.Remove(id);
        else set.Add(id);
    }

    private bool IsIdSelected(string id)
    {
        return !string.IsNullOrEmpty(id) && selectedIds.Contains(id);
    }

    private bool IsColorSelected(string code)
    {
        return !string.IsNullOrEmpty(code) && selectedColorCodes.Contains(NormalizeColorCode(code));
    }

    private static string NormalizeColorCode(string code)
    {
        return string.IsNullOrEmpty(code) ? string.Empty : code.ToLowerInvariant();
    }

    private void ClearContent()
    {
        DestroyChildren(categoriesContent);
        DestroyChildren(brandsContent);
        DestroyChildren(colorsContent);
    }

    private static void DestroyChildren(Transform parent)
    {
        if (parent == null) return;
        foreach (Transform child in parent)
            Destroy(child.gameObject);
    }

    private static Button FindButtonDeep(Transform root, string name)
    {
        Transform target = FindChildNamed(root, name);
        if (target == null) return null;

        Button button = target.GetComponent<Button>();
        if (button == null) button = target.gameObject.AddComponent<Button>();

        Image image = target.GetComponent<Image>();
        if (button != null && image != null)
            button.targetGraphic = image;

        return button;
    }

    private static Transform FindChildNamed(Transform root, string name)
    {
        foreach (Transform child in root)
        {
            if (child.name == name) return child;
            Transform result = FindChildNamed(child, name);
            if (result != null) return result;
        }
        return null;
    }

    private static void CollectNamed(Transform root, string name, List<Transform> results)
    {
        foreach (Transform child in root)
        {
            if (child.name == name) results.Add(child);
            CollectNamed(child, name, results);
        }
    }

    private static void WireButton(GameObject go, UnityEngine.Events.UnityAction action)
    {
        Button button = go.GetComponent<Button>();
        if (button == null) button = go.AddComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }
    }

    private void PushFilterBack()
    {
        if (filterBackAction == null)
        {
            filterBackAction = CloseFilterPage;
            BackStack.Push(filterBackAction);
        }
    }

    private void PopFilterBack()
    {
        if (filterBackAction != null)
        {
            BackStack.Remove(filterBackAction);
            filterBackAction = null;
        }
    }

    #endregion
}
