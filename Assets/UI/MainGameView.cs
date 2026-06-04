using PurrNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainGameView : View
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private RectTransform healthBar;
    public float width, height, maxHealth;
    [Header("Damage Flash")]
    [SerializeField] private Image damageOverlay;
    [SerializeField] private float flashDuration = 0.5f;
    private Coroutine flashRoutine;

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<MainGameView>();
    }

    public override void OnHide()
    {
        
    }

    public override void OnShow()
    {
        
    }

    public void UpdateHealth(int health)
    {
        healthText.text = health.ToString();
        float newWidth = (health / maxHealth) * width;
        healthBar.sizeDelta = new Vector2(newWidth, height);
    }

    public void DamageFlash()
    {
        Debug.Log("Damage flash called");
        if(flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        Debug.Log("Damage flash");
        damageOverlay.gameObject.SetActive(true);
        Color color = damageOverlay.color;
        color.a = 0.2f;
        damageOverlay.color = color;
        float timer = 0f;

        while (timer < flashDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(0.2f, 0f, timer / flashDuration);
            damageOverlay.color = color;
            yield return null;
        }

        color.a = 0f;
        damageOverlay.color = color;
        damageOverlay.gameObject.SetActive(false);
        flashRoutine = null;
    }
}
