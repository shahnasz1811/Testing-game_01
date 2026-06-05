using UnityEngine;

public class RopeTrigger : MonoBehaviour
{
    [SerializeField] private HangingObject hangingObject;

    private Collider2D Collider2D => GetComponent<Collider2D>();
    private bool playerInRange;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            hangingObject.Interact();
            Collider2D.enabled = false; // Disable this trigger after interaction
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerInRange = false;
    }
}