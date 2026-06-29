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
    public GameObject errorMessageParent;

    public GameObject successMessage;

    [Space(20)]
    public GameObject nextPanel;
    #endregion

    private void OnEnable()
    {
        errorMessageParent.SetActive(false);
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
        //if (!CheckInputData())
        //    return;

        MenuManager.Instance.loadingPanel.SetActive(true);
        errorMessageParent.SetActive(false);

        JsonDataStructure jsonData = new JsonDataStructure();
        jsonData.email = PlayerPrefs.GetString("Email");
        jsonData.otp = otpInput.text;

        string json = JsonUtility.ToJson(jsonData);

        StartCoroutine(AuthAPI.PostRequest(forgotVerifyUrl, json,
            (response) =>
            {
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(response);

                MenuManager.Instance.EnablePanel(MenuManager.Instance.resetPasswordPanel);

                PlayerPrefs.SetString("OTP", otpInput.text);
                MenuManager.Instance.loadingPanel.SetActive(false);
            },
            (error) =>
            {
                FirebaseAuthManager.ErrorResponse errorResponse = JsonUtility.FromJson<FirebaseAuthManager.ErrorResponse>(error);
                MenuManager.Instance.ShowError(errorResponse.message);

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

                successMessage.SetActive(true);
            },
            (error) =>
            {
                FirebaseAuthManager.ErrorResponse errorResponse = JsonUtility.FromJson<FirebaseAuthManager.ErrorResponse>(error);
                MenuManager.Instance.ShowError(errorResponse.message);

                MenuManager.Instance.loadingPanel.SetActive(false);
            }));

        MenuManager.Instance.loadingPanel.SetActive(false);
    }

    #endregion

    #region Data Validation
    /* Validation is currently done on the server side, but if we want to do it on the client side,
       we can use this function to validate the input data before sending it to the server.*/
    // Validate input data before sending to the server
    public bool CheckInputData()
    {
        errorMessageParent.SetActive(false);
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

    private void ShowError(string message, TMP_InputField field = null)
    {
        errorMessageParent.SetActive(false);
        errorMessageParent.SetActive(true);
        errorMessage.text = message;
        //field.Select();
        //field.ActivateInputField();
        if(field != null)
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