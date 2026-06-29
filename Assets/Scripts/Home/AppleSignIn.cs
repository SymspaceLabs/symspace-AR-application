#if UNITY_IOS
using System;
using System.Text;
using UnityEngine;
using AppleAuth.Native;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Interfaces;
using AppleAuth.Extensions;

public class AppleSignIn : MonoBehaviour
{
    private IAppleAuthManager _appleAuthManager;

    void Start()
    {
        // Initialize AppleAuthManager if supported
        if (AppleAuthManager.IsCurrentPlatformSupported)
        {
            _appleAuthManager = new AppleAuthManager(/*null*/new PayloadDeserializer());
        }
    }

    void Update()
    {
        // Keep AppleAuthManager updated
        if (_appleAuthManager != null)
        {
            _appleAuthManager.Update();
        }
    }

    public void SignInWithApple()
    {
        if (_appleAuthManager == null)
        {
            Debug.LogError("Apple Sign-In not supported on this platform.");
            return;
        }

        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

        Debug.Log("1");
        _appleAuthManager.LoginWithAppleId(
            loginArgs,
            credential =>
            {
                if (credential is IAppleIDCredential appleIdCredential)
                {
                    // Extract the identity token and authorization code
                    Debug.Log("2");
                    string identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
                    Debug.Log("3");
                    string authorizationCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode);
                    Debug.Log("4");

                    if (string.IsNullOrEmpty(identityToken))
                    {
                        Debug.LogError("Apple Sign-In failed: Missing identity token.");
                        return;
                    }


                    // Create a Firebase credential
                    Debug.Log("Apple Sign In Success");
                    // Sign in with Firebase
                    
                    Debug.Log("Apple User ID: " + appleIdCredential.User);
                    Debug.Log("Apple IdentityToken : " + identityToken);

                    //Debug.Log("User : " + appleIdCredential.FullName.GivenName + ", Email " + appleIdCredential.Email);

                    GetComponent<FirebaseAuthManager>().CallAppleLoginAPI(identityToken);

                }
                else
                {
                    Debug.LogError("Apple Sign-In failed: Invalid credential received.");
                }
            },
            error =>
            {
                Debug.Log("6");
                var errorCode = error.GetAuthorizationErrorCode();
                Debug.LogError($"Apple Sign-In Error: {errorCode}");
            }
        );
        Debug.Log("7");
    }
}
#endif