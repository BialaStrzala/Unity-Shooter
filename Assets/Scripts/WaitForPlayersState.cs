using UnityEngine;
using PurrNet.StateMachine;
using System.Collections.Generic;

public class WaitForPlayersState : StateNode
{
    [SerializeField] private int minPlayers = 1;

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);
        if(!asServer){return;}
        StartCoroutine(WaitForPlayers());
        
    }

    private IEnumerator<PlayerHealth> WaitForPlayers()
    {
        while(networkManager.players.Count < minPlayers)
        {
            yield return null;
        }
        machine.Next();
    }
}
