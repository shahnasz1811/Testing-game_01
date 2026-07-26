using UnityEngine;
using TMPro;

// Drop into a level's HUD Canvas and assign a TMP_Text. Reads straight from
// LevelManager.EnemiesRemaining every frame, so it updates on its own as
// EnemyDied() runs - no event wiring needed, nothing else to hook up.
public class EnemiesCounterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text counterText;
    [Tooltip("{0} gets replaced with the remaining count.")]
    [SerializeField] private string format = "Enemies Left: {0}";

    private void Update()
    {
        if (LevelManager.instance == null || counterText == null) return;

        counterText.text = string.Format(format, LevelManager.instance.EnemiesRemaining);
    }
}
