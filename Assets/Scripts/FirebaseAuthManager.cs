using Firebase;
using Firebase.Auth;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirebaseAuthManager : MonoBehaviour
{
    private FirebaseAuth auth;

    string googleLoginURL = AuthAPI.api + "login/google";
    string appleLoginURL = AuthAPI.api + "login/apple";
    string firebaseLoginURL = AuthAPI.api + "login/facebook";

    public TextMeshProUGUI errorMessage;

    void Start()
    {
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                Debug.Log("Firebase initialized");
            }
            else
            {
                Debug.LogError("Firebase init failed: " + task.Result);
            }
        });
    }

    #region Google Sign in

    public async void SignInWithGoogle()
    {
        if (auth == null)
        {
            Debug.LogError("Firebase Auth not initialized");
            return;
        }

        // Configure Google OAuth using FederatedOAuthProviderData
        var providerData = new FederatedOAuthProviderData
        {
            ProviderId = "google.com", // Fixed provider ID for Google
            Scopes = new List<string> { "profile", "email" } // Requested scopes
        };

        var provider = new FederatedOAuthProvider(providerData);

        try
        {
            // Sign in with Google
            AuthResult credential = await auth.SignInWithProviderAsync(provider);

            var tokenResult = await credential.User.TokenAsync(true);
            //Debug.Log("ID TOKEN : " + tokenResult);
            //Debug.Log("Google sign-in successful!");
            //Debug.Log("User: " + credential.User.DisplayName);
            //Debug.Log("Email: " + credential.User.Email);

            PlayerPrefs.SetString("myuserid", credential.User.UserId);

            LoginBody body = new LoginBody();
            body.idToken = tokenResult;

            string jsonBody = JsonUtility.ToJson(body);

            MenuManager.Instance.loadingPanel.SetActive(true);

            StartCoroutine(AuthAPI.PostRequest(googleLoginURL, jsonBody,
                (response) =>
                {
                    Response responseData = JsonUtility.FromJson<Response>(response);
                    Debug.Log("Google Sign In Success: " + responseData.user.firstName);

                    //Debug.Log("Email: " + responseData.user.email);
                    //Debug.Log("token: " + responseData.accessToken);
                    PlayerPrefs.SetString("Email", responseData.user.email);

                    PlayerPrefs.SetString("id", responseData.user.id);
                    PlayerPrefs.SetInt("RememberMe", 1);
                    SceneManager.LoadScene(SceneNames.Home);

                    // MenuManager.Instance.EnablePanel(MenuManager.Instance.signUpOTPVerifyPanel);
                    // MenuManager.Instance.loadingPanel.SetActive(false);
                },
            (error) =>
            {
                Debug.LogError("Google Authentication Failed");

                ErrorResponse errorResponse = JsonUtility.FromJson<ErrorResponse>(error);

                // if (errorResponse.message.Contains("Email already exists"))
                // {
                MenuManager.Instance.ShowError(errorResponse.message);
                // }
                MenuManager.Instance.loadingPanel.SetActive(false);
            }));
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Google sign-in failed: " + ex.Message);
            MenuManager.Instance.loadingPanel.SetActive(false);
        }
    }

    #endregion

    public async void SignInWithApple()
    {
        if (auth == null)
        {
            Debug.LogError("Firebase Auth not initialized");
            return;
        }

        // Configure Apple OAuth using FederatedOAuthProviderData
        var providerData = new FederatedOAuthProviderData
        {
            ProviderId = "apple.com", // Fixed provider ID for Apple
            Scopes = new List<string> { "profile", "email" } // Requested scopes
        };

        var provider = new FederatedOAuthProvider(providerData);

        try
        {
            // Sign in with Apple
            AuthResult credential = await auth.SignInWithProviderAsync(provider);
            Debug.Log("Apple sign-in successful!");
            //Debug.Log("User: " + credential.User.DisplayName);
            //Debug.Log("User: " + credential.User.ProviderId);
            //Debug.Log("Email: " + credential.User.Email);

            PlayerPrefs.SetString("myuserid", credential.User.UserId);

            LoginBody body = new LoginBody();
            body.idToken = credential.User.UserId;

            string jsonBody = JsonUtility.ToJson(body);

            StartCoroutine(AuthAPI.PostRequest(appleLoginURL, jsonBody,
                (response) =>
                {
                    Response responseData = JsonUtility.FromJson<Response>(response);
                    Debug.Log("Sign Up Success: " + responseData.user.firstName);

                    //Debug.Log("Email: " + responseData.user.email);
                    //Debug.Log("token: " + responseData.accessToken);
                    PlayerPrefs.SetString("Email", responseData.user.email);

                    PlayerPrefs.SetString("id", responseData.user.id);
                    PlayerPrefs.SetInt("RememberMe", 1);
                    SceneManager.LoadScene(SceneNames.Home);

                    // MenuManager.Instance.EnablePanel(MenuManager.Instance.signUpOTPVerifyPanel);
                    // MenuManager.Instance.loadingPanel.SetActive(false);
                },
            (error) =>
            {
                Debug.LogError("Sign in Failed: " + error);

                ErrorResponse errorResponse = JsonUtility.FromJson<ErrorResponse>(error);

                // if (errorResponse.message.Contains("Email already exists"))
                // {
                ShowError(errorResponse.message);
                // }
                MenuManager.Instance.loadingPanel.SetActive(false);
            }));
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Apple sign-in failed: " + ex.Message);
        }
    }

    public void CallAppleLoginAPI(string idToken)
    {
        LoginBody body = new LoginBody();
        body.idToken = idToken;

        string jsonBody = JsonUtility.ToJson(body);

        MenuManager.Instance.loadingPanel.SetActive(true);

        StartCoroutine(AuthAPI.PostRequest(appleLoginURL, jsonBody,
            (response) =>
            {
                Response responseData = JsonUtility.FromJson<Response>(response);
                Debug.Log("Sign Up Success: " + responseData.user.firstName);

                //Debug.Log("Email: " + responseData.user.email);
                //Debug.Log("token: " + responseData.accessToken);
                PlayerPrefs.SetString("Email", responseData.user.email);

                PlayerPrefs.SetString("id", responseData.user.id);
                PlayerPrefs.SetInt("RememberMe", 1);
                SceneManager.LoadScene(SceneNames.Home);

                // MenuManager.Instance.EnablePanel(MenuManager.Instance.signUpOTPVerifyPanel);
                // MenuManager.Instance.loadingPanel.SetActive(false);
            },
        (error) =>
        {
            Debug.LogError("Sign in Failed: " + error);

            ErrorResponse errorResponse = JsonUtility.FromJson<ErrorResponse>(error);

            // if (errorResponse.message.Contains("Email already exists"))
            // {
            MenuManager.Instance.ShowError(errorResponse.message);
            // }
            MenuManager.Instance.loadingPanel.SetActive(false);
        }));
    }

    private void ShowError(string message)
    {
        if (errorMessage != null)
        {
            Debug.Log("Message: " + message);
            errorMessage.gameObject.SetActive(true);
            errorMessage.text = message;
        }
    }

    [Serializable]
    public class LoginBody
    {
        public bool isSignedUpViaMobile;
        public string idToken;
    }

    [Serializable]
    public class Response
    {
        public string accessToken;
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

    [Serializable]
    public class ErrorResponse
    {
        public string message;
        public string error;
        public int statusCode;
    }
}