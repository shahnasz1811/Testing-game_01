using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Requirement #2: she stays pinned to a fixed horizontal spot (like a sentry
// on the wall she emerged from) and tracks the PLAYER'S HEIGHT - climb, she
// rises with you; drop back down, she follows you back down. There is no
// timer deciding WHEN she reacts - her target height is always read from the
// player's actual position. What IS timed now (on purpose, per feedback) is
// how she reacts: a short reaction delay before she starts responding to a
// height change, then a slow, heavy glide to catch up - so she reads as a
// big, weighty creature rather than a robot locked perfectly to your Y.
//
// Requirement #1 hooks in via PlayIntroRoutine()/BeginCombat(), called by
// BossIntroCutscene.cs.
// Requirement #3 hooks in via ICrushable.OnCrushed(), called by the modified
// CrushKill.cs when the dropped boulder lands on her.
[RequireComponent(typeof(Rigidbody2D))]
public class BossController : MonoBehaviour, ICrushable, IResettable
{
    private enum BossState { Hidden, Intro, Tracking, Defeated }

    private struct HeightSample
    {
        public float time;
        public float y;
    }

    [Header("References")]
    [SerializeField] private Transform player; // auto-found by tag if left empty
    [SerializeField] private BossProjectileSpawner spawner; // auto-found on this object if left empty
    [SerializeField] private Animator anim; // optional
    [Tooltip("Colliders that should only be live once the fight actually starts (hurtbox, crush point). Kept off while hidden/defeated.")]
    [SerializeField] private Collider2D[] combatColliders;

    [Header("Key Positions (empty child Transforms)")]
    [SerializeField] private Transform hiddenPoint;      // where she waits out of sight, e.g. below/inside the bushes
    [SerializeField] private Transform firstPerchPoint;  // where she settles once the intro finishes - this also sets her fixed X for the whole fight

    [Header("Vertical Tracking (big, heavy boss feel)")]
    [Tooltip("Lowest Y she'll follow the player down to.")]
    [SerializeField] private float minY = -2f;
    [Tooltip("Highest Y she'll follow the player up to - the lever room ceiling.")]
    [SerializeField] private float topY = 20f;
    [Tooltip("How long she waits before reacting to a change in the player's height - gives her a beat of 'noticing you' instead of instant reflexes. She tracks where you WERE this long ago, not where you are right now.")]
    [SerializeField] private float reactionDelay = 0.4f;
    [Tooltip("Roughly how long it takes her to glide to the target height once she starts moving (SmoothDamp time). Bigger = slower and heavier.")]
    [SerializeField] private float smoothTime = 0.5f;
    [Tooltip("Hard cap on how fast she can ever move vertically, even when catching up a big gap - keeps her from ever snapping.")]
    [SerializeField] private float maxTrackSpeed = 6f;
    [Tooltip("Optional vertical offset from the player's exact position (0 = dead level with the player).")]
    [SerializeField] private float heightOffset = 0f;

    [Header("Intro")]
    [SerializeField] private float introDuration = 1.1f;
    [SerializeField] private AnimationCurve introEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Health")]
    [Tooltip("How many boulder drops it takes to kill her. The reference fight uses 1.")]
    [SerializeField] private int hitsToDefeat = 1;

    [Header("Events")]
    [Tooltip("Wire up whatever should happen on death here: unlock a door, spawn the egg/reward, play a victory beat, etc.")]
    public UnityEvent onBossDefeated;

