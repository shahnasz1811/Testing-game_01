using UnityEngine;

public class CrushKill : MonoBehaviour
{
    [SerializeField] private float minImpactForce = 7f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Enemy")) return;

        float impact = collision.relativeVelocity.magnitude;

        if (impact > minImpactForce)
        {
            collision.gameObject.GetComponent<EnemyDeath>()?.Die();
        }
    }
}