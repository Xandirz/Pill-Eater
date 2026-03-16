using TMPro;
using UnityEngine;

public class MessagePopUp : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private RectTransform rect;

    [Header("Life")]
    [SerializeField] private float lifeTime = 1.2f;
    [SerializeField] private float moveUpPixelsPerSec = 30f;
    [SerializeField] private float fadeSpeed = 3f;

    [Header("Clamp inside canvas")]
    [SerializeField] private float marginPixels = 20f;

    private float t;
    private Color c;

    public enum Style
    {
        Info,
        Warning,
        Error
    }

    public static MessagePopUp Create(Vector3 worldPos, string msg, Style style = Style.Error)
    {
        if (GameAssets.i == null)
        {
            Debug.LogError("GameAssets instance not found in scene!");
            return null;
        }

        if (GameAssets.i.pfMessagePopUp == null)
        {
            Debug.LogError("pfMessagePopUp is not assigned in GameAssets!");
            return null;
        }

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas in scene for MessagePopUp!");
            return null;
        }

        Transform popupTransform = Instantiate(GameAssets.i.pfMessagePopUp, canvas.transform);
        MessagePopUp popup = popupTransform.GetComponent<MessagePopUp>();
        popup.Setup(worldPos, msg, style);
        return popup;
    }

    private void Awake()
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();

        if (text == null)
            text = GetComponentInChildren<TextMeshProUGUI>();

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (canvasRect == null && canvas != null)
            canvasRect = canvas.GetComponent<RectTransform>();

        if (text != null)
            c = text.color;
    }

    private void Setup(Vector3 worldPos, string msg, Style style)
    {
        if (text == null)
            return;

        text.text = msg;

        switch (style)
        {
            case Style.Info:
                text.color = Color.white;
                break;

            case Style.Warning:
                text.color = new Color(1f, 0.85f, 0.2f, 1f);
                break;

            default:
                text.color = new Color(1f, 0.25f, 0.25f, 1f);
                break;
        }

        c = text.color;
        SetPositionNearWorld(worldPos, true);
    }

    private void SetPositionNearWorld(Vector3 worldPos, bool addRandomOffset)
    {
        Camera cam = Camera.main;
        if (cam == null || canvasRect == null || rect == null)
            return;

        Vector3 screen = cam.WorldToScreenPoint(worldPos);

        if (screen.z <= 0f)
            return;

        if (addRandomOffset)
        {
            screen.x += Random.Range(-60f, 60f);
            screen.y += Random.Range(30f, 60f);
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screen,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
            out Vector2 localPoint
        );

        Vector2 half = canvasRect.rect.size * 0.5f;

        localPoint.x = Mathf.Clamp(localPoint.x, -half.x + marginPixels, half.x - marginPixels);
        localPoint.y = Mathf.Clamp(localPoint.y, -half.y + marginPixels, half.y - marginPixels);

        rect.anchoredPosition = localPoint;
    }

    private void Update()
    {
        if (rect == null || text == null)
            return;

        rect.anchoredPosition += Vector2.up * moveUpPixelsPerSec * Time.deltaTime;

        t += Time.deltaTime;

        if (t >= lifeTime)
        {
            c.a -= fadeSpeed * Time.deltaTime;
            text.color = c;

            if (c.a <= 0f)
                Destroy(gameObject);
        }
    }
}