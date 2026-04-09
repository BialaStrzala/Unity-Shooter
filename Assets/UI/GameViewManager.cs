using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class GameViewManager : MonoBehaviour
{
    [SerializeField] private List<View> allViews = new();
    [SerializeField] private View defaultView;

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
        foreach(var view in allViews)
        {
            HideViewInternal(view);
        }
        ShowViewInternal(defaultView);
    }

    private void Oestroy()
    {
        InstanceHandler.UnregisterInstance<GameViewManager>();        
    }

    private void HideViewInternal(View view)
    {
        view.canvasGroup.alpha = 0;
        view.OnHide();
    }
    private void ShowViewInternal(View view)
    {
        view.canvasGroup.alpha = 1;
        view.OnShow();
    }

    public void ShowView<T>(bool hideOthers = true) where T : View
    {
        foreach(var view in allViews)
        {
            if(view.GetType() == typeof(T))
            {
                ShowViewInternal(view);
            }
            else
            {
                if(hideOthers)
                    HideViewInternal(view);
            }
        }
    }

    public void HideView<T>() where T : View
    {
        foreach(var view in allViews)
        {
            if(view.GetType() == typeof(T))
            {
                HideViewInternal(view);
            }
        }
    }
}

public abstract class View : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public abstract void OnShow();
    public abstract void OnHide();
}