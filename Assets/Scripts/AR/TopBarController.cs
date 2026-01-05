using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TopBarController : MonoBehaviour
{
    [System.Serializable]
    public class ButtonPanelPair
    {
        public Button button;
        public GameObject panel;
    }

    [Header("Button-Panel Pairs")]
    public List<ButtonPanelPair> buttonPanelPairs;

    private Button currentButton;
    private GameObject currentPanel;

    void Start()
    {
        foreach (var pair in buttonPanelPairs)
        {
            pair.button.onClick.AddListener(() => OnButtonClicked(pair));
        }

        // Initialize first state (optional)
        if (buttonPanelPairs.Count > 0)
            OnButtonClicked(buttonPanelPairs[0]);
    }

    void OnButtonClicked(ButtonPanelPair pair)
    {
        // Re-enable previous button image
        if (currentButton != null)
            currentButton.image.enabled = true;

        // Disable this button's image
        pair.button.image.enabled = false;

        // Hide previous panel
        if (currentPanel != null)
            currentPanel.SetActive(false);

        // Show new one
        if(pair.panel != null)
           pair.panel.SetActive(true);

        // Update references
        if(pair.button != null)
            currentButton = pair.button;

        if(pair.panel != null)
            currentPanel = pair.panel;
    }
}
