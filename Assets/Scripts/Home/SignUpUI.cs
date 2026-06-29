using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using System;

public class SignUpUI : MonoBehaviour
{
    #region Parameters
    [Header("Input Fields")]
    [SerializeField] private TMP_InputField firstName;
    [SerializeField] private TMP_InputField lastName;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;

    [Space(5)]
    [SerializeField] private Button signUpButton;
    private string signUpUrl = AuthAPI.api + "signup";

    [Space(5)]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite errorInputFieldSprite;
    [SerializeField] private TextMeshProUGUI errorMessage;
    [SerializeField] private GameObject errorMessageParent;

    public SignUpOTPVerifyUI signUpOTPVerifyUI;

    //[Space(5)]
    //public Sprite[] confirmBtnIcons;
    #endregion

    private void OnEnable()
    {
        errorMessageParent.SetActive(false);
    }

    void Start()
    {
        signUpButton.onClick.AddListener(OnSignUpClicked);
    }
    
    #region API Call
    void OnSignUpClicked()
    {
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

                PlayerPrefs.SetString("Email", emailInput.text);

                MenuManager.Instance.EnablePanel(MenuManager.Instance.signUpOTPVerifyPanel);
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
        //passwordInput.ActivateInputField();

        // Set caret position to the end
        passwordInput.caretPosition = textLength;
        passwordInput.selectionAnchorPosition = textLength;
        passwordInput.selectionFocusPosition = textLength;
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
    #endregion
}
