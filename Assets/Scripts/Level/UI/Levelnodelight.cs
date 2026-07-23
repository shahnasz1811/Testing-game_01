using UnityEngine;
using UnityEngine.UI;

// Put this on a glow Image sitting on (or behind) a level node button - see
// setup notes for how to build that Image. Gives a flickering "lit torch"
// look when the level is unlocked, and stays off for locked levels.
// Reuses SaveManager.HighestUnlockedLevel - the same source of truth the
// rest of the save system already uses - instead of tracking its own
// locked/unlocked state.
[RequireComponent(typeof(Image))]
public class LevelNodeLight : MonoBehaviour
{
    [Header("Level")]
    [Tooltip("Which level this light belongs to.")]
    [SerializeField] private int levelNumber = 1;

    [Header("Flicker")]
    [SerializeField] private Color litColor = new Color(1f, 0.75f, 0.35f, 1f);
    [Range(0f, 1f)][SerializeField] private float minAlpha = 0.55f;
    [Range(0f, 1f)][SerializeField] private float maxAlpha = 1f;
    [Tooltip("Higher = faster, more jittery flicker. Lower = slow, gentle breathing.")]
    [SerializeField] private float flickerSpeed = 6f;

    private Image glow;
    private float noiseSeed;

    private void Awake()
    {
        glow = GetComponent<Image>();
        noiseSeed = Random.Range(0f, 1000f); // so multiple lights don't flicker in lockstep
    }

    private void Start()
    {
        bool isUnlocked = levelNumber <= SaveManager.HighestUnlockedLevel;
        glow.gameObject.SetActive(isUnlocked);

        if (isUnlocked)
            glow.color = litColor;
    }

    private void Update()
    {
        // The GameObject is simply inactive while locked, so Update() never
        // runs in that state - nothing else is needed to keep it off.
        float noise = Mathf.PerlinNoise(noiseSeed, Time.time * flickerSpeed);
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, noise);

        Color c = litColor;
        c.a = alpha;
        glow.color = c;
    }
}