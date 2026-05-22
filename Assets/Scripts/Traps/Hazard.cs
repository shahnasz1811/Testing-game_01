using UnityEngine;

public class Hazard : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Try PLAYER
        PlayerDeath player = collision.GetComponent<PlayerDeath>();
        if (player != null)
        {
            player.Die();
            return;
        }

        // Try ENEMY
        EnemyDeath enemy = collision.GetComponent<EnemyDeath>();
        if (enemy != null)
        {
            enemy.Die();
            return;
        }
    }
}