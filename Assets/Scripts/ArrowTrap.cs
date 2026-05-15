using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
    [SerializeField] private float attackCooldown;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject[] fireballs;
    private float cooldownTimer;

    private void Attack()
    {
        cooldownTimer = 0;

        int fireballIndex = FindFireball();

        if (fireballIndex < 0 || fireballIndex >= fireballs.Length)
        {
            Debug.LogWarning("No fireballs available!");
            return;
        }

        GameObject fireball = fireballs[fireballIndex];

        fireball.transform.position = firePoint.position;

        fireball.GetComponent<EnemyProjectile>()
            .SetDirection(Mathf.Sign(transform.localScale.x));
    }

    private int FindFireball()
    {
        for (int i = 0; i < fireballs.Length; i++)
        {
            if (!fireballs[i].activeInHierarchy)
            {
                return i;
            }
        }

        return -1;
    }

    private void Update()
    {
        cooldownTimer += Time.deltaTime;
        if (cooldownTimer >= attackCooldown)
        {
            Attack();
        }
    }
}
