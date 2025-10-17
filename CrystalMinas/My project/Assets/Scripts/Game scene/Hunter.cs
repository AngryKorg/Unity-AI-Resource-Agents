using UnityEngine;
using System.Collections;

public class Hunter : MonoBehaviour
{
    public float speed = 3f;
    public float detectionRadius = 5f;
    public float patrolRadius = 10f;
    public float waitAtPointTime = 2f;

    private Transform targetPlayer;
    private Vector2 patrolTarget;
    private Rigidbody2D rb;
    private bool waiting = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        SetNewPatrolPoint();
    }

    void Update()
    {
        targetPlayer = FindNearestPlayer();

        if (targetPlayer != null && Vector2.Distance(transform.position, targetPlayer.position) <= detectionRadius)
        {
            MoveTowards(targetPlayer.position);
        }
        else
        {
            if (!waiting)
            {
                MoveTowards(patrolTarget);

                if (Vector2.Distance(transform.position, patrolTarget) < 0.5f)
                {
                    StartCoroutine(WaitThenPatrol());
                }
            }
        }
    }

    private void MoveTowards(Vector2 destination)
    {
        Vector2 direction = (destination - (Vector2)transform.position).normalized;
        rb.velocity = direction * speed;
    }

    private IEnumerator WaitThenPatrol()
    {
        waiting = true;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(waitAtPointTime);
        SetNewPatrolPoint();
        waiting = false;
    }

    private void SetNewPatrolPoint()
    {
        Vector2 randomPoint = (Vector2)transform.position + Random.insideUnitCircle * patrolRadius;
        patrolTarget = randomPoint;
    }

    private Transform FindNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = player.transform;
            }
        }

        return closest;
    }
}
