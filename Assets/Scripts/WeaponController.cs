using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform firePoint;

    [Header("Position")]
    [SerializeField] private float distanceFromPlayer = 0.75f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 12f;
    [SerializeField] private float shootCooldown = 0.15f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float recoilForce = 0f;
    public float RecoilForce => recoilForce;
    private Vector2 aimDirection = Vector2.right;
    private float shootTimer;

    public float BulletSpeed => bulletSpeed;
    public float ShootCooldown => shootCooldown;
    public float ShotsPerSecond => shootCooldown > 0f ? 1f / shootCooldown : 0f;
    public int Damage => damage;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (player == null)
            Debug.LogError("Player is not assigned!", this);

        if (mainCamera == null)
            Debug.LogError("Main Camera is not assigned!", this);

        if (firePoint == null)
            Debug.LogError("Fire Point is not assigned!", this);

        if (bulletPrefab == null)
            Debug.LogError("Bullet Prefab is not assigned!", this);
    }

    private void Update()
    {
        HandleAim();
        HandleShootCooldown();
        HandleShoot();
    }

    private void HandleAim()
    {
        if (player == null || mainCamera == null)
            return;

        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = 0f;

        Vector2 direction = (mouseWorldPosition - player.position);

        if (direction.sqrMagnitude > 0.0001f)
            aimDirection = direction.normalized;

        transform.position = (Vector2)player.position + aimDirection * distanceFromPlayer;

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void HandleShootCooldown()
    {
        if (shootTimer > 0f)
            shootTimer -= Time.deltaTime;
    }

    private void HandleShoot()
    {
        if (!Input.GetMouseButton(0))
            return;

        if (shootTimer > 0f)
            return;

        Shoot();
        shootTimer = shootCooldown;
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
            bulletComponent.Initialize(Bullet.BulletOwner.Player, damage);

        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
            bulletRb.velocity = aimDirection * bulletSpeed;

        if (Mathf.Abs(recoilForce) > 0.001f)
            ApplyRecoil();
    }
    
    public void AddDamage(int amount)
    {
        damage += amount;

        if (damage < 1)
            damage = 1;
    }

    public void AddBulletSpeed(float amount)
    {
        bulletSpeed += amount;

        if (bulletSpeed < 3f)
            bulletSpeed = 3f;
    }
    public void AddRecoilForce(float amount)
    {
        recoilForce += amount;
    }

    private void ApplyRecoil()
    {
        if (player == null)
            return;

        Player playerComponent = player.GetComponent<Player>();
        if (playerComponent == null)
            return;

        Vector2 recoilDirection = -aimDirection.normalized;
        playerComponent.AddExternalVelocity(recoilDirection * recoilForce);
    }
    public void AddShotsPerSecond(float amount)
    {
        float currentShotsPerSecond = shootCooldown > 0f ? 1f / shootCooldown : 0f;
        currentShotsPerSecond += amount;

        if (currentShotsPerSecond < 0.5f)
            currentShotsPerSecond = 0.5f;

        shootCooldown = 1f / currentShotsPerSecond;
    }
}