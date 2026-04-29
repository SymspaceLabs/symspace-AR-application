using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.XRTools.ModuleLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using UnityEngine.XR.Management;

public class UIManagerAR : MonoBehaviour
{
    public static UIManagerAR instance;
    [Header("Tutorial Panels")]
    public GameObject[] tutorialPages;
    int currentPage = 0;

    [Space(10)]
    [Header("Measurement")]
    public bool isMeasurementOn = false;
    public Image measurementImg;
    public GameObject measurementSelectedSprite;
    
    [Space(10)]
    [Header("Reset")]
    public bool isResetOn = false;
    public Image resetImg;
    public GameObject resetSelectedSprite;

    [Space(5)]
    [Header("Items")]
    public GameObject itemsToPlaceParent;
    public Image item1;
    public Image item2;
    public Image item3;

    [Space(10)]
    [Header("Left Menu")]
    public GameObject menuBtn;
    public Animator menuAnime;

    [Space(10)]
    public GameObject smallDetail;
    public GameObject largeDetail;
    public GameObject objectDiscription;
    public GameObject shopUI;
    public GameObject crossBtn;

    public SlideUpPanel slidePanel;

    public ARPlaneManager planeManager;
    private bool visualsEnabled = true;

    public int objectSelectedIndex = 0;
    public int selectedMatIndex = 0;

    //public List<GameObject> plusBtns;
    //public int currentPlusBtn;

    public ARSession arSession;

    public GameObject modelPrefab;
    public List<GameObject> UI_3D_Models;
    public GameObject UI_3D_Models_Parent;

    public Camera arCamera;
    public DirectionalKeyMovement selectedObject;
    public GameObject movementUI;

    public RawImage modelViewer3D;
    public GameObject txt360View;
    public Image modelViewerImage;
    public ProductDetails selectedModelDetails;

    public GameObject eventSystem;

    [Header("Product Short Detail Parameters")]
    public TextMeshProUGUI SD_CompanyName;
    public TextMeshProUGUI SD_ProductName;
    public TextMeshProUGUI SD_Price;
    public TextMeshProUGUI SD_SalePrice;
    public TMP_Dropdown SD_sizes;

    public Transform colorParent_SD;
    public Transform colorParent_LD;
    public GameObject colorPrefab;

    public Image SD_Color1;
    public Image SD_Color2;
    public Image SD_Color3;

    [Header("Product Large Detail Parameters")]
    public TextMeshProUGUI LD_CompanyName;
    public TextMeshProUGUI LD_ProductName;
    public TextMeshProUGUI LD_Price;
    public TextMeshProUGUI LD_SalePrice;
    public TMP_Dropdown LD_sizes;

    public Image LD_Color1;
    public Image LD_Color2;
    public Image LD_Color3;

    public int selectedSizeIndex = 0;
    public int maxStocks = 0;

    public TextMeshProUGUI stocksSelected;

    public Action OnResetClick;

    private void Awake()
    {
        instance = this;

        if(PlayerPrefs.GetInt("Restart") == 1 && !SceneManager.GetActiveScene().name.Equals(SceneNames.ARBodyTrackingMars))
        {
            PlayerPrefs.SetInt("Restart", 0);
            StartCoroutine(RestartScene());
        }
    }

