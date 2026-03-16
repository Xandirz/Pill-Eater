using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private bool isPlayer = false;
    [SerializeField] private float popupHeightOffset = 0.5f;

    [Header("Health Bar")]
    [SerializeField] private SpriteRenderer healthBarSprite;

    private int currentHealth;
    private Vector3 healthBarStartScale;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public float HealthNormalized => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (!isPlayer && healthBarSprite != null)
        {
            healthBarStartScale = healthBarSprite.transform.localScale;
            UpdateHealthBar();
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;

        MessagePopUp.Create(
            transform.position + Vector3.up * popupHeightOffset,
            "-" + damage,
            MessagePopUp.Style.Error
        );

        if (!isPlayer)
            UpdateHealthBar();

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        MessagePopUp.Create(
            transform.position + Vector3.up * popupHeightOffset,
            "+" + amount,
            MessagePopUp.Style.Info
        );

        if (!isPlayer)
            UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBarSprite == null)
            return;

        float hp = Mathf.Clamp01(HealthNormalized);

        Vector3 scale = healthBarStartScale;
        scale.x = healthBarStartScale.x * hp;
        healthBarSprite.transform.localScale = scale;

        if (hp > 0.5f)
            healthBarSprite.color = Color.green;
        else if (hp > 0.25f)
            healthBarSprite.color = Color.yellow;
        else
            healthBarSprite.color = new Color(1f, 0.5f, 0f);
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}