using UnityEngine;
using System.Collections;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float resetTime = 1f;

    private float lifetime;
    private ParticleSystem particle;
    private SpriteRenderer spriteRenderer;
    private Collider2D projectileCollider;
    private bool isBreaking;

    private void Awake()
    {
        particle = GetComponentInChildren<ParticleSystem>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        projectileCollider = GetComponent<Collider2D>();
    }

    public void ActivateProjectile()
    {
        lifetime = 0f;
        isBreaking = false;

        // Re-enable visual and collision components when spawning/reusing
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (projectileCollider != null) projectileCollider.enabled = true;

        gameObject.SetActive(true);
    }

    private void Update()
    {
        // Don't move if we are in the process of breaking
        if (isBreaking) return;

        float movementSpeed = speed * Time.deltaTime;
        transform.Translate(movementSpeed, 0, 0);

        lifetime += Time.deltaTime;
        if (lifetime > resetTime)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isBreaking) return;

        if (collision.CompareTag("Hazard") || collision.CompareTag("Ground") || collision.CompareTag("Player"))
        {
            StartCoroutine(Break());
        }
    }

    private IEnumerator Break()
    {
        isBreaking = true;

        // Hide the arrow graphics and turn off collision
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (projectileCollider != null) projectileCollider.enabled = false;

        if (particle != null)
        {
            particle.Play();
            // Wait for the duration of the particle effect before hiding the GameObject
            yield return new WaitForSeconds(particle.main.duration);
        }

        gameObject.SetActive(false);
    }
}