using UnityEngine;

// OPTIONAL / cosmetic - none of the 3 requested systems depend on this.
// Stretches and rotates a "neck" sprite between a fixed base (in the bushes)
// and the moving head, so the long-neck silhouette from the reference art
// automatically follows wherever BossController moves the head.
//
// Setup: the neck sprite's pivot should be at its BOTTOM, drawn pointing
// straight up by default, and be exactly `restLength` world units tall at
// localScale.y = 1. Put this script anywhere and drag in the 3 references.
public class BossNeckStretch : MonoBehaviour
{
    [SerializeField] private Transform neckBase;    // fixed point in the bushes
    [SerializeField] private Transform head;        // the BossController's transform
    [SerializeField] private Transform neckSprite;  // the stretchy sprite (pivot at bottom)
    [SerializeField] private float restLength = 1f; // world-space length of neckSprite at localScale.y = 1

    private void LateUpdate()
    {
        if (neckBase == null || head == null || neckSprite == null) return;

        Vector2 baseToHead = head.position - neckBase.position;
        float distance = baseToHead.magnitude;
        float angle = Mathf.Atan2(baseToHead.y, baseToHead.x) * Mathf.Rad2Deg - 90f;

        neckSprite.position = neckBase.position;
        neckSprite.rotation = Quaternion.Euler(0f, 0f, angle);

        Vector3 scale = neckSprite.localScale;
        scale.y = distance / Mathf.Max(restLength, 0.001f);
        neckSprite.localScale = scale;
    }
}
