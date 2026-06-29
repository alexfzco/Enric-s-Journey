using UnityEngine;

public class TrampaSala : MonoBehaviour
{
    [SerializeField] private GameObject objetoBloqueo1;
    [SerializeField] private GameObject objetoBloqueo2;
    [SerializeField] private GameObject enemigoAMatar;

    private bool trampaActivada = false;
    private Collider2D miCollider;

    private void Start()
    {
        miCollider = GetComponent<Collider2D>();

        if (objetoBloqueo1 != null) objetoBloqueo1.SetActive(false);
        if (objetoBloqueo2 != null) objetoBloqueo2.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!trampaActivada && other.CompareTag("Player"))
        {
            ActivarTrampa();
        }
    }

    private void ActivarTrampa()
    {
        trampaActivada = true;

        if (objetoBloqueo1 != null) objetoBloqueo1.SetActive(true);
        if (objetoBloqueo2 != null) objetoBloqueo2.SetActive(true);

        if (miCollider != null) miCollider.enabled = false;
    }

    private void Update()
    {
        if (trampaActivada && enemigoAMatar == null)
        {
            LiberarJugador();
        }
    }

    private void LiberarJugador()
    {
        if (objetoBloqueo1 != null) objetoBloqueo1.SetActive(false);
        if (objetoBloqueo2 != null) objetoBloqueo2.SetActive(false);

        Destroy(gameObject);
    }
}