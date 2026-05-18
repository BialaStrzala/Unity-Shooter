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
    
    public Animator animator; 
    private float logTimer;

    public void SetData(WeaponData newData)
    {
        data = newData;
    }

    private void Update()
    {
        // 1. Najpierw twardo sprawdzamy: czy to nasz gracz?
        // Jeśli broń jest w rękach wroga — nie mamy prawa nasłuchiwać LPM ani konfigurować JEGO animatora!
        if (!isOwner) return;

        // 2. Agresywne wyszukiwanie komponentu Animator
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null) animator = GetComponentInChildren<Animator>(true);
            if (animator == null) animator = GetComponentInParent<Animator>();
        }

        // 3. Naprawa Animatora kontrolerem z karty danych (WeaponData)
        if (animator != null && animator.runtimeAnimatorController == null && data != null && data.animatorController != null)
        {
            animator.runtimeAnimatorController = data.animatorController;
        }

        // 4. Działanie IK (kinematyki odwrotnej) dla rąk
        if (rightHandTarget != null && rightIKTarget != null) 
        {
            SetIKTargets();
        }

        // 5. Obsługa naciśnięcia spustu
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        // Ustawienia domyślne na wypadek braku danych
        int currentDamage = 10; 
        float currentRange = 100f;
        float currentFireRate = 0.2f;

        if (data != null)
        {
            currentDamage = data.damage; // Teraz int pasuje do int bez błędów kompilacji!
            currentRange = data.range;
            currentFireRate = data.fireRate;
        }

        // Wyszukiwanie kamery gracza
        if (cameraTransform == null)
        {
            Camera localCam = transform.root.GetComponentInChildren<Camera>();
            if (localCam != null) cameraTransform = localCam.transform;
            else if (Camera.main != null) cameraTransform = Camera.main.transform;

            if (cameraTransform == null) return;
        }

        // Uruchomienie efektów błysku i animacji
        PlayShotEffect();
        
        // Fizyczny promień strzału (Raycast)
        if (!Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hit, currentRange, hitLayer))
        {
            Debug.Log("Promień strzału poleciał w niebo.");
            return;
        }

        // Rejestracja trafień
        if (hit.transform.TryGetComponent(out PlayerHealth playerHealth))
        {
            playerHealth.ChangeHealth(-currentDamage);
            PlayPlayerHitEffect(playerHealth, playerHealth.transform.InverseTransformPoint(hit.point), hit.normal);
            Debug.Log($"Trafiono gracza! Zadane obrażenia: {currentDamage}");
        }
        else
        {
            PlayEnvironmentHitEffect(hit.point, hit.normal);
            Debug.Log("Gdzieś trafiono! Obiekt: " + hit.transform.name);
        }
    }

    [ObserversRpc(runLocally: true)]
    private void PlayShotEffect()
    {
        if (muzzleFlash != null) muzzleFlash.Play();
        
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            // Zamiast Play("PistolAnim") aktywujemy trigger Shoot, 
            // aby zadziałała strzałka przejścia, którą widzieliśmy na zrzucie ekranu!
            animator.SetTrigger("Shoot");
        }
    }

    [ObserversRpc(runLocally: true)]
    private void PlayEnvironmentHitEffect(Vector3 position, Vector3 normal)
    {
        if (environmentHitEffect)
        {
            var effect = Instantiate(environmentHitEffect, position, Quaternion.LookRotation(normal));
            effect.Play();
        }
    }

    [ObserversRpc(runLocally: true)]
    private void PlayPlayerHitEffect(PlayerHealth player, Vector3 localPosition, Vector3 normal)
    {
        if (playerHitEffect && player)
        {
            var effect = Instantiate(playerHitEffect, player.transform.TransformPoint(localPosition), Quaternion.LookRotation(normal));
            effect.Play();
        }
    }

    private void SetIKTargets()
    {   
        if (rightHandTarget == null || rightIKTarget == null || leftIKTarget == null || leftHandTarget == null) return;
        rightIKTarget.SetPositionAndRotation(rightHandTarget.position, rightHandTarget.rotation);
        leftIKTarget.SetPositionAndRotation(leftHandTarget.position, leftHandTarget.rotation);
    }
}