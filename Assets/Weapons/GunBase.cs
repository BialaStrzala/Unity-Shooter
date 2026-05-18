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
        // --- БРОНЯ ОТ СЛОМАННОГО ИНСПЕКТОРА ---
        // Сбрасываем Аниматор, если он пустой ИЛИ если Инспектор подсунул нам мертвый файл из папки
        if (animator == null || !animator.gameObject.activeInHierarchy)
        {
            // 1. Сначала ищем живой компонент прямо на себе
            animator = GetComponent<Animator>();
            
            // 2. Если на себе нет, ищем строго на 1 уровень выше (корень префаба пушки)
            if (animator == null && transform.parent != null) 
            {
                animator = transform.parent.GetComponent<Animator>();
            }
        }
        // --------------------------------------

        if (Input.GetButtonDown("Fire1") && !isOwner)
        {
            Debug.LogError($"[СЕТЬ] Попытка выстрела заблокирована! Я не являюсь владельцем этой пушки (isOwner = false).");
        }

        // 1. Мониторинг статуса в консоли
        logTimer += Time.deltaTime;
        if (logTimer >= 1f)
        {
            logTimer = 0f;
            Debug.Log($"[GunBase Статус] Аниматор: {(animator != null ? "ОК" : "НЕТ")}, Рука: {(rightHandTarget != null ? "ОК" : "НЕТ")}");
        }

        // 2. Лечение Аниматора контроллером из карточки WeaponData
        if (animator != null && animator.runtimeAnimatorController == null && data != null && data.animatorController != null)
        {
            animator.runtimeAnimatorController = data.animatorController;
        }

        // 3. Работа IK для рук
        if (rightHandTarget != null && rightIKTarget != null) 
        {
            SetIKTargets();
        }

        // КРИТИЧЕСКИЙ СЕТЕВОЙ БАРЬЕР: Ввод с мышки слушает только владелец пушки
        if (!isOwner) return;

        // 4. Обработка нажатия курка
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        int currentDamage = 10; 
        float currentRange = 100f;
        float currentFireRate = 0.2f;

        if (data != null)
        {
            currentDamage = data.damage;
            currentRange = data.range;
            currentFireRate = data.fireRate;
        }

        if (Time.unscaledTime < nextFireTime) return;
        nextFireTime = Time.unscaledTime + currentFireRate;

        // Поиск камеры игрока
        if (cameraTransform == null)
        {
            Camera localCam = transform.root.GetComponentInChildren<Camera>();
            if (localCam != null) cameraTransform = localCam.transform;
            else if (Camera.main != null) cameraTransform = Camera.main.transform;

            if (cameraTransform == null) return;
        }

        // Запуск эффектов и анимации (разойдется по сети через RPC)
        //PlayShotEffect
        PlayLocalShotEffect();
        
        // Физический луч выстрела
        if (!Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hit, currentRange, hitLayer))
        {
            Debug.Log("Луч выстрела улетел в небо.");
            return;
        }

        // Регистрация попаданий
        if (hit.transform.TryGetComponent(out PlayerHealth playerHealth))
        {
            playerHealth.ChangeHealth(-currentDamage);
            PlayPlayerHitEffect(playerHealth, playerHealth.transform.InverseTransformPoint(hit.point), hit.normal);
            Debug.Log($"Попали в игрока! Нанесено урона: {currentDamage}");
        }
        else
        {
            PlayEnvironmentHitEffect(hit.point, hit.normal);
            Debug.Log("Куда-то попали! Объект: " + hit.transform.name);
        }
    }

/*    [ObserversRpc(runLocally: true)]
    private void PlayShotEffect()
    {
        if (muzzleFlash != null) muzzleFlash.Play();
        
        if (animator != null)
        {
            // ЭТА СТРОКА ПОКАЖЕТ КТО ИМЕННО ПОЛУЧАЕТ ТРИГГЕР
            Debug.Log($"[ОТЛАДКА] Отправляю триггер Shoot на объект: {animator.gameObject.name}", animator.gameObject);
            
            animator.SetTrigger("Shoot");
        }
    }
*/
    private void PlayLocalShotEffect()
    {
        Debug.Log("---> ШАГ 1: Метод визуализации запущен!");

        if (muzzleFlash != null) muzzleFlash.Play();
        
        if (animator == null)
        {
            animator = GetComponent<Animator>(); // Проверяем на самом себе
            
            if (animator == null && transform.parent != null) 
            {
                animator = transform.parent.GetComponent<Animator>(); // Проверяем строго на родительской детали
            }
        }

        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("---> ШАГ 3 ОШИБКА: У Аниматора пропал контроллер (runtimeAnimatorController == null)!");
            return;
        }

        Debug.Log("---> ШАГ 4 УСПЕХ: Аниматор готов! Врубаем отдачу PistolAnim!");
        
        // Временно отключаем IK, если он блокирует руки
        // ikWeight = 0f; 

        animator.Play("PistolAnim", 0, 0f);
        
        StopAllCoroutines(); 
        StartCoroutine(ReturnToIdleAfterShot());
    }
    private System.Collections.IEnumerator ReturnToIdleAfterShot()
    {
        yield return new WaitForSeconds(0.15f); // Длина твоей отдачи
        
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.Play("PistolIdle", 0, 0f);
        }
    }
    /*
    private System.Collections.IEnumerator ReturnToIdleAfterShot()
    {
        // Подождем 0.15 секунды (время, пока кубик делает откат назад)
        // Можешь настроить это число под длину своей анимации отдачи
        yield return new WaitForSeconds(0.15f);
        
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            // Возвращаемся в Idle также через CrossFade, чтобы прервать PistolAnim
            animator.CrossFadeInFixedTime("PistolIdle", 0.05f, 0);
        }
    }
    */
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