using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float minDistanceToPlayer = 3f;
    [SerializeField] private float maxDistanceToPlayer = 5f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Sprites")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite walkSprite1;
    [SerializeField] private Sprite walkSprite2;

    [Header("Walk Animation")]
    [SerializeField] private float walkAnimationSpeed = 0.15f;

    [Header("Weapon")]
    [SerializeField] private Transform weaponTransform;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float weaponDistanceFromEnemy = 0.5f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float shootCooldown = 0.8f;
    [SerializeField] private float shootRange = 7f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 aimDirection = Vector2.left;

    private float walkTimer;
    private bool walkFrameToggle;
    private bool isFacingLeft = true;
    private float shootTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (player == null)
            Debug.LogError("Player is not assigned!", this);

        if (spriteRenderer == null)
            Debug.LogError("Sprite Renderer is not assigned!", this);

        if (weaponTransform == null)
            Debug.LogError("Weapon Transform is not assigned!", this);

        if (firePoint == null)
            Debug.LogError("Fire Point is not assigned!", this);

        if (bulletPrefab == null)
            Debug.LogError("Bullet Prefab is not assigned!", this);
    }

    private void Update()
    {
        if (player == null)
            return;

        HandleAI();
        HandleFlip();
        HandleWalkAnimation();
        HandleWeaponAim();
        HandleShootCooldown();
        HandleShooting();
    }

    private void FixedUpdate()
    {
        rb.velocity = movement * moveSpeed;
    }

    private void HandleAI()
    {
        Vector2 toPlayer = player.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;

        if (distanceToPlayer > 0.001f)
            aimDirection = toPlayer.normalized;

        if (distanceToPlayer > maxDistanceToPlayer)
        {
            movement = aimDirection;
        }
        else if (distanceToPlayer < minDistanceToPlayer)
        {
            movement = -aimDirection;
        }
        else
        {
            movement = Vector2.zero;
        }
    }

    private void HandleFlip()
    {
        if (aimDirection.x < 0f && !isFacingLeft)
        {
            SetFacing(true);
        }
        else if (aimDirection.x > 0f && isFacingLeft)
        {
            SetFacing(false);
        }
    }

    private void SetFacing(bool faceLeft)
    {
        isFacingLeft = faceLeft;

        // Спрайт врага по умолчанию смотрит ВЛЕВО
        if (spriteRenderer != null)
            spriteRenderer.flipX = !faceLeft;
    }

    private void HandleWalkAnimation()
    {
        if (spriteRenderer == null)
            return;

        if (movement == Vector2.zero)
        {
            spriteRenderer.sprite = idleSprite;
            walkTimer = 0f;
            walkFrameToggle = false;
            return;
        }

        walkTimer += Time.deltaTime;

        if (walkTimer >= walkAnimationSpeed)
        {
            walkTimer = 0f;
            walkFrameToggle = !walkFrameToggle;
        }

        spriteRenderer.sprite = walkFrameToggle ? walkSprite1 : walkSprite2;
    }

    private void HandleWeaponAim()
    {
        if (weaponTransform == null)
            return;

        Vector2 weaponDirection = aimDirection.sqrMagnitude > 0.001f ? aimDirection.normalized : Vector2.left;

        weaponTransform.position = (Vector2)transform.position + weaponDirection * weaponDistanceFromEnemy;

        float angle = Mathf.Atan2(weaponDirection.y, weaponDirection.x) * Mathf.Rad2Deg;
        weaponTransform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void HandleShootCooldown()
    {
        if (shootTimer > 0f)
            shootTimer -= Time.deltaTime;
    }

    private void HandleShooting()
    {
        if (bulletPrefab == null || firePoint == null || player == null)
            return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > shootRange)
            return;

        if (shootTimer > 0f)
            return;

        Shoot();
        shootTimer = shootCooldown;
    }

    private void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.Initialize(Bullet.BulletOwner.Enemy);
        }

        SpriteRenderer bulletSprite = bullet.GetComponentInChildren<SpriteRenderer>();
        if (bulletSprite != null)
        {
            bulletSprite.color = Color.red;
        }

        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.velocity = aimDirection * bulletSpeed;
        }
    }
}