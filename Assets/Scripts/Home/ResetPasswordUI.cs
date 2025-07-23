using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class ResetPasswordUI : MonoBehaviour
{
    #region Parameters
    public TMP_InputField newPasswordInput;
    public TMP_InputField confirmNewPassword;
    public Button resetButton;
    private string resetURL = AuthAPI.api + "reset-password";

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
        resetButton.onClick.AddListener(HandleReset);
    }
    #region API Call
    private void HandleReset()
    {
        if (!CheckInputData())
            return;

        JsonDataStructure jsonData = new JsonDataStructure();
        jsonData.email = PlayerPrefs.GetString("Email");
        jsonData.newPassword = newPasswordInput.text;
        jsonData.otp = PlayerPrefs.GetString("OTP");

        string json = JsonUtility.ToJson(jsonData);

        StartCoroutine(AuthAPI.PostRequest(resetURL, json,
            (response) =>
            {
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(response);
                Debug.Log("Password reset success");
                MenuManager.Instance.EnablePanel(MenuManager.Instance.passwordResetSuccessPanel);
                MenuManager.Instance.loadingPanel.SetActive(false);
            },
            (error) => 
            {
                Debug.LogError("Password reset failed: " + error);
                MenuManager.Instance.loadingPanel.SetActive(false);
            }));
    }
    #endregion

    #region Data Validation
    public bool CheckInputData()
    {
        errorMessage.gameObject.SetActive(false);
        ResetInputFieldVisuals();

        if (string.IsNullOrWhiteSpace(newPasswordInput.text))
        {
            ShowError("Email is empty", newPasswordInput);
            return false;
        }

        if (string.IsNullOrWhiteSpace(confirmNewPassword.text))
        {
            ShowError("Password is empty", confirmNewPassword);
            return false;
        }

        if (newPasswordInput.text != confirmNewPassword.text)
        {
            errorMessage.gameObject.SetActive(true);
            errorMessage.text = "Password Don't Match";

            newPasswordInput.text = "";
            confirmNewPassword.text = "";

            newPasswordInput.GetComponent<Image>().sprite = errorInputFieldSprite;
            confirmNewPassword.GetComponent<Image>().sprite = errorInputFieldSprite;
            //resetButton.GetComponent<Image>().sprite = confirmBtnIcons[1];
            return false;
        }

        if (!IsValidPassword(newPasswordInput.text))
        {
            ShowError("The password must contain at least 8 characters, 1 uppercase letter, 1 lowercase letter,  1 number", newPasswordInput);
            
            newPasswordInput.text = "";
            confirmNewPassword.text = "";

            newPasswordInput.GetComponent<Image>().sprite = errorInputFieldSprite;

            return false;
        }

        // All inputs are valid
        MenuManager.Instance.loadingPanel.SetActive(true);
        return true;
    }

    public static bool IsValidPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        if (password.Length < 8)
            return false;

        bool hasUpper = Regex.IsMatch(password, "[A-Z]");
        bool hasLower = Regex.IsMatch(password, "[a-z]");
        bool hasDigit = Regex.IsMatch(password, "[0-9]");
        bool hasSpecial = Regex.IsMatch(password, "[^a-zA-Z0-9]"); // Checks for any non-alphanumeric character

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }

    private void ShowError(string message, TMP_InputField field)
    {
        errorMessage.gameObject.SetActive(true);
        errorMessage.text = message;
        //field.Select();
        //field.ActivateInputField();
        field.GetComponent<Image>().sprite = errorInputFieldSprite;
        //resetButton.GetComponent<Image>().sprite = confirmBtnIcons[1];
    }

    private void ResetInputFieldVisuals()
    {
        // Optionally reset all input field visuals (e.g., remove error sprites)
        newPasswordInput.GetComponent<Image>().sprite = normalSprite;
        confirmNewPassword.GetComponent<Image>().sprite = normalSprite;
    }

    public void ToggleNewPasswordVisibility(bool visible)
    {
        // Save the current text and caret position
        string currentText = newPasswordInput.text;
        int textLength = currentText.Length;

        // Toggle content type
        newPasswordInput.contentType = visible
            ? TMP_InputField.ContentType.Standard
            : TMP_InputField.ContentType.Password;

        // Force TMP_InputField to update the label
        newPasswordInput.ForceLabelUpdate();

        // Re-activate the input field
        newPasswordInput.ActivateInputField();

        // Set caret position to the end
        newPasswordInput.caretPosition = textLength;
        newPasswordInput.selectionAnchorPosition = textLength;
        newPasswordInput.selectionFocusPosition = textLength;
    }

    public void ToggleConfirmPasswordVisibility(bool visible)
    {
        // Save the current text and caret position
        string currentText = newPasswordInput.text;
        int textLength = currentText.Length;

        // Toggle content type
        confirmNewPassword.contentType = visible
            ? TMP_InputField.ContentType.Standard
            : TMP_InputField.ContentType.Password;

        // Force TMP_InputField to update the label
        confirmNewPassword.ForceLabelUpdate();

        // Re-activate the input field
        confirmNewPassword.ActivateInputField();

        // Set caret position to the end
        confirmNewPassword.caretPosition = textLength;
        confirmNewPassword.selectionAnchorPosition = textLength;
        confirmNewPassword.selectionFocusPosition = textLength;
    }

    #endregion

    #region Structure Classes
    private class JsonDataStructure
    {
        public string email;
        public string newPassword;
        public string otp;
    }

    private class ResponseData
    {
        public string message;
    }
    #endregion
}
