using UnityEngine;

public class DeactivateOnKey : MonoBehaviour
{
    [Header("Configuración de Referencias")]
    [Tooltip("Arrastra aquí el objeto que quieres que desaparezca de la escena.")]
    public GameObject objetoADestruir;

    [Header("Controles")]
    public KeyCode teclaInteraccion = KeyCode.E;

    private bool jugadorEnRango = false;

    void Update()
    {
        if (jugadorEnRango && Input.GetKeyDown(teclaInteraccion))
        {
            EjecutarDestruccion();
        }
    }

    private void EjecutarDestruccion()
    {
        if (objetoADestruir != null)
        {
            Destroy(objetoADestruir);
            Debug.Log("<color=green>Objeto destruido correctamente.</color>");
        }
        else
        {
            Debug.LogWarning("No hay ningún objeto asignado para destruir en el Inspector.");
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorEnRango = true;
            Debug.Log("Jugador cerca del botón. Pulsa " + teclaInteraccion);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorEnRango = false;
            Debug.Log("Jugador se ha alejado.");
        }
    }
}
