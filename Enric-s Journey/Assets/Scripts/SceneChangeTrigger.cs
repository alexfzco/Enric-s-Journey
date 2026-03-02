using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeTrigger : MonoBehaviour
{
    [Header("Configuración")]
    public string sceneToLoad;          
    public bool requirePlayerTag = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (requirePlayerTag)
        {
            if (!other.CompareTag("Player"))
                return;
        }

        ChangeScene();
    }

    private void ChangeScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("No se ha asignado ninguna escena en SceneChangeTrigger.");
        }
    }
}