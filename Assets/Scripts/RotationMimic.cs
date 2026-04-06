using PurrNet;
using UnityEngine;

public class RotationMimic : NetworkBehaviour
{
    [SerializeField] private Transform mimicObject;

    void Update()
    {
        if(!mimicObject){return;}
        transform.rotation = mimicObject.rotation;
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();
        enabled = isOwner;
    }
}
