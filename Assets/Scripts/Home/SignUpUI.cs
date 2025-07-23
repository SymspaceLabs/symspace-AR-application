using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using System;

public class SignUpUI : MonoBehaviour
{
    #region Parameters
    [Header("Input Fields")]
    public TMP_InputField firstName;
    public TMP_InputField lastName;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    [Space(5)]
    public Button signUpButton;
    private string signUpUrl = AuthAPI.api + "signup";

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

    void Start()
    {
        signUpButton.onClick.AddListener(OnSignUpClicked);
    }
    
    #region API Call
    void OnSignUpClicked()
    {
        if (!CheckInputData())
            return;
        
        //WWWForm form = new WWWForm();
        //form.AddField("firstName", firstName.text);
        //form.AddField("lastName", lastName.text);
        //form.AddField("email", emailInput.text);
        //form.AddField("password", passwordInput.text);

        MenuManager.Instance.loadingPanel.SetActive(true);

        JsonDataStructure jsonData = new JsonDataStructure();
        jsonData.firstName = firstName.text;
        jsonData.lastName = lastName.text;
        jsonData.email = emailInput.text;
        jsonData.password = passwordInput.text;

        string json = JsonUtility.ToJson(jsonData);

        StartCoroutine(AuthAPI.PostRequest(signUpUrl, json,
            (response) =>
            {
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(response);
                Debug.Log("Sign Up Success: " + responseData.message);

                Debug.Log("Email: " + emailInput.text);
                Debug.Log("token: " + responseData.token);
                PlayerPrefs.SetString("Email", emailInput.text);

                //MenuManager.Instance.signUpOTPVerifyPanel.GetComponent<SignUpOTPVerifyUI>().nextPanel = MenuManager.Instance.homePanel;
                MenuManager.Instance.EnablePanel(MenuManager.Instance.signUpOTPVerifyPanel);
                MenuManager.Instance.loadingPanel.SetActive(false);
            },
            (error) =>
            {
                Debug.LogError("Sign Up Failed: " + error);

                ErrorResponse errorResponse = JsonUtility.FromJson<ErrorResponse>(error);

                if(errorResponse.message.Contains("Email already exists"))
                {
                    //MenuManager.Instance.signUpOTPVerifyPanel.GetComponent<SignUpOTPVerifyUI>().nextPanel = MenuManager.Instance.homePanel;
                    PlayerPrefs.SetString("Email", emailInput.text);
                    MenuManager.Instance.EnablePanel(MenuManager.Instance.signUpOTPVerifyPanel);
                }

                //if (errorResponse.message.Contains("Email already exists"))
                //{
                //    MenuManager.Instance.otpVerifyPanel.GetComponent<OTPVerifyUI>().nextPanel = MenuManager.Instance.homePanel;
                //    MenuManager.Instance.EnablePanel(MenuManager.Instance.otpVerifyPanel);
                //}

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

        if (string.IsNullOrWhiteSpace(firstName.text))
        {
            ShowError("First Name is empty", firstName);
            return false;
        }

        if (string.IsNullOrWhiteSpace(lastName.text))
        {
            ShowError("Last Name is empty", lastName);
            return false;
        }

        if (string.IsNullOrWhiteSpace(passwordInput.text))
        {
            ShowError("Password is empty", passwordInput);
            return false;
        }

        if (!IsValidPassword(passwordInput.text))
        {
            ShowError("Invalid Password", passwordInput);
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

    public void TogglePasswordVisibility(bool visible)
    {
        // Save the current text and caret position
        string currentText = passwordInput.text;
        int textLength = currentText.Length;

        // Toggle content type
        passwordInput.contentType = visible
            ? TMP_InputField.ContentType.Standard
            : TMP_InputField.ContentType.Password;

        // Force TMP_InputField to update the label
        passwordInput.ForceLabelUpdate();

        // Re-activate the input field
        passwordInput.ActivateInputField();

        // Set caret position to the end
        passwordInput.caretPosition = textLength;
        passwordInput.selectionAnchorPosition = textLength;
        passwordInput.selectionFocusPosition = textLength;
    }

    private void ShowError(string message, TMP_InputField field)
    {
        errorMessage.gameObject.SetActive(true);
        errorMessage.text = message;
        //field.Select();
        //field.ActivateInputField();
        field.GetComponent<Image>().sprite = errorInputFieldSprite;
        //signUpButton.GetComponent<Image>().sprite = confirmBtnIcons[1];
    }

    private void ResetInputFieldVisuals()
    {
        // Optionally reset all input field visuals (e.g., remove error sprites)
        firstName.GetComponent<Image>().sprite = normalSprite;
        lastName.GetComponent<Image>().sprite = normalSprite;
        emailInput.GetComponent<Image>().sprite = normalSprite;
        passwordInput.GetComponent<Image>().sprite = normalSprite;
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
        public string firstName;
        public string lastName;
        public string email;
        public string password;
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
