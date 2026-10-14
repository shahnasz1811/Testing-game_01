using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Makes controller navigation go through a vertical menu strictly in order
/// (top -> bottom), instead of letting Unity guess with "Automatic" navigation.
///
/// Why: Automatic picks the neighbour with the best distance/direction score. When
/// buttons are different widths and left-aligned, their centres sit at different X
/// positions, so a button 2 rows away can outscore the one right below it and the
/// highlight appears to skip items.
///
/// Setup: add to the menu's Canvas/parent, drag the buttons in under Items in
/// top-to-bottom order. Optionally assign a Side Button (e.g. the leaderboard icon):
/// pressing right on any item goes to it, and left on it comes back to the top item.
/// Buttons that are inactive or non-interactable (e.g. Continue with no save) are skipped.
/// </summary>
public class VerticalMenuNavigation : MonoBehaviour
{
    [Tooltip("Buttons in the order they appear on screen, top to bottom.")]
    [SerializeField] private Selectable[] items;

    [Tooltip("Optional extra button off to the side (e.g. leaderboard icon).")]
    [SerializeField] private Selectable sideButton;

    [Tooltip("Going up from the top item jumps to the bottom one, and vice versa.")]
    [SerializeField] private bool wrapAround = true;

    private void OnEnable()
    {
        StartCoroutine(ApplyNextFrame());
    }

    private IEnumerator ApplyNextFrame()
    {
        // Wait a frame so other scripts (e.g. MainMenuManager hiding Continue) have run first.
        yield return null;
        Apply();
    }

    /// <summary>Call again if a button's active/interactable state changes at runtime.</summary>
    public void Apply()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (!IsUsable(items[i])) continue;

            Navigation nav = new Navigation { mode = Navigation.Mode.Explicit };
            nav.selectOnUp = Neighbour(i, -1);
            nav.selectOnDown = Neighbour(i, 1);
            nav.selectOnRight = IsUsable(sideButton) ? sideButton : null;
            items[i].navigation = nav;
        }

        if (IsUsable(sideButton))
        {
            Navigation nav = new Navigation { mode = Navigation.Mode.Explicit };
            nav.selectOnLeft = FirstUsable();
            sideButton.navigation = nav;
        }
    }

    private static bool IsUsable(Selectable s)
    {
        return s != null && s.IsActive() && s.IsInteractable();
    }

    private Selectable FirstUsable()
    {
        foreach (Selectable s in items)
            if (IsUsable(s)) return s;
        return null;
    }

    private Selectable Neighbour(int index, int step)
    {
        int i = index + step;

        while (true)
        {
            if (i < 0 || i >= items.Length)
            {
                if (!wrapAround) return null;
                i = (i + items.Length) % items.Length;
            }

            if (i == index) return null; // looped all the way round, no other usable button

            if (IsUsable(items[i])) return items[i];

            i += step;
        }
    }
}
