using System.Collections.Generic;
using UnityEngine;

public class PuzzleResetManager : MonoBehaviour
{
    private static PuzzleResetManager inst;
    private readonly List<MoveDownObject> moveDownObjects = new List<MoveDownObject>();

    void Awake()
    {
        inst = this;
    }

    public static void Register(MoveDownObject obj)
    {
        if (inst == null) return;
        if (obj == null) return;

        if (!inst.moveDownObjects.Contains(obj))
            inst.moveDownObjects.Add(obj);
    }

    public void ResetAll()
    {
        for (int i = 0; i < moveDownObjects.Count; i++)
        {
            if (moveDownObjects[i] != null)
                moveDownObjects[i].ResetObject();
        }
    }
}