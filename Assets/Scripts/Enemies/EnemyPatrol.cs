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
    [Tooltip("While chasing, if the player's X position is within this distance of the enemy's, don't change facing/direction. Without this, the player standing directly above/below the enemy makes it flicker left-right every frame as the sign of the tiny X difference keeps flipping.")]
    [SerializeField] private float chaseDeadzone = 0.15f;

    private float currentSpeed;
    private Vector3 initScale;
    [SerializeField] private bool movingLeft;
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

    // Set by MeleeEnemy instead of toggling `enabled` directly (see Update()).
    // Keeping this component enabled at all times means it can always react
    // instantly to ResetAI() on respawn, and it stays in charge of its own
    // animator state (no other script writes to "isMoving").
    public bool isInMeleeRange;

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

    // Set whenever the wall-check ray hits a Box instead of a real wall (see
    // IsTouchingWall()/CheckForBox() below). MeleeEnemy reads this to attack
    // the box instead of the player - see its Update()/DamagePlayer().
    public Box blockingBox;
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

        // Freeze immediately once the player is dead. This used to sit at
        // the bottom of Update(), but the isAlerting and isChasingPlayer
        // branches below both `return` before ever reaching it - so a
        // chasing enemy would keep walking straight at the player's frozen
        // death position for the whole death/respawn delay. Checking this
        // first makes it apply no matter what state the enemy is in.
        if (LevelManager.instance.isGameOver)
        {
            if (RB != null)
                RB.linearVelocity = Vector2.zero;

            anim.SetBool("isMoving", false);
            return;
        }

        // While MeleeEnemy has the player in melee range, just idle here.
        // We do NOT disable this component for that (MeleeEnemy used to do
        // `enemyPatrol.enabled = false`), because that could freeze this
        // script mid-update right after a respawn re-enabled/reset it - the
        // walk animation would keep looping (nothing else clears
        // "isMoving") while the transform stopped moving, and wall
        // detection (below) would stop running too. Idling in-place here
        // keeps animation state correct and keeps this Update loop alive.
        if (isInMeleeRange)
        {
            anim.SetBool("isMoving", false);
            return;
        }

        // A Box sitting on wallLayer used to read as a permanent wall here -
        // the enemy would idle/turn at it forever while its own collider
        // kept physically overlapping the box's (the actual "stuck" bug).
        // Catching it here instead freezes the enemy the same way
        // isInMeleeRange does, and lets MeleeEnemy attack it - once the box
        // breaks (its collider disables itself, see Box.Break()) this stops
        // detecting anything and patrol/chase resumes on its own next frame.
        blockingBox = CheckForBox();
        if (blockingBox != null)
        {
            anim.SetBool("isMoving", false);
            return;
        }

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

        // alertTimer used to only ever count up (see the isAlerting block
        // below) and simply froze once chasing began - so alertRatio stayed
        // maxed out forever, and the cone would get stuck on alertColor
        // instead of fading back to normalColor after losing the player.
        // Counting it back down whenever we're not actively building alert
        // lets the same Lerp below fade it back out gradually instead.
        if (!isAlerting)
        {
            alertTimer = Mathf.Max(0f, alertTimer - Time.deltaTime);
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
            Vector3 direction = Vector3.zero;

            if (playerTransform != null)
            {
                float xDiff = playerTransform.position.x - enemy.position.x;

                // Only change facing/direction once the player is far enough
                // to one side - inside the deadzone, keep whatever facing we
                // already had instead of re-deciding every frame.
                if (xDiff < -chaseDeadzone)
                {
                    direction = Vector3.left;
                    enemy.localScale = new Vector3(-Mathf.Abs(initScale.x), initScale.y, initScale.z);
                }
                else if (xDiff > chaseDeadzone)
                {
                    direction = Vector3.right;
                    enemy.localScale = new Vector3(Mathf.Abs(initScale.x), initScale.y, initScale.z);
                }
            }

            anim.SetBool("isMoving", direction != Vector3.zero);

            // Player is directly overhead (within the deadzone) - hold
            // position rather than moving toward a direction we didn't pick.
            if (direction == Vector3.zero)
                return;

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

    private RaycastHit2D WallCheckHit()
    {
        if (wallCheck == null) return default;
        Vector2 direction = movingLeft ? Vector2.left : Vector2.right;

        return Physics2D.Raycast(wallCheck.position, direction, wallCheckDistance, wallLayer);
    }

    private bool IsTouchingWall()
    {
        RaycastHit2D hit = WallCheckHit();
        // A Box on wallLayer isn't a real wall - CheckForBox()/blockingBox
        // handles it separately (attack it instead of turning around at it).
        return hit.collider != null && hit.collider.GetComponent<Box>() == null;
    }

    private Box CheckForBox()
    {
        RaycastHit2D hit = WallCheckHit();
        return hit.collider != null ? hit.collider.GetComponent<Box>() : null;
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
        isInMeleeRange = false;
        blockingBox = null;

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