    private Rigidbody2D rb;
    private BossState state = BossState.Hidden;
    private float fixedX; // her horizontal position for the whole fight, set once when combat begins
    private int hitsTaken;
    private bool hasEngaged; // true once BeginCombat() has ever actually run - see ResetState()
    private float verticalVelocity; // SmoothDamp's running velocity, not a designer-facing speed
    private readonly Queue<HeightSample> heightHistory = new Queue<HeightSample>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (spawner == null)
            spawner = GetComponent<BossProjectileSpawner>();

        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }
    }

    private void Start()
    {
        if (hiddenPoint != null)
            rb.position = hiddenPoint.position;

        fixedX = rb.position.x;

        SetCombatCollidersEnabled(false);

        if (LevelManager.instance != null)
            LevelManager.instance.RegisterResettable(this);
    }

    private void FixedUpdate()
    {
        if (LevelManager.instance != null && LevelManager.instance.isGameOver) return;

        if (state == BossState.Tracking)
        {
            RecordPlayerHeight();
            TrackPlayerVertically();
        }
    }

    private void RecordPlayerHeight()
    {
        if (player == null) return;

        heightHistory.Enqueue(new HeightSample { time = Time.time, y = player.position.y });

        // Trim samples older than we could ever need (a little slack past reactionDelay).
        while (heightHistory.Count > 0 && Time.time - heightHistory.Peek().time > reactionDelay + 0.5f)
            heightHistory.Dequeue();
    }

    // Walks the recorded history to find what the player's height was
    // reactionDelay seconds ago. Falls back to the player's current height if
    // there isn't enough history yet (e.g. the first instant combat begins).
    private float GetDelayedPlayerHeight()
    {
        if (player == null) return rb.position.y;

        float targetTime = Time.time - reactionDelay;
        float result = player.position.y;

        foreach (HeightSample sample in heightHistory)
        {
            if (sample.time > targetTime) break;
            result = sample.y;
        }

        return result;
    }

    // The only movement rule in the whole fight: glide toward the player's
    // height FROM A MOMENT AGO. No timers deciding whether to move, only how
    // quickly and with what lag - both purely cosmetic/feel knobs above.
    private void TrackPlayerVertically()
    {
        if (player == null) return;

        float delayedY = GetDelayedPlayerHeight();
        float targetY = Mathf.Clamp(delayedY + heightOffset, minY, topY);

        float newY = Mathf.SmoothDamp(rb.position.y, targetY, ref verticalVelocity, smoothTime, maxTrackSpeed, Time.fixedDeltaTime);
        rb.MovePosition(new Vector2(fixedX, newY));
    }

    // --- Requirement #1: called by BossIntroCutscene -------------------------

    // Eases her from hiddenPoint up to firstPerchPoint. Does NOT start
    // attacking yet - call BeginCombat() once the cutscene hands control back
    // to the player, so the first shots can't land while they're still frozen.
    public IEnumerator PlayIntroRoutine()
    {
        if (state == BossState.Defeated) yield break;

        state = BossState.Intro;
        SetCombatCollidersEnabled(false);

        Vector2 from = hiddenPoint != null ? (Vector2)hiddenPoint.position : rb.position;
        Vector2 to = firstPerchPoint != null ? (Vector2)firstPerchPoint.position : rb.position;

        rb.position = from;

        float t = 0f;
        while (t < introDuration)
        {
            t += Time.deltaTime;
            float eased = introEase.Evaluate(Mathf.Clamp01(t / introDuration));
            rb.MovePosition(Vector2.LerpUnclamped(from, to, eased));
            yield return null;
        }

        rb.MovePosition(to);
    }

    public void BeginCombat()
    {
        if (state == BossState.Defeated) return;

        fixedX = rb.position.x; // locks in wherever the intro left her horizontally - never changes again
        verticalVelocity = 0f;
        heightHistory.Clear();
        hasEngaged = true;
        state = BossState.Tracking;

        SetCombatCollidersEnabled(true);

        if (spawner != null)
            spawner.BeginAttacking(player);
    }

    // --- Requirement #3: called by CrushKill via ICrushable -------------------

    public void OnCrushed(float impactForce)
    {
        if (state == BossState.Defeated || state == BossState.Hidden) return;

        hitsTaken++;

        if (hitsTaken >= hitsToDefeat)
            Defeat();

        // If you want a multi-hit fight later, an "else" branch here is the
        // place for a stagger/flinch reaction that doesn't kill her outright.
    }

    private void Defeat()
    {
        state = BossState.Defeated;
        StopAllCoroutines();

        if (spawner != null)
        {
            spawner.StopAllPatterns();
            spawner.ClearActiveProjectiles();
        }

        SetCombatCollidersEnabled(false);

        if (anim != null)
            anim.SetTrigger("defeated");

        onBossDefeated?.Invoke();
    }

    // --- IResettable ------------------------------------------------------

    // NOTE: this only resumes the fight (at firstPerchPoint) if it had
    // actually started - i.e. BeginCombat() has run at least once. If the
    // player dies to something unrelated BEFORE ever reaching the
    // BossIntroCutscene trigger, this puts her back in Hidden instead of
    // waking her up early. If combat WAS already running, this resets to
    // "resets to just before the boss room" the same way the reference
    // fight does, rather than replaying the whole intro on every death.
    public void ResetState()
    {
        StopAllCoroutines();

        hitsTaken = 0;
        verticalVelocity = 0f;
        heightHistory.Clear();

        if (spawner != null)
        {
            // Stop the OLD attack loop before asking for a new one - it's a
            // coroutine owned by BossProjectileSpawner, not this component,
            // so StopAllCoroutines() above never touched it. Without this,
            // BeginAttacking()'s own "already running" guard would just
            // no-op and the old loop would keep going untouched.
            spawner.StopAllPatterns();
            spawner.ClearActiveProjectiles();
        }

        if (anim != null)
            anim.ResetTrigger("defeated");

        if (!hasEngaged)
        {
            state = BossState.Hidden;
            SetCombatCollidersEnabled(false);

            if (hiddenPoint != null)
                rb.position = hiddenPoint.position;

            fixedX = rb.position.x;
            return;
        }

        Vector2 resumePoint = firstPerchPoint != null ? (Vector2)firstPerchPoint.position : rb.position;
        rb.position = resumePoint;
        fixedX = resumePoint.x;

        state = BossState.Tracking;
        SetCombatCollidersEnabled(true);

        if (spawner != null)
            spawner.BeginAttacking(player);
    }

    private void SetCombatCollidersEnabled(bool value)
    {
        if (combatColliders == null) return;

        foreach (Collider2D col in combatColliders)
            if (col != null) col.enabled = value;
    }

    private void OnDrawGizmosSelected()
    {
        float x = hiddenPoint != null ? hiddenPoint.position.x : transform.position.x;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(x, minY, 0f), new Vector3(x, topY, 0f));

        if (hiddenPoint != null)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(hiddenPoint.position, 0.4f);
        }

        if (firstPerchPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firstPerchPoint.position, 0.4f);
        }
    }
}
