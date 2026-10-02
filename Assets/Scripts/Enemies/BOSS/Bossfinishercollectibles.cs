using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

// Level 10 win-condition piece: the player collects this (e.g. an egg/prize
// at the top of the arena), which freezes them, OPTIONALLY holds the camera
// on the boss for a beat, then releases one or more HangingObject rocks (see
// HangingObject.cs) so they drop and crush the boss through the EXISTING
// CrushKill -> ICrushable -> BossController.OnCrushed chain - same physics
// path as the manual RopeTrigger version, just fired automatically instead
// of requiring the player to press E.
//
// This script doesn't touch BossController directly at all. Defeat, the
// onBossDefeated event, and completing the level are still entirely
// BossController's job once a rock actually lands on her with enough force.
//
// Covers three different feels depending on what you leave assigned:
// - Instant (no pause, no camera): leave Camera Follow/Intro Camera AND
//   Hold Point empty, set Hold Duration to 0.
// - Brief pause, no camera movement: leave Camera Follow/Intro Camera and
//   Hold Point empty, set Hold Duration > 0.
// - Full cutscene: assign Hold Point plus EITHER Camera Follow OR Intro
//   Camera (only one - same rule as BossIntroCutscene), and Hold Duration.
public class BossFinisherCollectible : MonoBehaviour, IResettable
{
    [Header("Rocks To Release")]
    [Tooltip("The hanging rock(s) to drop once collected. Assign 1 for a single finishing rock, or several for a staggered sequence.")]
    [SerializeField] private HangingObject[] rocksToRelease;
    [Tooltip("Delay between each rock in the list above. Ignored if there's only one.")]
    [SerializeField] private float delayBetweenRocks = 0.3f;

    [Header("Cutscene (all optional - see class comment above)")]
    [Tooltip("Your existing Player_Core/CameraFollow.cs component. Leave empty if using Intro Camera instead, or leave both empty to skip camera movement.")]
    [SerializeField] private CameraFollow cameraFollow;
    [Tooltip("Your CinemachineCamera that normally tracks the player. Leave empty if using Camera Follow instead.")]
    [SerializeField] private CinemachineCamera introCamera;
    [Tooltip("Where the camera holds while the rocks fall - usually something framing the boss. Leave empty to skip camera movement entirely.")]
    [SerializeField] private Transform holdPoint;
    [Tooltip("How long to pause after collecting before the rocks release. Works even with no camera assigned (just a beat of stillness).")]
    [SerializeField] private float holdDuration = 1.2f;

    [Header("Pickup")]
    [SerializeField] private SpriteRenderer visual;      // auto-found on this object if left empty
    [SerializeField] private Collider2D triggerCollider; // auto-found on this object if left empty

    private bool collected;

    private void Awake()
    {
        if (visual == null) visual = GetComponent<SpriteRenderer>();
        if (triggerCollider == null) triggerCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (LevelManager.instance != null)
            LevelManager.instance.RegisterResettable(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collected || !collision.CompareTag("Player")) return;
        collected = true;

        PlayerMovement_02 movement = collision.GetComponent<PlayerMovement_02>();
        Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();

        StartCoroutine(FinisherRoutine(movement, playerRb));
    }

    private IEnumerator FinisherRoutine(PlayerMovement_02 movement, Rigidbody2D playerRb)
    {
        // Hide/disable the pickup immediately so it can't be grabbed twice.
        if (visual != null) visual.enabled = false;
        if (triggerCollider != null) triggerCollider.enabled = false;

        if (movement != null) movement.enabled = false;
        if (playerRb != null) playerRb.linearVelocity = Vector2.zero;

        bool drivingCamera = holdPoint != null && (cameraFollow != null || introCamera != null);
        Transform originalCameraFollowTarget = null;
        Transform originalCinemachineTarget = null;

        if (drivingCamera)
        {
            if (cameraFollow != null)
            {
                originalCameraFollowTarget = cameraFollow.player;
                cameraFollow.player = holdPoint;
            }

            if (introCamera != null)
            {
                // CameraTarget is a struct - copy out, edit, reassign.
                originalCinemachineTarget = introCamera.Target.TrackingTarget;
                CameraTarget camTarget = introCamera.Target;
                camTarget.TrackingTarget = holdPoint;
                introCamera.Target = camTarget;
            }
        }

        if (holdDuration > 0f)
            yield return new WaitForSeconds(holdDuration);

        if (drivingCamera)
        {
            if (cameraFollow != null)
                cameraFollow.player = originalCameraFollowTarget;

            if (introCamera != null)
            {
                CameraTarget camTarget = introCamera.Target;
                camTarget.TrackingTarget = originalCinemachineTarget;
                introCamera.Target = camTarget;
            }
        }

        yield return ReleaseRocks();

        if (movement != null) movement.enabled = true;
    }

    // Cuts each assigned rope in order. With one rock this just fires once;
    // with several, delayBetweenRocks staggers them instead of dumping
    // everything at the same instant.
    private IEnumerator ReleaseRocks()
    {
        if (rocksToRelease == null) yield break;

        for (int i = 0; i < rocksToRelease.Length; i++)
        {
            if (rocksToRelease[i] != null)
                rocksToRelease[i].Interact();

            if (i < rocksToRelease.Length - 1 && delayBetweenRocks > 0f)
                yield return new WaitForSeconds(delayBetweenRocks);
        }
    }

    public void ResetState()
    {
        collected = false;

        if (visual != null) visual.enabled = true;
        if (triggerCollider != null) triggerCollider.enabled = true;

        // Deliberately NOT resetting rocksToRelease here - each HangingObject
        // already registers itself with LevelManager and resets its own
        // hinge/position independently (see HangingObject.ResetState()).
    }
}