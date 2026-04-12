using UnityEngine;
using PurrNet.StateMachine;
using System.Collections.Generic;
using PurrNet;

public class RoundEndState : StateNode<PlayerID>
{
    [SerializeField] private int numberOfRounds = 3;
    [SerializeField] private StateNode spawningState;
    private int roundCount = 0;
    private WaitForSeconds delay = new(3f);
    private Dictionary<PlayerID, int> roundWins = new();

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);
        if(!asServer){return;}
        Debug.Log("Round ended with no winner");
        CheckForGameEnd();
    }

    //override?
    public override void Enter(PlayerID winner, bool asServer)
    {
        base.Enter(asServer);
        if(!asServer){return;}
        
        if(!roundWins.ContainsKey(winner))
        {
            roundWins.Add(winner, 0);
        }
        roundWins[winner]++;
        Debug.Log($"{winner}");
        CheckForGameEnd();
    }

    private void CheckForGameEnd()
    {
        roundCount++;
        if(roundCount >= numberOfRounds)
        {
            machine.Next(roundWins);
            return;
        }
        StartCoroutine(DelayNextState());
    }

    private IEnumerator<WaitForSeconds> DelayNextState()
    {
        yield return delay;
        machine.SetState(spawningState);
    }
}
