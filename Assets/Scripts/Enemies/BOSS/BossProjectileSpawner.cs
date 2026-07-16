using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Requirement #2 (the "shoots projectiles with different patterns" half).
// BossController calls BeginAttacking()/StopAllPatterns() - this script
// doesn't know or care about her vertical-tracking movement, it just fires
// patterns on a loop while active.
//
// All 3 patterns fire from firePoint (on the boss, fixed on one side of the
// arena) AIMED AT THE PLAYER - nothing spawns above the arena or drops down.
// The attack-loop timer below only controls the RATE of fire between
// patterns; it has nothing to do with her position, which is handled
// entirely by BossController reading the player's height every frame.
public class BossProjectileSpawner : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private BossProjectile projectilePrefab;
    [Tooltip("Where shots originate - place this at the beak, on the boss's fixed side of the arena.")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private int startPoolSize = 20;

    [Header("Pattern Rhythm")]
    [SerializeField] private float telegraphDuration = 0.35f;
    [SerializeField] private float timeBetweenPatterns = 1.1f;

    [Header("Telegraph Flash (optional)")]
    [Tooltip("The beak/head sprite to flash right before firing, so the player gets a warning. Leave empty to skip.")]
    [SerializeField] private SpriteRenderer telegraphSprite;
    [SerializeField] private Color telegraphColor = new Color(1f, 0.35f, 0.35f);

    [Header("Pattern: Aimed Burst (primary - snapshots player position)")]
    [SerializeField] private int aimedBurstCount = 3;
    [SerializeField] private float aimedBurstInterval = 0.15f;
    [SerializeField] private float aimedSpeed = 8f;

    [Header("Pattern: Spread Fan (centered on the player's direction)")]
    [SerializeField] private int spreadCount = 5;
    [SerializeField] private float spreadArcDegrees = 45f;
    [SerializeField] private float spreadSpeed = 6f;

    [Header("Pattern: Rapid Volley (fast flurry of single aimed shots)")]
    [SerializeField] private int volleyCount = 6;
    [SerializeField] private float volleyInterval = 0.12f;
    [SerializeField] private float volleySpeed = 9f;

    private readonly List<BossProjectile> pool = new List<BossProjectile>();
    private Transform player;
    private Coroutine attackLoop;
    private SpriteColorFlasher flasher;

    private void Awake()
    {
        for (int i = 0; i < startPoolSize; i++)
            pool.Add(CreateProjectile());

        if (telegraphSprite != null)
            flasher = gameObject.AddComponent<SpriteColorFlasher>();
    }

    private BossProjectile CreateProjectile()
    {
        BossProjectile p = Instantiate(projectilePrefab, transform);
        p.gameObject.SetActive(false);
        return p;
    }

    // Same idea as Traps/ArrowTrap.cs's FindArrow(): reuse an inactive
    // projectile if one exists, otherwise the pool grows by one.
    private BossProjectile GetProjectile()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].gameObject.activeInHierarchy)
                return pool[i];
        }

        BossProjectile fresh = CreateProjectile();
        pool.Add(fresh);
        return fresh;
    }

    public void BeginAttacking(Transform playerTransform)
    {
        player = playerTransform;
        if (attackLoop == null)
            attackLoop = StartCoroutine(AttackLoop());
    }

    public void StopAllPatterns()
    {
        if (attackLoop != null)
        {
            StopCoroutine(attackLoop);
            attackLoop = null;
        }
    }

    // Instantly clears any bullets currently in flight - call this when the
    // boss dies so the screen doesn't keep threatening the player after the fight is over.
    public void ClearActiveProjectiles()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i].gameObject.activeInHierarchy)
                pool[i].gameObject.SetActive(false);
        }
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            yield return Telegraph();

            // Cycle through the three patterns at random so the fight doesn't
            // become memorizable after one attempt. All three fire from
            // firePoint toward the player - none of them spawn overhead.
            switch (Random.Range(0, 3))
            {
                case 0: yield return AimedBurstRoutine(); break;
                case 1: yield return SpreadFanRoutine(); break;
                default: yield return RapidVolleyRoutine(); break;
            }

            yield return new WaitForSeconds(timeBetweenPatterns);
        }
    }

    private IEnumerator Telegraph()
    {
        if (telegraphSprite != null && flasher != null)
            flasher.FlashColor(telegraphSprite, telegraphDuration, telegraphColor);

        yield return new WaitForSeconds(telegraphDuration);
    }

    // Primary pattern: N shots in a row, each freshly aimed at wherever the
    // player currently is. Dodge = break line of sight or juke as she fires.
    private IEnumerator AimedBurstRoutine()
    {
        for (int i = 0; i < aimedBurstCount; i++)
        {
            GetProjectile().Fire(firePoint.position, GetAimDirection(), aimedSpeed);
            yield return new WaitForSeconds(aimedBurstInterval);
        }
    }

    // A narrow fan centered on the player's direction. Dodge = get out of the fan.
    private IEnumerator SpreadFanRoutine()
    {
        Vector2 origin = firePoint.position;
        Vector2 baseDir = GetAimDirection();
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
        float startAngle = baseAngle - spreadArcDegrees * 0.5f;
        float step = spreadCount > 1 ? spreadArcDegrees / (spreadCount - 1) : 0f;

        for (int i = 0; i < spreadCount; i++)
        {
            float angle = (startAngle + step * i) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            GetProjectile().Fire(origin, dir, spreadSpeed);
        }

        yield return null;
    }

    // A fast flurry of single aimed shots - more pressure than the burst,
    // still every single one aimed at the player from firePoint.
    private IEnumerator RapidVolleyRoutine()
    {
        for (int i = 0; i < volleyCount; i++)
        {
            GetProjectile().Fire(firePoint.position, GetAimDirection(), volleySpeed);
            yield return new WaitForSeconds(volleyInterval);
        }
    }

    private Vector2 GetAimDirection()
    {
        if (player == null) return Vector2.left;

        Vector2 dir = (Vector2)player.position - (Vector2)firePoint.position;
        return dir.sqrMagnitude > 0.001f ? dir.normalized : Vector2.left;
    }
}
