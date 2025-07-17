using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResetPasswordUI : MonoBehaviour
{
    #region Parameters
    public TMP_InputField newPasswordInput;
    public TMP_InputField confirmNewPassword;
    public Button resetButton;
    public string resetURL = "reset-password";

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

        if(newPasswordInput.text != confirmNewPassword.text)
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
        //resetButton.GetComponent<Image>().sprite = confirmBtnIcons[1];
    }

    private void ResetInputFieldVisuals()
    {
        // Optionally reset all input field visuals (e.g., remove error sprites)
        newPasswordInput.GetComponent<Image>().sprite = normalSprite;
        confirmNewPassword.GetComponent<Image>().sprite = normalSprite;
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
