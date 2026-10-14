using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Gives a UI screen (main menu, victory screen, name panel...) controller focus.
/// The EventSystem only moves between buttons if something is ALREADY selected.
/// With a mouse nothing needs to be selected, which is why the menus "work" but a
/// controller does nothing. This selects a button when the screen appears, and
/// re-selects one if the selection is lost (e.g. after clicking empty background).
/// Works while Time.timeScale = 0.
/// </summary>
public class MenuControllerFocus : MonoBehaviour
{
    [Tooltip("Button that gets focus first. Leave empty to use the first interactable one found below this object.")]
    [SerializeField] private Selectable firstSelected;

    [Tooltip("If focus is lost, restore it when the player presses a stick/d-pad/submit.")]
    [SerializeField] private bool reselectIfLost = true;

    private void OnEnable()
    {
        StartCoroutine(SelectNextFrame());
    }

    private IEnumerator SelectNextFrame()
    {
        // Wait a frame so the EventSystem and layout are ready after SetActive(true).
        yield return null;
        Select();
    }

    private void Update()
    {
        if (!reselectIfLost) return;

        EventSystem es = EventSystem.current;
        if (es == null) return;

        GameObject current = es.currentSelectedGameObject;
        bool lost = current == null || !current.activeInHierarchy;

        if (lost && NavigationInputPressed())
            Select();
    }

    private static bool NavigationInputPressed()
    {
        return Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5f
            || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.5f
            || Input.GetButtonDown("Submit");
    }

    public void Select()
    {
        EventSystem es = EventSystem.current;
        if (es == null) return;

        Selectable target = firstSelected;

        if (target == null || !target.IsActive() || !target.IsInteractable())
        {
            foreach (Selectable s in GetComponentsInChildren<Selectable>(false))
            {
                if (s.IsInteractable())
                {
                    target = s;
                    break;
                }
            }
        }

        if (target == null) return;

        es.SetSelectedGameObject(null); // clear first so the highlight state refreshes
        es.SetSelectedGameObject(target.gameObject);
    }
}
