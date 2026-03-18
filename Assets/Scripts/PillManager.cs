using UnityEngine;

public class PillManager : MonoBehaviour
{
    public static PillManager Instance { get; private set; }

    [SerializeField] private GameObject pillPrefab;
    [SerializeField] private GameObject specialPill;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SpawnRandomPill(Vector3 position)
    {
        if (pillPrefab == null)
        {
            Debug.LogError("Pill prefab is not assigned!", this);
            return;
        }

        GameObject pillObj = Instantiate(pillPrefab, position, Quaternion.identity);
        PillPickup pill = pillObj.GetComponent<PillPickup>();

        if (pill == null)
        {
            Debug.LogError("Spawned pill prefab has no PillPickup component!", pillObj);
            Destroy(pillObj);
            return;
        }

        pill.InitializeRandom();
    }

    public void SpawnSpecialPill(Vector3 position)
    {
        if (specialPill == null)
        {
            Debug.LogError("Special pill prefab is not assigned!", this);
            return;
        }

        Instantiate(specialPill, position, Quaternion.identity);
    }
}