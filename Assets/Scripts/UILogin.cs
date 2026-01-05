using System;
using TMPro;
//using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class UILogin : MonoBehaviour
{
    [SerializeField] private LoginController loginController;

    //PlayerProfile playerProfile;

    //private void OnEnable()
    //{
    //    loginController.OnSignedIn += LoginController_OnSignedIn;
    //    loginController.OnAvatarUpdate += LoginController_OnAvatarUpdate;
    //}

    //private void OnDisable()
    //{
    //    loginController.OnSignedIn -= LoginController_OnSignedIn;
    //    loginController.OnAvatarUpdate -= LoginController_OnAvatarUpdate;
    //}

    //public async void LoginButtonPressed()
    //{
    //    await loginController.InitSignIn();
    //}

    //private void LoginController_OnSignedIn(PlayerProfile profile)
    //{
    //    playerProfile = profile;
    //    string accessToken = AuthenticationService.Instance.AccessToken;
    //    Debug.Log("Access Token: " + accessToken);
    //    Debug.Log($"Id: {playerProfile.playerInfo.Id}");
    //    Debug.Log("Name: " + profile.Name);
    //}
    //private void LoginController_OnAvatarUpdate(PlayerProfile profile)
    //{
    //    playerProfile = profile;
    //}


}