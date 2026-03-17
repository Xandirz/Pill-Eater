using UnityEngine;

public class PillManager : MonoBehaviour
{
    public static PillManager Instance { get; private set; }

    [SerializeField] private GameObject pillPrefab;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnRandomPill(Vector3 position)
    {
        GameObject pillObj = Instantiate(pillPrefab, position, Quaternion.identity);

        PillPickup pill = pillObj.GetComponent<PillPickup>();

        int positive = Random.Range(0, 5);
        int negative = Random.Range(0, 5);

        pill.Initialize(positive, negative);
    }
}