using UnityEngine;

public class PlayerHeadFloat : MonoBehaviour
{
    [SerializeField] private float floatAmountX = 0.05f;
    [SerializeField] private float floatAmountY = 0.02f;
    [SerializeField] private float floatSpeed = 2f;

    private Vector3 startLocalPosition;

    private void Awake()
    {
        startLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        Vector3 pos = startLocalPosition;
        pos.x += Mathf.Sin(Time.time * floatSpeed) * floatAmountX;
        pos.y += Mathf.Cos(Time.time * floatSpeed * 0.8f) * floatAmountY;
        transform.localPosition = pos;
    }
}