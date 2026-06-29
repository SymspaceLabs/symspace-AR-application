using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class ForgotPasswordUI : MonoBehaviour
{
    #region Parameters
    public TMP_InputField emailInput;
    public Button sendButton;
    private string forgotPasswordURL = AuthAPI.api + "forgot-password";

    [Space(5)]
    public Sprite normalSprite;
    public Sprite errorInputFieldSprite;
    public TextMeshProUGUI errorMessage;
    public GameObject errorMessageParent;

    //[Space(5)]
    //public Sprite[] confirmBtnIcons;
    #endregion

    private void OnEnable()
    {
        errorMessageParent.SetActive(false);
    }

    private void Start()
    {
        sendButton.onClick.AddListener(SendResetLink);
    }

    #region API Call
    private void SendResetLink()
    {
        //if (!CheckInputData())
        //    return;

        JsonDataStructure jsonData = new JsonDataStructure();
        jsonData.email = emailInput.text;

        string json = JsonUtility.ToJson(jsonData);

        StartCoroutine(AuthAPI.PostRequest(forgotPasswordURL, json,
            (response) =>
            {
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(response);

                MenuManager.Instance.EnablePanel(MenuManager.Instance.forgotOTPVerifyPanel);
                PlayerPrefs.SetString("Email", emailInput.text);
                MenuManager.Instance.loadingPanel.SetActive(false);
            },
            (error) =>
            {
                FirebaseAuthManager.ErrorResponse errorResponse = JsonUtility.FromJson<FirebaseAuthManager.ErrorResponse>(error);
                MenuManager.Instance.ShowError(errorResponse.message);

                MenuManager.Instance.loadingPanel.SetActive(false);
            }));
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

    private void ShowError(string message, TMP_InputField field = null)
    {
        errorMessageParent.SetActive(false);
        errorMessageParent.SetActive(true);
        errorMessage.text = message;
        //field.Select();
        //field.ActivateInputField();
        if(field != null)
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
