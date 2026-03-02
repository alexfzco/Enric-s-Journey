using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField] private Transform puntoRespawn;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (puntoRespawn != null)
            {
                CheckpointManager.instancia.SetCheckpoint(puntoRespawn.position);
            }
            else
            {
                Debug.LogWarning("No hay puntoRespawn asignado en " + gameObject.name);
            }
        }
    }
}