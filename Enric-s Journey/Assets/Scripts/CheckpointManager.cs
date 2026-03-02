using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager instancia;

    private Vector3 ultimaPosicion;

    void Awake()
    {
        if (instancia == null) instancia = this;
        else { Destroy(gameObject); return; }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            ultimaPosicion = player.transform.position;
    }

    public void SetCheckpoint(Vector3 posicion)
    {
        ultimaPosicion = posicion;
    }

    public Vector3 GetUltimaPosicion()
    {
        return ultimaPosicion;
    }
}