using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DynamicVisionCone : MonoBehaviour
{
    [Header("Vision Geometry")]
    public float viewDistance = 5f;
    [Range(0, 180)] public float viewAngle = 90f;
    [SerializeField] private int rayCount = 30;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Color States")]
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.3f); // Soft white
    [SerializeField] private Color alertColor = new Color(1f, 0.5f, 0f, 0.4f);  // Orange
    [SerializeField] private Color chaseColor = new Color(1f, 0f, 0f, 0.5f);  // Red

    private Mesh mesh;
    private MeshRenderer meshRenderer;
    private Transform characterTransform;

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        meshRenderer = GetComponent<MeshRenderer>();
        characterTransform = transform.parent; // Parent is 'Character'
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

        // Origin vertex (at the enemy eyes)
        vertices[0] = Vector3.zero;
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

            // Environment Raycast using the accurate world direction
            RaycastHit2D hit = Physics2D.Raycast(transform.position, worldDir, viewDistance, obstacleLayer);

            if (hit.collider == null)
            {
                // Keep it local space
                vertices[vertexIndex] = localDir * viewDistance;
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
        if (playerTransform == null) return false;

        float distance = Vector3.Distance(characterTransform.position, playerTransform.position);
        if (distance > viewDistance) return false;

        Vector3 directionToPlayer = (playerTransform.position - characterTransform.position).normalized;
        Vector3 facingDirection = characterTransform.localScale.x > 0 ? Vector3.right : Vector3.left;

        float angle = Vector3.Angle(facingDirection, directionToPlayer);
        if (angle > viewAngle / 2f) return false;

        RaycastHit2D hit = Physics2D.Raycast(characterTransform.position, directionToPlayer, distance, obstacleLayer);
        return hit.collider == null;
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
}