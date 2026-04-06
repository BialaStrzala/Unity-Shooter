using PurrNet;
using UnityEngine;

public class GunBase : NetworkBehaviour
{
    private Transform cameraTransform;
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private WeaponData data;
    private float nextFireTime;
    protected override void OnSpawned()
    {
        base.OnSpawned();
        //enabled = isOwner;
    }
    public void Initialize(Transform cam)
    {
        cameraTransform = cam;
        Debug.Log("camera initialized at gunbase");
    }
    private void Update()
    {
        //if(!isOwner){return;} //not owner
        if (Input.GetKeyDown(KeyCode.Mouse0)){Shoot();}
    }

    public void SetData(WeaponData newData)
    {
        data = newData;
    }

    private void Shoot()
    {
        Debug.Log("Shot fired");
        //cooldown
        if(Time.time < nextFireTime){return;}
        nextFireTime = Time.time + data.fireRate;
        //didn't hit anything
        if(!Physics.Raycast(cameraTransform.position, cameraTransform.forward,out var hit, data.range, hitLayer)){return;}
        //if hit player
        if(hit.transform.TryGetComponent(out PlayerHealth health))
        {
            health.ChangeHealth(-data.damage);
        }
        Debug.Log($"Hit: {hit.transform.name}");
    }
}
