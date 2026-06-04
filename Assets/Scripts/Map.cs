using UnityEngine;
using System.Collections.Generic;

public class Map : MonoBehaviour
{
    [SerializeField] private List<Transform> spawnPoints;

    public List<Transform> SpawnPoints => spawnPoints;
}