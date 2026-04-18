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
        if(!asServer){return;}

        Debug.Log("Game has ended");
    }
}
