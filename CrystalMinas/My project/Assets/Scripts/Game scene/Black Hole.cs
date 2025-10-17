using UnityEngine;
using System.Collections.Generic;

public class BlackHole : MonoBehaviour
{
    private static List<BlackHole> allHoles = new List<BlackHole>();
    private bool isTeleporting = false;

    void OnEnable()
    {
        if (!allHoles.Contains(this))
            allHoles.Add(this);
    }

    void OnDisable()
    {
        if (allHoles.Contains(this))
            allHoles.Remove(this);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTeleporting)
        {
            StartCoroutine(Teleport(other.transform));
        }
    }

    private IEnumerator<UnityEngine.WaitForSeconds> Teleport(Transform obj)
    {
        isTeleporting = true;

        BlackHole targetHole = GetRandomOtherHole();

        if (targetHole != null)
        {
            // небольшая пауза для стабильности
            yield return new WaitForSeconds(0.1f);

            obj.position = targetHole.transform.position;
        }

        yield return new WaitForSeconds(0.2f); // защита от зацикливания
        isTeleporting = false;
    }

    private BlackHole GetRandomOtherHole()
    {
        List<BlackHole> others = new List<BlackHole>(allHoles);
        others.Remove(this);

        if (others.Count == 0) return null;

        int index = Random.Range(0, others.Count);
        return others[index];
    }
}
