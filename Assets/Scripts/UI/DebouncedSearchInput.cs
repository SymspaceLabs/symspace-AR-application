using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class DebouncedSearchInput : MonoBehaviour
{
    [Header("Search")]
    [Tooltip("If true, search fires while typing (after debounce). If false, only on submit.")]
    [SerializeField] private bool searchWhileTyping = false;

    [Tooltip("Delay in seconds used when searchWhileTyping is true")]
    [SerializeField] private float debounceSeconds = 0.3f;

    [Tooltip("Save submitted terms into Recent Searches")]
    [SerializeField] private bool saveRecentOnSubmit = true;

    [Tooltip("Invoked after the debounce (typing) or immediately on submit")]
    [SerializeField] private UnityEvent<string> onSearch = new UnityEvent<string>();

    public System.Action<string> onSearchExecuted;

    public float DebounceSeconds
    {
        get { return debounceSeconds; }
        set { debounceSeconds = value; }
    }

    private TMP_InputField input;
    private Coroutine debounceRoutine;

    private void Awake()
    {
        input = GetComponent<TMP_InputField>();
    }

    private void OnEnable()
    {
        if (input == null)
        {
            Debug.LogWarning("DebouncedSearchInput: no TMP_InputField on this object.", this);
            return;
        }

        if (searchWhileTyping)
            input.onValueChanged.AddListener(OnInputValueChanged);

        input.onSubmit.AddListener(OnInputSubmitted);
    }

    private void OnDisable()
    {
        if (input == null)
            return;

        input.onValueChanged.RemoveListener(OnInputValueChanged);
        input.onSubmit.RemoveListener(OnInputSubmitted);

        CancelDebounce();
    }

    public void Execute(string text)
    {
        CancelDebounce();
        Raise(text);
    }

    private void OnInputValueChanged(string text)
    {
        CancelDebounce();
        debounceRoutine = StartCoroutine(DebounceRoutine(text));
    }

    private IEnumerator DebounceRoutine(string text)
    {
        yield return new WaitForSeconds(debounceSeconds);
        debounceRoutine = null;
        Raise(text);
    }

    private void OnInputSubmitted(string text)
    {
        if (saveRecentOnSubmit)
            RecentSearches.Add(text);

        CancelDebounce();
        Raise(text);
    }

    private void CancelDebounce()
    {
        if (debounceRoutine != null)
        {
            StopCoroutine(debounceRoutine);
            debounceRoutine = null;
        }
    }

    private void Raise(string text)
    {
        if (onSearch != null)
            onSearch.Invoke(text);

        if (onSearchExecuted != null)
            onSearchExecuted(text);
    }
}