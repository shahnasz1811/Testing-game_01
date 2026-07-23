using UnityEngine;

public class MeleeEnemy : MonoBehaviour
{
    [Header("Attack Parameters")]
    [SerializeField] private float attackCooldown;
    [SerializeField] private float range;
    [SerializeField] private int damage;

    [Header("Collider Parameters")]
    [SerializeField] private float colliderDistance;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private float height;
    [SerializeField] private float verticalOffset;

    [Header("Player Layer")]
    [SerializeField] private LayerMask playerLayer;
    private float cooldownTimer = Mathf.Infinity;

    //References
    private Animator anim;
    private PlayerHealth playerHealth;
    private EnemyPatrol enemyPatrol;
    private EnemyDeath enemyDeath;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        enemyPatrol = GetComponentInParent<EnemyPatrol>();
        enemyDeath = GetComponent<EnemyDeath>();
    }

    private void Update()
    {
        // Don't run melee logic while dead / mid-respawn. This also avoids
        // racing with EnemyDeath.ResetState(), which re-enables and resets
        // EnemyPatrol on respawn.
        if (enemyDeath != null && enemyDeath.isDead) return;

        // Stop attacking the instant the player dies - otherwise this keeps
        // detecting/hitting them (and re-triggering their hurt/die
        // animation) for the whole respawn delay, since nothing else here
        // was checking game state.
        if (LevelManager.instance.isGameOver) return;

        cooldownTimer += Time.deltaTime;

        bool playerInSight = PlayerInSight();

        //attack only when player is in sight
        if (playerInSight)
        {
            if (cooldownTimer >= attackCooldown)
            {
                cooldownTimer = 0;
                anim.SetTrigger("meleeAttack");
            }
        }

        // NOTE: we used to do `enemyPatrol.enabled = !playerInSight()` here.
        // That directly fought with EnemyDeath.ResetState(), which
        // re-enables + resets EnemyPatrol on respawn. If the player (who
        // also just respawned) happened to be in melee range at that exact
        // moment, this would immediately disable EnemyPatrol again a frame
        // later - freezing its Transform while the Animator kept looping
        // the walk cycle (moonwalk look), and switching off the wall-check
        // that lives inside EnemyPatrol.Update() (so it could then walk
        // straight into a wall once patrol resumed). Setting a flag instead
        // keeps EnemyPatrol.Update() always running so it stays in control
        // of its own animation + wall-avoidance state.
        if (enemyPatrol != null)
            enemyPatrol.isInMeleeRange = playerInSight;
    }

    private bool PlayerInSight()
    {
        Vector3 boxCenter = boxCollider.bounds.center
        + transform.right * range * transform.localScale.x * colliderDistance
        + transform.up * verticalOffset;

        RaycastHit2D hit =
            Physics2D.BoxCast(boxCenter,
            new Vector3
            (boxCollider.bounds.size.x * range,
            boxCollider.bounds.size.y * height,
            boxCollider.bounds.size.z),
            0,
            Vector2.right * transform.localScale.x,
            0,
            playerLayer);

        if (hit.collider != null)
            playerHealth = hit.transform.GetComponent<PlayerHealth>();

        return hit.collider != null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 boxCenter = boxCollider.bounds.center
       + transform.right * range * transform.localScale.x * colliderDistance
       + transform.up * verticalOffset;

        Gizmos.DrawWireCube(boxCenter,
             new Vector3(
             boxCollider.bounds.size.x * range,
             boxCollider.bounds.size.y * height,
             boxCollider.bounds.size.z));
    }

    public void DamagePlayer()
    {
        Debug.Log("Enemy HIT player");

        if (PlayerInSight())
        {
            playerHealth.TakeDamage(damage);
        }
    }
}