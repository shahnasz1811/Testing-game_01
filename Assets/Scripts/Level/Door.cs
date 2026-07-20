using System.Collections;
using UnityEngine;

// Sits at the end of a level. Starts locked (physically blocks the player).
// LevelManager calls Open() once every enemy is dead, which lets the player
// pass through. Walking through the open door tells LevelManager to show
// the victory screen.
public class Door : MonoBehaviour, IResettable
{
    [Header("Behaviour")]
    [Tooltip("If true, the door physically blocks the player while locked. If false, it's purely visual and the player can already walk through it.")]
    [SerializeField] private bool blockPlayerWhileLocked = true;

    [Tooltip("Delay between the door unlocking and the player actually being able to walk through it. Use this to sync with an opening animation. Set to 0 for an instant opening.")]
    [SerializeField] private float openDelay = 0.3f;

    [Header("Visuals (all optional)")]
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;

    [Header("Audio (optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;

    private Collider2D doorCollider;
    private Coroutine openRoutine;
    private bool exitTriggered;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (anim == null)
            anim = GetComponent<Animator>();
    }

    private void Start()
    {
        // Register with the level so it knows this is the exit to open once
        // the objective is complete, and so it gets reset (re-locked) along
        // with everything else (enemies, boxes, ...) if the player dies.
        if (LevelManager.instance != null)
        {
            LevelManager.instance.RegisterExitDoor(this);
            LevelManager.instance.RegisterResettable(this);
        }

        ApplyLockedState();
    }

    // Called by LevelManager once every enemy in the level is dead.
    public void Open()
    {
        if (IsOpen) return;

        IsOpen = true;

        if (anim != null)
            anim.SetBool("IsOpen", true);

        if (spriteRenderer != null && openSprite != null)
            spriteRenderer.sprite = openSprite;

        if (audioSource != null && openSound != null)
            audioSource.PlayOneShot(openSound);

        Debug.Log(gameObject.name + ": objective complete, door opening.");

        if (openRoutine != null)
            StopCoroutine(openRoutine);

        openRoutine = StartCoroutine(AllowPassageAfterDelay());
    }

    private IEnumerator AllowPassageAfterDelay()
    {
        yield return new WaitForSeconds(openDelay);

        // Stop physically blocking the player and start listening for them
        // walking through.
        if (doorCollider != null)
            doorCollider.isTrigger = true;

        openRoutine = null;
    }

    // IResettable - called by LevelManager.ResetAll() (e.g. when the player
    // dies and the level resets). Re-locks the door so it matches the
    // enemies respawning.
    public void ResetState()
    {
        if (openRoutine != null)
        {
            StopCoroutine(openRoutine);
            openRoutine = null;
        }

        IsOpen = false;
        exitTriggered = false;
        ApplyLockedState();
    }

    private void ApplyLockedState()
    {
        if (doorCollider != null)
            doorCollider.isTrigger = !blockPlayerWhileLocked;

        if (spriteRenderer != null && closedSprite != null)
            spriteRenderer.sprite = closedSprite;

        if (anim != null)
            anim.SetBool("IsOpen", false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryReachExit(collision);
    }

    // Fallback: if the player was already standing against the door when it
    // unlocked, Unity can miss the Enter event since the collider merely
    // switches to a trigger rather than freshly entering it. Stay still
    // fires every physics step for anything currently overlapping, so this
    // catches that case. exitTriggered stops it firing more than once.
    private void OnTriggerStay2D(Collider2D collision)
    {
        TryReachExit(collision);
    }

    private void TryReachExit(Collider2D collision)
    {
        if (!IsOpen || exitTriggered) return;

        if (collision.CompareTag("Player") && LevelManager.instance != null)
        {
            exitTriggered = true;
            LevelManager.instance.OnPlayerReachedExit();
        }
    }
}