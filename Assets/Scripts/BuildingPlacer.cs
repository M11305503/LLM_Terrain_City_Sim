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

    [ContextMenu("���ͫؿv��")]
    public void PlaceBuildings()
    {
        var buildable = buildableGrid.GetBuildableGrid();
        var cityTypeGrid = caCityGrid.GetCityTypeGrid();

        if (buildable == null || cityTypeGrid == null)
        {
            UnityEngine.Debug.Log("�٨S��l�Ʀn");
            return;
        }

        int gridX = buildable.GetLength(0);
        int gridZ = buildable.GetLength(1);

        if (cityTypeGrid.GetLength(0) != gridX || cityTypeGrid.GetLength(1) != gridZ)
        {
            UnityEngine.Debug.LogWarning($"���A��Ƥ��@�P�Gbuildable {gridX}x{gridZ} vs cityTypeGrid {cityTypeGrid.GetLength(0)}x{cityTypeGrid.GetLength(1)}");
            return;
        }

        // �M���ª��ؿv��
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
                    // �i�ؿv��A�ھ� cityTypeGrid ����� prefab
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
                    // ���i�ؿv��A�����A�O����A�]�@�w�n�� parkPrefab
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
