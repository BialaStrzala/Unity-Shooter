using UnityEngine;
using PurrNet.StateMachine;
using System.Collections.Generic;
using PurrNet;

public class RoundEndState : StateNode
{
    [SerializeField] private int numberOfRounds = 3;
    [SerializeField] private StateNode spawningState;
    private int roundCount = 0;
    private WaitForSeconds delay = new(3f);

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);
        if(!asServer){return;}
        
        CheckForGameEnd();
    }

    private void CheckForGameEnd()
    {
        roundCount++;
        if(roundCount >= numberOfRounds)
        {
            machine.Next();
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
