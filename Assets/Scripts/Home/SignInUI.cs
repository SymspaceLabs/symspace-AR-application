using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;
using System.Text.RegularExpressions;

public class SignInUI : MonoBehaviour
{
    #region parameters
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button signInButton;
    public string signInUrl = "login";

    [Space(5)]
    public Sprite normalSprite;
    public TextMeshProUGUI errorMessage;
    public Sprite errorInputFieldSprite;

    //[Space(5)]
    //public Sprite[] confirmBtnIcons;
    #endregion

    private void OnEnable()
    {
        errorMessage.gameObject.SetActive(false);
    }

    void Start()
    {
        signInButton.onClick.AddListener(OnSignInClicked);
    }

    #region API Call
    void OnSignInClicked()
    {
        if (!CheckInputData())
            return;

        //WWWForm form = new WWWForm();
        //form.AddField("email", emailInput.text);
        //form.AddField("password", passwordInput.text);

        JsonDataStructure jsonData = new JsonDataStructure();
        jsonData.email = emailInput.text;
        jsonData.password = passwordInput.text;

        string json = JsonUtility.ToJson(jsonData);


        StartCoroutine(AuthAPI.PostRequest(signInUrl, json,
            (response) =>
            {

                Debug.Log($"Responseeee: {response}");
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(response);
                Debug.Log("Sign IN Success");

                Debug.Log("Access Token: " + responseData.accessToken);
                Debug.Log("User : " + responseData);
                Debug.Log("ID : " + responseData.user.id);
                Debug.Log("Email: " + responseData.user.email);
                Debug.Log("First Name : " + responseData.user.firstName);
                Debug.Log("Last Name : " + responseData.user.lastName);
                Debug.Log("Role : " + responseData.user.role);
                Debug.Log("isOnboardingFormFilled : " + responseData.user.isOnboardingFormFilled);

                PlayerPrefs.SetString("id", responseData.user.id);

                MenuManager.Instance.EnablePanel(MenuManager.Instance.homePanel);
                MenuManager.Instance.loadingPanel.SetActive(false);
            },
            (error) => 
            {
                Debug.LogError("Sign In Failed: " + error); 

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

        if (string.IsNullOrWhiteSpace(passwordInput.text))
        {
            ShowError("Password is empty", passwordInput);
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
        passwordInput.ActivateInputField();

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
