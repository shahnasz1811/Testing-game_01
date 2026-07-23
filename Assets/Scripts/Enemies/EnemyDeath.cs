using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyDeath : MonoBehaviour, IResettable
{
    private Animator anim;
    private Rigidbody2D body;

    [Header("Respawn Settings")]
    public Transform respawnPoint;

    public bool isDead;
    private SpriteRenderer[] spriteRenders;

    [Header("Dissolve Settings")]
    [SerializeField] private float dissolveDuration = 1f;

    //  Specify UnityEngine.Material explicitly:
    private List<UnityEngine.Material> materials = new List<UnityEngine.Material>();
    private float dissolveAmount = 0f;

    private EnemyPatrol enemyPatrol;
    private bool hasCountedKill = false;
    private Coroutine dissolveRoutine;

    // Cache the shader property ID for efficiency and safety
    private int dissolvePropertyID;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        spriteRenders = GetComponentsInChildren<SpriteRenderer>();
        enemyPatrol = GetComponentInParent<EnemyPatrol>();

        // Resolve the shader's property ID
        dissolvePropertyID = Shader.PropertyToID("_DissolveAmount");
    }

    private void Start()
    {
        LevelManager.instance.RegisterResettable(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Hazard") || collision.gameObject.CompareTag("DeathZone"))
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;

        if (!hasCountedKill)
        {
            hasCountedKill = true;
            LevelManager.instance.EnemyDied();
        }

        Debug.Log("DIE called on: " + gameObject.name);

        // Disable patrol immediately
        if (enemyPatrol != null)
        {
            enemyPatrol.enabled = false;
        }

        anim.SetBool("isMoving", false);

        materials.Clear();
        foreach (SpriteRenderer ren in spriteRenders)
        {
            if (ren != null)
            {
                Material dynamicMat = ren.material; // Automatically instantiates a local copy
                dynamicMat.SetFloat(dissolvePropertyID, 0f);
                materials.Add(dynamicMat);
            }
        }

        enemyPatrol.DisableAIOnDeath();

        Debug.Log("Dissolve started");
        dissolveRoutine = StartCoroutine(DissolveAndDie());

        // Disable collisions so dead enemy doesn't block player
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        body.linearVelocity = Vector2.zero;
        body.bodyType = RigidbodyType2D.Kinematic; // Keep it kinematic during dissolve so it doesn't fall through floor

        Debug.Log("Resettable count: " + LevelManager.instance.resettables.Count);
    }

    IEnumerator DissolveAndDie()
    {
        float time = 0f;

        while (time < dissolveDuration)
        {
            time += Time.deltaTime;
            dissolveAmount = Mathf.Clamp01(time / dissolveDuration);

            // 5. Loop through and dissolve every single sprite part together
            foreach (Material mat in materials)
            {
                if (mat != null)
                {
                    mat.SetFloat(dissolvePropertyID, dissolveAmount);
                }
            }

            Debug.Log($"Dissolving: {dissolveAmount}");
            yield return null;
        }

        foreach (UnityEngine.Material mat in materials)
        {
            if (mat != null)
            {
                mat.SetFloat(dissolvePropertyID, 1f);
            }
        }

        HideEnemy();
        dissolveRoutine = null;
    }

    private void HideEnemy()
    {
        gameObject.SetActive(false); // Hide the enemy now that the effect is done
    }

    public void ResetState()
    {
        bool wasDead = isDead;

        if (wasDead)
        {
            // CancelInvoke() doesn't touch this - DissolveAndDie() is a
            // coroutine, not an Invoke(). If it's still mid-flight (enemy
            // died right around the same time as the player), it must be
            // stopped explicitly or it'll finish on its own later, hide
            // this GameObject again, and leave isDead permanently false
            // since nothing else would re-flag it as dead.
            if (dissolveRoutine != null)
            {
                StopCoroutine(dissolveRoutine);
                dissolveRoutine = null;
            }

            isDead = false;
            hasCountedKill = false;

            transform.position = respawnPoint.position;
            body.bodyType = RigidbodyType2D.Dynamic;

            // Re-enable collider
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = true;

            // 6. Reset all dynamic materials back to normal upon respawning
            foreach (SpriteRenderer ren in spriteRenders)
            {
                if (ren != null)
                {
                    ren.material.SetFloat(dissolvePropertyID, 0f);
                }
            }
        }

        // Always drop back into a clean patrol state - whether this enemy
        // actually died, or was still alive and mid-chase/mid-alert when
        // the player died. Without this, a still-alive chasing enemy keeps
        // isChasingPlayer set and beelines straight for the player's fresh
        // spawn position the instant the game-over freeze lifts, instead of
        // needing to re-detect them through the vision cone first.
        if (enemyPatrol != null)
        {
            enemyPatrol.enabled = true;
            enemyPatrol.ResetAI();
        }

        if (wasDead)
        {
            gameObject.SetActive(true);
            Debug.Log("Enemy reset");
        }
    }
}