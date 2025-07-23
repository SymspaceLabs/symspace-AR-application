using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("Panels")]
    public GameObject signUpInPanel;
    public GameObject signUpPanel;
    public GameObject signUpOTPVerifyPanel;
    public GameObject signInPanel;
    public GameObject forgotPasswordPanel;
    public GameObject forgotOTPVerifyPanel;
    public GameObject resetPasswordPanel;
    public GameObject passwordResetSuccessPanel;
    public GameObject contactUsPanel;
    public GameObject homePanel;

    [Space(20)]
    public GameObject loadingPanel;

    [Space(20)]
    public GameObject lastActivePanel;    

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
    }

    public void EnablePanel(GameObject activePanel)
    {
        // Store the currently active panel before switching
        lastActivePanel = GetCurrentlyActivePanel();

        // Disable all panels
        signUpInPanel.SetActive(false);
        signUpPanel.SetActive(false);
        signUpOTPVerifyPanel.SetActive(false);
        signInPanel.SetActive(false);
        forgotPasswordPanel.SetActive(false);
        forgotOTPVerifyPanel.SetActive(false);
        resetPasswordPanel.SetActive(false);
        passwordResetSuccessPanel.SetActive(false);
        contactUsPanel.SetActive(false);
        homePanel.SetActive(false);

        // Enable the requested panel
        activePanel.SetActive(true);
    }


    private GameObject GetCurrentlyActivePanel()
    {
        if (signUpInPanel.activeSelf) return signUpInPanel;
        if (signUpPanel.activeSelf) return signUpPanel;
        if (signUpOTPVerifyPanel.activeSelf) return signUpOTPVerifyPanel;
        if (signInPanel.activeSelf) return signInPanel;
        if (forgotPasswordPanel.activeSelf) return forgotPasswordPanel;
        if (forgotOTPVerifyPanel.activeSelf) return forgotOTPVerifyPanel;
        if (resetPasswordPanel.activeSelf) return resetPasswordPanel;
        if (passwordResetSuccessPanel.activeSelf) return passwordResetSuccessPanel;
        if (contactUsPanel.activeSelf) return contactUsPanel;
        return null;
    }

    public void GoBackToLastPanel()
    {
        if (lastActivePanel != null)
        {
            EnablePanel(lastActivePanel);
        }
        else
        {
            Debug.LogWarning("No previous panel stored.");
        }
    }
}
