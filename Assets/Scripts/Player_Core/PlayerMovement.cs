using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D body;
    private Animator anim;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Jump Settings")]
    public float jumpForce = 7f;   // fixed jump force
    public float jumpCooldown = 0.1f;
    private float lastJumpTime;

    /*[Header("Charged Jump")]
    public float minJumpForce = 5f;
    public float maxJumpForce = 15f;
    public float maxChargeTime = 1.5f;*/

    /*private float currentCharge;
    private bool isCharging;

    [Header("Charge Delay")]
    public float chargeStartTime = 0.2f;

    [Header("Jump Cooldown")]
    public float jumpCooldown = 0.1f;
    private float lastJumpTime;*/

    //private float holdTime;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private bool isGrounded;
  
    private Vector3 originalScale;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;

        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        // LEFT & RIGHT MOVEMENT
        float moveInput = Input.GetAxisRaw("Horizontal");

        body.linearVelocity = new Vector2(moveInput * moveSpeed,body.linearVelocity.y);

        /* STOP MOVEMENT WHILE CHARGING
        if (isCharging)
        {
            body.linearVelocity = new Vector2(0,body.linearVelocity.y);
        }*/

        // FLIP SPRITE
        if (moveInput > 0)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(originalScale.x),
                originalScale.y,
                originalScale.z
            );
        }
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x),originalScale.y,originalScale.z);
        }

        // GROUND CHECK
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // TAP JUMP
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && Time.time > lastJumpTime + jumpCooldown)
        {
            body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            lastJumpTime = Time.time;

            // trigger jump animation if you have one
            anim.SetTrigger("Jump");
        }

        /* START HOLDING
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            holdTime = 0f;
            isCharging = false;
        }

        // HOLDING SPACE
        /*if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            holdTime += Time.deltaTime;

            // START CHARGING AFTER DELAY
            if (holdTime >= chargeStartTime)
            {
                isCharging = true;

                currentCharge += Time.deltaTime;

                currentCharge = Mathf.Clamp(currentCharge,0,maxChargeTime);

                // STOP MOVEMENT ONLY WHEN ACTUALLY CHARGING
                body.linearVelocity = new Vector2(0,body.linearVelocity.y);
            }
        }*/


        // RELEASE JUMP
        /*if (Input.GetKeyUp(KeyCode.Space) && isGrounded && Time.time > lastJumpTime + jumpCooldown)
        {
            float jumpForce;

            // TAP JUMP
            if (!isCharging)
            {
                jumpForce = minJumpForce;
            }
            // CHARGED JUMP
            else
            {
                float chargePercent = currentCharge / maxChargeTime;

                jumpForce = Mathf.Lerp(minJumpForce,maxJumpForce,chargePercent);
            }

            // Apply impulse force instead of setting velocity
            body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            // Record jump time for cooldown
            lastJumpTime = Time.time;

            // Reset states
            isCharging = false;
            currentCharge = 0f;
            holdTime = 0f;
        }*/

        // STOP ROTATION
        body.angularVelocity = 0;

        //Set animators parameters
        anim.SetBool("Run", moveInput != 0);

    }

    // GROUND CHECK VISUAL
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position,groundCheckRadius);
    }

}