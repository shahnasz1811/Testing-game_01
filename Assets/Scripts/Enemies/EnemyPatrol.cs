using GLTFast.Schema;
using NUnit.Framework.Internal;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    #region PARAMETERS
    [Header("Patrol Points")]
    [SerializeField] private Transform leftEdge;
    [SerializeField] private Transform rightEdge;

    [Header("Enemy")]
    [SerializeField] private Transform enemy;

    [Header("Movement parameters")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float acceleration = 3f;

    private float currentSpeed;
    private Vector3 initScale;
    private bool movingLeft;

    [Header("Idle Behaviour")]
    [SerializeField] private float idleDuration;
    private float idleTimer;

    [Header("Enemy Animator")]
    [SerializeField] private Animator anim;

    // 👁️ VISION
    [Header("Vision Cone")]
    public float viewDistance = 6f;
    [Range(0, 180)] public float viewAngle = 60f;
    [SerializeField] private FieldOfView fieldOfView;

    private Transform playerTransform;
    public bool isChasingPlayer;

    // ⚠️ ALERT SYSTEM
    [Header("Alert System")]
    [SerializeField] private float alertDuration = 1f;
    private float alertTimer;
    private bool isAlerting;

    // 🧠 LOSE SIGHT COOLDOWN
    [Header("Lose Player Cooldown")]
    [SerializeField] private float loseSightCooldown = 1.5f;
    private float loseSightTimer;

    [Header("Wall Check")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance = 0.2f;
    [SerializeField] private LayerMask wallLayer;


    private Rigidbody2D RB;
    private EnemyDeath enemyDeath;

    private bool waitingAtWall;
    #endregion

    private void Awake()
    {
        initScale = enemy.localScale;
        currentSpeed = patrolSpeed;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        RB = GetComponentInChildren<Rigidbody2D>();
        enemyDeath = GetComponentInChildren<EnemyDeath>();

        if (enemyDeath == null)
        {
            Debug.LogError("EnemyDeath script is missing on " + gameObject.name);
        }
    }

    /*private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        origin = Vector3.zero;
    }

    /*private void OnDisable()
    {
        anim.SetBool("isMoving", false);
    }*/

    private void Update()
    {
        Vector3 dir = enemy.localScale.x > 0 ? Vector3.right : Vector3.left;

        fieldOfView.SetOrigin(enemy.position);
        fieldOfView.SetAimDirection(dir);
        // 🔥 STEP 3: Re-assign player AFTER respawn
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }

        if (enemyDeath != null && enemyDeath.isDead)
        {
            anim.SetBool("isMoving", false);
            return;
        }

        #region ENEMY CHASE LOGIC
        bool canSeePlayer = CanSeePlayer();

        // ⚠️ START ALERT
        if (canSeePlayer && !isChasingPlayer && !isAlerting)
        {
            isAlerting = true;
            alertTimer = 0;
            loseSightTimer = loseSightCooldown;
        }

        // ⚠️ HANDLE ALERT
        if (isAlerting)
        {
            anim.SetBool("isMoving", false);

            if (alertTimer == 0)
                anim.SetTrigger("alert");

            alertTimer += Time.deltaTime;

            // 👁️ Maintain / reduce cooldown
            if (canSeePlayer)
                loseSightTimer = loseSightCooldown;
            else
                loseSightTimer -= Time.deltaTime;

            // ✅ Commit to chase
            if (alertTimer >= alertDuration && canSeePlayer)
            {
                isAlerting = false;
                isChasingPlayer = true;
            }

            // ❌ Give up
            if (loseSightTimer <= 0)
            {
                isAlerting = false;
                movingLeft = !movingLeft; // turn around
            }

            return;
        }

        // 🏃 HANDLE CHASE
        if (isChasingPlayer && playerTransform != null)
        {
            if (canSeePlayer)
            {
                loseSightTimer = loseSightCooldown;
            }
            else
            {
                loseSightTimer -= Time.deltaTime;

                if (loseSightTimer <= 0)
                {
                    isChasingPlayer = false;
                    movingLeft = !movingLeft; // turn around
                }
            }
        }

        // 🔥 SMOOTH SPEED
        float targetSpeed = isChasingPlayer ? chaseSpeed : patrolSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * acceleration);

        // 🏃 CHASE MOVEMENT
        if (isChasingPlayer)
        {
            anim.SetBool("isMoving", true);

            Vector3 direction;

            if (playerTransform != null && enemy.position.x > playerTransform.position.x)
            {
                direction = Vector3.left;
                enemy.localScale = new Vector3(-Mathf.Abs(initScale.x), initScale.y, initScale.z);
            }
            else
            {
                direction = Vector3.right;
                enemy.localScale = new Vector3(Mathf.Abs(initScale.x), initScale.y, initScale.z);
            }

            if (IsTouchingWall())
            {
                isChasingPlayer = false;
                waitingAtWall = true;
                anim.SetBool("isMoving", false);
                return;
            }

            enemy.position += direction * currentSpeed * Time.deltaTime;
            return;
        }
        #endregion


        #region ENEMY PATROL LOGIC
        // 🧠 PATROL

        if (IsTouchingWall())
        {
            waitingAtWall = true;
            anim.SetBool("isMoving", false);
        }

        if (waitingAtWall)
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= idleDuration)
            {
                movingLeft = !movingLeft;
                idleTimer = 0;
                waitingAtWall = false;
            }

            return;
        }

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
        #endregion


        #region GAME OVER CHECK
        if (LevelManager.instance.isGameOver)
        {
            RB.linearVelocity = Vector2.zero;
            return;
        }
        #endregion


    }

    #region ENEMY CHASE METHODS
    // 👁️ VISION
    private bool CanSeePlayer()
    {
        if (playerTransform == null) return false;

        Vector3 directionToPlayer = (playerTransform.position - enemy.position).normalized;

        float distance = Vector3.Distance(enemy.position, playerTransform.position);
        if (distance > viewDistance)
            return false;

        Vector3 facingDirection = enemy.localScale.x > 0 ? Vector3.right : Vector3.left;
        float angle = Vector3.Angle(facingDirection, directionToPlayer);

        return angle < viewAngle / 2f;
    }

    private void ChangeDirection()
    {
        anim.SetBool("isMoving", false);

        idleTimer += Time.deltaTime;

        if (idleTimer > idleDuration)
        {
            movingLeft = !movingLeft;
            idleTimer = 0;
            waitingAtWall = false;
        }

    }

    private void MoveInDirection(int _direction)
    {
        idleTimer = 0;
        anim.SetBool("isMoving", true);

        enemy.localScale = new Vector3(Mathf.Abs(initScale.x) * _direction,
        initScale.y, initScale.z);

        Vector3 direction = _direction == -1 ? Vector3.left : Vector3.right;

        enemy.position += direction * currentSpeed * Time.deltaTime;
    }
    #endregion
    private bool IsTouchingWall()
    {
        if (wallCheck == null) return false;

        Vector2 direction = movingLeft ? Vector2.left : Vector2.right;

        return Physics2D.Raycast(
            wallCheck.position,
            direction,
            wallCheckDistance,
            wallLayer
        );
    }

    public void ResetAI()
    {
        isChasingPlayer = false;
        isAlerting = false;

        alertTimer = 0f;
        loseSightTimer = 0f;

        waitingAtWall = false;

        currentSpeed = patrolSpeed;

        // Reset movement direction (optional)
        movingLeft = false;

        // Reset animation
        if (anim != null)
            anim.SetBool("isMoving", false);

        // 🔥 VERY IMPORTANT: reset player reference
        playerTransform = null;

        RB.linearVelocity = Vector2.zero;
    }

    // 👁️ DEBUG
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