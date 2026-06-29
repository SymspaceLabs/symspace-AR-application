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

    [Header("On Boarding Panels")]
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
    public TMP_InputField height_input;
    public TMP_InputField weight_input;

    [Space(20)]
    public TMP_InputField chest_input;
    public TMP_InputField waist_input;
    public TMP_InputField shoulders_input;
    public TMP_InputField armLength_input;
    public TMP_InputField shoeSize_input;
    #endregion

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
                //Debug.LogError("Invalid date: " + e.Message);
            }
        }
        else
        {
            ShowStatus("Invalid date: One or more date fields are invalid.", true);
            //Debug.LogError("One or more date fields are invalid.");
        }
    }

    public void SetHeight()
    {
        if (!ValidateData(height_input.text))
        {
            ShowStatus("Height input field is empty or invalid", true);
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

    public void CompleteBtn()
    {
        //Call API here

        //Go to next panel if API is successfull
        PlayerPrefs.SetInt("OnBoarding", 1);
        EnablePanel(success_Panel);
    }

    public void BeginSimulation()
    {
        PlayerPrefs.SetInt("OnBoarding", 1);
        discovery_Panel.SetActive(true);
        HideAll();
        foreach (var screen in screens)
            if (screen.backPanel) screen.backPanel.SetActive(false);

        gameObject.SetActive(false);
    }

    public UserProfile GetUserProfile()
    {
        return user;
    }

    public void EnablePanel(GameObject activePanel)
    {
        getStarted_Panel.SetActive(false);
        height_Panel.SetActive(false);
        weight_Panel.SetActive(false);
        size_Panel.SetActive(false);
        gender_Panel.SetActive(false);
        success_Panel.SetActive(false);

        activePanel.SetActive(true);
    }

    #endregion

    #region Data Validtion

    public bool ValidateData(string data)
    {
        if (string.IsNullOrEmpty(data)/* || string.IsNullOrWhiteSpace(data)*/)
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
        statusText.gameObject.SetActive(false);

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

    void Start()
    {
        if (PlayerPrefs.GetInt("OnBoarding", 0) == 0)
            loadingPanel.SetActive(false);

        discovery_Panel.SetActive(PlayerPrefs.GetInt("OnBoarding", 0) == 0 ? false : true);
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
                Debug.Log("Onboarding Successful: " + responseData.message);

                loadingPanel.SetActive(false);
            },
            (error) =>
            {
                Debug.LogError("Sign Up Failed: " + error);

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
