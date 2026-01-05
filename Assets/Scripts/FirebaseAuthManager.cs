//using Firebase;
//using Firebase.Auth;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class FirebaseAuthManager : MonoBehaviour
{
    //private FirebaseAuth auth;

    //string googleLoginURL = "login/google";
    //string appleLoginURL = "login/apple";
    //string firebaseLoginURL = "login/facebook";

    //public TextMeshProUGUI errorMessage;

    //void Start()
    //{
    //    InitializeFirebase();
    //}

    //private void InitializeFirebase()
    //{
    //    FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
    //    {
    //        if (task.Result == DependencyStatus.Available)
    //        {
    //            auth = FirebaseAuth.DefaultInstance;
    //            Debug.Log("Firebase initialized");
    //        }
    //        else
    //        {
    //            Debug.LogError("Firebase init failed: " + task.Result);
    //        }
    //    });
    //}

    //#region Google Sign in

    //public async void SignInWithGoogle()
    //{
    //    if (auth == null)
    //    {
    //        Debug.LogError("Firebase Auth not initialized");
    //        return;
    //    }

    //    // Configure Google OAuth using FederatedOAuthProviderData
    //    var providerData = new FederatedOAuthProviderData
    //    {
    //        ProviderId = "google.com", // Fixed provider ID for Google
    //        Scopes = new List<string> { "profile", "email" } // Requested scopes
    //    };

    //    var provider = new FederatedOAuthProvider(providerData);

    //    try
    //    {
    //        // Sign in with Google
    //        AuthResult credential = await auth.SignInWithProviderAsync(provider);
    //        Debug.Log("Google sign-in successful!");
    //        Debug.Log("User: " + credential.User.DisplayName);
    //        Debug.Log("User: " + credential.User.ProviderId);
    //        Debug.Log("Email: " + credential.User.Email);

    //        PlayerPrefs.SetString("myuserid", credential.User.UserId);

    //        LoginBody body = new LoginBody();
    //        body.idToken = credential.User.Id;

    //        string jsonBody = JsonUtility.ToJson(body);

    //        StartCoroutine(AuthAPI.PostRequest(googleLoginURL, jsonBody,
    //            (response) =>
    //            {
    //                Response responseData = JsonUtility.FromJson<Response>(response);
    //                Debug.Log("Sign Up Success: " + responseData.user.firstName);

    //                Debug.Log("Email: " + responseData.user.email);
    //                Debug.Log("token: " + responseData.accessToken);
    //                PlayerPrefs.SetString("Email", responseData.user.email);

    //                MenuManager.Instance.EnablePanel(MenuManager.Instance.signUpOTPVerifyPanel);
    //                MenuManager.Instance.loadingPanel.SetActive(false);
    //            },
    //        (error) =>
    //        {
    //            Debug.LogError("Sign Up Failed: " + error);

    //            ErrorResponse errorResponse = JsonUtility.FromJson<ErrorResponse>(error);

    //            if (errorResponse.message.Contains("Email already exists"))
    //            {
    //                ShowError("Email already exists. Please use a different email");
    //            }
    //            MenuManager.Instance.loadingPanel.SetActive(false);
    //        }));
    //    }
    //    catch (System.Exception ex)
    //    {
    //        Debug.LogError("Google sign-in failed: " + ex.Message);
    //    }
    //}

    //#endregion

    //public async void SingInWithApple()
    //{
    //    if (auth == null)
    //    {
    //        Debug.LogError("Firebase Auth not initialized");
    //        return;
    //    }

    //    // Configure Google OAuth using FederatedOAuthProviderData
    //    var providerData = new FederatedOAuthProviderData
    //    {
    //        ProviderId = "apple.com", // Fixed provider ID for Google
    //        Scopes = new List<string> { "profile", "email" } // Requested scopes
    //    };

    //    var provider = new FederatedOAuthProvider(providerData);

    //    try
    //    {
    //        // Sign in with Google
    //        AuthResult credential = await auth.SignInWithProviderAsync(provider);
    //        Debug.Log("Apple sign-in successful!");
    //        Debug.Log("User: " + credential.User.DisplayName);
    //        Debug.Log("User: " + credential.User.ProviderId);
    //        Debug.Log("Email: " + credential.User.Email);

    //        PlayerPrefs.SetString("myuserid", credential.User.UserId);

    //        LoginBody body = new LoginBody();
    //        body.idToken = credential.User.Id;

    //        string jsonBody = JsonUtility.ToJson(body);

    //        StartCoroutine(AuthAPI.PostRequest(appleLoginURL, jsonBody,
    //            (response) =>
    //            {
    //                Response responseData = JsonUtility.FromJson<Response>(response);
    //                Debug.Log("Sign Up Success: " + responseData.user.firstName);

    //                Debug.Log("Email: " + responseData.user.email);
    //                Debug.Log("token: " + responseData.accessToken);
    //                PlayerPrefs.SetString("Email", responseData.user.email);

    //                MenuManager.Instance.EnablePanel(MenuManager.Instance.signUpOTPVerifyPanel);
    //                MenuManager.Instance.loadingPanel.SetActive(false);
    //            },
    //        (error) =>
    //        {
    //            Debug.LogError("Sign in Failed: " + error);

    //            ErrorResponse errorResponse = JsonUtility.FromJson<ErrorResponse>(error);

    //            if (errorResponse.message.Contains("Email already exists"))
    //            {
    //                ShowError("Email already exists. Please use a different email");
    //            }
    //            MenuManager.Instance.loadingPanel.SetActive(false);
    //        }));
    //    }
    //    catch (System.Exception ex)
    //    {
    //        Debug.LogError("Apple sign-in failed: " + ex.Message);
    //    }
    //}

    //private void ShowError(string message, TMP_InputField field = null)
    //{
    //    errorMessage.gameObject.SetActive(true);
    //    errorMessage.text = message;
    //}

    //[Serializable]
    //public class LoginBody
    //{
    //    public string idToken;
    //}

    //[Serializable]
    //public class Response
    //{
    //    public string accessToken;
    //    public User user;
    //}

    //[Serializable]
    //public class User
    //{
    //    public string id;
    //    public string email;
    //    public string firstName;
    //    public string lastName;
    //    public string role;
    //    public string avatar;
    //    public bool isOnboardingFormFilled;
    //    public string token;
    //}

    //[Serializable]
    //public class ErrorResponse
    //{
    //    public string message;
    //    public string error;
    //    public int statusCode;
    //}
}