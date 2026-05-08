using UnityEngine;

public class VRVedioPlane : MonoBehaviour
{
    [Header("巨幕参数设置")]
    [Tooltip("屏幕弯曲的半径，值越大越平缓")]
    public float radius = 5f;
    [Tooltip("屏幕左右展开的弧度")]
    public float arcDegrees = 60f;
    [Tooltip("屏幕的高度")]
    public float height = 3f;
    [Tooltip("网格的精细度，越大越圆滑")]
    public int segments = 64;

    void Awake()
    {
        GenerateMesh();
    }

    // 在 Inspector 的右键菜单中可以手动触发生成，方便预览
    [ContextMenu("更新曲面网格 (Generate Mesh)")]
    public void GenerateMesh()
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh { name = "CurvedScreenMesh" };

        int verticesCount = (segments + 1) * 2;
        Vector3[] vertices = new Vector3[verticesCount];
        Vector2[] uv = new Vector2[verticesCount];
        int[] triangles = new int[segments * 6];

        float angleStep = arcDegrees / segments;
        float startAngle = -arcDegrees / 2f;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            float rad = currentAngle * Mathf.Deg2Rad;

            // 计算圆弧上的 X 和 Z 坐标
            float x = Mathf.Sin(rad) * radius;
            // 减去 radius 是为了让屏幕的中心点贴合 Transform 的本地原点
            float z = Mathf.Cos(rad) * radius - radius;

            // 底部顶点
            vertices[i] = new Vector3(x, -height / 2f, z);
            uv[i] = new Vector2((float)i / segments, 0f);

            // 顶部顶点
            vertices[i + segments + 1] = new Vector3(x, height / 2f, z);
            uv[i + segments + 1] = new Vector2((float)i / segments, 1f);
        }

        int ti = 0;
        for (int i = 0; i < segments; i++)
        {
            triangles[ti] = i;
            triangles[ti + 1] = i + segments + 1;
            triangles[ti + 2] = i + 1;

            triangles[ti + 3] = i + 1;
            triangles[ti + 4] = i + segments + 1;
            triangles[ti + 5] = i + segments + 2;
            ti += 6;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        filter.mesh = mesh;
    }
}
