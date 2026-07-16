using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class CrushKill : MonoBehaviour, IResettable
{
    [SerializeField] private float minImpactForce = 7f;
    private ParticleSystem particle;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private AudioSource audioSource;

    private Vector3 startPos;

    private void Awake()
    {
        particle = GetComponentInChildren<ParticleSystem>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        audioSource = GetComponentInChildren<AudioSource>();

    }

    private void Start()
    {
        startPos = transform.position;
        LevelManager.instance.RegisterResettable(this);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Enemy")) return;

        float impact = collision.relativeVelocity.magnitude;

        if (impact > minImpactForce)
        {
            // Anything with custom death/defeat logic (e.g. BossController)
            // implements ICrushable. GetComponentInParent so the crush
            // collider can live on a child hitbox while the actual
            // controller sits on the root object.
            ICrushable crushable = collision.gameObject.GetComponentInParent<ICrushable>();
            if (crushable != null)
            {
                crushable.OnCrushed(impact);
            }
            else
            {
                // Fallback: regular patrol enemies use EnemyDeath, unchanged.
                collision.gameObject.GetComponent<EnemyDeath>()?.Die();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hazard"))
        {
            StartCoroutine(Break());
        }
    }

    private IEnumerator Break()
    {
        if (particle != null)
        {
            particle.Play();
            audioSource.Play();
            spriteRenderer.enabled = false;
            boxCollider.enabled = false;
        }

        yield return new WaitForSeconds(particle.main.startLifetime.constantMax);
        //gameObject.SetActive(false);
    }

    public void ResetState()
    {
        transform.position = startPos;
        //gameObject.SetActive(true);
        spriteRenderer.enabled = true;
        boxCollider.enabled = true;
    }
}