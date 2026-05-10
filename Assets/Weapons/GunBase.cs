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
    [SerializeField] private ParticleSystem environmentHitEffect;
    [SerializeField] private ParticleSystem playerHitEffect;
    [SerializeField] private Animator animator;
    private Animator[] anims;
    private void Update()
    {
        SetIKTargets();
        if(!isOwner){return;} //not owner
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            Shoot();
        }
    }
    private void Awake()
    {
        Debug.Log("=== GUN DEBUG ===");
        Debug.Log("Object: " + gameObject.name);
    
        // ВАЖНО: Передаем 'true', чтобы искать даже в выключенных дочерних объектах
        anims = GetComponentsInChildren<Animator>(true); 
    
        Debug.Log("Animators found: " + anims.Length);
    
        if (anims.Length > 0)
        {
            animator = anims[0];
            Debug.Log($"Animator FOUND on {animator.gameObject.name}");
        }
        else
        {
            Debug.LogError($"CRITICAL: No Animator found in {gameObject.name} or its children!");
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
            return;
        }

        //if hit player
        if(hit.transform.TryGetComponent(out PlayerHealth playerHealth))
        {
            Debug.Log($"Hit player!!! With: {data.weaponName}, for dmg: -{data.damage}");
            playerHealth.ChangeHealth(-data.damage);
            PlayPlayerHitEffect(playerHealth, playerHealth.transform.InverseTransformPoint(hit.point), hit.normal);
        }
        //hit environment
        else
        {
            Debug.Log($"Hit: {hit.transform.name}");
            PlayEnvironmentHitEffect(hit.point, hit.normal);
        }
    }

    [ObserversRpc(runLocally:true)]
    private void PlayShotEffect()
    {
        if(muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
        if (animator == null)
        {
            Debug.LogError("Animator NOT assigned!");
        }
        else
        {
            Debug.Log("Animator OK: " + animator.gameObject.name);
        }
    }

    [ObserversRpc(runLocally:true)]
    private void PlayEnvironmentHitEffect(Vector3 position, Vector3 normal)
    {
        if(environmentHitEffect)
            {
                var effect = Instantiate(environmentHitEffect, position, Quaternion.LookRotation(normal));
                effect.Play();
                //Destroy(effect.gameObject, 2f);
            }
    }

    [ObserversRpc(runLocally:true)]
    private void PlayPlayerHitEffect(PlayerHealth player, Vector3 localPosition, Vector3 normal)
    {
        if (playerHitEffect && player)
        {
            var effect = Instantiate(playerHitEffect, player.transform.TransformPoint(localPosition), Quaternion.LookRotation(normal));
            effect.Play();
            //Destroy(effect.gameObject, 2f);
        }
    }

    private void SetIKTargets()
    {
        rightIKTarget.SetPositionAndRotation(rightHandTarget.position, rightHandTarget.rotation);
        leftIKTarget.SetPositionAndRotation(leftHandTarget.position, leftHandTarget.rotation);
    }
}
