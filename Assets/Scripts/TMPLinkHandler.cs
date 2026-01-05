using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TMPLinkHandler : MonoBehaviour, IPointerClickHandler
{
    private TextMeshProUGUI textMeshPro;

    void Awake()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(
            textMeshPro,
            eventData.position,
            eventData.pressEventCamera
        );

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
            string linkId = linkInfo.GetLinkID();

            Debug.Log("Clicked link: " + linkId);

            switch (linkId)
            {
                case "terms":
                    OpenTerms();
                    break;
                case "privacy":
                    OpenPrivacy();
                    break;
            }
        }
    }

    void OpenTerms()
    {
        //Debug.Log("Open Terms popup or URL");
        // Example:
        Application.OpenURL("https://www.symspacelabs.com/terms-and-conditions#terms");
        // or open an in-game panel
    }

    void OpenPrivacy()
    {
        Application.OpenURL("https://www.symspacelabs.com/terms-and-conditions#privacy-policy");
    }
}
