using TMPro;
using UnityEngine;

public class ProfilePage : BackPage
{
    public TextMeshProUGUI email_TMP;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (email_TMP != null)
            email_TMP.text = PlayerPrefs.GetString("Email", "No Email Found");
    }
}
