using UnityEngine;

public class Resource : MonoBehaviour
{
    // Enumeração que define os tipos possíveis de recursos no jogo
    public enum ResourceType { Dust, Metal, Crystal, Antimatter }
    public ResourceType resourceType;
    public float collectionTime = 0.5f;

    // Flag interna usada para impedir que o recurso seja recolhido mais de uma vez
    [HideInInspector]
    public bool isBeingCollected = false;

    public int GetPoints()// Retorna a quantidade de pontos atribuída ao tipo de recurso
    {
        switch (resourceType)
        {
            case ResourceType.Dust: return 1; // Pó dá 1 ponto
            case ResourceType.Metal: return 2;   // Metal dá 2 pontos
            case ResourceType.Crystal: return 3;  // Cristal dá 3 pontos
            case ResourceType.Antimatter: return -2; // Antimatéria reduz 2 pontos
            default: return 0;
        }
    }
}
