using UnityEngine;
using PurrNet;
using TMPro;

public class EndGameView : View
{
    [SerializeField] private TMP_Text winnerText;

    public void Awake()
    {
        InstanceHandler.RegisterInstance(this);
    }

    public void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<EndGameView>();
    }

    public void SetWinner(PlayerID winner)
    {
        winnerText.text = $"Player {winner.id} wins!";
    }

    public override void OnShow()
    {
        
    }

    public override void OnHide()
    {
        
    }

}
