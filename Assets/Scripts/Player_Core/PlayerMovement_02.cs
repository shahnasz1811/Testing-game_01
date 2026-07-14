using System.Collections;
using UnityEngine;

public class PlayerMovement_02 : MonoBehaviour
{
    public PlayerData Data;
    public bool IsDead { get; set; }

    // Enums to strictly manage our visual states
    private enum MovementState { Idle, Running, Jumping, Falling, WallSliding }
    private MovementState _currentState;

    #region Variables
    // Components
    public Rigidbody2D RB { get; private set; }
    private Animator _anim;

    // Movement Properties
    public bool IsFacingRight { get; private set; } = true;
    public bool IsJumping { get; private set; }
    public bool IsWallJumping { get; private set; }
    public bool IsSliding { get; private set; }

    // Timers
    public float LastOnGroundTime { get; private set; }
    public float LastOnWallTime { get; private set; }
    public float LastOnWallRightTime { get; private set; }
    public float LastOnWallLeftTime { get; private set; }
    public float LastPressedJumpTime { get; private set; }

    // Jump Flags
    private bool _isJumpCut;
    private bool _isJumpFalling;
    private float _wallJumpStartTime;
    private int _lastWallJumpDir;
    private Vector2 _moveInput;

    [Header("Checks")]
    [SerializeField] private Transform _groundCheckPoint;
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
    [Space(5)]
    [SerializeField] private Transform _frontWallCheckPoint;
    [SerializeField] private Transform _backWallCheckPoint;
    [SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);

