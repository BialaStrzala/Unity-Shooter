using UnityEngine;
using PurrNet.StateMachine;
using System.Collections.Generic;
using PurrNet;
using System.Linq;

public class GameEndState : StateNode
{
    [SerializeField] public Camera menuCamera;
    [SerializeField] private StateNode menuState;
    private WaitForSeconds delay = new(5f);
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
        
        DespawnPlayers();
        menuCamera.gameObject.SetActive(true);
        endGameView.SetWinner(winner);
        gameViewManager.ShowView<EndGameView>(true);
        //Debug.Log($"Game has ended with {winner} as the winner");
        if(asServer)
        {
            StartCoroutine(BackToMenu());
        }
    }

    private void DespawnPlayers()
    {
        var allPlayers = FindObjectsByType<PlayerHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach(var player in allPlayers)
        {
            Destroy(player.gameObject);
        }
    }

    private IEnumerator<WaitForSeconds> BackToMenu()
    {
        yield return delay;
        machine.SetState(menuState);
    }

    public override void Exit(bool asServer)
    {
        base.Exit(asServer);
        if (InstanceHandler.TryGetInstance(out GameViewManager gameViewManager))
        {
            gameViewManager.HideView<EndGameView>();
        }
    }
}
