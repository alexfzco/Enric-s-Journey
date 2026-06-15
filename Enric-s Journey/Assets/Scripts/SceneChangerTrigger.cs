using UnityEngine;
using UnityEngine.SceneManagement; 

public class SceneChangerTrigger : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("Escribe el nombre exacto de la escena a la que quieres ir")]
    [SerializeField] private string sceneToLoad;

    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag))
        {
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.LogError("¡Error! No has escrito el nombre de la escena en el Inspector.");
            }
        }
    }
}