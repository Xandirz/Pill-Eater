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
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float homingLifeTime = 10f;
    [SerializeField] private float homingTurnSpeed = 8f;
    [SerializeField] private SpriteRenderer bulletSpriteRenderer;

    private BulletOwner owner;
    private int damage;
    private int poisonStacksToApply;
    private bool isHoming;
    private float moveSpeed;
    private float currentLifeTime;

    public BulletOwner Owner => owner;
    public int Damage => damage;
    public int PoisonStacksToApply => poisonStacksToApply;
    public bool IsHoming => isHoming;

    public void Initialize(BulletOwner bulletOwner, int bulletDamage)
    {
        Initialize(bulletOwner, bulletDamage, 0, false, 12f);
    }

    public void Initialize(BulletOwner bulletOwner, int bulletDamage, bool homing, float speed)
    {
        Initialize(bulletOwner, bulletDamage, 0, homing, speed);
    }

    public void Initialize(BulletOwner bulletOwner, int bulletDamage, int poisonStacks, bool homing = false, float speed = 12f)
    {
        owner = bulletOwner;
        damage = bulletDamage;
        poisonStacksToApply = poisonStacks;
        isHoming = homing;
        moveSpeed = speed;
        currentLifeTime = isHoming ? homingLifeTime : lifeTime;
        UpdateBulletColor();
    }

    private void Awake()
    {
        if (bulletSpriteRenderer == null)
            bulletSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (currentLifeTime <= 0f)
            currentLifeTime = isHoming ? homingLifeTime : lifeTime;

        Destroy(gameObject, currentLifeTime);
        UpdateBulletColor();
    }

    private void Update()
    {
        if (!isHoming)
            return;

        if (owner != BulletOwner.Player && owner != BulletOwner.Reflected)
            return;

        Transform target = FindClosestEnemy();
        if (target == null)
            return;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            return;

        Vector2 currentDirection = rb.velocity.sqrMagnitude > 0.001f
            ? rb.velocity.normalized
            : ((Vector2)target.position - (Vector2)transform.position).normalized;

        Vector2 desiredDirection = ((Vector2)target.position - (Vector2)transform.position).normalized;
        Vector2 newDirection = Vector2.Lerp(currentDirection, desiredDirection, homingTurnSpeed * Time.deltaTime).normalized;

        rb.velocity = newDirection * moveSpeed;

        float angle = Mathf.Atan2(newDirection.y, newDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform closest = null;
        float bestDistanceSqr = float.MaxValue;
        Vector2 currentPosition = transform.position;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null)
                continue;

            float distanceSqr = ((Vector2)enemy.transform.position - currentPosition).sqrMagnitude;
            if (distanceSqr < bestDistanceSqr)
            {
                bestDistanceSqr = distanceSqr;
                closest = enemy.transform;
            }
        }

        return closest;
    }

    private void UpdateBulletColor()
    {
        if (bulletSpriteRenderer == null)
            return;

        switch (owner)
        {
            case BulletOwner.Player:
                bulletSpriteRenderer.color = isHoming ? new Color(0.6f, 1f, 0.6f, 1f) : Color.white;
                break;

            case BulletOwner.Enemy:
                bulletSpriteRenderer.color = Color.red;
                break;

            case BulletOwner.Reflected:
                bulletSpriteRenderer.color = isHoming ? new Color(1f, 1f, 0.5f, 1f) : Color.yellow;
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
        {
            health.TakeDamage(damage);

            if ((owner == BulletOwner.Player || owner == BulletOwner.Reflected) && poisonStacksToApply > 0)
                health.AddPoisonStacks(poisonStacksToApply);
        }
    }
}