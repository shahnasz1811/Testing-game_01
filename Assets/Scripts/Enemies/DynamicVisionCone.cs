using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DynamicVisionCone : MonoBehaviour
{
    [Header("Vision Geometry")]
    public float viewDistance = 5f;
    [Range(0, 180)] public float viewAngle = 90f;
    [SerializeField] private int rayCount = 30;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Detection Origin")]
    [Tooltip("Local-space offset from this object's pivot (rotates with it) — e.g. push it out to the snout/eyes. " +
             "Both the rendered cone (GenerateMesh) and CheckPlayerDetection() read from this SAME point, " +
             "so moving it here keeps them perfectly in sync — you never have to fix them separately.")]
    [SerializeField] private Vector2 originOffset = Vector2.zero;

    [Header("Color States")]
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.3f); // Soft white
    [SerializeField] private Color alertColor = new Color(1f, 0.5f, 0f, 0.4f);  // Orange
    [SerializeField] private Color chaseColor = new Color(1f, 0f, 0f, 0.5f);  // Red

    private Mesh mesh;
    private MeshRenderer meshRenderer;

    // Debug-only, populated by CheckPlayerDetection() each call so
    // OnDrawGizmosSelected() can show exactly why the last check passed or
    // failed (out of range vs out of angle vs blocked by an obstacle),
    // instead of just showing the cone shape with no insight into WHY a
    // given position wasn't detected.
    private bool lastCheckHadPlayer;
    private Vector3 lastCheckedPlayerPos;
    private bool lastCheckInRange;
    private bool lastCheckInAngle;
    private bool lastCheckLineOfSight;

    /// <summary>
    /// The single source of truth for "where the cone starts". GenerateMesh()
    /// and CheckPlayerDetection() both read this instead of maintaining their
    /// own separate origin, which is what caused the two to drift out of sync
    /// in the first place (one used transform.position, the other used the
    /// parent Character's position).
    /// </summary>
    private Vector3 OriginWorldPosition => transform.TransformPoint(originOffset);

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        meshRenderer = GetComponent<MeshRenderer>();
    }

    /// <summary>
    /// Updates the visual wedge shape and applies the alert color shift.
    /// </summary>
    public void UpdateVisionCone(bool canSeePlayer, bool isChasing, float alertProgress = 0f)
    {
        // Determine current base color using Lerp for the gradual transition
        Color targetColor = normalColor;
        if (isChasing)
        {
            targetColor = chaseColor;
        }
        else if (canSeePlayer || alertProgress > 0f)
        {
            targetColor = Color.Lerp(normalColor, alertColor, alertProgress);
        }

        GenerateMesh(targetColor);
    }

    private void GenerateMesh(Color coneColor)
    {
        float angleIncrease = viewAngle / rayCount;
        // Start angle relative to the character's forward direction
        float angle = -viewAngle / 2f;

        Vector3[] vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[rayCount * 3];
        Color[] colors = new Color[vertices.Length];

        // Raycasts fire from the shared OriginWorldPosition (not just transform.position),
        // and the resulting vertices are converted back into this object's local space so
        // the mesh still renders correctly relative to its own transform.
        Vector3 originWorld = OriginWorldPosition;
        Vector3 originLocal = transform.InverseTransformPoint(originWorld);

        // Origin vertex (at the adjustable eyes/origin point)
        vertices[0] = originLocal;
        colors[0] = coneColor;

        int vertexIndex = 1;
        int triangleIndex = 0;

        for (int i = 0; i <= rayCount; i++)
        {
            float rad = angle * Mathf.Deg2Rad;

            // 1. Calculate the direction purely in local space (always pointing right/forward)
            Vector3 localDir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0).normalized;

            // 2. Convert that direction into World Space so the physics raycast knows exactly where to look
            Vector3 worldDir = transform.TransformDirection(localDir);

            // Environment Raycast using the accurate world direction, from the shared origin
            RaycastHit2D hit = Physics2D.Raycast(originWorld, worldDir, viewDistance, obstacleLayer);

            if (hit.collider == null)
            {
                // Keep it local space, offset from the origin vertex (not object pivot)
                vertices[vertexIndex] = originLocal + localDir * viewDistance;
            }
            else
            {
                // Interacts with walls cleanly and translates back to local space
                vertices[vertexIndex] = transform.InverseTransformPoint(hit.point);
            }

            colors[vertexIndex] = coneColor;

            if (i > 0)
            {
                // 3. Since the parent handles culling direction shifts automatically now, 
                // we can use a single consistent clockwise triangle setup!
                triangles[triangleIndex++] = 0;
                triangles[triangleIndex++] = vertexIndex - 1;
                triangles[triangleIndex++] = vertexIndex;
            }

            vertexIndex++;
            angle += angleIncrease;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateBounds();
    }

    public bool CheckPlayerDetection(Transform playerTransform)
    {
        lastCheckHadPlayer = playerTransform != null;
        if (playerTransform == null) return false;

        lastCheckedPlayerPos = playerTransform.position;

        // Same origin the mesh was built from — this is the actual fix.
        // Previously this used characterTransform.position (the parent), while
        // GenerateMesh() used transform.position (this object) — two different
        // points, so range/angle checks silently disagreed with what was drawn.
        Vector3 origin = OriginWorldPosition;

        float distance = Vector3.Distance(origin, playerTransform.position);
        lastCheckInRange = distance <= viewDistance;
        if (!lastCheckInRange)
        {
            lastCheckInAngle = false;
            lastCheckLineOfSight = false;
            return false;
        }

        Vector3 directionToPlayer = (playerTransform.position - origin).normalized;

        // transform.right IS the same direction as angle=0 in GenerateMesh()
        // (Cos(0), Sin(0) = 1,0 -> TransformDirection), so detection matches
        // exactly what's rendered.
        Vector3 facingDirection = transform.TransformDirection(Vector3.right);

        float angle = Vector3.Angle(facingDirection, directionToPlayer);
        lastCheckInAngle = angle <= viewAngle / 2f;
        if (!lastCheckInAngle)
        {
            lastCheckLineOfSight = false;
            return false;
        }

        RaycastHit2D hit = Physics2D.Raycast(origin, directionToPlayer, distance, obstacleLayer);
        lastCheckLineOfSight = hit.collider == null;
        return lastCheckLineOfSight;
    }

    /// <summary>
    /// Instantly clears the mesh and resets it to a calm, empty state without firing physics raycasts.
    /// </summary>
    public void ResetCone()
    {
        if (mesh == null) return;

        // Clear the physical mesh data instantly to stop rendering stale structures
        mesh.Clear();

        // Regenerate a default, pristine "calm" mesh using our normal base color
        GenerateMesh(normalColor);
    }

    private void OnDrawGizmosSelected()
    {
        // Works in edit mode too — OriginWorldPosition is pure transform math,
        // no dependency on Awake() having run.
        Vector3 origin = OriginWorldPosition;

        // Small handle so you can see exactly where the shared origin sits
        // while you tune originOffset in the Inspector.
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(origin, 0.08f);

        // YELLOW - the cone edges, using the same origin + direction basis
        // that both GenerateMesh() and CheckPlayerDetection() use. There's
        // only one set of edges now, because there's only one origin.
        Vector3 facingDirection = transform.TransformDirection(Vector3.right);
        Vector3 leftEdge = Quaternion.Euler(0, 0, viewAngle / 2f) * facingDirection;
        Vector3 rightEdge = Quaternion.Euler(0, 0, -viewAngle / 2f) * facingDirection;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + leftEdge * viewDistance);
        Gizmos.DrawLine(origin, origin + rightEdge * viewDistance);

        // Faint rays - every individual raycast GenerateMesh() fires to build the mesh shape.
        Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
        float angleIncrease = viewAngle / rayCount;
        float angle = -viewAngle / 2f;
        for (int i = 0; i <= rayCount; i++)
        {
            float rad = angle * Mathf.Deg2Rad;
            Vector3 dir = transform.TransformDirection(new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0));
            Gizmos.DrawLine(origin, origin + dir * viewDistance);
            angle += angleIncrease;
        }

        // GREEN/RED - the last actual CheckPlayerDetection() call against a real
        // player position: green if it passed every check (range, angle, line of
        // sight), red if it failed any of them.
        if (lastCheckHadPlayer)
        {
            bool passed = lastCheckInRange && lastCheckInAngle && lastCheckLineOfSight;
            Gizmos.color = passed ? Color.green : Color.red;
            Gizmos.DrawLine(origin, lastCheckedPlayerPos);
            Gizmos.DrawWireSphere(lastCheckedPlayerPos, 0.15f);
        }
    }
}