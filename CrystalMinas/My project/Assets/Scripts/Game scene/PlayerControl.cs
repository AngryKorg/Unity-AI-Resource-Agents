using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    public int playerID;
    public float speed = 5f;
    public UpdatePoints uiUpdater;
    public LifeUI lifeUI;
    public LifeUI syncedLifeUI;
    public int lives = 3;
    public Transform spawnPoint;

    public AudioClip collectSound;
    public GameObject progressBarPrefab;
    public Transform worldCanvas;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector3 initialScale;

    public int score = 0;
    private bool isCollecting = false;
    private bool isInvulnerable = false;
    private Resource currentResource = null;
    private GameObject currentProgressBar;
    private Slider progressSlider;

    [Header("Rotation Settings")]
    public Vector3 rotationUp;
    public Vector3 rotationDown;
    public Vector3 rotationLeft;
    public Vector3 rotationRight;

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
    }

    void Update()
    {
        if (!isCollecting)
        {
            if (playerID == 1)
            {
                movement.x = Input.GetAxis("Horizontal2");
                movement.y = Input.GetAxis("Vertical2");
            }
            else if (playerID == 2)
            {
                if (Gamepad.current != null)
                {
                    movement = Gamepad.current.leftStick.ReadValue();
                }
                else
                {
                    movement.x = Input.GetAxis("Horizontal");
                    movement.y = Input.GetAxis("Vertical");
                }
            }
        }
        else
        {
            movement = Vector2.zero;
        }

        // Поворот по направлениям
        if (movement.y > 0.01f)
        {
            transform.eulerAngles = rotationUp;
            transform.localScale = initialScale;
        }
        else if (movement.y < -0.01f)
        {
            transform.eulerAngles = rotationDown;
            transform.localScale = initialScale;
        }
        else if (movement.x < -0.01f)
        {
            transform.eulerAngles = rotationLeft;
            transform.localScale = initialScale;
        }
        else if (movement.x > 0.01f)
        {
            transform.eulerAngles = rotationRight;
            transform.localScale = initialScale;
        }

        if (!isCollecting && currentResource != null)
        {
            if (playerID == 1 && Input.GetKeyDown(KeyCode.Q))
            {
                StartCoroutine(CollectResource(currentResource, false));
            }
            else if (playerID == 2)
            {
                bool collected = false;

                if (Gamepad.current != null)
                {
                    if (Gamepad.current.buttonSouth.wasPressedThisFrame)
                    {
                        StartCoroutine(CollectResource(currentResource, false));
                        collected = true;
                    }
                }

                if (!collected)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        StartCoroutine(CollectResource(currentResource, false));
                    }
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.velocity = movement * speed;
        }
    }

    public void AddScore(int points)
    {
        score += points;

        if (uiUpdater != null)
        {
            uiUpdater.UpdatePointsDisplay(playerID - 1, score);
        }
        else
        {
            Debug.LogWarning("UI Updater не назначен для Player " + playerID);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hunter") && !isInvulnerable)
        {
            TakeDamage();
            return;
        }

        Resource resource = other.GetComponent<Resource>();
        if (resource != null && !resource.isBeingCollected)
        {
            if (resource.resourceType == Resource.ResourceType.Antimatter)
            {
                resource.isBeingCollected = true;
                StartCoroutine(CollectResource(resource, true));
            }
            else if (currentResource == null)
            {
                currentResource = resource;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Resource resource = other.GetComponent<Resource>();
        if (resource != null && resource == currentResource && !isCollecting)
        {
            currentResource = null;
        }
    }

    private IEnumerator CollectResource(Resource resource, bool isAntimatter)
    {
        isCollecting = !isAntimatter;

        if (!isAntimatter)
        {
            Collider2D col = resource.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;
        }

        float waitTime = resource.collectionTime;
        int points = resource.GetPoints();

        if (!isAntimatter && progressBarPrefab != null && worldCanvas != null)
        {
            currentProgressBar = Instantiate(progressBarPrefab, worldCanvas);
            progressSlider = currentProgressBar.GetComponentInChildren<Slider>();
            if (progressSlider != null)
                progressSlider.value = 0f;
        }

        if (collectSound != null)
        {
            SoundManager.Instance.PlayLoop(collectSound);
        }

        float elapsed = 0f;
        float soundTimer = 0f;
        bool soundStopped = false;

        while (elapsed < waitTime)
        {
            elapsed += Time.deltaTime;

            if (!soundStopped)
            {
                soundTimer += Time.deltaTime;
                if (soundTimer >= 4f)
                {
                    SoundManager.Instance.Stop();
                    soundStopped = true;
                }
            }

            if (!isAntimatter && progressSlider != null)
            {
                progressSlider.value = Mathf.Clamp01(elapsed / waitTime);
                currentProgressBar.transform.position = transform.position + Vector3.up * 1f;
            }

            yield return null;
        }

        if (!soundStopped)
        {
            SoundManager.Instance.Stop();
        }

        AddScore(points);
        Destroy(resource.gameObject);

        if (currentProgressBar != null)
        {
            Destroy(currentProgressBar);
            currentProgressBar = null;
            progressSlider = null;
        }

        currentResource = null;
        isCollecting = false;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        foreach (var hit in hits)
        {
            Resource nearby = hit.GetComponent<Resource>();
            if (nearby != null && !nearby.isBeingCollected)
            {
                currentResource = nearby;
                break;
            }
        }
    }

    private void TakeDamage()
    {
        lives--;

        if (lifeUI != null)
        {
            lifeUI.UpdateHearts(lives);
        }

        if (syncedLifeUI != null)
        {
            syncedLifeUI.UpdateHearts(lives);
        }

        if (lives <= 0)
        {
            Debug.Log("Игрок " + playerID + " проиграл!");
            gameObject.SetActive(false);
            return;
        }

        Debug.Log("Игрок " + playerID + " потерял жизнь. Осталось: " + lives);
        StopAllCoroutines();
        if (currentProgressBar != null)
        {
            Destroy(currentProgressBar);
            currentProgressBar = null;
            progressSlider = null;
        }
        currentResource = null;
        isCollecting = false;

        StartCoroutine(RespawnWithInvulnerability());
    }

    private IEnumerator RespawnWithInvulnerability()
    {
        isInvulnerable = true;
        transform.position = spawnPoint.position;
        yield return new WaitForSeconds(2f);
        isInvulnerable = false;
    }
}
