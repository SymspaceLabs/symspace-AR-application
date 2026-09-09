using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.Analytics;

public class OnBoardingUI : MonoBehaviour
{
    public enum UIScreenType
    {
        None,
        getStarted,
        age,
        height,
        weight,
        size,
        gender,
        begin
    }

    public List<UIScreen> screens;

    public GameObject gradientBackground;

    public Button getStartBtn;
    public Button continueBtn_heightPanel;
    public Button continueBtn_weightPanel;
    public Button genderBtn_weightPanel;
    public Button begingSimulationBtn;

    public Button ageCrossBtn;
    public Button heightCrossBtn;
    public Button weightCrossBtn;
    public Button sizeCrossBtn;
    public Button genderCrossBtn;

    #region Set User Data
    #region Parameters
    private UserProfile user = new UserProfile();

    public GameObject discovery_Panel;
    public GameObject discovery_PanelBlur;

    [Header("On Boarding Panels")]
    public GameObject[] onBoardingPanels;
    public GameObject getStarted_Panel;
    public GameObject age_Panel;
    public GameObject height_Panel;
    public GameObject weight_Panel;
    public GameObject size_Panel;
    public GameObject gender_Panel;
    public GameObject success_Panel;

    [Space(20)]
    public TMP_InputField month_input;
    public TMP_InputField day_input;
    public TMP_InputField year_input;

    [Space(20)]
    public TextMeshProUGUI height_input;
    public TextMeshProUGUI weight_input;

    [Space(20)]
    public TextMeshProUGUI heightValueText;
    public TextMeshProUGUI weightValueText;
    public TextMeshProUGUI measurementValueText;
    public TextMeshProUGUI styleValueText;
    public TextMeshProUGUI descriptionText;

    [Space(20)]
    public CompletionAnimation completionAnimation;

    [Space(20)]
    public TMP_InputField chest_input;
    public TMP_InputField waist_input;
    public TMP_InputField shoulders_input;
    public TMP_InputField armLength_input;
    public TMP_InputField shoeSize_input;
    #endregion

    void SetupPlaceholders()
    {
        AttachPlaceholder(month_input);
        AttachPlaceholder(day_input);
        AttachPlaceholder(year_input);
        AttachPlaceholder(chest_input);
        AttachPlaceholder(waist_input);
        AttachPlaceholder(shoulders_input);
        AttachPlaceholder(armLength_input);
        AttachPlaceholder(shoeSize_input);
    }

    void SetupBackButtons()
    {
        foreach (var screen in screens)
        {
            WireBackButtonsIn(screen.backPanel);
            WireBackButtonsIn(screen.mainPanel);
        }
        WireBackButtonsIn(age_Panel);
        WireBackButtonsIn(height_Panel);
        WireBackButtonsIn(weight_Panel);
        WireBackButtonsIn(size_Panel);
        WireBackButtonsIn(gender_Panel);
    }

