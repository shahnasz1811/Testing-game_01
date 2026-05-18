using UnityEngine;

public class Hazard : MonoBehaviour
{
    [Header("Settings")]
    public string playerTag = "Player"; // make sure your player GameObject is tagged "Player"

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object that entered is the player
        if (collision.CompareTag(playerTag))
        {
            // Try to get the PlayerDeath script from the player
            PlayerDeath playerDeath = collision.GetComponent<PlayerDeath>();

            if (playerDeath != null)
            {
                playerDeath.Die(); // call the death method
            }
            else
            {
                Debug.LogWarning("PlayerDeath script not found on Player!");
            }
        }
    }
}
