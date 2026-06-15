using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    // Instancia estática para acceder desde cualquier script (Singleton)
    public static CollectibleManager Instance { get; private set; }

    [Header("Progreso Actual")]
    [SerializeField] private int collectedCount = 0;

    [Header("Configuración de Estrellas (Mínimo Requerido)")]
    [Tooltip("Cantidad mínima para obtener 3 estrellas")]
    public int minFor3Stars = 10;

    [Tooltip("Cantidad mínima para obtener 2 estrellas")]
    public int minFor2Stars = 7;

    [Tooltip("Cantidad mínima para obtener 1 estrella")]
    public int minFor1Star = 4;

    private void Awake()
    {
        // Configuración del Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Incrementa el contador de objetos recolectados.
    /// </summary>
    public void CollectItem()
    {
        collectedCount++;
        Debug.Log($"¡Objeto recogido! Total actual: {collectedCount}");
    }

    /// <summary>
    /// Devuelve la cantidad actual de objetos recogidos.
    /// </summary>
    public int GetCollectedCount()
    {
        return collectedCount;
    }

    /// <summary>
    /// Calcula y devuelve la cantidad de estrellas obtenidas (0, 1, 2 o 3).
    /// </summary>
    public int CalculateStarRating()
    {
        if (collectedCount >= minFor3Stars) return 3;
        if (collectedCount >= minFor2Stars) return 2;
        if (collectedCount >= minFor1Star) return 1;

        return 0; // Si no llegó ni al mínimo de 1 estrella
    }

    // --- HERRAMIENTA DE PRUEBA ---
    // Esto añade un botón en el menú de los tres puntitos del componente en el Inspector
    [ContextMenu("Probar Evaluación de Estrellas")]
    private void TestStarRating()
    {
        int estrellasObtenidos = CalculateStarRating();
        Debug.Log($"Resultado de la prueba: El jugador obtendría {estrellasObtenidos} estrellas con {collectedCount} objetos.");
    }
}