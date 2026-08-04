using System;
using System.Text;
using UnityEngine;
#if UNITY_IOS
using AppleAuth.Native;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Interfaces;
using AppleAuth.Extensions;
#endif

public class AppleSignIn : MonoBehaviour
{
    #if UNITY_IOS
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
#endif

    public void SignInWithApple()
    {
        #if UNITY_IOS
        if (_appleAuthManager == null)
        {
            Debug.LogError("Apple Sign-In not supported on this platform.");
            return;
        }

        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

        _appleAuthManager.LoginWithAppleId(
            loginArgs,
            credential =>
            {
                if (credential is IAppleIDCredential appleIdCredential)
                {
                    // Extract the identity token and authorization code
                    string identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
                    string authorizationCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode);

                    if (string.IsNullOrEmpty(identityToken))
                    {
                        Debug.LogError("Apple Sign-In failed: Missing identity token.");
                        return;
                    }

                    GetComponent<FirebaseAuthManager>().CallAppleLoginAPI(identityToken);

                }
                else
                {
                    Debug.LogError("Apple Sign-In failed: Invalid credential received.");
                }
            },
            error =>
            {
                var errorCode = error.GetAuthorizationErrorCode();
                Debug.LogError($"Apple Sign-In Error: {errorCode}");
            }
        );
#endif
    }
}