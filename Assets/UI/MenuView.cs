using UnityEngine;
using PurrNet;
using TMPro;

public class MenuView : View
{
    public void Awake()
    {
        InstanceHandler.RegisterInstance(this);
    }

    public void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<EndGameView>();
    }

    public override void OnShow()
    {
        
    }

    public override void OnHide()
    {
        
    }

}
