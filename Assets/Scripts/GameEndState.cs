using UnityEngine;
using PurrNet.StateMachine;
using System.Collections.Generic;
using PurrNet;
using System.Linq;

public class GameEndState : StateNode<Dictionary<PlayerID, int>>
{
    public override void Enter(Dictionary<PlayerID, int> roundWins, bool asServer)
    {
        base.Enter(asServer);
        if(!asServer){return;}

        var winner = roundWins.First();
        foreach(var player in roundWins)
        {
            if(player.Value > winner.Value)
            {
                winner = player;
            }
        }

        Debug.Log($"{winner} has won the game!");
        roundWins.Clear();
    }
}
