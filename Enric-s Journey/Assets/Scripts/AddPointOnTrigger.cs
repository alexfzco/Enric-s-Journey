using UnityEngine;

public class AddPointOnTrigger : MonoBehaviour
{
    public int pointsToAdd = 1;
    public bool destroyOnPickup = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddPoint(pointsToAdd);
            }

            if (destroyOnPickup)
                Destroy(gameObject);
        }
    }
}