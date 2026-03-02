using UnityEngine;
using System.Collections.Generic;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager instancia;
    public List<Transform> listaCheckpoints = new List<Transform>();

    private Vector3 ultimaPosicion;
    private int indiceActual = -1;

    void Awake()
    {
        if (instancia == null) instancia = this;
        else Destroy(gameObject);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) ultimaPosicion = player.transform.position;
    }

    public void ActualizarCheckpoint(Transform nuevoCheckpoint)
    {
        int nuevoIndice = listaCheckpoints.IndexOf(nuevoCheckpoint);

        if (nuevoIndice > indiceActual)
        {
            indiceActual = nuevoIndice;
            ultimaPosicion = nuevoCheckpoint.position;
        }
    }

    public Vector3 GetUltimaPosicion()
    {
        return ultimaPosicion;
    }
}