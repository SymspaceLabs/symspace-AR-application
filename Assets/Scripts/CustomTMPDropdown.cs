using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomTMPDropdown : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        StartCoroutine(DisableFirstItemNextFrame());
    }

    IEnumerator DisableFirstItemNextFrame()
    {
        // wait until dropdown finishes creating items
        yield return new WaitForSeconds(0.1f);

        Transform dropdownList = transform.Find("Dropdown List");
        if (!dropdownList) yield break;

        Transform content = dropdownList.Find("Viewport/Content");
        if (!content || content.childCount == 0 || content.childCount == 2) yield break;

        Debug.Log("Content child count : " + content.childCount);
        Toggle firstItem = content.GetChild(1).GetComponent<Toggle>();
        if (firstItem)
        {
            firstItem.interactable = false;
            TextMeshProUGUI tmpTxt = firstItem.transform.Find("Item Label").GetComponent<TextMeshProUGUI>();
            tmpTxt.fontStyle = FontStyles.Italic;

            tmpTxt.text = "<color=#B0B0B0>" + tmpTxt.text + "</color>";
        }
    }
}