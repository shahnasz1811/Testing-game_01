using UnityEngine;

// Drop into the Leaderboard scene alongside a VictoryScreen panel (build/drag
// in a VictoryScreen the same way it's set up on a normal level - stars,
// value texts, criteria icons all still need wiring in THIS scene's copy).
//
// Assign the SAME LevelData asset your final level's LevelManager uses -
// that's safe even though the level's own LevelManager/LevelStats were both
// destroyed when this scene loaded, because a LevelData ScriptableObject is
// a project asset, not a scene object, so the reference works from anywhere.
// The actual run numbers (time/deaths/enemies) come from the snapshot
// GameManager captured right before switching to this scene - see
// GameManager.finalLevelTime/finalLevelDeaths/etc and where LevelManager
// fills them in in ShowVictoryRoutine().
public class FinalVictoryDisplay : MonoBehaviour
{
    [SerializeField] private LevelData finalLevelData;
    [SerializeField] private VictoryScreen victoryScreen;

    private void Start()
    {
        if (GameManager.instance == null || victoryScreen == null || finalLevelData == null) return;

        bool allEnemiesKilled = GameManager.instance.finalLevelEnemiesKilled >= GameManager.instance.finalLevelTotalEnemies;

        victoryScreen.Show(
            finalLevelData,
            allEnemiesKilled,
            GameManager.instance.finalLevelEnemiesKilled,
            GameManager.instance.finalLevelTotalEnemies,
            GameManager.instance.finalLevelTime,
            GameManager.instance.finalLevelDeaths
        );
    }
}
