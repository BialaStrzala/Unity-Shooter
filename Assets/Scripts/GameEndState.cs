using UnityEngine;
using PurrNet.StateMachine;
using System.Collections.Generic;
using PurrNet;
using System.Linq;

public class GameEndState : StateNode
{
    public override void Enter(bool asServer)
    {
        base.Enter(asServer);
        //if(!asServer){return;}
        if(!InstanceHandler.TryGetInstance(out ScoreManager scoreManager))
        {
            Debug.Log($"Failed to get ScoreManager");
            return;
        }

        var winner = scoreManager.GetWinner();
        if(winner == default)
        {
            Debug.Log("Game has ended with no winner");
            return;
        }

        if(!InstanceHandler.TryGetInstance(out GameViewManager gameViewManager))
        {
            Debug.Log("Failed to get GameViewManager");
            return;
        }

        if(!InstanceHandler.TryGetInstance(out EndGameView endGameView))
        {
            Debug.Log("Failed to get end game view");
            return;
        }
        endGameView.SetWinner(winner);
        gameViewManager.ShowView<EndGameView>(true);
        //Debug.Log($"Game has ended with {winner} as the winner");
    }
}
