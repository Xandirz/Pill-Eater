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

    private Vector2 aimDirection = Vector2.right;
    private float shootTimer;

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
        {
            bulletComponent.Initialize(Bullet.BulletOwner.Player);
        }

        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.velocity = aimDirection * bulletSpeed;
        }
    }
}