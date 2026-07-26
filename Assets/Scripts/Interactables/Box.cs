using UnityEngine;
using System.Collections;

public class Box : MonoBehaviour, IResettable
{
    [Header("Breakable")]
    [Tooltip("How many enemy hits it takes to break.")]
    [SerializeField] private int health = 1;
    [Tooltip("Optional VFX prefab spawned where the box was when it breaks. Leave empty to skip.")]
    [SerializeField] private GameObject breakEffect;

    private Vector3 startPos;
    private int startHealth;
    private bool broken;
    private ParticleSystem particle;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;


    private void Awake()
    {
        particle = GetComponentInChildren<ParticleSystem>();
        audioSource = GetComponentInChildren<AudioSource>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }   

    private void Start()
    {
        startPos = transform.position;
        startHealth = health;
        LevelManager.instance.RegisterResettable(this);
    }

    // Called by MeleeEnemy.DamagePlayer() when an enemy's wall-check finds
    // this box blocking its path instead of a real wall.
    public void TakeDamage(int amount)
    {
        if (broken) return;

        health -= amount;

        if (health <= 0)
            StartCoroutine(Break());
    }

    private IEnumerator Break()
    {
        broken = true;

        if (particle != null)
        {
            particle.Play();
            audioSource.Play();
            spriteRenderer.enabled = false;
            boxCollider.enabled = false;
        }

        yield return new WaitForSeconds(particle.main.startLifetime.constantMax);

        // Disables the collider along with everything else, so the next
        // EnemyPatrol wall-check raycast simply stops hitting anything here
        // - no extra cleanup needed on the enemy's side.
        //gameObject.SetActive(false);
    }

    public void ResetState()
    {
        transform.position = startPos;
        health = startHealth;
        broken = false;
        spriteRenderer.enabled = true;
        boxCollider.enabled = true;
    }
}