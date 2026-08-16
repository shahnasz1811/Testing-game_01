using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Shows one icon per living enemy of each TYPE, in a row - no numbers, just
// icons. As each enemy dies, its icon burns away with the same dissolve
// look as the enemy itself (see UI-Dissolve.shader), then disappears. Type
// comes from EnemyDeath.EnemyType - a plain string set per prefab in the
// Inspector - so grouping is entirely data-driven. Adding a new enemy type
// later needs no code changes: give the new prefab's EnemyDeath component
// an Enemy Type string (e.g. "Flying"), then add one more row here with a
// matching Type Id, an icon sprite, and an empty container for its icons
// to spawn under.
//
// Independent of LevelManager's own totalEnemies/deadEnemies tracking (that
// keeps driving the door-opening logic, untouched) - this does its own scan
// purely for display.
public class PerTypeEnemyCounterUI : MonoBehaviour
{
    [System.Serializable]
    private class TypeRow
    {
        [Tooltip("Must match the Enemy Type string on that enemy's EnemyDeath component exactly (case-sensitive).")]
        public string typeId = "Melee";
        [Tooltip("The icon shown once per living enemy of this type.")]
        public Sprite icon;
        [Tooltip("Empty RectTransform this type's icons spawn under - add a Horizontal Layout Group component to it so they line up automatically.")]
        public RectTransform container;

        [System.NonSerialized] public List<GameObject> icons = new List<GameObject>();
        [System.NonSerialized] public List<EnemyDeath> enemies = new List<EnemyDeath>();
        [System.NonSerialized] public List<Material> materials = new List<Material>();
        [System.NonSerialized] public List<Coroutine> dissolveRoutines = new List<Coroutine>();
    }

    [SerializeField] private TypeRow[] rows;
    [Tooltip("Prefab for a single icon - just needs an Image component on it, sized/styled however you want. Leave empty and a plain default-sized Image gets created at runtime instead (fine for testing, but you'll likely want a real prefab for proper sizing).")]
    [SerializeField] private GameObject iconPrefab;

    [Header("Death Dissolve Effect")]
    [Tooltip("UI-Dissolve.shader. If left empty, icons fall back to instantly vanishing on death (old behaviour) instead of dissolving.")]
    [SerializeField] private Shader uiDissolveShader;
    [Tooltip("The REAL Material used on enemies (built from Dissolve.shadergraph). Its Dissolve Scale / Outline Thickness / Outline Color are copied onto every icon at spawn, so the UI burn edge always matches the in-world one - even if an artist retunes that material later, this stays in sync automatically.")]
    [SerializeField] private Material sourceDissolveMaterial;
    [Tooltip("How long the icon takes to fully dissolve, in seconds. Set this to whatever duration EnemyDeath actually animates its own _DissolveAmount over, so the icon and the enemy burn away in the same amount of time.")]
    [SerializeField] private float dissolveDuration = 1f;
    [Tooltip("Easing for the dissolve over dissolveDuration. Linear by default; use an ease-in/out curve if the real death animation isn't linear.")]
    [SerializeField] private AnimationCurve dissolveCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    private static readonly int DissolveAmountId = Shader.PropertyToID("_DissolveAmount");

    private void Start()
    {
        EnemyDeath[] allEnemies = FindObjectsOfType<EnemyDeath>();

        foreach (TypeRow row in rows)
        {
            if (row.container == null) continue;

            foreach (EnemyDeath e in allEnemies)
            {
                if (e == null || e.EnemyType != row.typeId) continue;

                row.enemies.Add(e);
                row.dissolveRoutines.Add(null);

                GameObject iconObj = SpawnIcon(row);
                row.icons.Add(iconObj);
                row.materials.Add(BuildDissolveMaterial(iconObj));
            }
        }
    }

