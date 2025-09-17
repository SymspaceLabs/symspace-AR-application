using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Threading.Tasks;

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
    public GameObject selectedSprite;

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

    public GameObject[] UI_3D_Models;

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

    }


    public void NextTutorialPage()
    {
        foreach (GameObject tutorialPage in tutorialPages)
            tutorialPage.SetActive(false);

        currentPage++;
        if(currentPage >= tutorialPages.Length)
        {
            PlayerPrefs.SetInt("FirstTimeAR", 1);
            itemsToPlaceParent.SetActive(true);
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
            selectedSprite.SetActive(true);
        }
        else
        {
            isMeasurementOn = false;
            selectedSprite.SetActive(false);
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
                model.SetActive(true);
    }
}
