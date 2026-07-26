using UnityEngine;
using TMPro;

// Drop into a level's HUD Canvas and assign a TMP_Text. Shows LevelStats'
// own timer for the CURRENT level only - it already ticks every frame
// (LevelStats.Update()) and already stops itself the moment the level ends
// (ShowVictoryRoutine() calls StopTimer()), so this just displays it, no
// separate start/stop logic needed here. Shows MM:SS, switching to
// HH:MM:SS if a level somehow runs past an hour.
public class LevelTimerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    private void Update()
    {
        if (LevelStats.instance == null || timerText == null) return;

        float totalSeconds = LevelStats.instance.timer;

        int hours = (int)(totalSeconds / 3600f);
        int minutes = (int)(totalSeconds / 60f) % 60;
        int seconds = (int)(totalSeconds % 60f);

        timerText.text = hours > 0
            ? string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds)
            : string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
