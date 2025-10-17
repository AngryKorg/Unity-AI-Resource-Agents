using UnityEngine;
using UnityEngine.UI;

public class ResourceCollectorBot : MonoBehaviour
{
    public int playerID = 0;
    public float moveSpeed = 5f;
    public string[] resourceTags = { "Dust", "Metal", "Crystal" };

    public GameObject progressBarPrefab;
    public Transform worldCanvas;
    public AudioClip collectSound;

    public LifeUI lifeUI;
    public LifeUI syncedLifeUI;
    public UpdatePoints uiUpdater;
    public Transform spawnPoint;

    [Header("Rotation Settings")]
    public Vector3 rotationUp;
    public Vector3 rotationDown;
    public Vector3 rotationLeft;
    public Vector3 rotationRight;

    public int lives = 3;
    public int score = 0;

    private Rigidbody2D rb;
    private Transform currentTarget;
    private bool isCollecting = false;
    private float collectTimer = 0f;
    public float collectDuration = 0.2f;

    private GameObject currentProgressBar;
    private Slider progressSlider;

    private bool isStuck = false;
    private float stuckTimer = 0f;
    private float nextStuckTime = 0f;

    private float reconsiderTimer = 0f;

    private int resourcesCollectedInBatch = 0;
    private bool isLongPause = false;
    private float longPauseTimer = 0f;
    private float longPauseDuration = 0f;

    private bool isInvulnerable = false;
    private Vector3 initialScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        initialScale = transform.localScale;

        if (spawnPoint == null)
            spawnPoint = this.transform;

        if (lifeUI != null)
        {
            lifeUI.Init();
            lifeUI.UpdateHearts(lives);
        }

        if (syncedLifeUI != null)
        {
            syncedLifeUI.Init();
            syncedLifeUI.UpdateHearts(lives);
        }

        ScheduleNextStuck();
    }

    void Update()
    {
        if (isCollecting || isLongPause || isStuck)
            return;

        if (currentTarget == null)
        {
            FindRandomNearbyResource();
        }
        else
        {
            Vector3 direction = (currentTarget.position - transform.position).normalized;

            Vector2 noise = Random.insideUnitCircle * 0.4f;
            direction.x += noise.x;
            direction.y += noise.y;
            direction.Normalize();

            float speedVariation = Random.Range(0.8f, 1.2f);
            Vector2 movement = direction * moveSpeed * speedVariation;

            if (rb != null)
                rb.velocity = movement;

            if (direction.y > 0.01f)
                transform.eulerAngles = rotationUp;
            else if (direction.y < -0.01f)
                transform.eulerAngles = rotationDown;
            else if (direction.x < -0.01f)
                transform.eulerAngles = rotationLeft;
            else if (direction.x > 0.01f)
                transform.eulerAngles = rotationRight;

            transform.localScale = initialScale;

            if (Vector3.Distance(transform.position, currentTarget.position) < 0.5f)
            {
                if (Random.value < 0.6f)
                {
                    isStuck = true;
                    stuckTimer = 0f;
                }
                else
                {
                    StartCollecting();
                }
            }
        }

        reconsiderTimer += Time.deltaTime;
        if (reconsiderTimer >= Random.Range(2f, 4f))
        {
            if (Random.value < 0.6f && currentTarget != null)
                currentTarget = null;

            reconsiderTimer = 0f;
        }
    }

    void LateUpdate()
    {
        if (!isCollecting && !isStuck && !isLongPause && Time.time >= nextStuckTime)
        {
            isStuck = true;
            stuckTimer = 0f;
        }

        if (currentProgressBar != null)
            currentProgressBar.transform.position = transform.position + Vector3.up * 1.2f;

        if (isStuck)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= Random.Range(1f, 2f))
            {
                isStuck = false;
                ScheduleNextStuck();
            }
        }

        if (isLongPause)
        {
            longPauseTimer += Time.deltaTime;
            Vector2 randomMove = Random.insideUnitCircle * moveSpeed * 0.3f;
            transform.position += new Vector3(randomMove.x, randomMove.y, 0f) * Time.deltaTime;

            if (longPauseTimer >= longPauseDuration)
            {
                isLongPause = false;
                resourcesCollectedInBatch = 0;
                ScheduleNextStuck();
            }
        }
    }

    void FindRandomNearbyResource()
    {
        GameObject[] allResources = GameObject.FindObjectsOfType<GameObject>();
        float closestDistance = Mathf.Infinity;
        Transform chosen = null;

        foreach (GameObject obj in allResources)
        {
            if (obj.CompareTag("Antimatter")) continue;

            foreach (string tag in resourceTags)
            {
                if (obj.CompareTag(tag))
                {
                    float dist = Vector3.Distance(transform.position, obj.transform.position);
                    if (Random.value < 0.6f && dist < 6f)
                    {
                        chosen = obj.transform;
                        break;
                    }
                    else if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        chosen = obj.transform;
                    }
                }
            }
        }

        if (chosen != null)
            currentTarget = chosen;
    }

    void StartCollecting()
    {
        isCollecting = true;
        collectTimer = 0f;

        if (progressBarPrefab != null && worldCanvas != null)
        {
            currentProgressBar = Instantiate(progressBarPrefab, worldCanvas);
            progressSlider = currentProgressBar.GetComponentInChildren<Slider>();
            if (progressSlider != null) progressSlider.value = 0f;
        }

        if (collectSound != null)
            SoundManager.Instance.PlayLoop(collectSound);
    }

    void FinishCollecting()
    {
        if (collectSound != null)
            SoundManager.Instance.Stop();

        if (currentTarget != null)
        {
            Destroy(currentTarget.gameObject);
            AddScore(1);
        }

        isCollecting = false;
        currentTarget = null;

        if (currentProgressBar != null)
            Destroy(currentProgressBar);

        resourcesCollectedInBatch++;

        if (resourcesCollectedInBatch >= Random.Range(1, 5))
        {
            isLongPause = true;
            longPauseTimer = 0f;
            longPauseDuration = Random.Range(1f, 3f);
        }
    }

    public void AddScore(int points)
    {
        score += points;
        if (uiUpdater != null)
            uiUpdater.UpdatePointsDisplay(playerID - 1, score);
    }

    public void TakeDamage()
    {
        if (isInvulnerable) return;

        lives--;
        if (lifeUI != null) lifeUI.UpdateHearts(lives);
        if (syncedLifeUI != null) syncedLifeUI.UpdateHearts(lives);

        if (lives <= 0)
        {
            Debug.Log("Бот уничтожен.");
            gameObject.SetActive(false);
            return;
        }

        StopAllCoroutines();
        if (currentProgressBar != null)
        {
            Destroy(currentProgressBar);
            currentProgressBar = null;
        }

        currentTarget = null;
        isCollecting = false;

        StartCoroutine(RespawnWithInvulnerability());
    }

    private System.Collections.IEnumerator RespawnWithInvulnerability()
    {
        isInvulnerable = true;
        transform.position = spawnPoint.position;
        yield return new WaitForSeconds(2f);
        isInvulnerable = false;
    }

    void ScheduleNextStuck()
    {
        nextStuckTime = Time.time + Random.Range(1f, 2f);
    }
}
