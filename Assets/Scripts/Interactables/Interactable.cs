using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private InteractPromptUI promptUI;

    private bool playerInRange;

    private void Start()
    {
        promptUI.Hide();
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    private void Interact()
    {
        Debug.Log("INTERACTED");

        // CALL YOUR ACTION HERE
        GetComponent<IInteractableAction>()?.Interact();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            promptUI.Show();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            promptUI.Hide();
        }
    }
}