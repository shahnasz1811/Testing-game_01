using UnityEngine;

public class EnemyDeath : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D body;

    [Header("Respawn Settings")]
    public Transform respawnPoint;

    private bool isDead;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();

        GameManager.instance.RegisterEnemy(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hazard") || collision.CompareTag("DeathZone"))
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Enemy died");

        anim.SetTrigger("die");
        Invoke(nameof(HideEnemy), 1f); // match animation length

        body.linearVelocity = Vector2.zero;
        body.bodyType = RigidbodyType2D.Kinematic;
 
    }

    /*private void Respawn()
    {
        transform.position = respawnPoint.position;

        body.bodyType = RigidbodyType2D.Dynamic;

        GetComponent<MeleeEnemy>().enabled = true;

        isDead = false;

        Debug.Log("Enemy respawned");
    }*/

    private void HideEnemy()
    {
        gameObject.SetActive(false); // hide enemy immediately
    }

    public void ResetEnemy()
    {
        CancelInvoke(); // stops any pending respawn

        transform.position = respawnPoint.position;

        body.bodyType = RigidbodyType2D.Dynamic;

        gameObject.SetActive(true); // show enemy again

        isDead = false;

        anim.ResetTrigger("die");
    }
}