    private GameObject SpawnIcon(TypeRow row)
    {
        GameObject iconObj;

        if (iconPrefab != null)
        {
            iconObj = Instantiate(iconPrefab, row.container, false);
        }
        else
        {
            iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(row.container, false);
        }

        Image img = iconObj.GetComponent<Image>();
        if (img != null)
            img.sprite = row.icon;

        return iconObj;
    }

    /// <summary>
    /// Gives this icon its own Material instance (so its dissolve progress is
    /// independent of every other icon) using UI-Dissolve.shader, and copies
    /// the noise/outline tuning straight off the real enemy material so the
    /// look can never drift out of sync with the in-world effect.
    /// Returns null (and the icon just uses its default UI material) if
    /// uiDissolveShader hasn't been assigned - death then falls back to an
    /// instant hide instead of a dissolve.
    /// </summary>
    private Material BuildDissolveMaterial(GameObject iconObj)
    {
        if (uiDissolveShader == null || iconObj == null) return null;

        Image img = iconObj.GetComponent<Image>();
        if (img == null) return null;

        Material mat = new Material(uiDissolveShader);

        if (sourceDissolveMaterial != null)
        {
            if (sourceDissolveMaterial.HasProperty("_DissolveScale"))
                mat.SetFloat("_DissolveScale", sourceDissolveMaterial.GetFloat("_DissolveScale"));
            if (sourceDissolveMaterial.HasProperty("_OutlineThickness"))
                mat.SetFloat("_OutlineThickness", sourceDissolveMaterial.GetFloat("_OutlineThickness"));
            if (sourceDissolveMaterial.HasProperty("_OutlineColor"))
                mat.SetColor("_OutlineColor", sourceDissolveMaterial.GetColor("_OutlineColor"));
        }

        mat.SetFloat(DissolveAmountId, 0f);
        img.material = mat;
        return mat;
    }

    private void Update()
    {
        foreach (TypeRow row in rows)
        {
            for (int i = 0; i < row.enemies.Count; i++)
            {
                EnemyDeath enemy = row.enemies[i];
                GameObject icon = row.icons[i];
                if (enemy == null || icon == null) continue;

                bool isDead = enemy.isDead;
                bool iconActive = icon.activeSelf;

                if (isDead && iconActive && row.dissolveRoutines[i] == null)
                {
                    // Just died and hasn't started burning away yet.
                    if (row.materials[i] != null)
                        row.dissolveRoutines[i] = StartCoroutine(PlayDissolve(row, i));
                    else
                        icon.SetActive(false); // no dissolve shader assigned - fall back to instant hide
                }
                else if (!isDead && !iconActive)
                {
                    // Enemy is alive again (e.g. pooled/respawned) - snap the icon straight back.
                    if (row.dissolveRoutines[i] != null)
                    {
                        StopCoroutine(row.dissolveRoutines[i]);
                        row.dissolveRoutines[i] = null;
                    }
                    if (row.materials[i] != null)
                        row.materials[i].SetFloat(DissolveAmountId, 0f);
                    icon.SetActive(true);
                }
            }
        }
    }

    private IEnumerator PlayDissolve(TypeRow row, int index)
    {
        Material mat = row.materials[index];
        GameObject icon = row.icons[index];

        float t = 0f;
        while (t < dissolveDuration)
        {
            t += Time.deltaTime;
            float progress = dissolveCurve.Evaluate(Mathf.Clamp01(t / dissolveDuration));
            if (mat != null) mat.SetFloat(DissolveAmountId, progress);
            yield return null;
        }

        if (mat != null) mat.SetFloat(DissolveAmountId, 1f);
        if (icon != null) icon.SetActive(false);
        row.dissolveRoutines[index] = null;
    }

    private void OnDestroy()
    {
        // Each icon got its own Material instance - clean them up rather than leaking.
        foreach (TypeRow row in rows)
        {
            foreach (Material m in row.materials)
            {
                if (m != null) Destroy(m);
            }
        }
    }
}