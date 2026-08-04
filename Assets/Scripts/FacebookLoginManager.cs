using UnityEngine;

#if UNITY_IOS
using Facebook.Unity;
#endif
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using TMPro;

public class FacebookLoginManager : MonoBehaviour
{
    public static FacebookLoginManager Instance;

    string facebookLoginURL = AuthAPI.api + "login/facebook";

    [Header("Debug")]
    [SerializeField] private bool autoInitOnStart = true;

    public TextMeshProUGUI errorMessage;

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (autoInitOnStart)
        {
            InitFacebook();
        }
    }

    /// <summary>
    /// Initialize Facebook SDK
    /// </summary>
    public void InitFacebook()
    {
        #if UNITY_IOS
        if (!FB.IsInitialized)
        {
            FB.Init(OnFacebookInitialized, OnHideUnity);
        }
        else
        {
            FB.ActivateApp();
        }
#endif
    }

#if UNITY_IOS
    private void OnFacebookInitialized()
    {
        if (FB.IsInitialized)
        {
            Debug.Log("Facebook SDK Initialized");
            FB.ActivateApp();
        }
        else
        {
            Debug.LogError("Failed to initialize Facebook SDK");
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        Time.timeScale = isGameShown ? 1 : 0;
    }

#endif

        /// <summary>
        /// Call this from a UI button
        /// </summary>
    public void Login()
    {
#if UNITY_IOS
        if (!FB.IsInitialized)
        {
            Debug.LogWarning("FB not initialized yet");
            return;
        }

        var permissions = new List<string>
        {
            "public_profile",
            "email"
        };

        FB.LogInWithReadPermissions(permissions, OnLoginResult);
#endif
    }

#if UNITY_IOS
    private void OnLoginResult(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            Debug.Log("Facebook login successful");

            APILogin(AccessToken.CurrentAccessToken.TokenString);
            //FetchUserProfile();
        }
        else
        {
            Debug.LogWarning("Facebook login cancelled or failed");

            if (!string.IsNullOrEmpty(result.Error))
            {
                Debug.LogError("FB Error: " + result.Error);
            }
        }
    }

    void APILogin(string accessToken)
    {
        LoginBody body = new LoginBody();
        body.acessToken = accessToken;

        string jsonBody = JsonUtility.ToJson(body);

        MenuManager.Instance.loadingPanel.SetActive(true);

        StartCoroutine(AuthAPI.PostRequest(facebookLoginURL, jsonBody,
            (response) =>
            {
                Response responseData = JsonUtility.FromJson<Response>(response);
                Debug.Log("Facebook login success");

                PlayerPrefs.SetString("id", responseData.user.id);
                PlayerPrefs.SetInt("RememberMe", 1);
                SceneManager.LoadScene("Blogs");
            },
            (error) =>
            {
                Debug.LogError("facebook login failed: " + error);
                FirebaseAuthManager.ErrorResponse errorResponse = JsonUtility.FromJson<FirebaseAuthManager.ErrorResponse>(error);
                // if (errorResponse.statusCode == 401)
                // {
                MenuManager.Instance.ShowError(errorResponse.message);
                // }
                MenuManager.Instance.loadingPanel.SetActive(false);
            }
            ));
    }

    /// <summary>
    /// Fetch basic profile info
    /// </summary>
    private void FetchUserProfile()
    {
        FB.API(
            "/me?fields=id,name,email",
            HttpMethod.GET,
            OnProfileResult
        );
    }

    private void OnProfileResult(IGraphResult result)
    {
        if (result.Error != null)
        {
            Debug.LogError("Graph API Error: " + result.Error);
            return;
        }

        // Example JSON fields:
        // id, name, email
    }

    /// <summary>
    /// Logout
    /// </summary>
    public void Logout()
    {
        if (FB.IsLoggedIn)
        {
            FB.LogOut();
            Debug.Log("Facebook logged out");
        }
    }

    private void ShowError(string message, TMP_InputField field = null)
    {
        errorMessage.gameObject.SetActive(true);
        errorMessage.text = message;
    }

    [Serializable]
    public class LoginBody
    {
        public string acessToken;
    }

    [Serializable]
    public class Response
    {
        public string acessToken;
        public User user;
    }

    [Serializable]
    public class User
    {
        public string id;
        public string email;
        public string firstName;
        public string lastName;
        public string role;
        public string avatar;
        public bool isOnboardingFormFilled;
        public string token;
    }
#endif
}