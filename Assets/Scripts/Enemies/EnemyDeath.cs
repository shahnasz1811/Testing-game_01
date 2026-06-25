using UnityEngine;

public class EnemyDeath : MonoBehaviour, IResettable
{
    private Animator anim;
    private Rigidbody2D body;

    [Header("Respawn Settings")]
    public Transform respawnPoint;

    public bool isDead;

    [Header("Flashing Settings")]
    [SerializeField] private SpriteColorFlasher spriteColorFlasher;
    private SpriteRenderer spriteRend; 

    private EnemyPatrol enemyPatrol;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        spriteColorFlasher = GetComponent<SpriteColorFlasher>();
        spriteRend = GetComponentInChildren<SpriteRenderer>();
        enemyPatrol = GetComponentInParent<EnemyPatrol>();
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

   /*private void OnCollisionEnter2D(Collision2D collision)
    {
        //Falling boxes can crush enemy
        if (collision.gameObject.CompareTag("Box"))
        {
            if (collision.relativeVelocity.magnitude > 7f)
            {
                Die();
            }
        }
    }*/

    public void Die()
    {
        LevelStats.instance.enemiesKilled++;

        if (isDead) return;
        isDead = true;
        
        GetComponentInParent<EnemyPatrol>().enabled = false; // stop patrol immediately

        anim.SetBool("isMoving", false);
        anim.SetTrigger("die");

        Debug.Log("Enemy died");

        GameManager.instance.EnemyDied();

        spriteColorFlasher.FlashColor(spriteRend, 0.5f, Color.white);

        Invoke(nameof(HideEnemy), 1f); // match animation length

        //GetComponent<Collider2D>().enabled = false;
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