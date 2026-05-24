using PurrNet;
using UnityEngine;
using System.Collections; // ДОБАВЛЕНО: Нужно для корутины (IEnumerator)

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
    [SerializeField] private Animator gunAnimator;

    private void Update()
    {
        SetIKTargets();
        
        if(!isOwner){return;} //not owner
        
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            Shoot();
        }
    }

    public void SetData(WeaponData newData)
    {
        data = newData;
        
        if (gunAnimator != null && data.animatorController != null)
        {
            gunAnimator.runtimeAnimatorController = data.animatorController;
        }
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
        
        PlayLocalAnimation();

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

    private void PlayLocalAnimation()
    {
        if (gunAnimator != null && gunAnimator.gameObject.activeInHierarchy)
        {
            // ИЗМЕНЕНО: Теперь скрипт вызывает универсальное имя "Shoot"
            gunAnimator.Play("Shoot", 0, 0f);
            StopAllCoroutines(); 
            StartCoroutine(ReturnToIdleAfterShot());
        }
    }

    // Таймер возврата в состояние покоя
    private IEnumerator ReturnToIdleAfterShot()
    {
        yield return new WaitForSeconds(0.15f); // Длина отдачи
        if (gunAnimator != null && gunAnimator.gameObject.activeInHierarchy)
        {
            // ИЗМЕНЕНО: Теперь скрипт вызывает универсальное имя "Idle"
            gunAnimator.Play("Idle", 0, 0f);
        }
    }

    [ObserversRpc(runLocally:true)]
    private void PlayShotEffect()
    {
        if(muzzleFlash != null)
        {
            muzzleFlash.Play();
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
        // Защита от NullReferenceException, если IK не назначен
        if (rightHandTarget == null || leftHandTarget == null || rightIKTarget == null || leftIKTarget == null) return;
        
        rightIKTarget.SetPositionAndRotation(rightHandTarget.position, rightHandTarget.rotation);
        leftIKTarget.SetPositionAndRotation(leftHandTarget.position, leftHandTarget.rotation);
    }
}