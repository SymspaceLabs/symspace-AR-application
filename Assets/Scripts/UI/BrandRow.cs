using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BrandRow : MonoBehaviour
{
    public Image checkboxImage;
    public Image selectedImage;
    public Image checkmarkImage;
    public Image minusImage;
    public TextMeshProUGUI label;
    public TextMeshProUGUI countText;
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
            if (checkboxImage == null)
                checkboxImage = checkbox.GetComponent<Image>();

            if (checkboxButton == null)
                checkboxButton = checkbox.GetComponent<Button>();

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

            if (minusImage == null)
            {
                Transform child = checkbox.Find("Minus");
                minusImage = child != null ? child.GetComponent<Image>() : null;
            }
        }

        if (label == null)
        {
            Transform child = transform.Find("Label");
            label = child != null ? child.GetComponent<TextMeshProUGUI>() : null;
        }

        if (countText == null)
        {
            Transform child = transform.Find("Items Count");
            countText = child != null ? child.GetComponent<TextMeshProUGUI>() : null;
        }
    }

    public void Setup(string text, int count)
    {
        AutoFind();

        if (label != null)
            label.text = text;

        if (countText != null)
            countText.text = count.ToString();
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
            checkmarkImage.gameObject.SetActive(selected);

        if (minusImage != null)
            minusImage.gameObject.SetActive(!selected && partial);
    }
}
