using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_Dropdown))]
public class SizeDropdownHandler : MonoBehaviour
{
    private TMP_Dropdown dropdown;
    private Transform activeDropdownList;
    private Image buttonImage;
    private bool hasSelectedRealSize;

    public Sprite defaultSprite;
    public Sprite selectedSprite;

    void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        buttonImage = GetComponent<Image>();
    }

    void Start()
    {
        if (dropdown != null)
            dropdown.onValueChanged.AddListener(OnSelectionChanged);
        UpdateButtonSprite(dropdown != null ? dropdown.value : 0);
    }

    public void Reset()
    {
        hasSelectedRealSize = false;
    }

    void Update()
    {
        Transform dl = transform.Find("Dropdown List");
        if (dl != null && dl != activeDropdownList)
        {
            activeDropdownList = dl;
            StartCoroutine(DelayedApply(dl, dropdown.value));
        }
        else if (dl == null && activeDropdownList != null)
        {
            activeDropdownList = null;
        }
    }

    private IEnumerator DelayedApply(Transform dl, int index)
    {
        yield return null;
        ApplyIndicator(dl, index);
        ScrollToSelected(dl, index);
        DisablePlaceholder(dl);
    }

    private void OnSelectionChanged(int index)
    {
        if (index > 0)
            hasSelectedRealSize = true;
        if (activeDropdownList != null)
            ApplyIndicator(activeDropdownList, index);
        UpdateButtonSprite(index);
    }

    public void RefreshButtonSprite()
    {
        UpdateButtonSprite(dropdown != null ? dropdown.value : 0);
    }

    private void UpdateButtonSprite(int selectedIndex)
    {
        if (buttonImage == null) return;

        bool isPlaceholder = selectedIndex == 0;

        if (isPlaceholder)
        {
            if (defaultSprite != null)
            {
                buttonImage.sprite = defaultSprite;
                buttonImage.type = Image.Type.Simple;
                if (dropdown != null && dropdown.captionText != null)
                    dropdown.captionText.color = Color.white;
            }
        }
        else
        {
            if (selectedSprite != null)
            {
                buttonImage.sprite = selectedSprite;
                buttonImage.type = Image.Type.Sliced;
                buttonImage.fillCenter = true;
                buttonImage.pixelsPerUnitMultiplier = 0.95f;
                if (dropdown != null && dropdown.captionText != null)
                    dropdown.captionText.color = Color.black;
            }
        }
    }

    private void DisablePlaceholder(Transform dropdownList)
    {
        if (!hasSelectedRealSize) return;

        Transform content = GetContent(dropdownList);
        if (content == null || content.childCount == 0 || dropdown == null) return;

        int offset = content.childCount - dropdown.options.Count;
        if (offset < 0 || offset >= content.childCount) return;

        Transform placeholderItem = content.GetChild(offset);
        Toggle toggle = placeholderItem.GetComponent<Toggle>();
        if (toggle != null)
            toggle.interactable = false;
    }

    private void ApplyIndicator(Transform dropdownList, int selectedIndex)
    {
        Transform content = GetContent(dropdownList);
        if (content == null) return;

        int childCount = content.childCount;
        int optionCount = dropdown != null ? dropdown.options.Count : 0;
        int offset = childCount - optionCount;
        int targetIndex = selectedIndex + offset;

        if (targetIndex < 0 || targetIndex >= childCount) return;

        for (int i = 0; i < childCount; i++)
        {
            Transform item = content.GetChild(i);
            Transform indicator = item.Find("Item Background");
            if (indicator != null)
            {
                Image img = indicator.GetComponent<Image>();
                if (img != null)
                    img.enabled = (i == targetIndex);

                indicator.GetChild(0).gameObject.SetActive(i == targetIndex);
            }
        }
    }

    private Transform GetContent(Transform dropdownList)
    {
        ScrollRect sr = dropdownList.GetComponentInChildren<ScrollRect>();
        return sr != null ? sr.content : null;
    }

    private void ScrollToSelected(Transform dropdownList, int selectedIndex)
    {
        ScrollRect sr = dropdownList.GetComponentInChildren<ScrollRect>();
        if (sr == null || sr.content == null) return;

        int total = sr.content.childCount;
        if (total <= 1) return;

        int optionCount = dropdown != null ? dropdown.options.Count : 0;
        int offset = total - optionCount;
        int targetIndex = selectedIndex + offset;
        targetIndex = Mathf.Clamp(targetIndex, 0, total - 1);

        float normPos = 1.0f - (float)targetIndex / (total - 1);
        sr.verticalNormalizedPosition = Mathf.Clamp01(normPos);
    }
}
