using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Proyectil : MonoBehaviour
{
    public float velocidad = 10f;
    public float tiempoDeVida = 3f;

    void Start()
    {
        Destroy(gameObject, tiempoDeVida);
    }

    void Update()
    {
        transform.Translate(Vector2.right * velocidad * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Cambiado para que detecte al jugador (tu cápsula roja)
        if (collision.CompareTag("Player"))
        {
            Debug.Log("¡La bala ha golpeado al jugador!");

            // Destruye la bala al impactar para que no lo atraviese
            Destroy(gameObject);
        }
    }
}