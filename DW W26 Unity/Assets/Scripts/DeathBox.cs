using UnityEngine;

public class DeathBox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Find the PlayerController3D on the entering object (or its parents)
        PlayerController3D player = other.GetComponentInParent<PlayerController3D>();
        if (player != null)
        {
            player.Die();
        }
    }
}
