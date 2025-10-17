using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;
using System.Collections.Generic;

public class AIPlayerAgent : Agent
{
    private Rigidbody2D rb;

    [Header("Movement")]
    public float baseSpeed = 5f;
    private float currentSpeed;
    private Vector2 moveDirection = Vector2.zero;
    private Vector2 lastMoveDirection = Vector2.zero;

    [Header("UI")]
    public UpdatePoints uiUpdater;
    public LifeUI lifeUI;
    public int lives = 3;
    public Transform spawnPoint;

    [Header("Collection")]
    public GameObject progressBarPrefab;
    public Transform worldCanvas;
    private GameObject currentProgressBar;
    private Slider progressSlider;
    private bool isCollecting = false;
    private bool isTouchingResource = false;
    private GameObject resourceInRange;

    [Header("Score")]
    public int score = 0;

    [Header("Player Info")]
    public int playerID = 1;

    private const int maxResources = 20;
    private float timeSinceLastCollect = 0f;
    private int wallHitCount = 0;
    private float lastWallHitTime = -1f;

    private float noCollectPenaltyTimer = 0f;
    private float idlePenaltyTimer = 0f;
    private float badPositionTimer = 0f;
    private int badPositionCount = 0;

    private Vector2 lastBadPosition = Vector2.zero;
    private int stepCount = 0;

    private GameObject currentTargetResource = null;
    private float progressCheckTimer = 0f;
    private float lastDistanceToTarget = float.MaxValue;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = baseSpeed;

        if (spawnPoint == null)
            spawnPoint = this.transform;

        if (lifeUI != null)
        {
            lifeUI.Init();
            lifeUI.UpdateHearts(lives);
        }

