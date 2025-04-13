using UnityEngine;

[ExecuteAlways]
public class BuildableGrid : MonoBehaviour
{
    [Header("格子設定")]
    public Terrain terrain;
    public int gridSizeX = 10;
    public int gridSizeZ = 10;
    [Range(1f, 60f)]
    public float maxSlope = 20f; // 允許最大坡度（預設放寬為20）

    private bool[,] buildableGrid;
    private TerrainData terrainData;

    void Update()
    {
        if (terrain == null) return;

        terrainData = terrain.terrainData;

        GenerateBuildableGrid();
    }

    void GenerateBuildableGrid()
    {
        Vector3 size = terrainData.size;

        buildableGrid = new bool[gridSizeX, gridSizeZ];
        int buildableCount = 0;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                float worldX = (x + 0.5f) / gridSizeX * size.x;
                float worldZ = (z + 0.5f) / gridSizeZ * size.z;
                float slope = GetSlope(worldX, worldZ);

                bool isBuildable = slope <= maxSlope;
                buildableGrid[x, z] = isBuildable;

                if (isBuildable) buildableCount++;
            }
        }

        float percent = (float)buildableCount / (gridSizeX * gridSizeZ) * 100f;
        Debug.Log($"✅ 可建築格子：{buildableCount}/{gridSizeX * gridSizeZ}（{percent:F1}%）");
    }

    float GetSlope(float worldX, float worldZ)
    {
        Vector3 normal = terrainData.GetInterpolatedNormal(worldX / terrainData.size.x, worldZ / terrainData.size.z);
        return Vector3.Angle(normal, Vector3.up);
    }

    void OnDrawGizmos()
    {
        if (buildableGrid == null || terrain == null) return;

        Vector3 pos = terrain.GetPosition();
        Vector3 size = terrain.terrainData.size;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                float cellX = (x + 0.5f) / gridSizeX * size.x;
                float cellZ = (z + 0.5f) / gridSizeZ * size.z;
                float height = terrain.SampleHeight(new Vector3(cellX, 0, cellZ)) + pos.y;

                Gizmos.color = buildableGrid[x, z] ? Color.green : Color.red;
                Gizmos.DrawCube(new Vector3(cellX, height + 1f, cellZ), Vector3.one * 2f);
            }
        }
    }
}
