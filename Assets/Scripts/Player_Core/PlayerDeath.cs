using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerDeath : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private PlayerMovement_02 movement;

    [Header("Respawn Settings")]
    public Transform respawnPoint;
    public float deathDelay = 1f;

    public bool isDead = false;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement_02>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.CompareTag("Hazard") ||
            collision.CompareTag("DeathZone") ||
            collision.CompareTag("Enemy"))
        {
            Die();
        }
    }

    public void Die()
    {
        LevelStats.instance.RegisterDeath();

        if (isDead) return;
        isDead = true;

        movement.IsDead = true;

        Debug.Log("Player Died");

        // Play animation
        if (anim != null)
        anim.SetTrigger("die");

        // Stop movement
        movement.enabled = false;

        // Stop physics completely
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        // Disable collider so no extra triggers
        GetComponent<Collider2D>().enabled = false;

        // Start respawn
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(deathDelay);

        GameManager.instance.GameOver();

        // OPTION 1: Reload scene
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        // OPTION 2: Respawn instead (comment above and uncomment below)
        //Respawn();
    }

    private void Respawn()
    {
        transform.position = respawnPoint.position;

        // Re-enable physics
        rb.simulated = true;

        // Enable movement
        movement.enabled = true;

        // Enable collider
        GetComponent<Collider2D>().enabled = true;

        isDead = false;
    }
}