        StartCoroutine(LogCumulativeRewardRoutine());
    }

    private IEnumerator LogCumulativeRewardRoutine()
    {
        while (true)
        {
            Debug.Log($"[RewardLogger] CumulativeReward: {GetCumulativeReward()}");
            yield return new WaitForSeconds(3f);
        }
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(
            Random.Range(-3f, 3f),
            Random.Range(-3f, 3f),
            0f);
        rb.velocity = Vector2.zero;
        lives = 3;
        isCollecting = false;
        isTouchingResource = false;
        resourceInRange = null;
        timeSinceLastCollect = 0f;
        noCollectPenaltyTimer = 0f;
        idlePenaltyTimer = 0f;
        badPositionTimer = 0f;
        badPositionCount = 0;
        moveDirection = Vector2.zero;
        lastMoveDirection = Vector2.zero;
        currentSpeed = baseSpeed;
        wallHitCount = 0;
        stepCount = 0;
        currentTargetResource = null;
        progressCheckTimer = 0f;
        lastDistanceToTarget = float.MaxValue;
        lastWallHitTime = -1f;

        if (lifeUI != null)
            lifeUI.UpdateHearts(lives);

        SetReward(0f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.y);
        sensor.AddObservation(isTouchingResource ? 1f : 0f);
        sensor.AddObservation(lives);
        sensor.AddObservation(lastMoveDirection.x);
        sensor.AddObservation(lastMoveDirection.y);
        sensor.AddObservation(moveDirection != Vector2.zero ? 1f : 0f);
        sensor.AddObservation(timeSinceLastCollect);

        List<ResourceInfo> resources = FindAllResourcesSorted();
        for (int i = 0; i < maxResources; i++)
        {
            if (i < resources.Count)
            {
                sensor.AddObservation(resources[i].position.x);
                sensor.AddObservation(resources[i].position.y);
                sensor.AddObservation(resources[i].points);
                float distance = Vector2.Distance(transform.position, resources[i].position);
                sensor.AddObservation(distance);
                sensor.AddObservation(resources[i].type);
            }
            else
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }
        }

        if (resources.Count > 0)
        {
            Vector2 dir = (resources[0].position - (Vector2)transform.position).normalized;
            sensor.AddObservation(dir.x);
            sensor.AddObservation(dir.y);
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        stepCount++;
        if (stepCount >= 20000)
        {
            EndEpisode();
            return;
        }

        float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveY = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        if (!isCollecting)
        {
            moveDirection = new Vector2(moveX, moveY) * baseSpeed;
        }
        else
        {
            moveDirection = Vector2.zero;
        }

        if (moveDirection != Vector2.zero && rb.velocity.magnitude > 0.1f)
        {
            lastMoveDirection = moveDirection;

            List<ResourceInfo> resources = FindAllResourcesSorted();
            if (currentTargetResource == null || !currentTargetResource || !currentTargetResource.activeInHierarchy)
            {
                var newTarget = resources.Find(r => r.type != 4f);
                if (newTarget.gameObjectRef != null)
                {
                    currentTargetResource = newTarget.gameObjectRef;
                    lastDistanceToTarget = Vector2.Distance(transform.position, currentTargetResource.transform.position);
                    progressCheckTimer = 0f;
                }
            }

            if (currentTargetResource != null)
            {
                Vector2 dirToTarget = ((Vector2)currentTargetResource.transform.position - (Vector2)transform.position).normalized;
                if (Vector2.Dot(moveDirection.normalized, dirToTarget) > 0.6f)
                {
                    AddReward(0.03f);
                }

                RaycastHit2D hit = Physics2D.Linecast(transform.position, currentTargetResource.transform.position);
                if (hit.collider != null && hit.collider.CompareTag("Antimatter"))
                {
                    AddReward(-0.02f);
                }

                float currentDistance = Vector2.Distance(transform.position, currentTargetResource.transform.position);
                progressCheckTimer += Time.deltaTime;

                if (progressCheckTimer >= 8f)
                {
                    if (lastDistanceToTarget - currentDistance < 0.1f)
                    {
                        AddReward(-0.5f);
                        currentTargetResource = null;
                    }
                    progressCheckTimer = 0f;
                    lastDistanceToTarget = currentDistance;
                }
            }
        }

        if (!isCollecting && resourceInRange != null)
            StartCoroutine(CollectResource(resourceInRange));

        if (!isCollecting)
        {
            timeSinceLastCollect += Time.deltaTime;
            noCollectPenaltyTimer += Time.deltaTime;
            badPositionTimer += Time.deltaTime;

            if (timeSinceLastCollect > 30f)
            {
                AddReward(-0.5f);
                EndEpisode();
                return;
            }

            if (!isCollecting && rb.velocity.magnitude < 0.2f && Vector2.Distance(transform.position, lastBadPosition) < 0.5f)
            {
                if (badPositionTimer >= 1f)
                {
                    badPositionCount++;
                    badPositionTimer = 0f;
                    AddReward(-0.2f);
                    if (badPositionCount >= 15)
                    {
                        AddReward(-1.0f);
                        EndEpisode();
                    }
                }
            }
            else
            {
                lastBadPosition = transform.position;
                badPositionCount = 0;
                badPositionTimer = 0f;
            }
        }

        if (FindAllResourcesSorted().FindAll(r => r.type != 4f).Count == 0)
        {
            AddReward(+3.0f);
            EndEpisode();
            return;
        }

        List<ResourceInfo> allResources = FindAllResourcesSorted();
        foreach (var res in allResources)
        {
            if (res.type == 4f)
            {
                float dist = Vector2.Distance(transform.position, res.position);
                float penalty = Mathf.Clamp(0.01f * (2f - dist), 0f, 0.05f);
                AddReward(-penalty);

                Vector2 dirToAntimatter = (res.position - (Vector2)transform.position).normalized;
                if (Vector2.Dot(moveDirection.normalized, dirToAntimatter) > 0.6f)
                {
                    if (dist < 1f)
                    {
                        AddReward(-0.1f);
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.velocity = moveDirection;
        }
    }

    private IEnumerator CollectResource(GameObject resourceObj)
    {
        isCollecting = true;

        Resource resource = resourceObj.GetComponent<Resource>();
        if (resource != null)
        {
            resource.isBeingCollected = true;
            Collider2D col = resource.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;

            float waitTime = resource.collectionTime;
            float elapsed = 0f;

            if (progressBarPrefab != null && worldCanvas != null)
            {
                currentProgressBar = Instantiate(progressBarPrefab, worldCanvas);
                progressSlider = currentProgressBar.GetComponentInChildren<Slider>();
                if (progressSlider != null)
                    progressSlider.value = 0f;
            }

            while (elapsed < waitTime)
            {
                elapsed += Time.deltaTime;

                if (progressSlider != null)
                {
                    progressSlider.value = Mathf.Clamp01(elapsed / waitTime);
                    currentProgressBar.transform.position = transform.position + Vector3.up * 1f;
                }

                yield return null;
            }

            AddScore(resource.GetPoints());
            Destroy(resourceObj);

            if (currentProgressBar != null)
                Destroy(currentProgressBar);
        }

        isCollecting = false;
        resourceInRange = null;
        isTouchingResource = false;
        timeSinceLastCollect = 0f;
        noCollectPenaltyTimer = 0f;
        idlePenaltyTimer = 0f;
        badPositionTimer = 0f;
        currentTargetResource = null;
    }

    private void AddScore(int points)
    {
        score += points;
        currentSpeed = baseSpeed;

        float rewardGiven = 0f;
        if (points == 1) rewardGiven = 2.0f;
        else if (points == 2) rewardGiven = 3.0f;
        else if (points == 3) rewardGiven = 4.0f;
        else if (points == -2) rewardGiven = -7.0f;
        else rewardGiven = points;

        AddReward(rewardGiven);
        Debug.Log($"[AddScore] Points: {points}, Reward: {rewardGiven}, CumulativeReward: {GetCumulativeReward()}");

        if (uiUpdater != null)
            uiUpdater.UpdatePointsDisplay(playerID - 1, score);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hunter"))
        {
            TakeDamage();
        }
        else if (other.CompareTag("Antimatter"))
        {
            AddScore(-3);
            Destroy(other.gameObject);
        }
        else if (IsResource(other))
        {
            isTouchingResource = true;
            resourceInRange = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsResource(other))
        {
            isTouchingResource = false;
            resourceInRange = null;
        }
    }

    private bool IsResource(Collider2D col)
    {
        return col.CompareTag("Dust") ||
               col.CompareTag("Metal") ||
               col.CompareTag("Crystal");
    }

    private void TakeDamage()
    {
        lives--;
        AddReward(-2.0f);

        if (lifeUI != null)
            lifeUI.UpdateHearts(lives);

        if (lives <= 0)
        {
            AddReward(-5.0f);
            EndEpisode();
        }
        else
        {
            StartCoroutine(RespawnWithInvulnerability());
        }
    }

    private IEnumerator RespawnWithInvulnerability()
    {
        transform.position = spawnPoint.position;
        yield return new WaitForSeconds(2f);
    }

    private struct ResourceInfo
    {
        public Vector2 position;
        public float points;
        public float type;
        public GameObject gameObjectRef;
    }

    private List<ResourceInfo> FindAllResourcesSorted()
    {
        List<ResourceInfo> result = new List<ResourceInfo>();
        Vector2 myPos = transform.position;

        string[] resourceTags = { "Dust", "Metal", "Crystal", "Antimatter" };

        foreach (string tag in resourceTags)
        {
            GameObject[] resources = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject res in resources)
            {
                Resource r = res.GetComponent<Resource>();
                if (r != null)
                {
                    float type = 0f;
                    if (tag == "Dust") type = 1f;
                    else if (tag == "Metal") type = 2f;
                    else if (tag == "Crystal") type = 3f;
                    else if (tag == "Antimatter") type = 4f;

                    result.Add(new ResourceInfo
                    {
                        position = res.transform.position,
                        points = r.GetPoints(),
                        type = type,
                        gameObjectRef = res
                    });
                }
            }
        }

        result.Sort((a, b) =>
            Vector2.Distance(myPos, a.position).CompareTo(Vector2.Distance(myPos, b.position))
        );

        return result;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Border"))
        {
            if (Time.time - lastWallHitTime >= 1f)
            {
                wallHitCount++;
                float penalty = Mathf.Max(-2f, -0.75f - 0.25f * wallHitCount);
                AddReward(penalty);
                Debug.Log($"[WallPenalty] hit {wallHitCount}, penalty {penalty}, total reward {GetCumulativeReward()}");
                lastWallHitTime = Time.time;
            }
        }
    }
}
