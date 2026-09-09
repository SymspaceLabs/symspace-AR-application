using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UnitToggle : MonoBehaviour
{
    public List<TextMeshProUGUI> unitLabel;
    public Image unit1ButtonImage;
    public Image unit2ButtonImage;
    public Sprite selectedSprite;
    public Sprite unSelectedSprite;
    public TextMeshProUGUI btnChildText1;
    public TextMeshProUGUI btnChildText2;

    public string unit1Text;
    public string unit2Text;

    void Start()
    {
        SetUnit1();
    }

    public void SetUnit1()
    {
        foreach (var label in unitLabel)
        {
            label.text = unit1Text;
        }
        unit1ButtonImage.sprite = selectedSprite;
        unit2ButtonImage.sprite = unSelectedSprite;
        btnChildText1.color = Color.white;
        btnChildText2.color = Color.black;
    }

    public void SetUnit2()
    {
        foreach (var label in unitLabel)
        {
            label.text = unit2Text;
        }
        unit2ButtonImage.sprite = selectedSprite;
        unit1ButtonImage.sprite = unSelectedSprite;
        btnChildText2.color = Color.white;
        btnChildText1.color = Color.black;
    }
}
