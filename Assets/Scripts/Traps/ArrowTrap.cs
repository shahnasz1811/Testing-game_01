using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
    [SerializeField] private float attackCooldown;
    [SerializeField] private float initialDelay = 0f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject[] arrows;
    private float cooldownTimer;

    private void Start()
    {
        // Fire the first shot after just initialDelay seconds, instead of
        // always waiting a full attackCooldown like every shot after it.
        // Defaults to 0, so it fires as soon as it spawns unless you set
        // this to something else (e.g. to telegraph the trap fairly).
        cooldownTimer = attackCooldown - initialDelay;
    }

    private void Attack()
    {
        cooldownTimer = 0;

        int arrowIndex = FindArrow();

        if (arrowIndex < 0 || arrowIndex >= arrows.Length)
        {
            Debug.LogWarning("No arrows available!");
            return;
        }

        GameObject arrow = arrows[arrowIndex];

        arrow.transform.position = firePoint.position;

        arrow.GetComponent<EnemyProjectile>().ActivateProjectile();
    }

    private int FindArrow()
    {
        for (int i = 0; i < arrows.Length; i++)
        {
            if (!arrows[i].activeInHierarchy)
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