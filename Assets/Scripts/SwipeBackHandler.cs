using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SwipeBackHandler : MonoBehaviour
{
    [SerializeField] private float swipeThreshold = 75f;

    private Vector2 touchStartPos;
    private float touchStartTime;
    private bool isTracking;
    private bool isOnRuler;


    void Update()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (scene == SceneNames.ARScene || scene == SceneNames.ARFace ||
            scene == SceneNames.ARBodyTracking || scene == SceneNames.ARBodyTrackingMars)
            return;

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput(scene);
#else
        HandleTouchInput(scene);
#endif
    }

    void HandleTouchInput(string scene)
    {
        if (Input.touchCount <= 0) return;
        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                touchStartPos = touch.position;
                touchStartTime = Time.time;
                isOnRuler = IsPointerOnRuler(touch.position);
                isTracking = true;
                break;

            case TouchPhase.Ended:
                if (!isTracking) break;
                Vector2 delta = touch.position - touchStartPos;
                float elapsed = Time.time - touchStartTime;
                bool edgeStart = touchStartPos.x < Screen.width * 0.15f;
                bool onBoardingActive = IsOnboardingActive();
                if (!isOnRuler && delta.x > swipeThreshold && (edgeStart || onBoardingActive) &&
                    Mathf.Abs(delta.y) < delta.x * 0.6f && elapsed < 0.7f)
                    ExecuteBack(scene);
                isTracking = false;
                break;

            case TouchPhase.Canceled:
                isTracking = false;
                break;
        }
    }

    void HandleMouseInput(string scene)
    {
        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
            touchStartTime = Time.time;
            isOnRuler = IsPointerOnRuler(Input.mousePosition);
            isTracking = true;
        }
        else if (Input.GetMouseButtonUp(0) && isTracking)
        {
            Vector2 delta = (Vector2)Input.mousePosition - touchStartPos;
            float elapsed = Time.time - touchStartTime;
            bool edgeStart = touchStartPos.x < Screen.width * 0.15f;
            bool onBoardingActive = IsOnboardingActive();
            if (!isOnRuler && delta.x > swipeThreshold && (edgeStart || onBoardingActive) &&
                Mathf.Abs(delta.y) < delta.x * 0.6f && elapsed < 0.7f)
                ExecuteBack(scene);
            isTracking = false;
        }
    }

    bool IsPointerOnRuler(Vector2 screenPos)
    {
        var eventSystem = EventSystem.current;
        if (eventSystem == null) return false;
        var pointerData = new PointerEventData(eventSystem) { position = screenPos };
        var results = new List<RaycastResult>();
        eventSystem.RaycastAll(pointerData, results);
        for (int i = 0; i < results.Count; i++)
        {
            if (results[i].gameObject.GetComponentInParent<RulerSlider>() != null ||
                results[i].gameObject.GetComponentInParent<PriceRangeHandle>() != null ||
                results[i].gameObject.GetComponentInParent<PriceRangeSlider>() != null)
                return true;
        }
        return false;
    }

    bool IsOnboardingActive()
    {
        var onboarding = UnityEngine.Object.FindFirstObjectByType<OnBoardingUI>();
        return onboarding != null && onboarding.gameObject.activeSelf;
    }

    void ExecuteBack(string scene)
    {
        if (BackStack.GoBack())
            return;

        switch (scene)
        {
            case "Home":
                var menu = FindFirstObjectByType<MenuManager>();
                if (menu != null) menu.GoBackToLastPanel();
                break;

            case "Blogs":
                var onboard = FindFirstObjectByType<OnBoardingUI>();
                if (onboard != null && onboard.gameObject.activeSelf)
                {
                    onboard.GoBack();
                    break;
                }
                var fav = FavoritesManager.Instance;
                if (fav != null && fav.favoritesPanel != null && fav.favoritesPanel.activeSelf)
                {
                    fav.ClosePanel();
                    break;
                }
                var cart = CartManager.Instance;
                if (cart != null && cart.cartPanel != null && cart.cartPanel.activeSelf)
                {
                    cart.CloseCart();
                    break;
                }
                var blogs = FindFirstObjectByType<BlogsUI>();
                if (blogs != null && blogs.blogDetailPage != null && blogs.blogDetailPage.activeSelf)
                {
                    blogs.ShowBlurBlogsList();
                    blogs.blogDetailPage.SetActive(false);
                    blogs.blurBlogDetailPage.SetActive(false);
                    break;
                }
                var cat = FindFirstObjectByType<CategoriesUI>();
                if (cat != null && cat.itemDetailPanel != null && cat.itemDetailPanel.activeSelf)
                {
                    cat.BackToShop();
                }
                break;
        }
    }
}
