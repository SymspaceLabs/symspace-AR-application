using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorRow : MonoBehaviour
{
    public Image swatchImage;
    public Image borderImage;
    public Image selectedImage;
    public Image checkmarkImage;
    public TextMeshProUGUI label;
    public Button checkboxButton;

    private void Awake()
    {
        AutoFind();
    }

    private void AutoFind()
    {
        Transform checkbox = transform.Find("Checkbox");

        if (checkbox != null)
        {
            if (swatchImage == null)
                swatchImage = checkbox.GetComponent<Image>();

            if (checkboxButton == null)
                checkboxButton = checkbox.GetComponent<Button>();

            if (borderImage == null)
            {
                Transform child = checkbox.Find("Border");
                borderImage = child != null ? child.GetComponent<Image>() : null;
            }

            if (selectedImage == null)
            {
                Transform child = checkbox.Find("Selected");
                selectedImage = child != null ? child.GetComponent<Image>() : null;
            }

            if (checkmarkImage == null)
            {
                Transform child = checkbox.Find("Checkmark");
                checkmarkImage = child != null ? child.GetComponent<Image>() : null;
            }
        }

        if (label == null)
        {
            Transform child = transform.Find("Color Name");
            label = child != null ? child.GetComponent<TextMeshProUGUI>() : null;
        }
    }

    public void Setup(string name, string code)
    {
        AutoFind();

        if (label != null)
            label.text = name;

        if (swatchImage != null && !string.IsNullOrEmpty(code))
        {
            Color color;

            if (ColorUtility.TryParseHtmlString(code, out color))
                swatchImage.color = color;
        }
    }

    public void SetLabelColor(Color color)
    {
        if (label != null)
            label.color = color;
    }

    public void SetState(bool selected, bool partial)
    {
        if (selectedImage != null)
            selectedImage.gameObject.SetActive(selected || partial);

        if (checkmarkImage != null)
            checkmarkImage.gameObject.SetActive(selected || partial);
    }
}