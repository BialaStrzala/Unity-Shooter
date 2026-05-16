using UnityEngine;
using PurrNet.StateMachine;
using System.Collections.Generic;
using PurrNet;

public class MenuState : StateNode
{
    [SerializeField] public Camera menuCamera;

    // Tracks which players have clicked "Join Match"
    private readonly HashSet<PlayerID> _readyPlayers = new();

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);
        menuCamera.gameObject.SetActive(true);

        if (asServer)
        {
            _readyPlayers.Clear();
            Debug.Log("Entered MenuState (server)");
        }
    }

    /// Called by the MainMenuView button — runs on the local client.
    /// Notifies the server.
    public void OnJoinMatchClicked()
    {
        if (InstanceHandler.TryGetInstance(out GameViewManager gameViewManager))
            gameViewManager.ShowView<WaitForPlayersView>();
        NotifyServerPlayerReady();
    }

    public void EnterMatch()
    {
        OnJoinMatchClicked();
    }

    /// Client → Server RPC. Registers the calling player as ready.
    /// Only the server processes this; it advances the state when everyone is ready.
    [ServerRpc(requireOwnership: false)]
    private void NotifyServerPlayerReady(RPCInfo info = default)
    {
        PlayerID sender = info.sender;

        if (_readyPlayers.Contains(sender))
            return;

        _readyPlayers.Add(sender);
        Debug.Log($"[MenuState] Player ready: {sender} — {_readyPlayers.Count}/{networkManager.players.Count}");

        // Only advance once every connected player has clicked Join
        if (_readyPlayers.Count >= networkManager.players.Count)
        {
            Debug.Log("[MenuState] All players ready — moving to next state");

            if (InstanceHandler.TryGetInstance(out GameViewManager gameViewManager))
                gameViewManager.ShowView<MainGameView>();

            machine.Next();
        }
    }

    public override void Exit(bool asServer)
    {
        base.Exit(asServer);
    }
}