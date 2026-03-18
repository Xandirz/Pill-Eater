using TMPro;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health")] [SerializeField] private int maxHealth = 5;
    [SerializeField] private bool isPlayer = false;
    [SerializeField] private float popupHeightOffset = 0.5f;

    [Header("Health Bar")] [SerializeField]
    private SpriteRenderer healthBarSprite;

    [Header("Poison")] [SerializeField] private TMP_Text poisonStacksText;
    [SerializeField] private float poisonTickInterval = 1f;

    private int currentHealth;
    private Vector3 healthBarStartScale;

    private int poisonStacks;
    private float poisonTickTimer;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public float HealthNormalized => maxHealth > 0 ? (float) currentHealth / maxHealth : 0f;
    public int PoisonStacks => poisonStacks;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (!isPlayer && healthBarSprite != null)
        {
            healthBarStartScale = healthBarSprite.transform.localScale;
            UpdateHealthBar();
        }

        UpdatePoisonText();
    }

    private void Update()
    {
        if (isPlayer)
            return;

        if (poisonStacks <= 0)
            return;

        if (currentHealth <= 0)
            return;

        poisonTickTimer += Time.deltaTime;

        if (poisonTickTimer >= poisonTickInterval)
        {
            poisonTickTimer -= poisonTickInterval;
            ApplyPoisonTick();
        }
    }

    public void AddMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;

        if (!isPlayer)
            UpdateHealthBar();
    }

    public void AddPoisonStacks(int amount)
    {
        if (amount <= 0)
            return;

        if (isPlayer)
            return;

        poisonStacks += amount;
        UpdatePoisonText();
    }

    private void ApplyPoisonTick()
    {
        if (poisonStacks <= 0)
            return;

        int poisonDamage = poisonStacks;

        TakeDamage(poisonDamage, MessagePopUp.Style.Info);

        poisonStacks -= 1;
        if (poisonStacks < 0)
            poisonStacks = 0;

        UpdatePoisonText();
    }

    public void TakeDamage(int damage)
    {
        TakeDamage(damage, MessagePopUp.Style.Error);
    }

    public void TakeDamage(int damage, MessagePopUp.Style popupStyle)
    {
        if (damage <= 0 || currentHealth <= 0)
            return;

        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;

        MessagePopUp.Create(
            transform.position + Vector3.up * popupHeightOffset,
            "-" + damage,
            popupStyle
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

    private void UpdatePoisonText()
    {
        if (poisonStacksText == null)
            return;

        if (poisonStacks > 0)
        {
            poisonStacksText.gameObject.SetActive(true);
            poisonStacksText.text = poisonStacks.ToString();
            poisonStacksText.color = new Color(0.35f, 1f, 0.35f, 1f);
        }
        else
        {
            poisonStacksText.text = "";
            poisonStacksText.gameObject.SetActive(false);
        }
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
        if (!isPlayer)
        {
            gameObject.tag = "Untagged";

            Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
            for (int i = 0; i < colliders.Length; i++)
                colliders[i].enabled = false;

            TrySpawnExplosionBullets();

            if (PillManager.Instance != null)
                PillManager.Instance.SpawnRandomPill(transform.position);
        }

        Destroy(gameObject);
    }

    private void TrySpawnExplosionBullets()
    {
        Player player = FindAnyObjectByType<Player>();
        if (player == null)
            return;

        WeaponController weapon = player.GetComponentInChildren<WeaponController>();
        if (weapon == null)
            return;

        if (weapon.BulletPrefab == null)
            return;

        if (Random.Range(0, 100) >= weapon.ExplosionChance)
            return;

        int bulletCount = 9 * Mathf.Max(1, weapon.ProjectilesPerShot);
        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = angleStep * i;
            float radians = angle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;

            GameObject bullet = Instantiate(
                weapon.BulletPrefab,
                transform.position,
                Quaternion.Euler(0f, 0f, angle)
            );

            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bool isHoming = Random.Range(0, 100) < weapon.HomingChance;

                bulletComponent.Initialize(
                    Bullet.BulletOwner.Player,
                    weapon.Damage,
                    weapon.Poisonous,
                    isHoming,
                    weapon.BulletSpeed
                );
            }

            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
                bulletRb.velocity = direction * weapon.BulletSpeed;
        }
    }
    
}