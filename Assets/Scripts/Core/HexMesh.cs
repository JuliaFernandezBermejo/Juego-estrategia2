using UnityEngine;

/// <summary>
/// Generates hexagon mesh geometry.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    private const float outerRadius = 1.0f; // Doubled to match hexSize spacing
    private const float innerRadius = outerRadius * 0.866025404f; // sqrt(3)/2

    public static Mesh CreateHexagonMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Hexagon";

        // Hexagon vertices (7 vertices: center + 6 corners)
        Vector3[] vertices = new Vector3[7];
        vertices[0] = Vector3.zero; // Center

        // 6 corners (flat-top orientation)
        for (int i = 0; i < 6; i++)
        {
            float angle = 60f * i * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(
                outerRadius * Mathf.Cos(angle),
                0f,
                outerRadius * Mathf.Sin(angle)
            );
        }

        // Triangles (6 triangles from center to edges)
        // Counter-clockwise winding for upward-facing normals
        int[] triangles = new int[18];
        for (int i = 0; i < 6; i++)
        {
            int triIndex = i * 3;
            triangles[triIndex] = 0; // Center
            triangles[triIndex + 1] = (i + 1) % 6 + 1; // Reversed winding
            triangles[triIndex + 2] = i + 1;           // Reversed winding
        }

        // UVs (simple mapping)
        Vector2[] uvs = new Vector2[7];
        uvs[0] = new Vector2(0.5f, 0.5f);
        for (int i = 0; i < 6; i++)
        {
            float angle = 60f * i * Mathf.Deg2Rad;
            uvs[i + 1] = new Vector2(
                0.5f + 0.5f * Mathf.Cos(angle),
                0.5f + 0.5f * Mathf.Sin(angle)
            );
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        return mesh;
    }

    public void Initialize()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = CreateHexagonMesh();

        // Create simple material if none exists
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer.material == null)
        {
            renderer.material = new Material(Shader.Find("Standard"));
        }
    }
}
