using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CañonTorreta : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject prefabProyectil; // Tu Prefab de la bala
    public Transform puntoDisparo;     // El objeto hijo 'PuntoDisparo'

    [Header("Configuración de Disparo")]
    public float tiempoEntreDisparos = 1.0f;
    private float cronometro;

    [Header("Configuración de Apuntado")]
    public float velocidadRotacion = 5.0f; // Qué tan rápido gira el cañón
    public float anguloMargenDisparo = 5.0f; // Ángulo de error aceptable para disparar

    private Transform objetivo; // El Transform del Jugador si está en rango
    private bool objetivoEnRango = false;

    void Update()
    {
        if (objetivo != null && objetivoEnRango)
        {
            // 1. APUNTAR AL OBJETIVO
            GirarHaciaObjetivo();

            // 2. GESTIONAR DISPARO SI ESTÁ ALINEADO
            cronometro += Time.deltaTime;

            // Calculamos la diferencia de ángulo entre la dirección del cañón y el objetivo
            Vector2 direccionHaciaObjetivo = (objetivo.position - transform.position).normalized;
            float anguloHaciaObjetivo = Mathf.Atan2(direccionHaciaObjetivo.y, direccionHaciaObjetivo.x) * Mathf.Rad2Deg;
            float anguloActualCañon = transform.eulerAngles.z;
            float diferenciaAngulo = Mathf.DeltaAngle(anguloActualCañon, anguloHaciaObjetivo);

            // Solo dispara si el cronómetro está listo Y el cañón está apuntando *casi* directamente (dentro del margen)
            if (cronometro >= tiempoEntreDisparos && Mathf.Abs(diferenciaAngulo) < anguloMargenDisparo)
            {
                Disparar();
                cronometro = 0f;
            }
        }
        else
        {
            // Opcional: El cañón vuelve a su posición original si no hay objetivo
            //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.deltaTime * velocidadRotacion);
        }
    }

    void GirarHaciaObjetivo()
    {
        // Calculamos la dirección del vector desde el cañón al objetivo
        Vector2 direccion = objetivo.position - transform.position;

        // Calculamos el ángulo en grados usando Atan2
        float anguloZ = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

        // Creamos la rotación objetivo solo en el eje Z
        Quaternion rotacionObjetivo = Quaternion.Euler(0, 0, anguloZ);

        // Suavizamos la rotación para que se mueva gradualmente con Slerp
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, velocidadRotacion * Time.deltaTime);
    }

    void Disparar()
    {
        if (prefabProyectil != null && puntoDisparo != null)
        {
            // Instancia la bala. Ya no necesitamos corregir los -90 grados, 
            // porque el cañón ahora se orienta dinámicamente.
            Instantiate(prefabProyectil, puntoDisparo.position, puntoDisparo.rotation);
        }
    }

    // --- DETECCIÓN DE ENEMIGOS USANDO TRIGGERS ---

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si entra algo con el Tag "Player" (Asegúrate de asignarlo a tu jugador)
        if (collision.CompareTag("Player"))
        {
            objetivo = collision.transform; // Guardamos su Transform
            objetivoEnRango = true;
            Debug.Log("Torreta: Jugador detectado.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Si el objeto que sale es el jugador
        if (collision.CompareTag("Player") && collision.transform == objetivo)
        {
            objetivoEnRango = false;
            objetivo = null; // Olvidamos el objetivo
            Debug.Log("Torreta: Jugador fuera de rango.");
        }
    }
}
