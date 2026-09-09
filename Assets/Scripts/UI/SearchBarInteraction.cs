using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class SearchBarInteraction : MonoBehaviour
{
    [Header("Expand")]
    public float expandWidth = 6f;
    public float expandDuration = 0.2f;

    [Header("Dim")]
    public float dimAlpha = 0.45f;
    public float dimDuration = 0.2f;

    [Header("Recent")]
    public bool showRecentOnFocus = false;
    public float recentStagger = 0.03f;
    public float recentRevealDuration = 0.12f;
    public float recentRowHeight = 40f;
    public float recentTopOffset = 8f;
    public float recentSpacing = 2f;
    public float recentFontSize = 20f;

    public System.Action<string> onRecentSelected;

    private RectTransform barRect;
    private float restDeltaX;
    private Image dimOverlay;
    private Canvas searchCanvas;
    private bool isAR;
    private Coroutine expandRoutine;
    private Coroutine dimRoutine;
    private Coroutine recentRoutine;
    private bool expanded;
    private bool wired;

    private RectTransform recentContainer;
    private readonly List<RectTransform> recentRows = new List<RectTransform>();
    private TMP_FontAsset recentFont;

    void Awake()
    {
        barRect = GetComponent<RectTransform>();
        if (barRect != null)
            restDeltaX = barRect.sizeDelta.x;

        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        isAR = scene.Contains("AR") || scene == "Hand Tracking";

        BuildDimOverlay();
    }

    void OnEnable()
    {
        WireInput();
    }

    void WireInput()
    {
        if (wired)
            return;

        TMP_InputField field = GetComponent<TMP_InputField>();
        if (field == null)
            return;

        field.onSelect.AddListener((s) => SetFocused(true));
        field.onDeselect.AddListener((s) => SetFocused(false));
        field.onSubmit.AddListener((s) => SetFocused(false));

        if (field.textComponent != null)
            recentFont = field.textComponent.font;

        wired = true;
    }

    public void SetFocused(bool focused)
    {
        StopAllRoutines();

        if (dimOverlay != null)
            dimOverlay.raycastTarget = focused;

        if (focused)
        {
            if (isAR)
            {
                Transform searchParent = transform.parent;
                if (searchParent != null)
                {
                    searchCanvas = searchParent.GetComponent<Canvas>();
                    if (searchCanvas == null)
                        searchCanvas = searchParent.gameObject.AddComponent<Canvas>();
                    searchCanvas.overrideSorting = true;
                    searchCanvas.sortingOrder = 10;
                }
            }
        }
        else
        {
            if (searchCanvas != null)
            {
                Destroy(searchCanvas);
                searchCanvas = null;
            }
        }

        Canvas myCanvas = GetComponentInParent<Canvas>();
        if (myCanvas != null)
        {
            Canvas[] all = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (Canvas c in all)
            {
                if (c == myCanvas || c == searchCanvas) continue;
                GraphicRaycaster gr = c.GetComponent<GraphicRaycaster>();
                if (gr != null)
                    gr.enabled = !focused;
            }
        }

        if (focused)
        {
            if (!expanded)
                expandRoutine = StartCoroutine(ExpandRoutine());
            dimRoutine = StartCoroutine(FadeDimRoutine(dimAlpha));
            if (showRecentOnFocus)
                ShowRecent();
        }
        else
        {
            if (expanded)
                expandRoutine = StartCoroutine(CollapseRoutine());
            dimRoutine = StartCoroutine(FadeDimRoutine(0f));
            HideRecent();
        }
    }

    void Update()
    {
        if (!expanded)
            return;

        Vector2 pos = Vector2.zero;
        bool pressed = false;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                pressed = true;
                pos = touch.position;
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            pressed = true;
            pos = Input.mousePosition;
        }

        if (!pressed || EventSystem.current == null)
            return;

        if (barRect != null && IsPointerInside(barRect, pos))
            return;

        if (recentContainer != null &&
            recentContainer.gameObject.activeInHierarchy &&
            IsPointerInside(recentContainer, pos))
            return;

        ClearFocus();
    }

    private static bool IsPointerInside(RectTransform rt, Vector2 screenPos)
    {
        if (rt == null)
            return false;

        Canvas canvas = rt.GetComponentInParent<Canvas>();
        Camera cam = canvas != null ? canvas.worldCamera : null;

        return RectTransformUtility.RectangleContainsScreenPoint(
            rt, screenPos, cam);
    }

    IEnumerator ExpandRoutine()
    {
        if (barRect == null || expanded)
        {
            expandRoutine = null;
            yield break;
        }

        expanded = true;
        float from = barRect.sizeDelta.x;

        yield return UIAnim.CoTween(
            expandDuration,
            UIAnim.EaseOutCubic,
            (t) =>
            {
                if (barRect == null) return;
                BarDeltaX(Mathf.LerpUnclamped(from, restDeltaX + expandWidth, t));
            });

        if (barRect != null)
            BarDeltaX(restDeltaX + expandWidth);

        expandRoutine = null;
    }

    IEnumerator CollapseRoutine()
    {
        if (barRect == null || !expanded)
        {
            expandRoutine = null;
            yield break;
        }

        expanded = false;
        float from = barRect.sizeDelta.x;

        yield return UIAnim.CoTween(
            expandDuration,
            UIAnim.EaseInCubic,
            (t) =>
            {
                if (barRect == null) return;
                BarDeltaX(Mathf.LerpUnclamped(from, restDeltaX, t));
            });

        if (barRect != null)
            BarDeltaX(restDeltaX);

        expandRoutine = null;
    }

    void BarDeltaX(float deltaX)
    {
        Vector2 size = barRect.sizeDelta;
        size.x = deltaX;
        barRect.sizeDelta = size;
    }

    IEnumerator FadeDimRoutine(float target)
    {
        if (dimOverlay == null)
        {
            dimRoutine = null;
            yield break;
        }

        float from = dimOverlay.color.a;
        bool fadeOut = target < from;

        yield return UIAnim.CoTween(
            dimDuration,
            fadeOut ? UIAnim.EaseInCubic : UIAnim.EaseOutCubic,
            (t) =>
            {
                if (dimOverlay == null) return;
                Color c = dimOverlay.color;
                c.a = Mathf.LerpUnclamped(from, target, t);
                dimOverlay.color = c;
            });

        if (dimOverlay != null)
        {
            Color c = dimOverlay.color;
            c.a = target;
            dimOverlay.color = c;
        }

        dimRoutine = null;
    }

    void BuildDimOverlay()
    {
        dimOverlay = null;

        Transform parent = transform.parent;
        if (parent == null)
            return;

        Canvas canvas = GetComponentInParent<Canvas>();
        Transform overlayParent = isAR && canvas != null ? canvas.transform : parent;

        GameObject overlay = new GameObject(
            "Search Dim Overlay",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));

        overlay.transform.SetParent(overlayParent, false);
        overlay.transform.SetAsLastSibling();

        RectTransform rt = overlay.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;

        // Expand the overlay beyond its parent so it covers the full screen even
        // when a SafeArea component shrinks the parent on notched devices.
        Rect safe = Screen.safeArea;
        float sw = Screen.width;
        float sh = Screen.height;
        float cw = canvas != null ? canvas.pixelRect.width : sw;
        float ch = canvas != null ? canvas.pixelRect.height : sh;

        float left   = (safe.x / sw) * cw;
        float right  = ((sw - safe.xMax) / sw) * cw;
        float bottom = (safe.y / sh) * ch;
        float top    = ((sh - safe.yMax) / sh) * ch;

        rt.offsetMin = new Vector2(-left, -bottom);
        rt.offsetMax = new Vector2(right, top);

        Image image = overlay.GetComponent<Image>();
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        image.sprite = Sprite.Create(
            tex,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f));
        image.color = new Color(0f, 0f, 0f, 0f);
        image.raycastTarget = false;

        overlay.AddComponent<SearchDimClick>().Setup(this);

        dimOverlay = image;

        if (barRect != null)
            barRect.SetAsLastSibling();
    }

    void ClearFocus()
    {
        TMP_InputField field = GetComponent<TMP_InputField>();
        if (field != null)
            field.DeactivateInputField();
        else
            SetFocused(false);
    }

    void ShowRecent()
    {
        List<string> items = RecentSearches.Get();
        if (items.Count == 0)
        {
            HideRecent();
            return;
        }

        BuildRecentContainer();

        foreach (RectTransform row in recentRows)
        {
            if (row != null && row.gameObject != null)
                Destroy(row.gameObject);
        }
        recentRows.Clear();

        for (int i = 0; i < items.Count; i++)
        {
            string term = items[i];
            CreateRecentRow(term, i);
        }

        recentContainer.gameObject.SetActive(true);
        recentRoutine = StartCoroutine(RevealRecentRoutine());
    }

    void HideRecent()
    {
        if (recentRoutine != null)
        {
            StopCoroutine(recentRoutine);
            recentRoutine = null;
        }

        if (recentContainer != null)
            recentContainer.gameObject.SetActive(false);
    }

    void BuildRecentContainer()
    {
        if (recentContainer != null)
            return;

        GameObject go = new GameObject(
            "Recent Searches",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(VerticalLayoutGroup),
            typeof(ContentSizeFitter));

        go.transform.SetParent(barRect, false);

        Image panel = go.GetComponent<Image>();
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        panel.sprite = Sprite.Create(
            tex,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f));
        panel.color = new Color(0.11f, 0.12f, 0.15f, 0.98f);
        panel.raycastTarget = true;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -(barRect.rect.height + recentTopOffset));
        rt.sizeDelta = new Vector2(0f, 0f);

        VerticalLayoutGroup layout = go.GetComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = recentSpacing;
        layout.padding = new RectOffset(0, 0, 4, 4);

        ContentSizeFitter fitter = go.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        recentContainer = rt;
        recentContainer.gameObject.SetActive(false);

        barRect.SetAsLastSibling();
    }

    void CreateRecentRow(string term, int index)
    {
        if (recentContainer == null)
            return;

        GameObject row = new GameObject(
            "Recent " + index,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button),
            typeof(CanvasGroup));

        row.transform.SetParent(recentContainer, false);

        RectTransform rt = row.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(0f, recentRowHeight);

        Image bg = row.GetComponent<Image>();
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        bg.sprite = Sprite.Create(
            tex,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f));
        bg.color = new Color(0.17f, 0.18f, 0.22f, 1f);
        bg.raycastTarget = true;

        Button button = row.GetComponent<Button>();
        button.targetGraphic = bg;
        button.transition = Selectable.Transition.ColorTint;
        button.colors = new ColorBlock
        {
            colorMultiplier = 1f,
            fadeDuration = 0.1f,
            normalColor = new Color(0.17f, 0.18f, 0.22f, 1f),
            highlightedColor = new Color(0.28f, 0.3f, 0.36f, 1f),
            pressedColor = new Color(0.1f, 0.11f, 0.14f, 1f),
            selectedColor = new Color(0.28f, 0.3f, 0.36f, 1f),
            disabledColor = new Color(0.17f, 0.18f, 0.22f, 1f),
        };

        button.onClick.AddListener(() => OnRecentClicked(term));

        GameObject labelGo = new GameObject(
            "Label",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));

        labelGo.transform.SetParent(rt, false);

        RectTransform lrt = labelGo.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = new Vector2(14f, 0f);
        lrt.offsetMax = new Vector2(-14f, 0f);

        TextMeshProUGUI label = labelGo.GetComponent<TextMeshProUGUI>();
        if (recentFont != null)
            label.font = recentFont;
        label.fontSize = recentFontSize;
        label.color = new Color(0.92f, 0.92f, 0.95f, 1f);
        label.alignment = TextAlignmentOptions.Left;
        label.raycastTarget = false;
        label.text = term;

        CanvasGroup group = row.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        recentRows.Add(rt);
    }

    void OnRecentClicked(string term)
    {
        TMP_InputField field = GetComponent<TMP_InputField>();
        if (field != null)
            field.SetTextWithoutNotify(term);

        if (onRecentSelected != null)
            onRecentSelected(term);

        ClearFocus();
    }

    IEnumerator RevealRecentRoutine()
    {
        if (recentContainer == null)
        {
            recentRoutine = null;
            yield break;
        }

        for (int i = 0; i < recentRows.Count; i++)
        {
            RectTransform row = recentRows[i];
            if (row == null)
                continue;

            CanvasGroup group = row.GetComponent<CanvasGroup>();
            if (group == null)
                continue;

            float delay = i * recentStagger;
            float timer = 0f;
            while (timer < delay)
            {
                timer += Time.unscaledDeltaTime;
                yield return null;
            }

            yield return UIAnim.CoTween(
                recentRevealDuration,
                UIAnim.EaseOutCubic,
                (t) =>
                {
                    if (row == null || group == null)
                        return;

                    group.alpha = t;
                    group.blocksRaycasts = t >= 1f;
                    group.interactable = t >= 1f;
                });

            if (group != null)
            {
                group.alpha = 1f;
                group.blocksRaycasts = true;
                group.interactable = true;
            }
        }

        recentRoutine = null;
    }

    class SearchDimClick : MonoBehaviour, IPointerClickHandler
    {
        private SearchBarInteraction owner;

        public void Setup(SearchBarInteraction o)
        {
            owner = o;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (owner != null)
                owner.ClearFocus();
        }
    }

    void StopAllRoutines()
    {
        if (expandRoutine != null)
        {
            StopCoroutine(expandRoutine);
            expandRoutine = null;
        }

        if (dimRoutine != null)
        {
            StopCoroutine(dimRoutine);
            dimRoutine = null;
        }

        if (recentRoutine != null)
        {
            StopCoroutine(recentRoutine);
            recentRoutine = null;
        }
    }

    void OnDisable()
    {
        if (recentContainer != null)
            recentContainer.gameObject.SetActive(false);
    }
}