using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControladorTorreta : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject prefabProyectil; 
    public Transform puntoDisparo;     
    public float tiempoEntreDisparos = 1.5f;

    private float cronometro;

    void Update()
    {
        cronometro += Time.deltaTime;

        if (cronometro >= tiempoEntreDisparos)
        {
            Disparar();
            cronometro = 0f;
        }
    }

    void Disparar()
    {
        if (prefabProyectil != null && puntoDisparo != null)
        {
            
            Quaternion rotacionBala = puntoDisparo.rotation;

            
            rotacionBala *= Quaternion.Euler(0, 0, -90f);

           
            Instantiate(prefabProyectil, puntoDisparo.position, rotacionBala);
        }
    }
}