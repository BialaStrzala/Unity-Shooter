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
        InstanceHandler.UnregisterInstance<WaitForPlayersView>();
    }

    public void SetWaitingText(int currentPlayers, int requiredPlayers)
    {
        waitingText.text = "Waiting for players (" + currentPlayers + "/" + requiredPlayers + ")...";
    }

    public override void OnShow()
    {
        Debug.Log("Show wait for players view");
    }

    public override void OnHide()
    {
        
    }

}
