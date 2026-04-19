using PurrNet;
using UnityEngine;

public class GunBase : NetworkBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private WeaponData data;
    private float nextFireTime;
    [SerializeField] private Transform rightHandTarget, leftHandTarget;
    [SerializeField] private Transform rightIKTarget, leftIKTarget;
    [SerializeField] private ParticleSystem muzzleFlash;

    private void Update()
    {
        SetIKTargets();
        if(!isOwner){return;} //not owner
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            Debug.Log("Pew pew");
            Shoot();
        }
    }

    public void SetData(WeaponData newData)
    {
        data = newData;
    }

    private void Shoot()
    {
        //has weapon equipped
        if(!data){return;}
        //cooldown
        if(Time.unscaledTime < nextFireTime){return;}
        nextFireTime = Time.unscaledTime + data.fireRate;

        //animation
        PlayShotEffect();

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

    [ObserversRpc(runLocally:true)]
    private void PlayShotEffect()
    {
        if(muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }

    private void SetIKTargets()
    {
        rightIKTarget.SetPositionAndRotation(rightHandTarget.position, rightHandTarget.rotation);
        leftIKTarget.SetPositionAndRotation(leftHandTarget.position, leftHandTarget.rotation);
    }
}
