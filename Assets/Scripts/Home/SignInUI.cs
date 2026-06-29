using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

public class SignInUI : MonoBehaviour
{
    #region parameters
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button signInButton;
    private string signInUrl = AuthAPI.api + "login";

    [Space(5)]
    public Sprite normalSprite;
    public TextMeshProUGUI errorMessage;
    public GameObject errorMessageParent;

    public Sprite errorInputFieldSprite;


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
        signInButton.onClick.AddListener(OnSignInClicked);
    }

    #region API Call
    void OnSignInClicked()
    {
        JsonDataStructure jsonData = new JsonDataStructure();
        jsonData.email = emailInput.text;
        jsonData.password = passwordInput.text;
        PlayerPrefs.SetString("Email", emailInput.text);

        string json = JsonUtility.ToJson(jsonData);

        StartCoroutine(AuthAPI.PostRequest(signInUrl, json,
            (response) =>
            {
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(response);

                PlayerPrefs.SetString("id", responseData.user.id);
                PlayerPrefs.SetInt("RememberMe", 1);
                SceneManager.LoadScene(SceneNames.Home);

                MenuManager.Instance.loadingPanel.SetActive(false);
            },
            (error) => 
            {
                FirebaseAuthManager.ErrorResponse errorResponse = JsonUtility.FromJson<FirebaseAuthManager.ErrorResponse>(error);

                if(errorResponse.message.Contains("not verified"))
                {
                    MenuManager.Instance.ShowError(errorResponse.message);

                    MenuManager.Instance.EnablePanel(MenuManager.Instance.signUpOTPVerifyPanel);
                }
                else
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

        if (string.IsNullOrWhiteSpace(passwordInput.text))
        {
            ShowError("Password is empty", passwordInput);
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
        //signInButton.GetComponent<Image>().sprite = confirmBtnIcons[1];
    }

    private void ResetInputFieldVisuals()
    {
        // Optionally reset all input field visuals (e.g., remove error sprites)
        emailInput.GetComponent<Image>().sprite = normalSprite;
        passwordInput.GetComponent<Image>().sprite = normalSprite;
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
        public string password;
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
