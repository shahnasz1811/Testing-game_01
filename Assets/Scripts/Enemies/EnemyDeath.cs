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
        StartCoroutine(DissolveAndDie());

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
    }

    private void HideEnemy()
    {
        gameObject.SetActive(false); // Hide the enemy now that the effect is done
    }

    public void ResetState()
    {
        if (isDead)
        {
            CancelInvoke();

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

            // Reset the AI (position, facing/scale, animator state) BEFORE
            // the object becomes visible again, so it never shows for a
            // frame in its old, pre-death orientation/pose.
            if (enemyPatrol != null)
            {
                enemyPatrol.enabled = true;
                enemyPatrol.ResetAI();
            }

            gameObject.SetActive(true);

            Debug.Log("Enemy reset");
        }
    }
}