    void WireBackButtonsIn(GameObject root)
    {
        if (root == null) return;
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name != "Back Btn") continue;
            Button b = t.GetComponent<Button>();
            if (b == null) b = t.gameObject.AddComponent<Button>();
            b.onClick.RemoveListener(GoBack);
            b.onClick.AddListener(GoBack);
        }
    }

    static void AttachPlaceholder(TMP_InputField field)
    {
        if (field == null) return;
        if (field.GetComponent<PlaceholderInput>() == null)
            field.gameObject.AddComponent<PlaceholderInput>();
    }

    public void ShowScreen(UIScreenType type)
    {
        HideAll();

        foreach (var screen in screens)
        {
            if (screen.type == type)
            {
                if (screen.mainPanel) screen.mainPanel.SetActive(true);
                if (screen.blurPanel) screen.blurPanel.SetActive(true);
                if (screen.backPanel) screen.backPanel.SetActive(true);
                break;
            }
        }
    }

    public void HideAll()
    {
        foreach (var screen in screens)
        {
            if (screen.mainPanel) screen.mainPanel.SetActive(false);
            if (screen.blurPanel) screen.blurPanel.SetActive(false);
            //if (screen.backPanel) screen.backPanel.SetActive(false);
        }
    }

    public void SetDate()
    {
        if (!ValidateData(month_input.text) || !ValidateData(day_input.text) || !ValidateData(year_input.text))
        {
            ShowStatus("Invalid Date", true);
            return;
        }

        if (int.TryParse(month_input.text, out int mm) &&
            int.TryParse(day_input.text, out int dd) &&
            int.TryParse(year_input.text, out int yyyy))
        {
            try
            {
                // Create DateTime in UTC (assuming input is in UTC or normalized time)
                DateTime date = new DateTime(yyyy, mm, dd, 16, 0, 0, DateTimeKind.Utc); // fixed 4PM UTC time
                user.FormattedDate = date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                EnablePanel(height_Panel);
                ShowScreen(UIScreenType.height);
            }
            catch (Exception e)
            {
                ShowStatus("Invalid date: " + e.Message, true);
                //if (CategoriesUI.Instance.isDebug) Debug.LogError("Invalid date: " + e.Message);
            }
        }
        else
        {
            ShowStatus("Invalid date: One or more date fields are invalid.", true);
            //if (CategoriesUI.Instance.isDebug) Debug.LogError("One or more date fields are invalid.");
        }
    }

    public void SetHeight()
    {
        if (!ValidateData(height_input.text))
        {
            ShowStatus("Height value must be greater than 0", true);
            return;
        }

        user.Height = height_input.text;

        EnablePanel(weight_Panel);
        ShowScreen(UIScreenType.weight);
    }

    public void SetWeight()
    {
        if (!ValidateData(weight_input.text))
        {
            ShowStatus("weight input field is empty or invalid", true);
            return;
        }
        user.Weight = weight_input.text;
        EnablePanel(size_Panel);
        ShowScreen(UIScreenType.size);
    }

    public void SetSize()
    {
        if (!ValidateData(chest_input.text))
        {
            ShowStatus("Chest input field is empty or invalid", true);
            return;
        }
        user.Size.Chest = chest_input.text;
        if (!ValidateData(waist_input.text))
        {
            ShowStatus("waist input field is empty or invalid", true);
            return;
        }
        user.Size.Waist = waist_input.text;
        if (!ValidateData(shoulders_input.text))
        {
            ShowStatus("Shoulders input field is empty or invalid", true);
            return;
        }
        user.Size.Shoulders = shoulders_input.text;
        if (!ValidateData(armLength_input.text))
        {
            ShowStatus("Arm length input field is empty or invalid", true);
            return;
        }
        user.Size.ArmLength = armLength_input.text;
        if (!ValidateData(shoeSize_input.text))
        {
            ShowStatus("Shoe Size input field is empty or invalid", true);
            return;
        }
        user.Size.ShoeSize = shoeSize_input.text;
        EnablePanel(gender_Panel);
        ShowScreen(UIScreenType.gender);
    }

    public void SetGender(string gender)
    {
        user.Gender = gender;
    }

    void WireGenderButtons()
    {
        WireGenderButton("Male Btn", "Male");
        WireGenderButton("Female Btn", "Female");
        WireGenderButton("Both Btn", "Both");
        WireGenderButton("Prefer not to say Btn", "PreferNotToSay");
    }

    void WireGenderButton(string btnName, string gender)
    {
        foreach (Transform t in transform.GetComponentsInChildren<Transform>(true))
        {
            if (t.name != btnName) continue;
            Button b = t.GetComponent<Button>();
            if (b == null) b = t.gameObject.AddComponent<Button>();
            string value = gender;
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(delegate { SetGender(value); });
            return;
        }
    }

    public void CompleteBtn()
    {
        //Call API here
        if (string.IsNullOrEmpty(user.Gender)) user.Gender = "Male";
        PopulateSummary();

        //Go to next panel if API is successfull
        PlayerPrefs.SetInt("OnBoarding", 1);
        EnablePanel(success_Panel);
        HideBackButtons(success_Panel);
        if (completionAnimation) completionAnimation.Play();
    }

    void HideBackButtons(GameObject root)
    {
        if (root == null) return;
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name != "Back Btn") continue;
            t.gameObject.SetActive(false);
        }
    }

    void PopulateSummary()
    {
        if (heightValueText) heightValueText.text = user.Height + " cm";
        if (weightValueText) weightValueText.text = user.Weight + " kg";
        if (measurementValueText) measurementValueText.text = FilledSizeCount() + " of 5";
        if (styleValueText) styleValueText.text = string.IsNullOrEmpty(user.Gender) ? "-" : user.Gender;
        if (descriptionText) descriptionText.text = FormatDate();
    }

    int FilledSizeCount()
    {
        int count = 0;
        if (!string.IsNullOrEmpty(user.Size.Chest)) count++;
        if (!string.IsNullOrEmpty(user.Size.Waist)) count++;
        if (!string.IsNullOrEmpty(user.Size.Shoulders)) count++;
        if (!string.IsNullOrEmpty(user.Size.ArmLength)) count++;
        if (!string.IsNullOrEmpty(user.Size.ShoeSize)) count++;
        return count;
    }

    string FormatDate()
    {
        if (string.IsNullOrEmpty(user.FormattedDate)) return "";
        if (DateTime.TryParse(user.FormattedDate, out DateTime d))
            return d.ToString("MMMM d, yyyy");
        return user.FormattedDate;
    }

    public void GoBack()
    {
        if (success_Panel.activeSelf)
            return;

        for (int i = 0; i < screens.Count; i++)
        {
            if (screens[i].mainPanel != null && screens[i].mainPanel.activeSelf)
            {
                if (i == 0) return;

                UIScreenType prev = screens[i - 1].type;
                ShowScreen(prev);
                PanelForScreen(i - 1);
                return;
            }
        }
    }

    void PanelForScreen(int screenIndex)
    {
        if(screenIndex < onBoardingPanels.Length) 
            EnablePanel(onBoardingPanels[screenIndex]);

        if (screenIndex == 0)
            EnablePanel(getStarted_Panel);
        else if (screenIndex == 1)
            EnablePanel(age_Panel);
        else if (screenIndex == 2)
            EnablePanel(height_Panel);
        else if (screenIndex == 3)
            EnablePanel(weight_Panel);
        else if (screenIndex == 4)
            EnablePanel(size_Panel);
        else if (screenIndex == 5)
            EnablePanel(gender_Panel);
    }

    public void BeginSimulation()
    {
        PlayerPrefs.SetInt("OnBoarding", 1);
        discovery_Panel.SetActive(true);
        SetDiscoveryBlurPanels(true);
        HideAll();
        foreach (var screen in screens)
            if (screen.backPanel) screen.backPanel.SetActive(false);

        gameObject.SetActive(false);
    }

    private void SetDiscoveryBlurPanels(bool active)
    {
        if (discovery_PanelBlur != null) discovery_PanelBlur.SetActive(active);
        var blurPage = discovery_PanelBlur != null && discovery_PanelBlur.transform.parent != null
            ? discovery_PanelBlur.transform.parent.gameObject
            : discovery_PanelBlur;
        if (blurPage != null) blurPage.SetActive(active);
    }

    public UserProfile GetUserProfile()
    {
        return user;
    }

    public void EnablePanel(GameObject activePanel)
    {
        getStarted_Panel.SetActive(false);
        age_Panel.SetActive(false);
        height_Panel.SetActive(false);
        weight_Panel.SetActive(false);
        size_Panel.SetActive(false);
        gender_Panel.SetActive(false);
        success_Panel.SetActive(false);

        activePanel.SetActive(true);

        if (activePanel == gender_Panel && string.IsNullOrEmpty(user.Gender))
            SetGender("Male");
    }

    #endregion

    #region Data Validtion

    public bool ValidateData(string data)
    {
        if (data.Length <= 0 || int.Parse(data) <= 0)
            return false;

        return true;
    }

    private void ShowStatus(string message, bool isError)
    {
        statusText.gameObject.SetActive(false);
        statusText.gameObject.SetActive(true);
        statusText.text = message;
        statusText.color = isError ? Color.red : Color.white;
    }


    #endregion

    #region API Call
    [Space(20)]
    public TextMeshProUGUI statusText;
    public Button completeBtn;
    public GameObject loadingPanel;
    private string onBoardingURL = AuthAPI.api + "categories";

    private void OnEnable()
    {
        if (gradientBackground) gradientBackground.SetActive(true);

        statusText.gameObject.SetActive(false);
        WireGenderButtons();

        if(getStartBtn) getStartBtn.onClick.AddListener(() =>
        {
            ShowScreen(UIScreenType.age);
        });

        if (ageCrossBtn) ageCrossBtn.onClick.AddListener(() =>
        {
            ShowScreen(UIScreenType.height);
        });
        if (heightCrossBtn) heightCrossBtn.onClick.AddListener(() =>
        {
            ShowScreen(UIScreenType.weight);
        });
        if (weightCrossBtn) weightCrossBtn.onClick.AddListener(() =>
        {
            ShowScreen(UIScreenType.size);
        });
    }

    private void OnDisable()
    {
        if (gradientBackground) gradientBackground.SetActive(false);
    }

    void Start()
    {
        SetupPlaceholders();
        SetupBackButtons();
        if (PlayerPrefs.GetInt("OnBoarding", 0) == 0)
            loadingPanel.SetActive(false);
        discovery_Panel.SetActive(PlayerPrefs.GetInt("OnBoarding", 0) == 0 ? false : true);
        SetDiscoveryBlurPanels(PlayerPrefs.GetInt("OnBoarding", 0) == 0 ? false : true);
        getStarted_Panel.SetActive(PlayerPrefs.GetInt("OnBoarding", 0) == 0 ? true : false);

        HideAll();
        foreach(var screen in screens)
            if(screen.backPanel) screen.backPanel.SetActive(false);

        gameObject.SetActive(PlayerPrefs.GetInt("OnBoarding", 0) == 0 ? true : false);

        if(PlayerPrefs.GetInt("OnBoarding") == 0)
        {
            ShowScreen(UIScreenType.getStarted);
        }

        //completeBtn.onClick.AddListener(OnBoardingAPI);
    }

    void OnBoardingAPI()
    {
        loadingPanel.SetActive(true);

        string json = JsonUtility.ToJson(user);

        StartCoroutine(AuthAPI.PostRequest(onBoardingURL, json,
            (response) =>
            {
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(response);
                if (CategoriesUI.Instance.isDebug) Debug.Log("Onboarding Successful: " + responseData.message);

                loadingPanel.SetActive(false);
            },
            (error) =>
            {
                if (CategoriesUI.Instance.isDebug) Debug.LogError("Sign Up Failed: " + error);

                FirebaseAuthManager.ErrorResponse errorResponse = JsonUtility.FromJson<FirebaseAuthManager.ErrorResponse>(error);

                if (errorResponse.message.Contains("Expected"))
                {
                    loadingPanel.SetActive(false);
                }
                loadingPanel.SetActive(false);
            }));

    }

    #endregion

    #region Structered Classes
    [Serializable]
    public class UserProfile
    {
        public string FormattedDate { get; set; }
        public string Height { get; set; }         // in cm
        public string Weight { get; set; }         // in kg

        public BodySize Size { get; set; } = new BodySize();
        public string Gender { get; set; }
    }

    [Serializable]
    public class BodySize
    {
        public string Chest { get; set; }
        public string Waist { get; set; }
        public string Shoulders { get; set; }
        public string ArmLength { get; set; }
        public string ShoeSize { get; set; }
    }

    private class ResponseData
    {
        public string message;
        public string token;
    }
    #endregion

    [System.Serializable]
    public class UIScreen
    {
        public UIScreenType type;
        public GameObject mainPanel;
        public GameObject blurPanel;
        public GameObject backPanel;
    }

}
