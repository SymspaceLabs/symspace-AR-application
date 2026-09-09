using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RadioButtonGroup : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite selectedSprite;
    public Sprite defaultSprite;

    [Header("Text Colors")]
    public Color selectedTextColor = Color.white;
    public Color defaultTextColor = Color.black;

    [Header("State")]
    [SerializeField] private int selectedIndex = 0;

    public event Action<int> onValueChanged;

    private List<Button> buttons;
    private List<Image> images;
    private List<TextMeshProUGUI> texts;

    public int SelectedIndex
    {
        get { return selectedIndex; }
        set
        {
            if (value < 0 || value >= buttons.Count || value == selectedIndex) return;
            selectedIndex = value;
            ApplyVisuals();
            if (onValueChanged != null) onValueChanged(value);
        }
    }

    void Awake()
    {
        buttons = new List<Button>();
        images = new List<Image>();
        texts = new List<TextMeshProUGUI>();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (!child.gameObject.activeSelf) continue;

            Button btn = child.GetComponent<Button>();
            if (btn == null) continue;

            Image img = btn.targetGraphic as Image;
            if (img == null) img = child.GetComponent<Image>();

            buttons.Add(btn);
            images.Add(img);
            texts.Add(child.GetComponentInChildren<TextMeshProUGUI>());

            int index = i;
            btn.onClick.AddListener(delegate { Select(index); });
        }

        if (selectedIndex >= 0 && selectedIndex < buttons.Count)
            Select(selectedIndex);
        else
            ApplyVisuals();
    }

    public void Select(int index)
    {
        if (index < 0 || index >= buttons.Count || index == selectedIndex) return;
        selectedIndex = index;
        ApplyVisuals();
        if (onValueChanged != null) onValueChanged(index);
    }

    void ApplyVisuals()
    {
        for (int i = 0; i < images.Count; i++)
        {
            if (images[i] != null)
                images[i].sprite = (i == selectedIndex) ? selectedSprite : defaultSprite;

            if (texts[i] != null)
                texts[i].color = (i == selectedIndex) ? selectedTextColor : defaultTextColor;
        }
    }
}