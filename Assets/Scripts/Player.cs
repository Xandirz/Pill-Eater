using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    private Vector2 externalVelocity;
    [SerializeField] private float externalVelocityDecay = 8f;

    [Header("Restart")]
    [SerializeField] private float restartHoldDuration = 2f;
    private float restartHoldTimer;
    [SerializeField] private BoxCollider2D mapBounds;
    [SerializeField] private float boundsPadding = 0.25f;
    [Header("Body Parts")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer headRenderer;
    [Header("Head Sprites")]
    [SerializeField] private List<Sprite> playerHeads = new();
    [Header("Body Sprites")]
    [SerializeField] private Sprite idleBodySprite;
    [SerializeField] private Sprite walkBodySprite1;
    [SerializeField] private Sprite walkBodySprite2;

    [Header("Walk Animation")]
    [SerializeField] private float walkAnimationSpeed = 0.15f;

    [Header("Stats UI")]
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI statsTextSecondColumn;
    [SerializeField] private TextMeshProUGUI statsTextThirdColumn;
    [SerializeField] private Health health;
    [SerializeField] private WeaponController weaponController;

    [Header("Size")]
    [SerializeField] private float playerSize = 1f;
    [SerializeField] private float minPlayerSize = 0.5f;
    [SerializeField] private float maxPlayerSize = 5f;

    private Vector3 startScale;
    private Rigidbody2D rb;
    private Vector2 movement;

    private float walkTimer;
    private bool walkFrameToggle;
    private bool isFacingLeft;

    private string lastStatsText;
    private string lastStatsTextSecondColumn;
    private string lastStatsTextThirdColumn;

    public float PlayerSize => playerSize;
    public Vector2 Movement => movement;
    public float MoveSpeed => moveSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (health == null)
            health = GetComponent<Health>();

        if (weaponController == null)
            weaponController = GetComponentInChildren<WeaponController>();

        startScale = transform.localScale;

        if (bodyRenderer == null)
            Debug.LogError("Body Renderer is not assigned!", this);

        if (headRenderer == null)
            Debug.LogError("Head Renderer is not assigned!", this);

        ApplyPlayerSize();
    }

    private void Update()
    {
        HandleRestartInput();
        if (health != null && health.IsDead)
        {
            
            movement = Vector2.zero;
            UpdateStatsText();
            return;
        }

        HandleInput();
        HandleFlip();
        HandleWalkAnimation();
        UpdateStatsText();
    }

    private void FixedUpdate()
    {
        if (health != null && health.IsDead)
        {
            rb.velocity = Vector2.zero;
            externalVelocity = Vector2.zero;
            return;
        }

        rb.velocity = movement * moveSpeed + externalVelocity;
        externalVelocity = Vector2.Lerp(externalVelocity, Vector2.zero, externalVelocityDecay * Time.fixedDeltaTime);
        ClampInsideMapBounds();

    }
    private void ClampInsideMapBounds()
    {
        if (mapBounds == null)
            return;

        Bounds bounds = mapBounds.bounds;
        Vector3 pos = transform.position;

        float clampedX = Mathf.Clamp(pos.x, bounds.min.x + boundsPadding, bounds.max.x - boundsPadding);
        float clampedY = Mathf.Clamp(pos.y, bounds.min.y + boundsPadding, bounds.max.y - boundsPadding);

        bool hitHorizontalEdge = !Mathf.Approximately(pos.x, clampedX);
        bool hitVerticalEdge = !Mathf.Approximately(pos.y, clampedY);

        pos.x = clampedX;
        pos.y = clampedY;
        transform.position = pos;

        if (hitHorizontalEdge)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            externalVelocity.x = 0f;
        }

        if (hitVerticalEdge)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            externalVelocity.y = 0f;
        }
    }
    private void HandleInput()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;
    }

    private void HandleRestartInput()
    {
        if (Input.GetKey(KeyCode.R))
        {
            restartHoldTimer += Time.deltaTime;

            if (restartHoldTimer >= restartHoldDuration)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
        else
        {
            restartHoldTimer = 0f;
        }
    }

    private void HandleFlip()
    {
        if (movement.x < 0f && !isFacingLeft)
            SetFacing(true);
        else if (movement.x > 0f && isFacingLeft)
            SetFacing(false);
    }

    private void SetFacing(bool faceLeft)
    {
        isFacingLeft = faceLeft;

        if (bodyRenderer != null)
            bodyRenderer.flipX = faceLeft;

        if (headRenderer != null)
            headRenderer.flipX = faceLeft;
    }

    private void HandleWalkAnimation()
    {
        if (bodyRenderer == null)
            return;

        if (movement == Vector2.zero)
        {
            bodyRenderer.sprite = idleBodySprite;
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

        bodyRenderer.sprite = walkFrameToggle ? walkBodySprite1 : walkBodySprite2;
    }

    private void UpdateStatsText()
    {
        if (statsText == null && statsTextSecondColumn == null && statsTextThirdColumn == null)
            return;

        string firstColumnText =
            $"HP: {(health != null ? $"{health.CurrentHealth}/{health.MaxHealth}" : "-")}\n" +
            $"Move Speed: {moveSpeed:0.##}\n" +
            $"Fire Rate: {(weaponController != null ? weaponController.ShotsPerSecond.ToString("0.##") : "-")}/sec\n" +
            $"Damage: {(weaponController != null ? weaponController.Damage.ToString() : "-")}\n" +
            $"Bullet Speed: {(weaponController != null ? weaponController.BulletSpeed.ToString("0.##") : "-")}";

        string secondColumnText =
            $"Recoil: {(weaponController != null ? weaponController.RecoilForce.ToString("0.##") : "-")}\n" +
            $"Size: {playerSize:0.##}\n" +
            $"Poisonous: {(weaponController != null ? weaponController.Poisonous.ToString() : "-")}\n" +
            $"Explosion: {(weaponController != null ? weaponController.ExplosionChance.ToString() : "-")}%\n" +
            $"Homing: {(weaponController != null ? weaponController.HomingChance.ToString() : "-")}%";

        string thirdColumnText =
            $"Bullet Size: {(weaponController != null ? weaponController.BulletSize.ToString("0.##") : "-")}";
        
        if (statsText != null && lastStatsText != firstColumnText)
        {
            statsText.text = firstColumnText;
            lastStatsText = firstColumnText;
        }

        if (statsTextSecondColumn != null && lastStatsTextSecondColumn != secondColumnText)
        {
            statsTextSecondColumn.text = secondColumnText;
            lastStatsTextSecondColumn = secondColumnText;
        }

        if (statsTextThirdColumn != null && lastStatsTextThirdColumn != thirdColumnText)
        {
            statsTextThirdColumn.text = thirdColumnText;
            lastStatsTextThirdColumn = thirdColumnText;
        }
    }
    public void ChangeHeadRandomly()
    {
        if (headRenderer == null || playerHeads == null || playerHeads.Count == 0)
            return;

        if (playerHeads.Count == 1)
        {
            headRenderer.sprite = playerHeads[0];
            return;
        }

        Sprite currentHead = headRenderer.sprite;
        Sprite newHead = currentHead;

        int safety = 0;
        while (newHead == currentHead && safety < 20)
        {
            newHead = playerHeads[Random.Range(0, playerHeads.Count)];
            safety++;
        }

        headRenderer.sprite = newHead;
    }
    public void AddExternalVelocity(Vector2 velocity)
    {
        externalVelocity += velocity;
    }

    public void AddMoveSpeed(float amount)
    {
        moveSpeed += amount;

        if (moveSpeed < 3f)
            moveSpeed = 3f;
    }

    public void AddPlayerSize(float amount)
    {
        playerSize += amount;
        playerSize = Mathf.Clamp(playerSize, minPlayerSize, maxPlayerSize);
        ApplyPlayerSize();
    }

    private void ApplyPlayerSize()
    {
        transform.localScale = startScale * playerSize;
    }
}