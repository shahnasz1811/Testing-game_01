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

    [Header("Flashing Settings")]
    [SerializeField] private SpriteColorFlasher spriteColorFlasher;
    private SpriteRenderer spriteRend;

    [Header("Dissolve Settings")]
    [SerializeField] private float dissolveDuration = 1f;

    private Material mat;
    private float dissolveAmount = 0f;

    private EnemyPatrol enemyPatrol;
    private bool hasCountedKill = false;

    // Cache the shader property ID for efficiency and safety
    private int dissolvePropertyID;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        spriteColorFlasher = GetComponent<SpriteColorFlasher>();
        spriteRend = GetComponentInChildren<SpriteRenderer>();
        enemyPatrol = GetComponentInParent<EnemyPatrol>();

        // Resolve the shader's property ID
        dissolvePropertyID = Shader.PropertyToID("_DissolveAmount");
    }

    private void Start()
    {
        LevelManager.instance.RegisterResettable(this);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Color c = Color.red;
            spriteRend.color = c;
        }
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

        // Get the active material instance safely right when death starts
        if (spriteRend != null)
        {
            mat = spriteRend.material; // accessing .material automatically instantiates a local copy in Unity
            mat.SetFloat(dissolvePropertyID, 0f);
        }

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

            if (mat != null)
            {
                mat.SetFloat(dissolvePropertyID, dissolveAmount); // Update shader
            }

            Debug.Log($"Dissolving: {dissolveAmount}");
            yield return null;
        }

        if (mat != null)
        {
            mat.SetFloat(dissolvePropertyID, 1f);
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

            gameObject.SetActive(true);

            // Reset dynamic material state
            if (spriteRend != null)
            {
                mat = spriteRend.material;
                mat.SetFloat(dissolvePropertyID, 0f);
            }

            if (enemyPatrol != null)
            {
                enemyPatrol.enabled = true;
                enemyPatrol.ResetAI();
            }

            Debug.Log("Enemy reset");
        }
    }
}