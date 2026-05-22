using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerDeath : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D body;
    private PlayerHealth playerHealth;
    private EnemyDeath enemyRespawn;

    [Header("Respawn Settings")]
    public Transform respawnPoint; // drag a respawn location in Inspector

    private void Awake()
    {
        anim = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
        enemyRespawn = GetComponent<EnemyDeath>();
    }

    /*private void OnTriggerEnter2D(Collider2D collision)
    {
        // If player touches hazard or death zone
        if (collision.CompareTag("Hazard") || collision.CompareTag("DeathZone") || collision.CompareTag("Enemy"))
        {
            Die();
        }
    }*/

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Died from hazard
        if (collision.CompareTag("Hazard") || collision.CompareTag("DeathZone") || collision.CompareTag("Enemy"))
        {
            Die();
        }
    }

    public void Die()
    {
        // Play death animation
        anim.SetTrigger("die");

        // Disable movement temporarily
        body.linearVelocity = Vector2.zero;
        body.bodyType = RigidbodyType2D.Kinematic;

        // Tell movement script to stop
        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<Collider2D>().enabled = false;

        // Respawn after short delay
        //Invoke(nameof(Respawn), 1f);

        // Delay scene reload instead of calling it instantly
        StartCoroutine(DeathRoutine());

        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        /*if (fromHazard)
        {
            
        }*/
        
     }

     IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(1f); // Wait for death animation to play
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /*[System.Obsolete]
    public void Die()
    {
        // Play death animation
        anim.SetTrigger("die");

        // Disable movement temporarily
        body.linearVelocity = Vector2.zero;
        body.bodyType = RigidbodyType2D.Kinematic;

        // Tell movement script to stop
        GetComponent<PlayerMovement>().enabled = false;

        // Respawn after short delay
        Invoke(nameof(Respawn), 1f);

        GameManager.instance.ResetEnemies();
    }*/

    private void Respawn()
    {
        // Move player back to respawn point
        transform.position = respawnPoint.position;

        // Re-enable physics
        body.bodyType = RigidbodyType2D.Dynamic;

        // Allow movement again
        GetComponent<PlayerMovement>().enabled = true;
        GetComponent<Collider2D>().enabled = true;

        playerHealth.ResetHealth();
    }
}
