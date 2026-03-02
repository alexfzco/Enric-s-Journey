using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.position = CheckpointManager.instancia.GetUltimaPosicion();

            if (other.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                rb.linearVelocity = Vector2.zero;
            }

            var reset = FindFirstObjectByType<PuzzleResetManager>();
            if (reset != null) reset.ResetAll();
        }
    }
}