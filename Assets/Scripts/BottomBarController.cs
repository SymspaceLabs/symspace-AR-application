using UnityEngine;
using UnityEngine.UI;

public class BottomBarController : MonoBehaviour
{
    public Image homeImage;
    public Button homebtn;
    public Image cartBtn;
    public Image profileBtn;
    public Image arBtn;

    public Sprite homeBlack;
    public Sprite homeBlue;
    public Sprite cartBlack;
    public Sprite cartBlue;
    public Sprite profileBlack;
    public Sprite profileBlue;
    public Sprite arBlack;
    public Sprite arBlue;

    public GameObject cartPanel;

    private string currentTab = "home";
    private bool wasCartActive;

    void Start()
    {
        wasCartActive = cartPanel != null && cartPanel.activeSelf;
        SetActiveTab(wasCartActive ? "cart" : "home");

        if (homebtn != null) homebtn.onClick.AddListener(OnHomeClicked);

        if (cartBtn != null)
        {
            Button btn = cartBtn.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(OnCartClicked);
        }
    }

    void Update()
    {
        if (cartPanel == null) return;
        bool isCartActive = cartPanel.activeSelf;
        if (isCartActive != wasCartActive)
        {
            wasCartActive = isCartActive;
            if (isCartActive)
                SetActiveTab("cart");
            else if (currentTab == "cart")
                SetActiveTab("home");
        }
    }

    public void OnHomeClicked()
    {
        if (CartManager.Instance != null)
            CartManager.Instance.CloseCart();
        SetActiveTab("home");
    }

    void OnCartClicked()
    {
        SetActiveTab("cart");
    }

    public void SetActiveTab(string tab)
    {
        ResetButton(homeImage, homeBlack);
        ResetButton(cartBtn, cartBlack);
        if (profileBtn != null) ResetButton(profileBtn, profileBlack);
        if (arBtn != null) ResetButton(arBtn, arBlack);

        currentTab = tab;

        switch (tab)
        {
            case "home":
                SetSelected(homeImage, homeBlue);
                break;
            case "cart":
                SetSelected(cartBtn, cartBlue);
                break;
            case "profile":
                if (profileBtn != null) SetSelected(profileBtn, profileBlue);
                break;
            case "ar":
                if (arBtn != null) SetSelected(arBtn, arBlue);
                break;
        }
    }

    static void ResetButton(Image btn, Sprite unselected)
    {
        if (btn == null) return;
        btn.sprite = unselected;
        if (btn.transform.childCount > 0)
            btn.transform.GetChild(0).gameObject.SetActive(false);
    }

    static void SetSelected(Image btn, Sprite selected)
    {
        if (btn == null) return;
        btn.sprite = selected;
        if (btn.transform.childCount > 0)
            btn.transform.GetChild(0).gameObject.SetActive(true);
    }
}
