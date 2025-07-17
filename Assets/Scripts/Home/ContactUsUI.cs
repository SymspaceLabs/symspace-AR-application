using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class ContactUsUI : MonoBehaviour
{
    #region Parameters
    [Header("Input Fields")]
    public TMP_InputField firstNameInput;
    public TMP_InputField emailInput;
    public TMP_Dropdown topicDropdown;
    public TMP_InputField messageInput;

    [Space(5)]
    public Button sendButton;
    public string contactUsURL = "contact-us";

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
        sendButton.onClick.AddListener(SendMessage);
    }

    #region API Call
    private void SendMessage()
    {
        if (!CheckInputData())
            return;

        MenuManager.Instance.loadingPanel.SetActive(true);

        JsonDataStructure jsonData = new JsonDataStructure();

        jsonData.firstName = firstNameInput.text;
        jsonData.email = emailInput.text;
        jsonData.topic = topicDropdown.options[topicDropdown.value].text;
        jsonData.message = messageInput.text;

        string json = JsonUtility.ToJson(jsonData);

        StartCoroutine(AuthAPI.PostRequest(contactUsURL, json,
            (response) =>
            {
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(response);

                Debug.Log("Status : " + responseData.status);
                Debug.Log("Message: " + responseData.message);

                firstNameInput.text = "";
                emailInput.text = "";
                messageInput.text = "";
                topicDropdown.value = 0;
                MenuManager.Instance.loadingPanel.SetActive(false);
            },
            (error) =>
            {
                Debug.LogError("Contact message failed: " + error);
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

        if (string.IsNullOrWhiteSpace(firstNameInput.text))
        {
            ShowError("First Name is empty", firstNameInput);
            return false;
        }

        if (!IsValidEmail(emailInput.text))
        {
            ShowError("Email format is incorrect", emailInput);
            return false;
        }

        if(topicDropdown.value == 0)
        {
            ShowError("Please Select Option", null, topicDropdown);
            return false;
        }

        //if (string.IsNullOrEmpty(messageInput.text))
        //{
        //    ShowError("Password is empty");
        //    return false;
        //}

        // All inputs are valid
        MenuManager.Instance.loadingPanel.SetActive(true);
        return true;
    }

    private void ShowError(string message, TMP_InputField field = null, TMP_Dropdown dropDown = null)
    {
        errorMessage.gameObject.SetActive(true);
        errorMessage.text = message;
        if (field != null)
        {
            field.Select();
            field.ActivateInputField();
            field.GetComponent<Image>().sprite = errorInputFieldSprite;
        }
        else if(dropDown != null)
        {
            dropDown.Select();
            dropDown.GetComponent<Image>().sprite = errorInputFieldSprite;
        }
        //sendButton.GetComponent<Image>().sprite = confirmBtnIcons[1];
    }

    private void ResetInputFieldVisuals()
    {
        // Optionally reset all input field visuals (e.g., remove error sprites)
        firstNameInput.GetComponent<Image>().sprite = normalSprite;
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
        public string firstName;
        public string email;
        public string topic;
        public string message;
    }

    private class ResponseData
    {
        public string status;
        public string message;
    }
    #endregion
}
