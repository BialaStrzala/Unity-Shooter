using UnityEngine;
using PurrNet;
using System.Collections.Generic;
public class MapManager : NetworkBehaviour
{
    [SerializeField] private Map[] maps;
    private Map currentMap;
    public Map CurrentMap => currentMap;

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
        // Menu map
        SetMap(0);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        InstanceHandler.UnregisterInstance<MapManager>();
    }

    public void SetMap(int index)
    {
        //foreach(var map in maps)
            //map.gameObject.SetActive(false);

        currentMap = maps[index];
        //currentMap.gameObject.SetActive(true);
        Debug.Log("Set map: " + currentMap.name);
    }

    public void SetRandomMap()
    {
        int index = Random.Range(0, maps.Length);
        SetMap(index);
    }
}