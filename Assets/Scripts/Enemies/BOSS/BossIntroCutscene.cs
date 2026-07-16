using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

// Requirement #1. Place this on a tall, thin trigger zone at the entrance to
// the boss arena (a wall the player has to walk into to proceed right).
//
// Sequence: freeze the player -> pan camera to where the boss is hiding ->
// boss emerges (BossController.PlayIntroRoutine) -> hold -> pan back ->
// unfreeze -> BossController.BeginCombat().
[RequireComponent(typeof(Collider2D))]
public class BossIntroCutscene : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BossController boss;
    [Tooltip("Your existing Player_Core/CameraFollow.cs component. Leave empty if you're driving the camera with the Cinemachine vcam instead (use Intro Camera below).")]
    [SerializeField] private CameraFollow cameraFollow;
    [Tooltip("Your CinemachineCamera (Cinemachine 3.x) that normally tracks the player. Leave empty if you're driving the camera with CameraFollow instead. Only fill in ONE of CameraFollow / Intro Camera.")]
    [SerializeField] private CinemachineCamera introCamera;
    [Tooltip("An empty Transform used as the camera's temporary focus point during the cutscene. Position doesn't matter, it gets moved every frame.")]
    [SerializeField] private Transform cutsceneFocusPoint;

    [Header("Behaviour")]
    [Tooltip("Only fires while the player is facing/moving right, matching 'goes to the right' rather than backtracking into the zone.")]
    [SerializeField] private bool requireMovingRight = true;
    [SerializeField] private bool triggerOnce = true;

    [Header("Timing")]
    [SerializeField] private float panToBossTime = 1f;
    [SerializeField] private float holdOnBossTime = 0.75f;
    [SerializeField] private float panBackTime = 0.8f;

    private bool hasTriggered;
    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered && triggerOnce) return;
        if (!other.CompareTag("Player")) return;

        if (requireMovingRight)
        {
            PlayerMovement_02 facingCheck = other.GetComponent<PlayerMovement_02>();
            // IsFacingRight tracks last horizontal input direction, which is a
            // steadier signal than instantaneous velocity (which can read ~0
            // for a frame at a jump's apex even while moving right overall).
            if (facingCheck != null && !facingCheck.IsFacingRight) return;
        }

        hasTriggered = true;
        StartCoroutine(PlayCutscene(other.transform));
    }

    private IEnumerator PlayCutscene(Transform player)
    {
        PlayerMovement_02 movement = player.GetComponent<PlayerMovement_02>();
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();

        if (movement != null) movement.enabled = false;
        if (playerRb != null) playerRb.linearVelocity = Vector2.zero;

        bool drivingCamera = cutsceneFocusPoint != null && (cameraFollow != null || introCamera != null);
        Transform originalCameraFollowTarget = null;
        Transform originalCinemachineTarget = null;

        if (drivingCamera)
        {
            cutsceneFocusPoint.position = player.position;

            if (cameraFollow != null)
            {
                originalCameraFollowTarget = cameraFollow.player;
                cameraFollow.player = cutsceneFocusPoint;
            }

            if (introCamera != null)
            {
                // CameraTarget is a struct, so it has to be copied out, edited,
                // and reassigned - you can't write introCamera.Target.TrackingTarget directly.
                originalCinemachineTarget = introCamera.Target.TrackingTarget;
                CameraTarget camTarget = introCamera.Target;
                camTarget.TrackingTarget = cutsceneFocusPoint;
                introCamera.Target = camTarget;
            }

            Vector3 startPos = player.position;
            Vector3 bossRevealPos = boss.transform.position; // still at hiddenPoint at this stage
            float t = 0f;
            while (t < panToBossTime)
            {
                t += Time.deltaTime;
                cutsceneFocusPoint.position = Vector3.Lerp(startPos, bossRevealPos, t / panToBossTime);
                yield return null;
            }
        }

        // She rises from hiddenPoint to firstPerchPoint while the camera holds on the reveal spot.
        yield return StartCoroutine(boss.PlayIntroRoutine());

        if (drivingCamera)
        {
            cutsceneFocusPoint.position = boss.transform.position; // settle on wherever she ended up
            yield return new WaitForSeconds(holdOnBossTime);

            Vector3 panBackStart = cutsceneFocusPoint.position;
            float t = 0f;
            while (t < panBackTime)
            {
                t += Time.deltaTime;
                cutsceneFocusPoint.position = Vector3.Lerp(panBackStart, player.position, t / panBackTime);
                yield return null;
            }

            if (cameraFollow != null)
                cameraFollow.player = originalCameraFollowTarget;

            if (introCamera != null)
            {
                CameraTarget camTarget = introCamera.Target;
                camTarget.TrackingTarget = originalCinemachineTarget;
                introCamera.Target = camTarget;
            }
        }
        else
        {
            yield return new WaitForSeconds(holdOnBossTime);
        }

        if (movement != null) movement.enabled = true;
        col.enabled = false; // prevent re-triggering if the player backtracks

        boss.BeginCombat();
    }
}
