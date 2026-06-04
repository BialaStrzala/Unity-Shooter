using UnityEngine;
using PurrNet.StateMachine;
using PurrNet;
using System.Collections.Generic;

public class PlayerSpawningState : StateNode
{
    [SerializeField] private PlayerHealth playerPrefab;
    //[SerializeField] private List<Transform> spawnPoints = new();
    private List<Transform> spawnPoints;
    [SerializeField] public Camera menuCamera;

    public override void Enter(bool asServer)
    {
        Debug.Log("Spawning players state");
        menuCamera.gameObject.SetActive(false);

        // Game
        if(InstanceHandler.TryGetInstance(out GameViewManager gameViewManager))
            gameViewManager.ShowView<MainGameView>(true);

        base.Enter(asServer);
        if(!asServer){return;}
        DespawnPlayers();
        var spawnedPlayers = SpawnPlayers();
        Debug.Log("Spawned players as server");
        machine.Next(spawnedPlayers);
    }

    private List<PlayerHealth> SpawnPlayers()
    {
        // Pick map
        if(!InstanceHandler.TryGetInstance(out MapManager mapManager))
        {
            Debug.LogError("MapManager not found");
            return null;
        }
        mapManager.SetRandomMap();
        spawnPoints = mapManager.CurrentMap.SpawnPoints;
        ShuffleSpawnPoints(spawnPoints);
        Debug.Log(spawnPoints[0].name);

        // Spawn players
        var spawnedPlayers = new List<PlayerHealth>();
        int currentSpawnIndex = 0;
        foreach(var player in networkManager.players)
        {
            var spawnPoint = spawnPoints[currentSpawnIndex];
            var newPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            newPlayer.GiveOwnership(player);
            spawnedPlayers.Add(newPlayer);
            currentSpawnIndex++;

            if(currentSpawnIndex >= spawnPoints.Count)
            {
                currentSpawnIndex = 0;
            }
            Debug.Log("Spawned player: " + player + " at " + spawnPoint.name + " in map: " + mapManager.CurrentMap.name);
        }
        return spawnedPlayers;
    }

    private void DespawnPlayers()
    {
        var allPlayers = FindObjectsByType<PlayerHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach(var player in allPlayers)
        {
            Destroy(player.gameObject);
        }
    }

    private void ShuffleSpawnPoints(List<Transform> points)
    {
        for (int i = points.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (points[i], points[randomIndex]) =
                (points[randomIndex], points[i]);
        }
    }

    public override void Exit(bool asServer)
    {
        base.Exit(asServer);
    }
}
