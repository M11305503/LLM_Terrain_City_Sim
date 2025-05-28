using UnityEngine;

[ExecuteAlways]
public class BuildableGrid : MonoBehaviour
{
    [Header("格子設定")]
    public Terrain terrain;
    public int gridX = 25;
    public int gridZ = 25;
    [Range(1f, 60f)]
    public float maxSlope = 21.5f; // 允許最大坡度（預設放寬為21.5）

    public bool[,] GetBuildableGrid() => buildableGrid;

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

        buildableGrid = new bool[gridX, gridZ];
        int buildableCount = 0;

        for (int x = 0; x < gridX; x++)
        {
            for (int z = 0; z < gridZ; z++)
            {
                float worldX = (x + 0.5f) / gridX * size.x;
                float worldZ = (z + 0.5f) / gridZ * size.z;
                float slope = GetSlope(worldX, worldZ);

                bool isBuildable = slope <= maxSlope;
                buildableGrid[x, z] = isBuildable;

                if (isBuildable) buildableCount++;
            }
        }

        float percent = (float)buildableCount / (gridX * gridZ) * 100f;
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

        for (int x = 0; x < gridX; x++)
        {
            for (int z = 0; z < gridZ; z++)
            {
                float cellX = (x + 0.5f) / gridX * size.x;
                float cellZ = (z + 0.5f) / gridZ * size.z;
                float height = terrain.SampleHeight(new Vector3(cellX, 0, cellZ)) + pos.y;

                Gizmos.color = buildableGrid[x, z] ? Color.green : Color.red;
                Gizmos.DrawCube(new Vector3(cellX, height + 1f, cellZ), Vector3.one * 2f);
            }
        }
    }
}
