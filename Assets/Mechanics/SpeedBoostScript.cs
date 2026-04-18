using UnityEngine;

public class SpeedBoostScript : MonoBehaviour
{
    [SerializeField] private float boostForce = 15f;
    [SerializeField] private float boostDuration = 1f;

    private void OnTriggerStay(Collider other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (player != null)
        {
            Vector3 direction = transform.forward;
            direction.y = 0f;
            direction.Normalize();
            player.ApplyBoost(direction * boostForce, boostDuration);
        }
    }
}