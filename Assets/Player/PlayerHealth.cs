using UnityEngine;
using PurrNet;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private SyncVar<int> health = new(100);
    [SerializeField] private int selfLayer, otherLayer;

    public int GetHealth => health.value;

    protected override void OnSpawned()
    {
        base.OnSpawned();
        var actualLayer = isOwner ? selfLayer : otherLayer;
        SetLayerResursive(gameObject, actualLayer);

        if (isOwner)
        {
            health.onChanged += OnHealthChanged;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        health.onChanged -= OnHealthChanged;
    }

    private void SetLayerResursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach(Transform child in obj.transform)
        {
            SetLayerResursive(child.gameObject, layer);
        }
    }
    
    [ServerRpc(requireOwnership:false)]
    public void ChangeHealth(int amount) 
    { 
        health.value += amount; 
        Debug.Log($"Changed health: {health}/100");
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnHealthChanged(int newHealth)
    {
        InstanceHandler.GetInstance<MainGameView>().UpdateHealth(newHealth);
    }
}