    IEnumerator RestartScene()
    {
        yield return LoaderUtility.Initialize();
        LoaderUtility.GetActiveLoader()?.Start();
        yield return null;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Start()
    {
        if (PlayerPrefs.GetInt("FirstTimeAR", 0) == 0)
        {
            tutorialPages[0].SetActive(true);
        }
        else
        {
            foreach (GameObject tutorialPage in tutorialPages)
                tutorialPage.SetActive(false);

            //itemsToPlaceParent.SetActive(true);
            menuBtn.SetActive(true);
            largeDetail.SetActive(true);
            crossBtn.SetActive(false);
        }

#if !UNITY_IOS
        DisableOcclusion();
#endif
    }

    void DisableOcclusion()
    {
        //Camera.main.depthTextureMode = DepthTextureMode.None;
        List<XROcclusionSubsystem> subsystems = new List<XROcclusionSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);
        if (subsystems.Count < 1)
            Debug.Log("no subsytem found");

        foreach (var subsystem in subsystems)
        {
            if (subsystem != null && subsystem.running)
            {
                Debug.Log("Stopping XROcclusionSubsystem in this scene");
                subsystem.Stop();
            }
        }
    }
    private void Update()
    {
        if (arCamera == null)
        {
            arCamera = Camera.main;
            return;
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = arCamera.ScreenPointToRay(Input.GetTouch(0).position);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                hit.transform.TryGetComponent<DirectionalKeyMovement>(out selectedObject);
                if(selectedObject != null)
                    Debug.Log("Selected: " + selectedObject.name);

                // Optionally: highlight, select, etc.
                // selectedObject.GetComponent<Renderer>().material.color = Color.green;
            }
        }
    }

    public void NextTutorialPage()
    {
        foreach (GameObject tutorialPage in tutorialPages)
            tutorialPage.SetActive(false);

        currentPage++;
        if(currentPage >= tutorialPages.Length)
        {
            PlayerPrefs.SetInt("FirstTimeAR", 1);
            //itemsToPlaceParent.SetActive(true);
            menuBtn.SetActive(true);
            largeDetail.SetActive(true);
            crossBtn.SetActive(false);
            return;
        }

        foreach(GameObject tutorialPage in tutorialPages)
            tutorialPage.SetActive(false);

        tutorialPages[currentPage].SetActive(true);
    }


    public void ToggleMeasurement()
    {
        if (!isMeasurementOn)
        {
            isMeasurementOn = true;
            measurementSelectedSprite.SetActive(true);
        }
        else
        {
            isMeasurementOn = false;
            measurementSelectedSprite.SetActive(false);
        }

        var arDimensions = FindObjectsByType<ARDimensionVisualizer>(FindObjectsSortMode.None);
        foreach (var arDimension in arDimensions)
            arDimension.ToggleMeasurement();
    }

    public void BlogScene()
    {
        SceneManager.LoadScene(SceneNames.Blogs);
        //ARSceneHelper.CleanLoad(SceneNames.Home);
    }

    public void ChangeARScene(string sceneName)
    {

        StartCoroutine(SwitchScene(sceneName));
        //SceneManager.LoadScene(SceneNames.ARScene);
    }

    private IEnumerator SwitchScene(string sceneName)
    {
        yield return null;

        DisableOcclusion();
#if UNITY_IOS
        if (sceneName.Equals(SceneNames.ARBodyTrackingMars))
            LoaderUtility.Deinitialize();
#endif
#if UNITY_ANDROID
        if (!sceneName.Equals(SceneNames.ARBodyTrackingMars) && !sceneName.Equals(SceneNames.HandTracking))
#endif
            SceneManager.LoadScene(sceneName);

        //ARSceneHelper.CleanLoad(sceneName);
    }

    public void MenuBtn()
    {
        menuAnime.SetBool("Slide Up", false);
        menuAnime.SetBool("Slide Down", true);
        StartCoroutine(EnableDisableObject(menuBtn, false, 0.2f));
    }

    public void CrossBtn()
    {
        menuAnime.SetBool("Slide Up", true);
        StartCoroutine(EnableDisableObject(menuBtn, true, 0.3f));
    }

    IEnumerator EnableDisableObject(GameObject obj, bool state, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(state);
    }

    public void PlusBtn(ProductDetails pd/*string id, int textureIndex*/)
    {
        smallDetail.SetActive(true);
        SelectModel(pd/*id, textureIndex*/);
        //plusBtns[currentPlusBtn].SetActive(false);
    }

    public void CloseSmallDetail()
    {
        smallDetail.SetActive(false);
        //plusBtns[currentPlusBtn].SetActive(true);
    }

    public void EnlargeDetail()
    {
        shopUI.SetActive(false);
        objectDiscription.SetActive(true);
        slidePanel.ShowPanel();


        modelViewerImage.enabled = false;
        modelViewer3D.enabled = true;
        txt360View.SetActive(true);
        currentImage = 0;
    }

    public void ShowShop()
    {
        shopUI.SetActive(true);
        objectDiscription.SetActive(false);
        slidePanel.ShowPanel();
    }

    public void CloseLargeDetail()
    {
        slidePanel.HidePanel();
    }

    #region Toggle ARPlane Visual

    public void TogglePlaneVisuals(bool state)
    {
        planeManager.trackablesChanged.RemoveListener(OnPlanesChanged);
        visualsEnabled = state;

        planeManager.SetTrackablesActive(state);
        //foreach(var plane in planeManager.trackables)
        //    plane.enabled = false;

        planeManager.trackablesChanged.AddListener(OnPlanesChanged);
    }

    private void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> args)
    {
        // Apply current visual state to any newly added planes
        foreach (var addedPlane in args.added)
        {
            addedPlane.enabled = visualsEnabled;
            //addedPlane.GetComponent<MeshRenderer>().enabled = visualsEnabled;
        }
    }
    #endregion

    public void SelectModel(ProductDetails pd/*string id, int textureIndex*/)
    {
        foreach (var model in UI_3D_Models)
            model.SetActive(false);

        //selectedModelDetails = GhostPlacementController.Instance.spawnedObjects[objectSelectedIndex].GetComponent<ProductDetails>();

        string id = pd.product.id;
        int textureIndex = pd.selectedColorIndex;

        selectedModelDetails = pd;
        //foreach (var model in GhostPlacementController.Instance.spawnedObjects)
        //    if (model.GetComponent<ProductDetails>().product.id == id)
        //        selectedModelDetails = model.GetComponent<ProductDetails>();

        GameObject UIModel = null;
        foreach (var model in UI_3D_Models)
            if (model.GetComponent<ProductDetails>().product.id == id)
                UIModel = model;

        ProductDetails UIModelDetails = null;
        if (UIModel != null)
            UIModel.SetActive(true);
            //UIModelDetails = UIModel.GetComponent<ProductDetails>();

        //UIModelDetails = selectedModelDetails;

        //foreach (var model in GhostPlacementController.Instance.spawnedObjects)
        //{
        //    selectedModelDetails = model.GetComponent<ProductDetails>();
        //    if (selectedModelDetails.product.id == id)
        //    {

        CategoryManager.Products product = selectedModelDetails.product;


        SD_CompanyName.text = product.company.entityName;
        SD_ProductName.text = product.name;
        SD_Price.text = "$" + product.displayPrice.price.ToString();

        LD_CompanyName.text = product.company.entityName;
        LD_ProductName.text = product.name;
        LD_Price.text = "$" + product.displayPrice.price.ToString();

        LD_sizes.ClearOptions();

        SD_sizes.ClearOptions();

        if (product.sizes.Count == 1)
        {
            LD_sizes.options.Add(new TMP_Dropdown.OptionData(product.variants[0].size.size));
            SD_sizes.options.Add(new TMP_Dropdown.OptionData(product.variants[0].size.size));
        }
        else
        {
            LD_sizes.options.Add(new TMP_Dropdown.OptionData("Size"));
            foreach (var option in product.variants)
            {
                LD_sizes.options.Add(new TMP_Dropdown.OptionData(option.size.size));
            }

            SD_sizes.options.Add(new TMP_Dropdown.OptionData("Size"));
            foreach (var option in product.variants)
            {
                SD_sizes.options.Add(new TMP_Dropdown.OptionData(option.size.size));
            }
        }

        if (product.sizes.Count == 1)
        {
            ChangeSize_SD();
            ChangeSize_LD();
        }

        LD_sizes.RefreshShownValue();

        SD_sizes.value = 0;
        SD_sizes.RefreshShownValue();

        LD_sizes.value = selectedModelDetails.selectedSizeIndex;
        SD_sizes.value = selectedModelDetails.selectedSizeIndex;
        if (product.displayPrice.salePrice < product.displayPrice.price)
        {
            SD_SalePrice.text = "$" + product.displayPrice.salePrice.ToString();
            LD_SalePrice.text = "$" + product.displayPrice.salePrice.ToString();

            SD_Price.text = "<s>$" + product.displayPrice.price + "<s>";
            LD_Price.text = "<s>$" + product.displayPrice.price + "<s>";
                    
        }

        foreach (Transform obj in colorParent_SD)
            Destroy(obj.gameObject);

        for (int i = 0; i < product.colors.Count; i++)
        {
            Color newColor1;
            ColorUtility.TryParseHtmlString(product.colors[i].code, out newColor1);
            ModelVariant mv = Instantiate(colorPrefab, colorParent_SD).GetComponent<ModelVariant>();
            mv.img.color = newColor1;
            mv.index = i;
        }


        foreach (Transform obj in colorParent_LD)
            Destroy(obj.gameObject);

        for (int i = 0; i < product.colors.Count; i++)
        {
            Color newColor1;
            ColorUtility.TryParseHtmlString(product.colors[i].code, out newColor1);
            ModelVariant mv = Instantiate(colorPrefab, colorParent_LD).GetComponent<ModelVariant>();
            mv.img.color = newColor1;
            mv.index = i;
        }


        //largeDetail.GetComponent<CategoryManager>().SetProductImages(selectedModelDetails);

        if(GhostPlacementController.Instance != null)
            GhostPlacementController.Instance.objectColors.Clear();
        foreach (Transform obj in CategoryManager.Instance.modelVariantParent)
            Destroy(obj.gameObject);

        for (int i = 0; i < product.colors.Count; i++)
        {
            UnityEngine.Color newColor1;
            ColorUtility.TryParseHtmlString(product.colors[i].code, out newColor1);
            if (GhostPlacementController.Instance != null)
                GhostPlacementController.Instance.objectColors.Add(newColor1);
            ModelVariant mv = Instantiate(CategoryManager.Instance.modelVariantPrefab, CategoryManager.Instance.modelVariantParent).GetComponent<ModelVariant>();
            mv.index = i;

            UnityEngine.Color newColor;
            if (ColorUtility.TryParseHtmlString(product.colors[i].code, out newColor))
            {
                mv.colorImg.color = newColor;
                //mv.colorName.text = product.colors[i].name;
            }
        }
        if (GhostPlacementController.Instance != null)
        {
            GhostPlacementController.Instance.ChangeTextureByIndex(textureIndex);
            //if (spawner.ChangeTextureByIndex(textureIndex))
            //{
                UpdateDetailData();
            //}
        }

        ChangeModelVariant();
        //UIModel.GetComponentInChildren<MeshRenderer>().material.mainTexture = model.GetComponent<ProductDetails>().textures[textureIndex];
                //break;
        //    }
        //}

        //spawner.ObjectSelected(index);
    }

    public void UpdateDetailData()
    {
        if (selectedModelDetails == null)
            return;

        if (selectedModelDetails.isSizeSelected)
        {
            selectedSizeIndex = LD_sizes.value;

            int tempSizeIndex = selectedSizeIndex;

            if (selectedModelDetails.product.sizes.Count > 1 && selectedSizeIndex == 0)
                return;

            if (selectedModelDetails.product.sizes.Count == 1)
                tempSizeIndex = selectedSizeIndex;
            else
                tempSizeIndex = selectedSizeIndex - 1;

            if (selectedModelDetails.product.variants[tempSizeIndex].salePrice > 0 && selectedModelDetails.product.variants[tempSizeIndex].price > selectedModelDetails.product.variants[tempSizeIndex].salePrice)
            {
                //if(selectedModelDetails.product.variants.Count > selectedSizeIndex)
                    //maxStocks = selectedModelDetails.product.variants[selectedSizeIndex].stock;
                SD_Price.text = "<s>$" + selectedModelDetails.product.variants[tempSizeIndex].price + "<s>";
                SD_Price.color = new Color(0.7f, 0.7f, 0.7f);
                SD_SalePrice.text = "$" + selectedModelDetails.product.variants[tempSizeIndex].salePrice;
                SD_SalePrice.gameObject.SetActive(true);

                LD_Price.text = "<s>$" + selectedModelDetails.product.variants[tempSizeIndex].price + "<s>";
                LD_Price.color = new Color(0.7f, 0.7f, 0.7f);
                LD_SalePrice.text = "$" + selectedModelDetails.product.variants[tempSizeIndex].salePrice;
                LD_SalePrice.gameObject.SetActive(true);
            }
            else
            {
                SD_Price.text = "$" + selectedModelDetails.product.variants[tempSizeIndex].price;
                SD_Price.color = new Color(1f, 1f, 1f);
                SD_SalePrice.gameObject.SetActive(false);

                LD_Price.text = "$" + selectedModelDetails.product.variants[tempSizeIndex].price;
                LD_Price.color = new Color(1f, 1f, 1f);
                LD_SalePrice.gameObject.SetActive(false);
            }
            Debug.Log($"tempSizeIndex : {tempSizeIndex}");
            ProductDetails pd = GhostPlacementController.Instance.spawnedObjects[objectSelectedIndex].GetComponent<ProductDetails>();
            foreach (var v in pd.product.variants)
            {
                if (v.color.id == pd.product.colors[pd.selectedColorIndex].id)
                {
                    maxStocks = v.stock;
                    Debug.Log("Max Stocks " + maxStocks);
                    //if (v.salePrice > 0 && v.salePrice < v.price)
                    //{
                    //    SD_Price.text = "<s>$" + v.price + "<s>";
                    //    SD_Price.color = new Color(0.7f, 0.7f, 0.7f);
                    //    SD_SalePrice.text = "$" + v.salePrice;
                    //    SD_SalePrice.gameObject.SetActive(true);

                    //    LD_Price.text = "<s>$" + v.price + "<s>";
                    //    LD_Price.color = new Color(0.7f, 0.7f, 0.7f);
                    //    LD_SalePrice.text = "$" + v.salePrice;
                    //    LD_SalePrice.gameObject.SetActive(true);
                    //}
                    //else
                    //{
                    //    SD_Price.text = v.price.ToString();
                    //    SD_Price.color = new Color(1, 1, 1);
                    //    SD_SalePrice.gameObject.SetActive(false);

                    //    LD_Price.text = v.price.ToString();
                    //    LD_Price.color = new Color(1, 1, 1);
                    //    LD_SalePrice.gameObject.SetActive(false);

                    //}
                }
            }

            //if (maxStocks > 0)
            //    stocksSelected.text = "1";
            //else
            stocksSelected.text = "1";
        }
        else
        {
            SD_Price.text = selectedModelDetails.product.displayPrice.range;
            SD_Price.color = new Color(1, 1, 1);
            SD_SalePrice.gameObject.SetActive(false);
            
            LD_Price.text = selectedModelDetails.product.displayPrice.range;
            LD_Price.color = new Color(1, 1, 1);
            LD_SalePrice.gameObject.SetActive(false);


            stocksSelected.text = "1";
        }
    }

    public void UpdateDetailUI()
    {
        int tempSizeIndex = selectedSizeIndex;

        if (selectedModelDetails.product.sizes.Count == 1)
            tempSizeIndex = selectedSizeIndex;
        else
            tempSizeIndex = selectedSizeIndex - 1;

        if (selectedModelDetails.product.variants[tempSizeIndex].salePrice > 0 && selectedModelDetails.product.variants[tempSizeIndex].price > selectedModelDetails.product.variants[tempSizeIndex].salePrice)
        {
            //maxStocks = selectedModelDetails.product.variants[selectedSizeIndex - 1].stock;
            SD_Price.text = "<s>$" + selectedModelDetails.product.variants[tempSizeIndex].price + "<s>";
            SD_Price.color = new Color(0.7f, 0.7f, 0.7f);
            SD_SalePrice.text = "$" + selectedModelDetails.product.variants[tempSizeIndex].salePrice;
            SD_SalePrice.gameObject.SetActive(true);

            LD_Price.text = "<s>$" + selectedModelDetails.product.variants[tempSizeIndex].price + "<s>";
            LD_Price.color = new Color(0.7f, 0.7f, 0.7f);
            LD_SalePrice.text = "$" + selectedModelDetails.product.variants[tempSizeIndex].salePrice;
            LD_SalePrice.gameObject.SetActive(true);
        }
        else
        {
            SD_Price.text = "$" + selectedModelDetails.product.variants[tempSizeIndex].price;
            SD_Price.color = new Color(1f, 1f, 1f);
            SD_SalePrice.gameObject.SetActive(false);

            LD_Price.text = "$" + selectedModelDetails.product.variants[tempSizeIndex].price;
            LD_Price.color = new Color(1f, 1f, 1f);
            LD_SalePrice.gameObject.SetActive(false);
        }

        CategoryManager.Variants variant = selectedModelDetails.product.variants[tempSizeIndex];

        maxStocks = variant.stock;

        if (variant.salePrice > 0 && variant.salePrice < variant.price)
        {
            SD_Price.text = "<s>$" + variant.price + "<s>";
            SD_Price.color = new Color(0.7f, 0.7f, 0.7f);
            SD_SalePrice.text = "$" + variant.salePrice;
            SD_SalePrice.gameObject.SetActive(true);

            LD_Price.text = "<s>$" + variant.price + "<s>";
            LD_Price.color = new Color(0.7f, 0.7f, 0.7f);
            LD_SalePrice.text = "$" + variant.salePrice;
            LD_SalePrice.gameObject.SetActive(true);
        }
        else
        {
            SD_Price.text = variant.price.ToString();
            SD_Price.color = new Color(1, 1, 1);
            SD_SalePrice.gameObject.SetActive(false);

            LD_Price.text = variant.price.ToString();
            LD_Price.color = new Color(1, 1, 1);
            LD_SalePrice.gameObject.SetActive(false);
        }

    }

    public void ChangeSize_LD()
    {
        selectedModelDetails.isSizeSelected = true;
        selectedSizeIndex = LD_sizes.value;
        SD_sizes.value = selectedSizeIndex;
        
        if (selectedModelDetails.product.sizes.Count > 1 && selectedSizeIndex == 0)
            return;
        
        Debug.Log("LD size changed");
        ProductDetails pd = null;
        if (GhostPlacementController.Instance != null)
            pd = GhostPlacementController.Instance.spawnedObjects[objectSelectedIndex].GetComponent<ProductDetails>();

        UpdateDetailUI();

        //if (maxStocks > 0)
        //    stocksSelected.text = "1";
        //else
        stocksSelected.text = "1";

        if(GhostPlacementController.Instance != null)
            CategoryManager.Instance.UpdateObjectScale(pd, GhostPlacementController.Instance.spawnedObjects[objectSelectedIndex], 
                !pd.product.ar_type.Equals("horizontal-plane detection"), selectedModelDetails.product.sizes.Count == 1? selectedSizeIndex : selectedSizeIndex -1);
    }

    public void ChangeSize_SD()
    {
        selectedModelDetails.isSizeSelected = true;
        selectedSizeIndex = SD_sizes.value;
        LD_sizes.value = selectedSizeIndex;

        if (selectedModelDetails.product.sizes.Count > 1 && selectedSizeIndex == 0)
            return;

        Debug.Log("SD size changed");
        selectedModelDetails.selectedSizeIndex = selectedSizeIndex;
        if(GhostPlacementController.Instance != null)
            /*ProductDetails pd = */GhostPlacementController.Instance.spawnedObjects[objectSelectedIndex].GetComponent<ProductDetails>();

        UpdateDetailUI();

        stocksSelected.text = "1";

        //CategoryManager.Instance.UpdateObjectScale(pd.product, spawner.objectsSpawned[objectSelectedIndex], !pd.product.ar_type.Equals("horizontal-plane detection"), selectedSizeIndex - 1);
    }

    public void ChangeItemAmount(int value)
    {
        int currentStocks = int.Parse(stocksSelected.text);

        currentStocks += value;

        if (currentStocks < 1)
            currentStocks = 1;
        if (currentStocks > maxStocks)
            currentStocks = maxStocks;

        if (currentStocks < 1)
            currentStocks = 1;

        stocksSelected.text = currentStocks.ToString();
    }

    public void ChangeModelVariant()
    {
        modelViewerImage.enabled = false;
        modelViewer3D.enabled = true;
        txt360View.SetActive(true);
        currentImage = 0;
    }

    int currentImage = 0;

    public void NextPreviousImage(int index)
    {
        ProductDetails pd = null;
        if (GhostPlacementController.Instance != null)
            pd = GhostPlacementController.Instance.spawnedObjects[objectSelectedIndex].GetComponent<ProductDetails>();
        else
            pd = UI_3D_Models[objectSelectedIndex].GetComponent<ProductDetails>();
        string colorCode = pd.product.colors[pd.selectedColorIndex].code;
        Debug.Log("Object selected : " + pd.selectedColorIndex);
        Debug.Log("ID: " + pd.product.id);
        Debug.Log("current Index before : " + currentImage);
        currentImage += index;

        if (currentImage < -1)
            currentImage = selectedModelDetails.sprites.Count - 1;
        if (currentImage >= selectedModelDetails.sprites.Count)
        {
            currentImage = -1;
        }
        
        if(currentImage != -1)
            while (selectedModelDetails.product.images[currentImage].colorCode != colorCode)
            {
                currentImage += index;

                if (currentImage == -1)
                    break;

                if (currentImage < -1)
                    currentImage = selectedModelDetails.sprites.Count - 1;
                if (currentImage >= selectedModelDetails.sprites.Count)
                {
                    currentImage = -1;
                    break;
                }
            }

        Debug.Log("current Index : " + currentImage);

        if(currentImage == -1)
        {
            modelViewerImage.enabled = false;
            modelViewer3D.enabled = true;
            txt360View.SetActive(true);
        }
        else
        {
            modelViewer3D.enabled = false;
            txt360View.SetActive(false);
            modelViewerImage.enabled = true;
            if (selectedModelDetails.sprites[currentImage] == null)
                NextPreviousImage(currentImage);
            else
                modelViewerImage.sprite = selectedModelDetails.sprites[currentImage];
        }

    }

    public bool arController = true;
    public ObjectSpawner spawner;

    public IEnumerator ChangeMovementControllers(GameObject obj)
    {
        yield return new WaitForSeconds(1f);
        obj.GetComponent<XRGrabInteractable>().enabled = arController;
        obj.GetComponent<ARTransformer>().enabled = arController;
        obj.GetComponent<ARObjectManipulator>().enabled = !arController;
    }

    public void ChangeMovementControllers()
    {
        isResetOn = !isResetOn;
        resetSelectedSprite.SetActive(isResetOn);

        arController = !arController;

        foreach(var obj in GhostPlacementController.Instance.spawnedObjects)
        {
            obj.GetComponent<XRGrabInteractable>().enabled = arController;
            obj.GetComponent<ARTransformer>().enabled = arController;
            obj.GetComponent<ARObjectManipulator>().enabled = !arController;
        }
    }

    public void DeleteAllSpawnedObjects()
    {
        GhostPlacementController.Instance.DeleteAllSpawnedObjects();
        UI_3D_Models.Clear();
        //CategoryManager.Instance.downloadedModels.Clear();
        CategoryManager.Instance.tempModels.Clear();
        foreach (Transform obj in UI_3D_Models_Parent.transform)
            Destroy(obj.gameObject);

        itemsToPlaceParent.SetActive(false);
        TogglePlaneVisuals(false);

        OnResetClick?.Invoke();
    }

    public void ToggleMovementUI()
    {
        //movementUI.SetActive(!movementUI.activeSelf);
        movementUI.SetActive(true);
        CrossBtn();
    }

    public void MoveUp()
    {
        if (selectedObject != null)
        {
            selectedObject.SetMovementDirection(GetCameraRelativeDirection("forward"));
        }
        
    }

    public void MoveDown()
    {
        if (selectedObject != null)
        {
            selectedObject.SetMovementDirection(GetCameraRelativeDirection("back"));
        }
    }

    public void MoveLeft()
    {
        if (selectedObject != null)
        {
            selectedObject.SetMovementDirection(GetCameraRelativeDirection("left"));
        }
    }

    public void MoveRight()
    {
        if (selectedObject != null)
        {
            selectedObject.SetMovementDirection(GetCameraRelativeDirection("right"));
        }
    }

    public void StopMovement()
    {
        if (selectedObject != null)
            selectedObject.StopMovement();
    }

    public void RotateLeft()
    {
        if (selectedObject != null)
        {
            selectedObject.SetRotationDirection(GetRotationAxis("left"));
        }
    }

    public void RotateRight()
    {
        if (selectedObject != null)
        {
            selectedObject.SetRotationDirection(GetRotationAxis("right"));
        }
    }

    public void StopRotating()
    {
        if (selectedObject != null)
            selectedObject.StopRotation();
    }

    private float GetRotationAxis(string dir)
    {
        var planeMode = selectedObject.GetComponent<ARTransformer>().objectPlaneTranslationMode;


        int directionMultiplier = 0;
        if (dir == "left") directionMultiplier = 1;
        else if (dir == "right") directionMultiplier = -1;
        else return 0; // invalid input

        // Return axis * direction multiplier based on plane mode
        if (planeMode == ARTransformer.PlaneTranslationMode.Horizontal)
        {
            // Horizontal object: rotate around Y axis
            return directionMultiplier;
        }
        else if (planeMode == ARTransformer.PlaneTranslationMode.Vertical)
        {
            // Vertical object: rotate around X axis
            return directionMultiplier;
        }

        return 0;
    }

    private Vector3 GetCameraRelativeDirection(string dir)
    {
        var transformer = selectedObject.GetComponent<ARTransformer>();

        if (transformer.objectPlaneTranslationMode == ARTransformer.PlaneTranslationMode.Horizontal)
        {
            // Horizontal surfaces (e.g., floor/table)
            Vector3 forward = arCamera.transform.forward;
            Vector3 right = arCamera.transform.right;

            // Keep movement parallel to ground
            forward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
            right = Vector3.ProjectOnPlane(right, Vector3.up).normalized;

            switch (dir)
            {
                case "forward": return forward;
                case "back": return -forward;
                case "left": return -right;
                case "right": return right;
            }
        }
        else if (transformer.objectPlaneTranslationMode == ARTransformer.PlaneTranslationMode.Vertical)
        {
            // Vertical surfaces (e.g., wall)
            // Use the object's local axes to determine direction along the wall
            Transform t = selectedObject.transform;

            // Define based on your custom orientation rule
            Vector3 planeNormal = t.up;           // Wall's outward normal
            Vector3 planeRight = t.forward;       // Wall's right direction
            Vector3 planeUp = -t.right;           // Wall's upward direction (red arrow downward ? invert)

            switch (dir)
            {
                //case "up": return planeUp;
                //case "down": return -planeUp;
                case "left": return -planeRight;
                case "right": return planeRight;
                case "forward": return planeUp;   // Move away from wall
                case "back": return -planeUp;     // Move into wall
            }
        }

        return Vector3.zero;
    }

}
