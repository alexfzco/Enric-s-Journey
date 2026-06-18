using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimientoJugador : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 8f;
    public float fuerzaSalto = 12f;
    private float movimientoH;

    [Header("Detección de Suelo")]
    public Transform verificadorSuelo; // Un objeto vacío a los pies del jugador
    public Vector2 dimensionesCaja;    // Tamaño de la zona de detección
    public LayerMask capaSuelo;         // Selecciona la capa "Suelo" en el inspector
    private bool enSuelo;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Movimiento horizontal (A / D o Flechas)
        movimientoH = Input.GetAxisRaw("Horizontal");

        // Detectamos si tocamos el suelo usando una pequeña caja invisible en los pies
        enSuelo = Physics2D.OverlapBox(verificadorSuelo.position, dimensionesCaja, 0f, capaSuelo);

        // Salto (Espacio)
        if (Input.GetButtonDown("Jump") && enSuelo)
        {
            rb.velocity = new Vector2(rb.velocity.x, fuerzaSalto);
        }
    }

    void FixedUpdate()
    {
        // Aplicamos la velocidad en el Rigidbody para un movimiento físico limpio
        rb.velocity = new Vector2(movimientoH * velocidad, rb.velocity.y);
    }

    // Dibuja la caja de suelo en el editor para que puedas verla y ajustarla
    private void OnDrawGizmos()
    {
        if (verificadorSuelo != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(verificadorSuelo.position, dimensionesCaja);
        }
    }
}
