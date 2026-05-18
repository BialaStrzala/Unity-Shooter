using UnityEngine;

public class JumpPadScript : MonoBehaviour
{
    [SerializeField] private float boostForce = 15f;
    [SerializeField] private float boostDuration = 1f;

    private void OnTriggerStay(Collider other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (player != null)
        {
            //Debug.Log("Applying speed boost");
            Vector3 direction = transform.up * boostForce;
            direction.x = 0f;
            direction.Normalize();
            player.ApplyBoost(direction * boostForce, boostDuration);
        }
    }
}