using PurrNet;
using UnityEngine;

public class GunBase : NetworkBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private WeaponData data;
    private float nextFireTime;
    protected override void OnSpawned()
    {
        base.OnSpawned();
        //enabled = isOwner;
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
        //has weapon equipped
        if(!data){return;}
        //Debug.Log($"Shot fired from {data.weaponName}");
        //cooldown
        if(Time.time < nextFireTime){return;}
        nextFireTime = Time.time + data.fireRate;
        //didn't hit anything
        if(!Physics.Raycast(cameraTransform.position, cameraTransform.forward,out var hit, data.range, hitLayer))
        {
            //Debug.Log("Nothing hit");
            return;
        }
        //if hit player
        if(hit.transform.TryGetComponent(out PlayerHealth health))
        {
            Debug.Log($"Hit player!!! With: {data.weaponName}, for dmg: -{data.damage}");
            health.ChangeHealth(-data.damage);
        }
        //Debug.Log($"Hit: {hit.transform.name}");
    }
}
