using System;
using UnityEngine;
using CodeMonkey.Utils;

public class FieldOfView : MonoBehaviour
{
    // 👁️ VISION
    [Header("Vision Cone")]
    [SerializeField] private LayerMask layerMask;
    private Mesh mesh;
    private Vector3 origin;
    [SerializeField]private float fov;
    [SerializeField] private float startingAngle;

    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        fov = 90f;
    }

    public void SetOrigin(Vector3 origin)
    {
        this.origin = origin;
    }

    public void SetAimDirection(Vector3 aimDirection)
    {
        startingAngle = UtilsClass.GetAngleFromVectorFloat(aimDirection) - fov / 2f;
    }

    // Update is called once per frame
    private void Update()
    {
        origin = transform.position;
        int rayCount = 50;
        float angle = startingAngle;
        float angleIncrease = fov / rayCount;
        float viewDistance = 5f;

        Vector3[] vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = Vector3.zero;

        int vertexIndex = 1;
        int triangleIndex = 0;

        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 dir = UtilsClass.GetVectorFromAngle(angle);

            RaycastHit2D hit = Physics2D.Raycast(origin, dir, viewDistance, layerMask);

            Vector3 vertex;

            // ✅ FIXED LOGIC
            if (hit.collider == null)
            {
                vertex = origin + dir * viewDistance;
            }
            else
            {
                vertex = hit.point;
            }

            // ✅ FIXED LOCAL SPACE
            vertices[vertexIndex] = transform.InverseTransformPoint(vertex);

            if (i > 0)
            {
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
    }
}
