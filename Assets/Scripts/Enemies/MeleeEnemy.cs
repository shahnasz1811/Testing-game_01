using UnityEngine;

public class MeleeEnemy : MonoBehaviour
{
    [Header("Attack Parameters")]
    [SerializeField] private float attackCooldown;
    [SerializeField] private float range;

    [Header("Collider Parameters")]
    [SerializeField] private float colliderDistance;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private float height;
    [SerializeField] private float verticalOffset;

    [Header("Player Layer")]
    [SerializeField] private LayerMask playerLayer;
    private float cooldownTimer = Mathf.Infinity;

    [Header("Enemy Damage")]
    [SerializeField] private int damage = 1;

    //References
    private Animator anim;
    private PlayerHealth playerHealth;

    private EnemyPatrol enemyPatrol;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        enemyPatrol = GetComponentInParent<EnemyPatrol>();
    }

    private void Update()
    {
        cooldownTimer += Time.deltaTime;

        //attack only when player is in sight
        if (PlayerInSight())
        {
            if (cooldownTimer >= attackCooldown)
            {
                cooldownTimer = 0;
                anim.SetTrigger("meleeAttack");
            }
        }

        if(enemyPatrol != null)
            enemyPatrol.enabled = !PlayerInSight();
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
        {
            playerHealth = hit.transform.GetComponent<PlayerHealth>();
        }
       
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

    private void DamagePlayer()
    {
        Debug.Log("DamagePlayer CALLED");

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
    }
}