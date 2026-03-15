using UnityEngine;

public class Bullet : MonoBehaviour
{
    public enum BulletOwner
    {
        Player,
        Enemy,
        Reflected
    }

    [Header("Settings")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifeTime = 3f;

    [Header("References")]
    [SerializeField] private SpriteRenderer bulletSpriteRenderer;

    private BulletOwner owner;

    public BulletOwner Owner => owner;

    public void Initialize(BulletOwner bulletOwner)
    {
        owner = bulletOwner;
        UpdateBulletColor();
    }

    private void Awake()
    {
        if (bulletSpriteRenderer == null)
            bulletSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
        UpdateBulletColor();
    }

    private void UpdateBulletColor()
    {
        if (bulletSpriteRenderer == null)
            return;

        switch (owner)
        {
            case BulletOwner.Player:
                bulletSpriteRenderer.color = Color.white;
                break;

            case BulletOwner.Enemy:
                bulletSpriteRenderer.color = Color.red;
                break;

            case BulletOwner.Reflected:
                bulletSpriteRenderer.color = Color.yellow;
                break;
        }
    }

    public void ReflectFromPlayerBullet()
    {
        owner = BulletOwner.Reflected;
        UpdateBulletColor();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = -rb.velocity;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (owner == BulletOwner.Player)
        {
            if (other.CompareTag("Enemy"))
            {
                DealDamage(other);
                Destroy(gameObject);
            }
        }
        else if (owner == BulletOwner.Enemy)
        {
            if (other.CompareTag("Player"))
            {
                DealDamage(other);
                Destroy(gameObject);
            }
        }
        else if (owner == BulletOwner.Reflected)
        {
            if (other.CompareTag("Enemy"))
            {
                DealDamage(other);
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Bullet otherBullet = collision.gameObject.GetComponent<Bullet>();
        if (otherBullet == null)
            return;

        // Пуля игрока попадает во вражескую пулю:
        // вражеская становится отраженной, а пуля игрока НЕ исчезает
        if (owner == BulletOwner.Player && otherBullet.Owner == BulletOwner.Enemy)
        {
            otherBullet.ReflectFromPlayerBullet();
        }
        else if (owner == BulletOwner.Enemy && otherBullet.Owner == BulletOwner.Player)
        {
            ReflectFromPlayerBullet();
        }
    }

    private void DealDamage(Collider2D other)
    {
        Health health = other.GetComponentInParent<Health>();
        if (health == null)
            health = other.GetComponent<Health>();

        if (health != null)
            health.TakeDamage(damage);
    }
}