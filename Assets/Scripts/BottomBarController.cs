using UnityEngine;
using UnityEngine.UI;

public class BottomBarController : MonoBehaviour
{
    public static BottomBarController Instance;

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
    public GameObject favoritesPanel;
    public GameObject profilePanel;

    void Awake()
    {
        Instance = this;
    }

    private string currentTab = "home";
    private bool wasCartContextActive;
    private bool wasProfileActive;

    bool IsCartContextActive()
    {
        return (cartPanel != null && cartPanel.activeSelf) ||
               (favoritesPanel != null && favoritesPanel.activeSelf);
    }

    void Start()
    {
        wasCartContextActive = IsCartContextActive();
        wasProfileActive = profilePanel != null && profilePanel.activeSelf;

        if (wasProfileActive)
            SetActiveTab("profile");
        else
            SetActiveTab(wasCartContextActive ? "cart" : "home");

        if (homebtn != null) homebtn.onClick.AddListener(OnHomeClicked);

        if (cartBtn != null)
        {
            Button btn = cartBtn.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(OnCartClicked);
        }

        if (profileBtn != null)
        {
            Button btn = profileBtn.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(OnProfileClicked);
        }

        CartManager.OnCartOpened += OnCartOpenedHandler;

        WireFilterPageControls();
    }

    void WireFilterPageControls()
    {
        foreach (CategoryFilterUI filter in Resources.FindObjectsOfTypeAll<CategoryFilterUI>())
        {
            if (filter == null || filter.gameObject == null)
                continue;
            if (string.IsNullOrEmpty(filter.gameObject.scene.name))
                continue;

            filter.WireFilterControls();
        }
    }

    void OnDestroy()
    {
        CartManager.OnCartOpened -= OnCartOpenedHandler;
    }

    void OnCartOpenedHandler()
    {
        if (profilePanel != null)
            profilePanel.SetActive(false);
    }

    void Update()
    {
        if (cartPanel != null || favoritesPanel != null)
        {
            bool isCartContextActive = IsCartContextActive();
            if (isCartContextActive != wasCartContextActive)
            {
                wasCartContextActive = isCartContextActive;
                if (isCartContextActive)
                    SetActiveTab("cart");
                else if (currentTab == "cart")
                    SetActiveTab(profilePanel != null && profilePanel.activeSelf ? "profile" : "home");
            }
        }

        if (profilePanel != null)
        {
            bool isProfileActive = profilePanel.activeSelf;
            if (isProfileActive != wasProfileActive)
            {
                wasProfileActive = isProfileActive;
                if (isProfileActive)
                {
                    BlurPanelManager.Cover();
                    SetActiveTab("profile");
                }
                else
                {
                    BlurPanelManager.Uncover();
                    if (currentTab == "profile")
                        SetActiveTab("home");
                }
            }
        }
    }

    public void OnHomeClicked()
    {
        if (CartManager.Instance != null)
            CartManager.Instance.CloseCart();
        if (favoritesPanel != null)
            favoritesPanel.SetActive(false);
        if (profilePanel != null)
            profilePanel.SetActive(false);
        SetActiveTab("home");
    }

    void OnCartClicked()
    {
        if (favoritesPanel != null)
            favoritesPanel.SetActive(false);
        SetActiveTab("cart");
    }

    void OnProfileClicked()
    {
        if (CartManager.Instance != null)
            CartManager.Instance.CloseCart();
        if (FavoritesManager.Instance != null)
            FavoritesManager.Instance.ClosePanel();
        SetActiveTab("profile");
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
