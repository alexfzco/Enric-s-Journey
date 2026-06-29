using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimientoJugador : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 8f;
    public float fuerzaSalto = 12f;
    private float movimientoH;

    [Header("Detecci�n de Suelo")]
    public Transform verificadorSuelo; // Un objeto vac�o a los pies del jugador
    public Vector2 dimensionesCaja;    // Tama�o de la zona de detecci�n
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

        // Detectamos si tocamos el suelo usando una peque�a caja invisible en los pies
        enSuelo = Physics2D.OverlapBox(verificadorSuelo.position, dimensionesCaja, 0f, capaSuelo);

        // Salto (Espacio)
        if (Input.GetButtonDown("Jump") && enSuelo)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
        }
    }

    void FixedUpdate()
    {
        // Aplicamos la velocidad en el Rigidbody para un movimiento f�sico limpio
        rb.linearVelocity = new Vector2(movimientoH * velocidad, rb.linearVelocity.y);
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
