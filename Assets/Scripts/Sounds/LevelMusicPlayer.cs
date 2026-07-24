using UnityEngine;

// Drop one of these into each level's scene to start that level's normal
// track the moment it loads. For level 10, this plays your normal-area
// track as usual - BossIntroCutscene cuts to the boss track once the player
// crosses into the arena (see its new Boss Music field), and this component
// doesn't need to know that switch is coming.
public class LevelMusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip levelMusic;

    private void Start()
    {
        if (MusicManager.instance != null)
            MusicManager.instance.PlayTrack(levelMusic);
    }
}
