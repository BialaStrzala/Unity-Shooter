using UnityEngine;
using PurrNet.StateMachine;
using System.Collections;
using PurrNet;

public class WaitForPlayersState : StateNode
{
    [SerializeField] private int minPlayers = 1;
    public override void Enter(bool asServer)
    {
        base.Enter(asServer);
        Debug.Log("Entering wait for players state as " + (asServer ? "server" : "client"));
        if (!asServer) { return; }

        Debug.Log("Entered WaitForPlayersState");

        if (!InstanceHandler.TryGetInstance(out GameViewManager gameViewManager))
        {
            Debug.Log("Failed to get GameViewManager");
            return;
        }
        if (!InstanceHandler.TryGetInstance(out WaitForPlayersView waitForPlayersView))
        {
            Debug.Log("Failed to get WaitForPlayersView");
            return;
        }

        waitForPlayersView.SetWaitingText(networkManager.players.Count, minPlayers);
        gameViewManager.ShowView<WaitForPlayersView>();

        StartCoroutine(WaitForPlayers());
    }

    private IEnumerator WaitForPlayers()
    {
        while (networkManager.players.Count < minPlayers)
        {
            if (InstanceHandler.TryGetInstance(out WaitForPlayersView waitForPlayersView))
                waitForPlayersView.SetWaitingText(networkManager.players.Count, minPlayers);

            yield return null;
        }

        if (InstanceHandler.TryGetInstance(out GameViewManager gameViewManager))
            gameViewManager.HideView<WaitForPlayersView>();

        machine.Next();
    }
}