    [Header("Layers & Tags")]
    [SerializeField] private LayerMask _groundLayer;
    #endregion

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
    }

    private void Start()
    {
        SetGravityScale(Data.gravityScale);
    }

    private void Update()
    {
        if (IsDead) return;

        HandleTimers();
        HandleInput();
        HandleCollisions();
        HandleJumpLogic();
        HandleSlideLogic();
        HandleGravity();

        // Separated completely from physics logic
        DetermineMovementState();
        AnimatePlayer();
    }

    private void FixedUpdate()
    {
        if (IsDead) return;

        Run(IsWallJumping ? Data.wallJumpRunLerp : 1f);

        if (IsSliding)
            Slide();
    }

    #region CORE LOGIC SUBSTEPS
    private void HandleTimers()
    {
        LastOnGroundTime -= Time.deltaTime;
        LastOnWallTime -= Time.deltaTime;
        LastOnWallRightTime -= Time.deltaTime;
        LastOnWallLeftTime -= Time.deltaTime;
        LastPressedJumpTime -= Time.deltaTime;
    }

    private void HandleInput()
    {
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");

        if (_moveInput.x != 0)
            CheckDirectionToFace(_moveInput.x > 0);

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.J))
            OnJumpInput();

        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.C) || Input.GetKeyUp(KeyCode.J))
            OnJumpUpInput();
    }

    private void HandleCollisions()
    {
        if (IsJumping) return;

        // Ground Check
        if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer))
        {
            LastOnGroundTime = Data.coyoteTime;
        }

        // Right Wall Check
        if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)
                || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)) && !IsWallJumping)
            LastOnWallRightTime = Data.coyoteTime;

        // Left Wall Check
        if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)
            || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)) && !IsWallJumping)
            LastOnWallLeftTime = Data.coyoteTime;

        LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
    }

    private void HandleJumpLogic()
    {
        if (IsJumping && RB.linearVelocity.y < 0)
        {
            IsJumping = false;
            if (!IsWallJumping) _isJumpFalling = true;
        }

        if (IsWallJumping && Time.time - _wallJumpStartTime > Data.wallJumpTime)
        {
            IsWallJumping = false;
        }

        if (LastOnGroundTime > 0 && !IsJumping && !IsWallJumping)
        {
            _isJumpCut = false;
            _isJumpFalling = false;
        }

        // Triggering Jumps
        if (CanJump() && LastPressedJumpTime > 0)
        {
            IsJumping = true;
            IsWallJumping = false;
            _isJumpCut = false;
            _isJumpFalling = false;
            Jump();
        }
        else if (CanWallJump() && LastPressedJumpTime > 0)
        {
            IsWallJumping = true;
            IsJumping = false;
            _isJumpCut = false;
            _isJumpFalling = false;
            _wallJumpStartTime = Time.time;
            _lastWallJumpDir = (LastOnWallRightTime > 0) ? -1 : 1;

            WallJump(_lastWallJumpDir);
        }
    }

    private void HandleSlideLogic()
    {
        IsSliding = CanSlide() && ((LastOnWallLeftTime > 0 && _moveInput.x < 0) || (LastOnWallRightTime > 0 && _moveInput.x > 0));
    }

    private void HandleGravity()
    {
        if (IsSliding)
        {
            SetGravityScale(0);
        }
        else if (RB.linearVelocity.y < 0 && _moveInput.y < 0) // Fast Fall
        {
            SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
            RB.linearVelocity = new Vector2(RB.linearVelocity.x, Mathf.Max(RB.linearVelocity.y, -Data.maxFastFallSpeed));
        }
        else if (_isJumpCut) // Short hop cut
        {
            SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
            RB.linearVelocity = new Vector2(RB.linearVelocity.x, Mathf.Max(RB.linearVelocity.y, -Data.maxFallSpeed));
        }
        else if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.linearVelocity.y) < Data.jumpHangTimeThreshold) // Apex Float
        {
            SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
        }
        else if (RB.linearVelocity.y < 0) // Normal Fall
        {
            SetGravityScale(Data.gravityScale * Data.fallGravityMult);
            RB.linearVelocity = new Vector2(RB.linearVelocity.x, Mathf.Max(RB.linearVelocity.y, -Data.maxFallSpeed));
        }
        else // Grounded or normal ascending
        {
            SetGravityScale(Data.gravityScale);
        }
    }
    #endregion

    #region MOVEMENT PHYSICS EXECUTIONS
    private void Run(float lerpAmount)
    {
        float targetSpeed = _moveInput.x * Data.runMaxSpeed;
        targetSpeed = Mathf.Lerp(RB.linearVelocity.x, targetSpeed, lerpAmount);

        float accelRate;
        if (LastOnGroundTime > 0)
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
        else
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;

        if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.linearVelocity.y) < Data.jumpHangTimeThreshold)
        {
            accelRate *= Data.jumpHangAccelerationMult;
            targetSpeed *= Data.jumpHangMaxSpeedMult;
        }

        if (Data.doConserveMomentum && Mathf.Abs(RB.linearVelocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(RB.linearVelocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
        {
            accelRate = 0;
        }

        float speedDif = targetSpeed - RB.linearVelocity.x;
        float movement = speedDif * accelRate;
        RB.AddForce(movement * Vector2.right, ForceMode2D.Force);

        if (LastOnGroundTime > 0 && Mathf.Abs(_moveInput.x) < 0.01f && Mathf.Abs(RB.linearVelocity.x) < 0.15f)
        {
            RB.linearVelocity = new Vector2(0f, RB.linearVelocity.y);
        }
    }

    private void Slide()
    {
        float speedDif = Data.slideSpeed - RB.linearVelocity.y;
        float movement = speedDif * Data.slideAccel;
        movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));
        RB.AddForce(movement * Vector2.up);
    }

    private void Jump()
    {
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;

        float force = Data.jumpForce;
        if (RB.linearVelocity.y < 0) force -= RB.linearVelocity.y;

        RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        _anim.SetTrigger("Jump");
    }

    private void WallJump(int dir)
    {
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;
        LastOnWallRightTime = 0;
        LastOnWallLeftTime = 0;

        Vector2 force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y) { x = Data.wallJumpForce.x * dir };

        if (Mathf.Sign(RB.linearVelocity.x) != Mathf.Sign(force.x))
            force.x -= RB.linearVelocity.x;

        if (RB.linearVelocity.y < 0)
            force.y -= RB.linearVelocity.y;

        RB.AddForce(force, ForceMode2D.Impulse);

        IsSliding = false;
        _anim.SetTrigger("Jump");
    }

    private void Turn()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        IsFacingRight = !IsFacingRight;
    }
    #endregion

    #region ANIMATION & STATE ENGINE
    private void DetermineMovementState()
    {
        // 1. Structural priority: Are we wall sliding?
        if (IsSliding)
        {
            _currentState = MovementState.WallSliding;
            return;
        }

        // 2. Are we in mid-air?
        if (LastOnGroundTime <= 0)
        {
            _currentState = RB.linearVelocity.y > 0.1f ? MovementState.Jumping : MovementState.Falling;
            return;
        }

        // 3. We are grounded
        _currentState = (Mathf.Abs(_moveInput.x) > 0.01f && Mathf.Abs(RB.linearVelocity.x) > 0.15f)
            ? MovementState.Running
            : MovementState.Idle;
    }

    private void AnimatePlayer()
    {
        // Update standard structural bools cleanly without conflicts
        _anim.SetBool("isGrounded", LastOnGroundTime > 0);
        _anim.SetBool("isSliding", _currentState == MovementState.WallSliding);

        // Pass speed value out for a blend tree or running transitions
        float runValue = (_currentState == MovementState.Running) ? Mathf.Abs(RB.linearVelocity.x) : 0f;
        _anim.SetFloat("Run", runValue);

        // Optional: you can pass yVelocity directly to your animator if you use vertical blend trees
        _anim.SetFloat("yVelocity", RB.linearVelocity.y);
    }
    #endregion

    public void SetGravityScale(float scale)
    {
        if (RB != null)
        {
            RB.gravityScale = scale;
        }
    }

    #region HELPER CHECKS
    public void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != IsFacingRight) Turn();
    }

    private bool CanJump() => LastOnGroundTime > 0 && !IsJumping;
    private bool CanJumpCut() => IsJumping && RB.linearVelocity.y > 0;
    private bool CanWallJumpCut() => IsWallJumping && RB.linearVelocity.y > 0;

    private bool CanWallJump()
    {
        return LastPressedJumpTime > 0 && LastOnWallTime > 0 && LastOnGroundTime <= 0 && (!IsWallJumping ||
             (LastOnWallRightTime > 0 && _lastWallJumpDir == 1) || (LastOnWallLeftTime > 0 && _lastWallJumpDir == -1));
    }

    public bool CanSlide()
    {
        return LastOnWallTime > 0 && !IsJumping && !IsWallJumping && LastOnGroundTime <= 0 && RB.linearVelocity.y < 0;
    }
    #endregion

    public void OnJumpInput() => LastPressedJumpTime = Data.jumpInputBufferTime;
    public void OnJumpUpInput() { if (CanJumpCut() || CanWallJumpCut()) _isJumpCut = true; }

    private void OnDrawGizmosSelected()
    {
        // Draw Ground Check Box
        if (_groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
        }

        // Draw Front Wall Check Box
        if (_frontWallCheckPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
        }

        // Draw Back Wall Check Box
        if (_backWallCheckPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
        }
    }
}