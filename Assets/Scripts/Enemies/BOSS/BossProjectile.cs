using UnityEngine;

// Same idea as Traps/EnemyProjectile.cs (pooled, self-deactivating), but it can
// travel in ANY direction, which the arrow trap version doesn't need to do.
//
// SETUP (per prefab instance):
//   - Tag the GameObject "Hazard". PlayerDeath.cs already kills the player on
//     contact with anything tagged "Hazard" - no extra code needed here.
//   - Collider2D must be "Is Trigger" ON.
//   - Add a Rigidbody2D, Body Type = Kinematic, Gravity Scale doesn't matter.
[RequireComponent(typeof(Rigidbody2D))]
public class BossProjectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 6f;

    private Rigidbody2D rb;
    private float timer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Called by BossProjectileSpawner when this projectile is pulled from the pool.
    public void Fire(Vector2 position, Vector2 direction, float speed)
    {
        transform.position = position;
        timer = 0f;
        gameObject.SetActive(true);
        rb.linearVelocity = direction.normalized * speed;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
            Deactivate();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignore anything that's ALSO just a trigger. Logic-only zones in this
        // project - the boss intro cutscene wall, rope-interaction zones,
        // checkpoints, etc. - are always set up as "Is Trigger" colliders with
        // no real geometry behind them, so a shot flying through one isn't
        // actually hitting anything. Ground/walls and the player use solid
        // (non-trigger) colliders, so they still stop the shot below.
        if (collision.isTrigger) return;

        // Ignore the boss's own hurtbox/crush-point so it can't insta-despawn
        // its own bullets the moment they spawn at the beak.
        if (collision.CompareTag("Enemy")) return;

        // Ignore other projectiles. Patterns like the spread fan fire several
        // of these from the exact same point in the exact same instant, so
        // without this they detect each other as a "hit" on the very next
        // physics step and deactivate immediately - looks like a blink
        // instead of a shot.
        if (collision.GetComponent<BossProjectile>() != null) return;

        Deactivate(); // hit the player, a wall, cover, whatever - either way it's done
    }

    private void Deactivate()
    {
        rb.linearVelocity = Vector2.zero;
        gameObject.SetActive(false);
    }
}