using System;
using UnityEngine;

public class BackPage : MonoBehaviour
{
    [SerializeField] private GameObject backTarget;
    [SerializeField] private bool coversBlur = true;

    private Action backAction;

    protected virtual void OnOpened()
    {
        if (coversBlur) BlurPanelManager.Cover();
    }

    protected virtual void OnClosed()
    {
        if (coversBlur) BlurPanelManager.Uncover();
    }

    protected virtual void OnEnable()
    {
        OnOpened();
        backAction = GoBack;
        BackStack.Push(backAction);
    }

    protected virtual void OnDisable()
    {
        if (backAction != null)
        {
            BackStack.Remove(backAction);
            backAction = null;
        }
        OnClosed();
    }

    protected virtual void OnDestroy()
    {
        if (backAction != null)
        {
            BackStack.Remove(backAction);
            backAction = null;
        }
    }

    public void GoBack()
    {
        if (this == null) return;

        GameObject target = backTarget != null ? backTarget : gameObject;
        if (target != null)
            target.SetActive(false);
    }
}
