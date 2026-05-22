using UnityEngine;

public class EnemyDeath : MonoBehaviour, IResettable
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

        
    }

    private void Start()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.RegisterResettable(this);
        }
        else
        {
            Debug.LogError("GameManager instance is NULL!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Hazard") || collision.gameObject.CompareTag("DeathZone"))
        {
            Die();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Falling boxes can crush enemy
        if (collision.gameObject.CompareTag("Box"))
        {
            if (collision.relativeVelocity.magnitude > 7f)
            {
                Die();
            }
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
        body.bodyType = RigidbodyType2D.Dynamic;

        Debug.Log(string.Join(", ", GameManager.instance.resettables));
 
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

    public void ResetState()
    {
        if (isDead)
        {
            CancelInvoke(); // stops any pending respawn
            isDead = false;
            transform.position = respawnPoint.position;
            body.bodyType = RigidbodyType2D.Dynamic;
            gameObject.SetActive(true); // show enemy again
            Debug.Log("Enemy reset");
        }
    }

    /*public void ResetEnemy()
    {
        if (!isDead)
        {
            CancelInvoke(); // stops any pending respawn
            isDead = false;
            transform.position = respawnPoint.position;
            body.bodyType = RigidbodyType2D.Dynamic;
            gameObject.SetActive(true); // show enemy again
        }
    }*/
}