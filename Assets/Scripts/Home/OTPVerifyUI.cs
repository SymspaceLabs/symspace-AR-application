using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using System;

public class OTPVerifyUI : MonoBehaviour
{
    #region Parameters
    //public TMP_InputField emailInput;
    public TextMeshProUGUI userMailText;
    public TMP_InputField otpInput;
    public Button verifyButton;
    public Button resendOTPButton;
    public string verifyUrl = "verify-otp";
    public string resendUrl = "resend-otp";

    [Space(5)]
    public Sprite normalSprite;
    public Sprite errorInputFieldSprite;
    public TextMeshProUGUI errorMessage;

    //[Space(5)]
    //public Sprite[] confirmBtnIcons;

    [Space(20)]
    public GameObject nextPanel;
    #endregion

    private void OnEnable()
    {
        errorMessage.gameObject.SetActive(false);
        userMailText.text = $"Enter the 6-digit code we sent to {PlayerPrefs.GetString("Email")} to continue";
    }

    void Start()
    {
        verifyButton.onClick.AddListener(OnVerifyClicked);
        resendOTPButton.onClick.AddListener(OnResendCodeClicked);
    }

    #region API Call
    void OnVerifyClicked()
    {
        if (!CheckInputData())
            return;

        MenuManager.Instance.loadingPanel.SetActive(true);
        errorMessage.gameObject.SetActive(false);

        JsonDataStructure jsonData = new JsonDataStructure();
        jsonData.email = PlayerPrefs.GetString("Email");
        jsonData.otp = otpInput.text;

        string json = JsonUtility.ToJson(jsonData);

        StartCoroutine(AuthAPI.PostRequest(verifyUrl, json,
            (response) => 
            {
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(response);

                Debug.Log("OTP Verified");
                Debug.Log("Message: " + responseData.message);

                if (nextPanel != null)
                    MenuManager.Instance.EnablePanel(nextPanel);

                PlayerPrefs.SetString("OTP", otpInput.text);
                MenuManager.Instance.loadingPanel.SetActive(false);
            },
            (error) =>
            {
                Debug.LogError("OTP Verification Failed: " + error);
                errorMessage.text = "Incorrect Code";
                errorMessage.gameObject.SetActive(true);
                MenuManager.Instance.loadingPanel.SetActive(false);
            }));
    }

    void OnResendCodeClicked()
    {
        JsonDataStructure jsonData = new JsonDataStructure();
        jsonData.email = PlayerPrefs.GetString("Email");

        string json = JsonUtility.ToJson(jsonData);

        StartCoroutine(AuthAPI.PostRequest(resendUrl, json,
            (response) =>
            {
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(response);

                Debug.Log("OTP sent again successfully");
                Debug.Log("Message: " + responseData.message);
            },
            (error) => Debug.LogError("OTP Verification Failed: " + error)));

        MenuManager.Instance.loadingPanel.SetActive(false);
    }

    #endregion

    #region Data Validation
    public bool CheckInputData()
    {
        errorMessage.gameObject.SetActive(false);
        ResetInputFieldVisuals();

        if (string.IsNullOrWhiteSpace(otpInput.text))
        {
            ShowError("Email is empty", otpInput);
            return false;
        }

        // All inputs are valid
        MenuManager.Instance.loadingPanel.SetActive(true);
        return true;
    }

    private void ShowError(string message, TMP_InputField field)
    {
        errorMessage.gameObject.SetActive(true);
        errorMessage.text = message;
        field.Select();
        field.ActivateInputField();
        field.GetComponent<Image>().sprite = errorInputFieldSprite;
        //verifyButton.GetComponent<Image>().sprite = confirmBtnIcons[1];
    }

    private void ResetInputFieldVisuals()
    {
        // Optionally reset all input field visuals (e.g., remove error sprites)
        otpInput.GetComponent<Image>().sprite = normalSprite;
    }
    #endregion

    #region Structure Classes
    private class JsonDataStructure
    {
        public string email;
        public string otp;
    }

    private class ResponseData
    {
        public string message;
        public string accessToken;
        public UserData user;
    }

    [Serializable]
    private class UserData
    {
        public string id;
        public string email;
        public string firstName;
        public string lastName;
        public string role;
        public bool isOnboardingFormFilled;
        public string company;
    }
    #endregion
}
