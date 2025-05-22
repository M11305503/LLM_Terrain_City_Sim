using System.Diagnostics;
using UnityEngine;

[ExecuteAlways]
public class BuildingPlacer : MonoBehaviour
{
    public BuildableGrid buildableGrid;
    public CACityGrid caCityGrid;
    public Terrain terrain;

    public GameObject highrisePrefab;
    public GameObject residentialPrefab;
    public GameObject parkPrefab;
    public GameObject industrialPrefab;

    private GameObject[,] placedBuildings;

    [ContextMenu("產生建築物")]
    public void PlaceBuildings()
    {
        var buildable = buildableGrid.GetBuildableGrid();
        var cityTypeGrid = caCityGrid.GetCityTypeGrid();

        if (buildable == null || cityTypeGrid == null)
        {
            UnityEngine.Debug.Log("還沒初始化好");
            return;
        }

        int gridX = buildable.GetLength(0);
        int gridZ = buildable.GetLength(1);

        if (cityTypeGrid.GetLength(0) != gridX || cityTypeGrid.GetLength(1) != gridZ)
        {
            UnityEngine.Debug.LogWarning($"型態格數不一致：buildable {gridX}x{gridZ} vs cityTypeGrid {cityTypeGrid.GetLength(0)}x{cityTypeGrid.GetLength(1)}");
            return;
        }

        // 清除舊的建築物
        if (placedBuildings != null)
        {
            foreach (var b in placedBuildings)
            {
                if (b != null) DestroyImmediate(b);
            }
        }
        placedBuildings = new GameObject[gridX, gridZ];

        var size = terrain.terrainData.size;

        int count = 0;
        for (int x = 0; x < gridX; x++)
        {
            for (int z = 0; z < gridZ; z++)
            {
                GameObject prefab = null;

                if (buildable[x, z])
                {
                    // 可建築格，根據 cityTypeGrid 放相應 prefab
                    switch (cityTypeGrid[x, z])
                    {
                        case CellType.Highrise: prefab = highrisePrefab; break;
                        case CellType.Residential: prefab = residentialPrefab; break;
                        case CellType.Park: prefab = parkPrefab; break;
                        case CellType.Industrial: prefab = industrialPrefab; break;
                        default: break;
                    }
                }
                else if (cityTypeGrid[x, z] == CellType.Park)
                {
                    // 不可建築格，但型態是公園，也一定要放 parkPrefab
                    prefab = parkPrefab;
                }

                if (prefab == null) continue;

                float cellX = (x + 0.5f) / gridX * size.x;
                float cellZ = (z + 0.5f) / gridZ * size.z;
                float height = terrain.SampleHeight(new Vector3(cellX, 0, cellZ)) + terrain.GetPosition().y;
                float heightOffset = prefab.transform.localScale.y / 2f;

                Vector3 pos = new Vector3(cellX, height + heightOffset, cellZ);

                placedBuildings[x, z] = Instantiate(prefab, pos, Quaternion.identity, transform);
                UnityEngine.Debug.Log($"Instantiate {prefab.name} at ({x},{z}) type:{cityTypeGrid[x, z]}");
                count++;
            }
        }
    }
}
