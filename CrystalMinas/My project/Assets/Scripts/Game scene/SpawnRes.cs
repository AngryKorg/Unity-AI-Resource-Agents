using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    [Header("Настройки ресурсов")]
    public GameObject[] resourcePrefabs;
    public int numberOfResources = 20;

    [Header("Область спавна")]
    public Vector2 spawnAreaMin;
    public Vector2 spawnAreaMax;
    public float checkRadius = 0.4f;

    [Header("Автоспавн при отсутствии положительных ресурсов")]
    public bool autoRespawn = false;

    [Header("Спавнить антиматерию?")]
    public bool spawnAntimatter = true; // <--- Новая галочка

    void Start()
    {
        SpawnAllResources();
    }

    void Update()
    {
        if (autoRespawn && !HasPositiveResources())
        {
            ClearAllResources();
            SpawnAllResources();
        }
    }

    void SpawnAllResources()
    {
        for (int i = 0; i < numberOfResources; i++)
        {
            SpawnResource();
        }
    }

    void SpawnResource()
    {
        const int maxAttempts = 50;
        int attempts = 0;
        bool positionFound = false;
        Vector2 position = Vector2.zero;

        while (!positionFound && attempts < maxAttempts)
        {
            position = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );

            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, checkRadius);
            if (colliders.Length == 0)
            {
                positionFound = true;
                break;
            }

            attempts++;
        }

        if (positionFound)
        {
            int randomIndex = Random.Range(0, resourcePrefabs.Length);
            GameObject prefabToSpawn = resourcePrefabs[randomIndex];

            // Проверка: если антиматерия и она отключена — не спавним, а ищем другой ресурс
            if (!spawnAntimatter && prefabToSpawn.CompareTag("Antimatter"))
            {
                // Просто повторяем попытку
                SpawnResource();
                return;
            }

            Instantiate(prefabToSpawn, position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Не удалось найти свободное место для ресурса после " + maxAttempts + " попыток.");
        }
    }

    bool HasPositiveResources()
    {
        string[] positiveTags = { "Dust", "Metal", "Crystal" };

        foreach (string tag in positiveTags)
        {
            GameObject[] found = GameObject.FindGameObjectsWithTag(tag);
            if (found.Length > 0)
            {
                return true;
            }
        }

        return false;
    }

    void ClearAllResources()
    {
        string[] allTags = { "Dust", "Metal", "Crystal", "Antimatter" };

        foreach (string tag in allTags)
        {
            GameObject[] found = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in found)
            {
                Destroy(obj);
            }
        }
    }
}
