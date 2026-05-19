using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;

    private int currentHealth;
    private Animator anim;
    private bool isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int _damage)
    {
        if (isDead) return;

        currentHealth -= _damage;

        Debug.Log("Player took damage! Current health: " + currentHealth);

        if (anim != null)
            anim.SetTrigger("hurt"); // optional animation

        if (currentHealth <= 0)
        {
            GetComponent<PlayerDeath>().Die(false);
            anim.SetTrigger("die");
        }
    }

    

    /*private void Die()
    {
        isDead = true;

        Debug.Log("Player died!");

        if (anim != null)
            anim.SetTrigger("die"); // optional animation

        // Disable movement (if you have a movement script)
        // GetComponent<PlayerMovement>().enabled = false;

        // Disable collider (optional)
        // GetComponent<Collider2D>().enabled = false;

        // Optional: destroy after delay
        // Destroy(gameObject, 1f);
    }*/
}