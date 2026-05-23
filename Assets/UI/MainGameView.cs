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
    [SerializeField] private Image damageBorder;
    [SerializeField] private float flashDuration = 0.2f;
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
        if(flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        damageBorder.gameObject.SetActive(true);

        Color color = damageBorder.color;
        color.a = 1f;
        damageBorder.color = color;

        float timer = 0f;

        while(timer < flashDuration)
        {
            timer += Time.deltaTime;

            float alpha = Mathf.Lerp(1f, 0f, timer / flashDuration);

            color.a = alpha;
            damageBorder.color = color;

            yield return null;
        }

        damageBorder.gameObject.SetActive(false);
    }
}
