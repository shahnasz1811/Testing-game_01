using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header ("Patrol Points")] 
    [SerializeField] private Transform leftEdge;
    [SerializeField] private Transform rightEdge;

    [Header("Enemy")]
    [SerializeField] private Transform enemy;

    [Header("Movement parameters")]
    [SerializeField] private float speed;
    private Vector3 initScale;
    private bool movingLeft;

    [Header("Idle Behaviour")]
    [SerializeField] private float idleDuration;
    private float idleTimer;

    [Header("Enemy Animator")]
    [SerializeField] private Animator anim;

    // 🔥 VISION CONE SETTINGS
    [Header("Vision Cone")]
    public float viewDistance = 6f;
    [Range(0, 180)] public float viewAngle = 60f;

    private Transform playerTransform;
    public bool isChasingPlayer;

    private void Awake()
    {
        initScale = enemy.localScale;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    private void OnDisable()
    {
        anim.SetBool("isMoving", false);
    }

    private void Update()
    {
        // 👁️ CHECK VISION
        if (CanSeePlayer())
        {
            isChasingPlayer = true;
        }
        else
        {
            isChasingPlayer = false;
        }

        // 🔥 CHASE LOGIC
        if (isChasingPlayer)
        {
            anim.SetBool("isMoving", true);

            if (enemy.position.x > playerTransform.position.x)
            {
                enemy.localScale = new Vector3(-Mathf.Abs(initScale.x), initScale.y, initScale.z);
                enemy.position += Vector3.left * speed * Time.deltaTime;
            }
            else
            {
                enemy.localScale = new Vector3(Mathf.Abs(initScale.x), initScale.y, initScale.z);
                enemy.position += Vector3.right * speed * Time.deltaTime;
            }

            return; // IMPORTANT: stop patrol when chasing
        }

        // 🧠 PATROL LOGIC (unchanged)
        if (movingLeft)
        {
            if (enemy.position.x >= leftEdge.position.x)
                MoveInDirection(-1);
            else
                ChangeDirection();
        }
        else
        {
            if (enemy.position.x <= rightEdge.position.x)
                MoveInDirection(1);
            else
                ChangeDirection();
        }
    }

    // 👁️ VISION CONE FUNCTION
    private bool CanSeePlayer()
    {
        if (playerTransform == null) return false;

        Vector2 directionToPlayer = (playerTransform.position - enemy.position).normalized;

        float distance = Vector2.Distance(enemy.position, playerTransform.position);
        if (distance > viewDistance)
            return false;

        // 👇 IMPORTANT: use facing direction
        Vector2 facingDirection = enemy.localScale.x > 0 ? Vector2.right : Vector2.left;

        float angle = Vector2.Angle(facingDirection, directionToPlayer);

        if (angle < viewAngle / 2f)
        {
            return true;
        }

        return false;
    }

    private void ChangeDirection()
    {
        anim.SetBool("isMoving", false);

        idleTimer += Time.deltaTime;

        if (idleTimer > idleDuration)
            movingLeft = !movingLeft;
    }

    private void MoveInDirection(int _direction)
    {
        idleTimer = 0;
        anim.SetBool("isMoving", true);

        enemy.localScale = new Vector3(Mathf.Abs(initScale.x) * _direction, 
        initScale.y, initScale.z);

        enemy.position = new Vector3(
            enemy.position.x + Time.deltaTime * _direction * speed,
            enemy.position.y,
            enemy.position.z);
    }

    // 👁️ DEBUG VISION (VERY USEFUL)
    private void OnDrawGizmosSelected()
    {
        if (enemy == null) return;

        Gizmos.color = Color.yellow;

        Vector3 left = Quaternion.Euler(0, 0, viewAngle / 2) * Vector3.right;
        Vector3 right = Quaternion.Euler(0, 0, -viewAngle / 2) * Vector3.right;

        Gizmos.DrawRay(enemy.position, left * viewDistance);
        Gizmos.DrawRay(enemy.position, right * viewDistance);
    }
}