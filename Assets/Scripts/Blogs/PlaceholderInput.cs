using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_InputField))]
public class PlaceholderInput : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Tooltip("Value shown when the field has no user input. If empty, the input field's placeholder child text is used.")]
    public string defaultPlaceholder = "";

    TMP_InputField field;
    TextMeshProUGUI defaultPlaceholder_txt;

    public TextMeshProUGUI unitText;
    public Color blueColor = Color.blue;

    void Awake()
    {
        field = GetComponent<TMP_InputField>();
    }

    void Start()
    {
        if (field == null) return;

        if (string.IsNullOrEmpty(defaultPlaceholder) && field.placeholder != null)
        {
            var t = field.placeholder.GetComponentInChildren<TextMeshProUGUI>();
            if (t != null)
            {
                defaultPlaceholder_txt = t;
                defaultPlaceholder = t.text;
            }
        }
    }

    void Reset()
    {
        if (field == null) field = GetComponent<TMP_InputField>();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (field == null) return;

        if (field.text.Length == 0)
        {
            field.text = "";
            defaultPlaceholder_txt.text = "";
            field.ActivateInputField();
        }

        if(unitText != null)
        {
            unitText.color = blueColor;
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (field == null) return;

        if(field.text.Length == 0)
        {
            defaultPlaceholder_txt.text = defaultPlaceholder;

            if(unitText != null)
            {
                unitText.color = Color.black;
            }
        }
        else
        {
            if (unitText != null)
            {
                unitText.color = blueColor;
            }
        }
    }
}
