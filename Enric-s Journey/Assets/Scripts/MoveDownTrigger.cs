using UnityEngine;

public class MoveDownTrigger : MonoBehaviour
{
    public MoveDownObject target;
    public string requiredTag = "Player";

    public float cooldown = 0f;
    private float nextActivationTime = 0f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
            return;

        if (Time.time < nextActivationTime)
            return;

        if (target != null)
        {
            bool ok = target.Activate();
            if (ok && cooldown > 0f)
                nextActivationTime = Time.time + cooldown;
        }
    }
}