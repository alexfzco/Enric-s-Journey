using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player"; 

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag(playerTag))
        {
            if (CollectibleManager.Instance != null)
            {
                CollectibleManager.Instance.CollectItem();
            }
            else
            {
                Debug.LogWarning("No se encontró un CollectibleManager en la escena.");
            }
            Destroy(gameObject);
        }
    }
}