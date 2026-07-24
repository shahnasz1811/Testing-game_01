using System.Collections;
using UnityEngine;

// Persistent audio singleton - survives scene loads (DontDestroyOnLoad), same
// pattern as your LevelManager/GameManager instance fields. Handles
// background music only; one-off sound effects (like CrushKill's break
// sound) keep using their own local AudioSources, untouched by this.
//
// Setup: create an empty GameObject in your FIRST scene (e.g. a boot/menu
// scene that loads before anything else), name it "MusicManager", add this
// script. You DON'T need to add an AudioSource yourself - Awake() adds two
// of its own at runtime (needed for the crossfade: one fades out while the
// other fades in, so there's no gap of silence between tracks). Because of
// DontDestroyOnLoad it only needs to exist once, ever - don't put one in
// every level scene.
public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.6f;
    [Tooltip("Used whenever PlayTrack/FadeOut are called without an explicit duration.")]
    [SerializeField] private float defaultFadeDuration = 1f;

    private AudioSource sourceA;
    private AudioSource sourceB;
    private AudioSource activeSource; // whichever of the two is the current "main" track
    private AudioClip currentClip;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // A MusicManager already exists from a previous scene load -
            // this duplicate isn't needed.
            Destroy(gameObject);
            return;
        }

        sourceA = gameObject.AddComponent<AudioSource>();
        sourceB = gameObject.AddComponent<AudioSource>();

        foreach (AudioSource s in new[] { sourceA, sourceB })
        {
            s.loop = true;
            s.playOnAwake = false;
            s.volume = 0f;
        }

        activeSource = sourceA;
    }

    // Crossfades to a new track over fadeDuration seconds (defaultFadeDuration
    // if not specified). Safe to call repeatedly/every time a trigger fires -
    // a no-op if the clip passed in is already the one currently playing or
    // mid-fade-in, so it won't restart the same track from 0.
    public void PlayTrack(AudioClip clip, float fadeDuration = -1f)
    {
        if (clip == null || clip == currentClip) return;

        if (fadeDuration < 0f) fadeDuration = defaultFadeDuration;

        currentClip = clip;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(CrossfadeRoutine(clip, fadeDuration));
    }

    // Fades whatever's currently playing down to silence, then stops it.
    // Call this on level end instead of PlayTrack(null) - e.g. from
    // LevelManager.ShowVictoryRoutine().
    public void FadeOut(float fadeDuration = -1f)
    {
        if (fadeDuration < 0f) fadeDuration = defaultFadeDuration;

        currentClip = null;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutRoutine(fadeDuration));
    }

    private IEnumerator CrossfadeRoutine(AudioClip clip, float duration)
    {
        AudioSource fadingIn = activeSource == sourceA ? sourceB : sourceA;
        AudioSource fadingOut = activeSource;

        fadingIn.clip = clip;
        fadingIn.volume = 0f;
        fadingIn.Play();

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float frac = Mathf.Clamp01(t / duration);
            fadingIn.volume = Mathf.Lerp(0f, volume, frac);
            fadingOut.volume = Mathf.Lerp(volume, 0f, frac);
            yield return null;
        }

        fadingIn.volume = volume;
        fadingOut.volume = 0f;
        fadingOut.Stop();

        activeSource = fadingIn;
        fadeCoroutine = null;
    }

    private IEnumerator FadeOutRoutine(float duration)
    {
        AudioSource source = activeSource;
        float startVolume = source.volume;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        source.volume = 0f;
        source.Stop();
        fadeCoroutine = null;
    }
}