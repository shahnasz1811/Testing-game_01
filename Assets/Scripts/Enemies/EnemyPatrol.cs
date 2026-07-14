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
    private bool defaultMovingLeft; // Caches the starting direction

    [Header("Idle Behaviour")]
    [SerializeField] private float idleDuration;
    private float idleTimer;

    [Header("Enemy Animator")]
    [SerializeField] private Animator anim;

    // 👁️ VISION
    [Header("Vision Cone")]
    [SerializeField] private DynamicVisionCone dynamicVisionCone;

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

        // Track your initial direction choice setup in inspector
        defaultMovingLeft = movingLeft;

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

    private void Update()
    {
        if (enemyDeath != null && enemyDeath.isDead) return;

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }

        bool canSeePlayer = false;
        if (dynamicVisionCone != null)
        {
            canSeePlayer = dynamicVisionCone.CheckPlayerDetection(playerTransform);
        }

        float alertRatio = alertDuration > 0 ? (alertTimer / alertDuration) : 0f;

        if (dynamicVisionCone != null)
        {
            dynamicVisionCone.UpdateVisionCone(canSeePlayer, isChasingPlayer, alertRatio);
        }

        #region ENEMY CHASE LOGIC
        if (canSeePlayer && !isChasingPlayer && !isAlerting)
        {
            isAlerting = true;
            alertTimer = 0;
            loseSightTimer = loseSightCooldown;
        }

        if (isAlerting)
        {
            anim.SetBool("isMoving", false);

            if (alertTimer == 0)
                anim.SetTrigger("alert");

            alertTimer += Time.deltaTime;

            if (canSeePlayer)
                loseSightTimer = loseSightCooldown;
            else
                loseSightTimer -= Time.deltaTime;

            if (alertTimer >= alertDuration && canSeePlayer)
            {
                isAlerting = false;
                isChasingPlayer = true;
            }

            if (loseSightTimer <= 0)
            {
                isAlerting = false;
                movingLeft = !movingLeft;
            }

            return;
        }

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
                    movingLeft = !movingLeft;
                }
            }
        }

        float targetSpeed = isChasingPlayer ? chaseSpeed : patrolSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * acceleration);

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

        enemy.localScale = new Vector3(Mathf.Abs(initScale.x) * _direction, initScale.y, initScale.z);
        Vector3 direction = _direction == -1 ? Vector3.left : Vector3.right;
        enemy.position += direction * currentSpeed * Time.deltaTime;
    }

    private bool IsTouchingWall()
    {
        if (wallCheck == null) return false;
        Vector2 direction = movingLeft ? Vector2.left : Vector2.right;

        return Physics2D.Raycast(wallCheck.position, direction, wallCheckDistance, wallLayer);
    }

    // Called instantly by EnemyDeath.Die() to prevent vision cone bugs
    public void DisableAIOnDeath()
    {
        this.enabled = false;
        if (dynamicVisionCone != null)
        {
            dynamicVisionCone.gameObject.SetActive(false);
        }
    }

    public void ResetAI()
    {
        this.enabled = true;

        isChasingPlayer = false;
        isAlerting = false;
        waitingAtWall = false;

        alertTimer = 0f;
        loseSightTimer = 0f;
        idleTimer = 0f;

        currentSpeed = patrolSpeed;
        movingLeft = defaultMovingLeft;

        // Restore initial spatial scale context
        enemy.localScale = initScale;

        if (RB != null)
        {
            RB.linearVelocity = Vector2.zero;
        }

        // Clean up the animator state completely
        if (anim != null)
        {
            anim.SetBool("isMoving", false);
            anim.SetBool("isSliding", false);
            anim.ResetTrigger("alert"); // Flushes any cached triggers
        }

        playerTransform = null;

        if (dynamicVisionCone != null)
        {
            dynamicVisionCone.gameObject.SetActive(true);
        }
    }
}