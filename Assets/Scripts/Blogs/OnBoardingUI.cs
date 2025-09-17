using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.Analytics;

public class OnBoardingUI : MonoBehaviour
{
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
        
    }

    void Start()
    {
        if (PlayerPrefs.GetInt("OnBoarding", 0) == 0)
            loadingPanel.SetActive(false);

        discovery_Panel.SetActive(PlayerPrefs.GetInt("OnBoarding", 0) == 0 ? false : true);
        getStarted_Panel.SetActive(PlayerPrefs.GetInt("OnBoarding", 0) == 0 ? true : false);
        gameObject.SetActive(PlayerPrefs.GetInt("OnBoarding", 0) == 0 ? true : false);

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

                ErrorResponse errorResponse = JsonUtility.FromJson<ErrorResponse>(error);

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

    private class ErrorResponse
    {
        public string message;
        public string error;
        public string statusCode;
    }
    #endregion
}
