using UnityEngine;
using UnityEngine.UI;

// Lives in the Settings scene. Wires up the sliders/toggle to
// SettingsManager (persistent, survives this scene being unloaded). Every
// change saves immediately - SetMusicVolume()/SetSFXVolume() on
// SettingsManager already call PlayerPrefs.Save() on every call, so there's
// nothing to add here for that; no separate Save button, nothing to lose by
// closing the scene right after adjusting a slider.
public class SettingsMenu : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Display")]
    [Tooltip("Optional - mainly useful for a WebGL/itch.io build, since the player might be playing in a small embedded frame.")]
    [SerializeField] private Toggle fullscreenToggle;

    private void Start()
    {
        if (SettingsManager.instance == null) return;

        // Set starting slider positions WITHOUT firing the onValueChanged
        // listeners below (SetValueWithoutNotify) - otherwise setting the
        // slider's value here would immediately call SetMusicVolume() with
        // the value it was already at, which is harmless but pointless.
        if (masterSlider != null)
        {
            masterSlider.SetValueWithoutNotify(SettingsManager.instance.MasterVolume);
            masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(SettingsManager.instance.MusicVolume);
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(SettingsManager.instance.SFXVolume);
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }
    }

    private void OnMasterVolumeChanged(float value)
    {
        SettingsManager.instance.SetMasterVolume(value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        SettingsManager.instance.SetMusicVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        SettingsManager.instance.SetSFXVolume(value);
    }

    private void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    // Hook this up to a "Back"/"Close" button.
    public void CloseSettings()
    {
        if (SettingsManager.instance != null)
            SettingsManager.instance.CloseSettings();
    }
}