using PurrNet;
using UnityEngine;
using System.Collections.Generic;

public class ScoreboardView : View
{
    [SerializeField] private Transform scoreboardEntriesParent;
    [SerializeField] private ScoreboardEntry scoreboardEntryPrefab;
    private GameViewManager gameViewManager;

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
    }

    private void Start()
    {
        gameViewManager = InstanceHandler.GetInstance<GameViewManager>();
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<ScoreboardView>();
    }

    public void SetData(Dictionary<PlayerID, ScoreManager.ScoreData> data)
    {
        foreach(Transform child in scoreboardEntriesParent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach(var playerScore in data)
        {
            var entry = Instantiate(scoreboardEntryPrefab, scoreboardEntriesParent);
            entry.SetData(playerScore.Key.id.ToString(), playerScore.Value.kills, playerScore.Value.deaths);
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            gameViewManager.ShowView<ScoreboardView>(false);
        }
        if(Input.GetKeyUp(KeyCode.Tab))
        {
            gameViewManager.HideView<ScoreboardView>();
        }
    }
    public override void OnHide()
    {
        
    }

    public override void OnShow()
    {
        
    }
}
