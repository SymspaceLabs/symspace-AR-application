using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class ForgotPasswordUI : MonoBehaviour
{
    #region Parameters
    public TMP_InputField emailInput;
    public Button sendButton;
    public string forgotPasswordURL = "forgot-password";

    [Space(5)]
    public Sprite normalSprite;
    public Sprite errorInputFieldSprite;
    public TextMeshProUGUI errorMessage;

    //[Space(5)]
    //public Sprite[] confirmBtnIcons;
    #endregion

    private void OnEnable()
    {
        errorMessage.gameObject.SetActive(false);
    }

    private void Start()
    {
        sendButton.onClick.AddListener(SendResetLink);
    }

    #region API Call
    private void SendResetLink()
    {
        if (!CheckInputData())
            return;

        JsonDataStructure jsonData = new JsonDataStructure();
        jsonData.email = emailInput.text;

        string json = JsonUtility.ToJson(jsonData);

        StartCoroutine(AuthAPI.PostRequest(forgotPasswordURL, json,
            (response) =>
            {
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(response);

                Debug.Log("Reset link sent");
                Debug.Log("Message: " + responseData.message);

                MenuManager.Instance.otpVerifyPanel.GetComponent<OTPVerifyUI>().nextPanel = MenuManager.Instance.resetPasswordPanel;
                MenuManager.Instance.EnablePanel(MenuManager.Instance.otpVerifyPanel);
                PlayerPrefs.SetString("Email", emailInput.text);
                MenuManager.Instance.loadingPanel.SetActive(false);
            },
            (error) =>
            {
                Debug.LogError("Reset request failed: " + error);
                MenuManager.Instance.loadingPanel.SetActive(false);
            }));
    }
    #endregion

    #region Data Validation
    public bool CheckInputData()
    {
        errorMessage.gameObject.SetActive(false);
        ResetInputFieldVisuals();

        if (string.IsNullOrWhiteSpace(emailInput.text))
        {
            ShowError("Email is empty", emailInput);
            return false;
        }

        if (!IsValidEmail(emailInput.text))
        {
            ShowError("Email format is incorrect", emailInput);
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
        //sendButton.GetComponent<Image>().sprite = confirmBtnIcons[1];
    }

    private void ResetInputFieldVisuals()
    {
        // Optionally reset all input field visuals (e.g., remove error sprites)
        emailInput.GetComponent<Image>().sprite = normalSprite;
    }

    private bool IsValidEmail(string email)
    {
        // Simple email validation
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }
    #endregion

    #region Structure Classes
    private class JsonDataStructure
    {
        public string email;
    }

    private class ResponseData
    {
        public string message;
    }
    #endregion
}
