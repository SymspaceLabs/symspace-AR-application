using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using TMPro;

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
    public TMP_Dropdown sizes;

    public Image LD_Color1;
    public Image LD_Color2;
    public Image LD_Color3;

    private void Awake()
    {
        instance = this;
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
        SceneManager.LoadScene("Blogs");
    }

    public void ChangeARScene(string sceneName)
    {

        StartCoroutine(SwitchScene(sceneName));
        //SceneManager.LoadScene("AR Scene");
    }

    private IEnumerator SwitchScene(string sceneName)
    {
        if (arSession != null)
        {
            arSession.Reset(); // Or arSession.enabled = false;
            arSession.enabled = false;
        }

        // Wait a frame or two to let ARSession properly stop
        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene(sceneName);
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

    public void PlusBtn(string name)
    {
        smallDetail.SetActive(true);
        SelectModel(name);
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

    public void SelectModel(string name)
    {
        foreach (var model in UI_3D_Models)
            model.SetActive(false);

        foreach (var model in UI_3D_Models)
            if (model.name == name)
            {
                model.SetActive(true);
                selectedModelDetails = model.GetComponent<ProductDetails>();
                CategoryManager.Products product = selectedModelDetails.product;

                SD_CompanyName.text = product.company.entityName;
                SD_ProductName.text = product.name;
                SD_Price.text = "$" + product.displayPrice.price.ToString();

                LD_CompanyName.text = product.company.entityName;
                LD_ProductName.text = product.name;
                LD_Price.text = "$" + product.displayPrice.price.ToString();

                sizes.ClearOptions();
                foreach (var option in product.sizes)
                {
                    sizes.options.Add(new TMP_Dropdown.OptionData(option.size));
                }

                if (product.displayPrice.salePrice < product.displayPrice.price)
                {
                    SD_SalePrice.text = "$" + product.displayPrice.salePrice.ToString();
                    LD_SalePrice.text = "$" + product.displayPrice.salePrice.ToString();
                }

                foreach (Transform obj in colorParent_SD)
                    Destroy(obj.gameObject);

                for (int i = 0; i < product.threeDModels.Count; i++)
                {
                    Color newColor1;
                    ColorUtility.TryParseHtmlString(product.threeDModels[i].colorCode, out newColor1);
                    ModelVariant mv = Instantiate(colorPrefab, colorParent_SD).GetComponent<ModelVariant>();
                    mv.img.color = newColor1;
                }

                foreach (Transform obj in colorParent_LD)
                    Destroy(obj.gameObject);

                for (int i = 0; i < product.threeDModels.Count; i++)
                {
                    Color newColor1;
                    ColorUtility.TryParseHtmlString(product.threeDModels[i].colorCode, out newColor1);
                    ModelVariant mv = Instantiate(colorPrefab, colorParent_LD).GetComponent<ModelVariant>();
                    mv.img.color = newColor1;
                }

                //Color newColor;
                //if (product.threeDModels.Count > 0 && ColorUtility.TryParseHtmlString(product.colors[0].code, out newColor))
                //{
                //    SD_Color1.color = newColor;
                //    SD_Color1.gameObject.SetActive(true);

                //    LD_Color1.color = newColor;
                //    LD_Color1.gameObject.SetActive(true);
                //    LD_Color1.transform.parent.gameObject.SetActive(true);
                //}
                //else
                //{
                //    SD_Color1.gameObject.SetActive(false);

                //    LD_Color1.gameObject.SetActive(false);
                //    LD_Color1.transform.parent.gameObject.SetActive(false);
                //}

                //if (product.threeDModels.Count > 1 && ColorUtility.TryParseHtmlString(product.colors[1].code, out newColor))
                //{
                //    SD_Color2.color = newColor;
                //    SD_Color2.gameObject.SetActive(true);

                //    LD_Color2.color = newColor;
                //    LD_Color2.gameObject.SetActive(true);
                //    LD_Color2.transform.parent.gameObject.SetActive(true);
                //}
                //else
                //{
                //    SD_Color2.gameObject.SetActive(false);

                //    LD_Color2.gameObject.SetActive(false);
                //    LD_Color2.transform.parent.gameObject.SetActive(false);
                //}

                //if (product.threeDModels.Count > 2 && ColorUtility.TryParseHtmlString(product.threeDModels[2].colorCode, out newColor))
                //{
                //    SD_Color3.color = newColor;
                //    SD_Color3.gameObject.SetActive(true);

                //    LD_Color3.color = newColor;
                //    LD_Color3.gameObject.SetActive(true);
                //}
                //else
                //{
                //    SD_Color3.gameObject.SetActive(false);

                //    LD_Color3.gameObject.SetActive(false);
                //}


                largeDetail.GetComponent<CategoryManager>().SetProductImages(selectedModelDetails);
            }
    }

    int currentImage = 0;

    public void NextPreviousImage(int index)
    {
        currentImage += index;
        if(currentImage < 0)
            currentImage = selectedModelDetails.sprites.Count - 1;
        if (currentImage >= selectedModelDetails.sprites.Count)
            currentImage = 0;

        if(currentImage == 0)
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

        foreach(var obj in spawner.objectsSpawned)
        {
            obj.GetComponent<XRGrabInteractable>().enabled = arController;
            obj.GetComponent<ARTransformer>().enabled = arController;
            obj.GetComponent<ARObjectManipulator>().enabled = !arController;
        }
    }

    public void DeleteAllSpawnedObjects()
    {
        spawner.DeleteAllSpawnedObjects();
        UI_3D_Models.Clear();
        foreach (Transform obj in UI_3D_Models_Parent.transform)
            Destroy(obj.gameObject);

        itemsToPlaceParent.SetActive(false);
        TogglePlaneVisuals(false);
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
