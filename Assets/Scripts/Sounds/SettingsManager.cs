using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

// Persistent singleton (DontDestroyOnLoad, same pattern as GameManager/
// MusicManager) holding the player's audio preferences.
//
// Settings lives in its OWN scene, loaded ADDITIVELY on top of whatever's
// currently active - from the main menu that's harmless either way, but
// opened from the Pause Menu it's the difference between "settings opens
// over the paused level, still fully intact underneath" and "settings
// replaces the scene and the run is gone." CloseSettings() just unloads it
// again, dropping back into whatever was underneath exactly as it was left
// - including Time.timeScale, which Pause already set to 0. That's fine:
// Unity's UI input system runs on unscaled time, so sliders/buttons in the
// Settings scene work normally even while the game underneath is frozen.
//
// Volume is driven straight through your AudioMixer's exposed parameters
// (MusicVolume/SfxVolume) rather than through individual scripts - so this
// automatically covers every AudioSource routed to the Music/Sfx groups,
// with no per-script code needed anywhere else.
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    public const string MasterVolumeKey = "MasterVolume";
    public const string MusicVolumeKey = "MusicVolume";
    public const string SFXVolumeKey = "SFXVolume";

    [Tooltip("Your AudioMixer asset - the one with the Music/Sfx groups and the MusicVolume/SfxVolume exposed parameters.")]
    [SerializeField] private AudioMixer audioMixer;

    // Must match the exposed parameter names exactly (case-sensitive) - see
    // your Audio Mixer window's "Exposed Parameters" dropdown.
    private const string MasterVolumeParam = "MasterVolume";
    private const string MusicVolumeParam = "MusicVolume";
    private const string SfxVolumeParam = "SfxVolume";

    public float MasterVolume { get; private set; } = 1f;
    public float MusicVolume { get; private set; } = 1f;
    public float SFXVolume { get; private set; } = 1f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        SFXVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 1f);
    }

    private void Start()
    {
        // Deliberately NOT in Awake(): AudioMixer.SetFloat() calls made that
        // early can silently fail to take effect, since the mixer's audio
        // graph hasn't finished initializing yet at that point in the frame
        // - it doesn't error, the value just doesn't stick until something
        // sets it again later (e.g. touching the slider). Mixer parameters
        // don't persist between sessions on their own - PlayerPrefs is the
        // actual save, this just re-applies it on launch.
        ApplyMasterVolume(MasterVolume);
        ApplyMusicVolume(MusicVolume);
        ApplySFXVolume(SFXVolume);
    }

    public void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
        PlayerPrefs.Save();

        ApplyMasterVolume(MasterVolume);
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
        PlayerPrefs.Save();

        ApplyMusicVolume(MusicVolume);
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SFXVolumeKey, SFXVolume);
        PlayerPrefs.Save();

        ApplySFXVolume(SFXVolume);
    }

    private void ApplyMasterVolume(float linear)
    {
        if (audioMixer != null)
            audioMixer.SetFloat(MasterVolumeParam, LinearToDecibel(linear));
    }

    private void ApplyMusicVolume(float linear)
    {
        if (audioMixer != null)
            audioMixer.SetFloat(MusicVolumeParam, LinearToDecibel(linear));
    }

    private void ApplySFXVolume(float linear)
    {
        if (audioMixer != null)
            audioMixer.SetFloat(SfxVolumeParam, LinearToDecibel(linear));
    }

    // Straight linear interpolation across the dB range (0dB at full, -80dB
    // at zero) rather than converting a raw amplitude ratio - that's what
    // puts the slider's midpoint at exactly -40dB instead of only ~-6dB.
    private float LinearToDecibel(float linear)
    {
        return Mathf.Lerp(-80f, 0f, Mathf.Clamp01(linear));
    }

    public void OpenSettings()
    {
        if (SceneManager.GetSceneByName("Settings").isLoaded) return;
        SceneManager.LoadScene("Settings", LoadSceneMode.Additive);
    }

    public void CloseSettings()
    {
        SceneManager.UnloadSceneAsync("Settings");
    }
}