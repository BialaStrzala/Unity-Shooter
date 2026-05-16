using UnityEngine;
using PurrNet;
using TMPro;
using System;

public class WaitForPlayersView : View
{
    [SerializeField] private TMP_Text waitingText;

    public void Awake()
    {
        InstanceHandler.RegisterInstance(this);
    }

    public void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<EndGameView>();
    }

    public void SetWaitingText(int currentPlayers, int requiredPlayers)
    {
        waitingText.text = "Waiting for players (" + currentPlayers + "/" + requiredPlayers + ")...";
    }

    public override void OnShow()
    {
        
    }

    public override void OnHide()
    {
        
    }

}
