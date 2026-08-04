using UnityEngine;
using UnityEngine.SceneManagement;

public class SwipeBackHandler : MonoBehaviour
{
    [SerializeField] private float swipeThreshold = 75f;

    private Vector2 touchStartPos;
    private float touchStartTime;
    private bool isTracking;


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
                isTracking = true;
                break;

            case TouchPhase.Ended:
                if (!isTracking) break;
                Vector2 delta = touch.position - touchStartPos;
                float elapsed = Time.time - touchStartTime;
                if (delta.x > swipeThreshold && touchStartPos.x < Screen.width * 0.15f &&
                    Mathf.Abs(delta.y) < delta.x * 0.6f && elapsed < 0.5f)
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
            isTracking = true;
        }
        else if (Input.GetMouseButtonUp(0) && isTracking)
        {
            Vector2 delta = (Vector2)Input.mousePosition - touchStartPos;
            float elapsed = Time.time - touchStartTime;
            if (delta.x > swipeThreshold && touchStartPos.x < Screen.width * 0.15f &&
                Mathf.Abs(delta.y) < delta.x * 0.6f && elapsed < 0.5f)
                ExecuteBack(scene);
            isTracking = false;
        }
    }

    void ExecuteBack(string scene)
    {
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
                    blogs.blurBlogsPanel.SetActive(true);
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
