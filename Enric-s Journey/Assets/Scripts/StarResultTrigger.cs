using UnityEngine;

public class StarResultTrigger : MonoBehaviour
{
    [Header("Objetos de tu Jerarquía (Canvas)")]
    [Tooltip("Arrastra aquí el objeto '0 estrellas'")]
    [SerializeField] private GameObject objeto0Estrellas;

    [Tooltip("Arrastra aquí el objeto '1 estrella'")]
    [SerializeField] private GameObject objeto1Estrella;

    [Tooltip("Arrastra aquí el objeto '2 estrellas'")]
    [SerializeField] private GameObject objeto2Estrellas;

    [Tooltip("Arrastra aquí el objeto '3 estrellas'")]
    [SerializeField] private GameObject objeto3Estrellas;

    [Header("Configuración")]
    [SerializeField] private string playerTag = "Player";

    private bool levelFinished = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag(playerTag) && !levelFinished)
        {
            levelFinished = true;
            Time.timeScale = 0f;

            MostrarEstrellas();
        }
    }

    private void MostrarEstrellas()
    {
        if (CollectibleManager.Instance == null)
        {
            Debug.LogError("No se encontró el CollectibleManager en la escena.");
            return;
        }

        int estrellasGanadas = CollectibleManager.Instance.CalculateStarRating();
        if (objeto0Estrellas != null) objeto0Estrellas.SetActive(false);
        if (objeto1Estrella != null) objeto1Estrella.SetActive(false);
        if (objeto2Estrellas != null) objeto2Estrellas.SetActive(false);
        if (objeto3Estrellas != null) objeto3Estrellas.SetActive(false);
        switch (estrellasGanadas)
        {
            case 3:
                if (objeto3Estrellas != null) objeto3Estrellas.SetActive(true);
                break;
            case 2:
                if (objeto2Estrellas != null) objeto2Estrellas.SetActive(true);
                break;
            case 1:
                if (objeto1Estrella != null) objeto1Estrella.SetActive(true);
                break;
            default:
                if (objeto0Estrellas != null) objeto0Estrellas.SetActive(true);
                break;
        }

        Debug.Log($"Resultado mostrado: {estrellasGanadas} estrellas. Juego en pausa.");
    }
}