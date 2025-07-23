using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;
public class ForgotOTPVerifyUI : MonoBehaviour
{
    #region Parameters
    //public TMP_InputField emailInput;
    public TextMeshProUGUI userMailText;
    public TMP_InputField otpInput;
    public Button verifyButton;
    public Button resendOTPButton;

    private string forgotVerifyUrl = AuthAPI.api + "verify-forgot-password-otp";
    private string forgotResendUrl = AuthAPI.api + "resend-forgot-password-otp";

    [Space(5)]
    public Sprite normalSprite;
    public Sprite errorInputFieldSprite;
    public TextMeshProUGUI errorMessage;
    public GameObject successMessage;

    //[Space(5)]
    //public Sprite[] confirmBtnIcons;

    [Space(20)]
    public GameObject nextPanel;
    #endregion

    private void OnEnable()
    {
        errorMessage.gameObject.SetActive(false);
        successMessage.SetActive(false);
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

        StartCoroutine(AuthAPI.PostRequest(forgotVerifyUrl, json,
            (response) =>
            {
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(response);

                Debug.Log("OTP Verified");
                Debug.Log("Message: " + responseData.message);

                //if (nextPanel != null)
                //    MenuManager.Instance.EnablePanel(nextPanel);

                MenuManager.Instance.EnablePanel(MenuManager.Instance.resetPasswordPanel);

                PlayerPrefs.SetString("OTP", otpInput.text);
                MenuManager.Instance.loadingPanel.SetActive(false);
            },
            (error) =>
            {
                ErrorResponse errorResponse = JsonUtility.FromJson<ErrorResponse>(error);
                Debug.LogError("OTP Verification Failed: " + errorResponse.message);
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

        StartCoroutine(AuthAPI.PostRequest(forgotResendUrl, json,
            (response) =>
            {
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(response);

                Debug.Log("OTP sent again successfully");
                Debug.Log("Message: " + responseData.message);
                successMessage.SetActive(true);
            },
            (error) =>
            {
                ErrorResponse errorResponse = JsonUtility.FromJson<ErrorResponse>(error);
                Debug.LogError("OTP Verification Failed: " + errorResponse.message);
                errorMessage.text = "Incorrect Code";
                errorMessage.gameObject.SetActive(true);
                MenuManager.Instance.loadingPanel.SetActive(false);
            }));

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
        //field.Select();
        //field.ActivateInputField();
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

    private class ErrorResponse
    {
        public string message;
        public string error;
        public string statusCode;
    }
    #endregion
}