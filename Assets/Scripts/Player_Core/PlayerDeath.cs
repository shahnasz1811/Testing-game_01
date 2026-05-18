using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D body;

    [Header("Respawn Settings")]
    public Transform respawnPoint; // drag a respawn location in Inspector

    private void Awake()
    {
        anim = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If player touches hazard or death zone
        if (collision.CompareTag("Hazard") || collision.CompareTag("DeathZone") || collision.CompareTag("DeathZone"))
        {
            Die();
        }
    }

    [System.Obsolete]
    public void Die()
    {
        // Play death animation
        anim.SetTrigger("die");

        // Disable movement temporarily
        body.linearVelocity = Vector2.zero;
        body.bodyType = RigidbodyType2D.Kinematic;

        // Tell movement script to stop
        GetComponent<CatMovement>().enabled = false;

        // Respawn after short delay
        Invoke(nameof(Respawn), 1f);
    }

    private void Respawn()
    {
        // Move player back to respawn point
        transform.position = respawnPoint.position;

        // Re-enable physics
        body.bodyType = RigidbodyType2D.Dynamic;

        // Allow movement again
        GetComponent<CatMovement>().enabled = true;
    }
}
