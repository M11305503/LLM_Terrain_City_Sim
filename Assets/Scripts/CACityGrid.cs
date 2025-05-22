using System;
using System.Diagnostics;
using UnityEngine;

public enum CellType
{
    None,         // 不可建築
    Residential,  // 住宅
    Highrise,     // 高樓
    Park,         // 公園
    Industrial    // 工業
}

public class CACityGrid : MonoBehaviour
{
    public BuildableGrid buildableGrid; // Inspector 指定

    [Header("格子參數")]
    public int gridX = 25;
    public int gridZ = 25;

    [Header("型態資料")]
    public CellType[,] cityTypeGrid;

    private bool[,] _latestBuildableGrid; // 記錄最新可建築格，方便演化時判斷

    private void OnEnable()
    {
        TryInitCityGrid();
    }

    public void TryInitCityGrid()
    {
        var buildable = buildableGrid?.GetBuildableGrid();
        if (buildable != null)
        {
            gridX = buildable.GetLength(0);
            gridZ = buildable.GetLength(1);
            _latestBuildableGrid = buildable;
            InitCityTypeGrid();
            SetInitialSeeds();
            UnityEngine.Debug.Log("cityTypeGrid 已初始化");
        }
        else
        {
            UnityEngine.Debug.LogWarning("buildableGrid 尚未就緒，cityTypeGrid 未初始化");
        }
    }

    public void InitCityTypeGrid()
    {
        cityTypeGrid = new CellType[gridX, gridZ];
        var buildable = buildableGrid.GetBuildableGrid();
        for (int x = 0; x < gridX; x++)
        {
            for (int z = 0; z < gridZ; z++)
            {
                // 只要不能建築，直接設成公園
                cityTypeGrid[x, z] = (buildable != null && !buildable[x, z]) ? CellType.Park : CellType.Residential;
            }
        }
    }

    public void SetInitialSeeds()
    {
        // 初始化時保證所有不可建築格都設成 Park
        var buildable = buildableGrid.GetBuildableGrid();
        for (int x = 0; x < gridX; x++)
            for (int z = 0; z < gridZ; z++)
                if (buildable != null && !buildable[x, z])
                    cityTypeGrid[x, z] = CellType.Park;
                else
                    cityTypeGrid[x, z] = CellType.Residential;

        // 其餘格子進行 seed 分布（下方操作都只動可建築格）

        // 中央高樓
        int cx = gridX / 2, cz = gridZ / 2, highriseR = UnityEngine.Random.Range(2, 5);
        for (int x = cx - highriseR; x <= cx + highriseR; x++)
            for (int z = cz - highriseR; z <= cz + highriseR; z++)
                if (IsBuildable(x, z) && UnityEngine.Random.value < 0.85f)
                    cityTypeGrid[x, z] = CellType.Highrise;

        // 隨機工業（邊緣）
        for (int x = 0; x < gridX; x++)
            for (int z = 0; z < gridZ; z++)
                if (IsBuildable(x, z) && (x < 2 || z < 2 || x >= gridX - 2 || z >= gridZ - 2) && UnityEngine.Random.value < 0.7f)
                    cityTypeGrid[x, z] = CellType.Industrial;

        // 隨機公園（可建築區才有可能再出現小量綠色）
        for (int i = 0; i < gridX * gridZ / 16; i++)
        {
            int rx = UnityEngine.Random.Range(0, gridX), rz = UnityEngine.Random.Range(0, gridZ);
            if (IsBuildable(rx, rz) && UnityEngine.Random.value < 0.5f)
                cityTypeGrid[rx, rz] = CellType.Park;
        }
    }

    // 判斷這個點是不是可建築（且不越界）
    private bool IsBuildable(int x, int z)
    {
        return x >= 0 && z >= 0 && x < gridX && z < gridZ && _latestBuildableGrid != null && _latestBuildableGrid[x, z];
    }

    public void StepAutomaton()
    {
        var nextGrid = new CellType[gridX, gridZ];
        var buildable = buildableGrid.GetBuildableGrid();

        for (int x = 0; x < gridX; x++)
            for (int z = 0; z < gridZ; z++)
            {
                // 不可建築格永遠公園
                if (buildable != null && !buildable[x, z])
                {
                    nextGrid[x, z] = CellType.Park;
                    continue;
                }

                // 如果自己本來就是公園，並且有公園鄰居，可以有機率保留公園狀態
                int parkNeighbors = CountNeighbors(x, z, CellType.Park);

                // 如果周圍公園多，有機率變成公園
                if (parkNeighbors >= 3 && UnityEngine.Random.value < 0.08f)
                {
                    nextGrid[x, z] = CellType.Park;
                    continue;
                }

                // 高樓聚集效應
                int highriseNeighbors = CountNeighbors(x, z, CellType.Highrise);
                int residentialNeighbors = CountNeighbors(x, z, CellType.Residential);
                int industrialNeighbors = CountNeighbors(x, z, CellType.Industrial);

                float highriseProb = 0.10f + 0.1f * highriseNeighbors; // 周圍高樓越多，變高樓機率越大
                float industrialProb = 0.05f + 0.08f * industrialNeighbors; // 工業區也有聚集傾向
                float residentialProb = 1f - highriseProb - industrialProb;

                float r = UnityEngine.Random.value;
                if (r < highriseProb)
                    nextGrid[x, z] = CellType.Highrise;
                else if (r < highriseProb + industrialProb)
                    nextGrid[x, z] = CellType.Industrial;
                else
                    nextGrid[x, z] = CellType.Residential;
            }
        cityTypeGrid = nextGrid;
    }

    private int CountNeighbors(int x, int z, CellType type)
    {
        int cnt = 0;
        for (int dx = -1; dx <= 1; dx++)
            for (int dz = -1; dz <= 1; dz++)
            {
                if (dx == 0 && dz == 0) continue;
                int nx = x + dx, nz = z + dz;
                if (nx >= 0 && nz >= 0 && nx < gridX && nz < gridZ && cityTypeGrid[nx, nz] == type)
                    cnt++;
            }
        return cnt;
    }


    public CellType[,] GetCityTypeGrid() => cityTypeGrid;

    public void AutoStep10()
    {
        for (int i = 0; i < 10; i++)
            StepAutomaton();
        UnityEngine.Debug.Log("已自動演化10步");
    }

    public void LogCellTypeStats()
    {
        int res = 0, high = 0, ind = 0, park = 0;
        for (int x = 0; x < gridX; x++)
            for (int z = 0; z < gridZ; z++)
                switch (cityTypeGrid[x, z])
                {
                    case CellType.Residential: res++; break;
                    case CellType.Highrise: high++; break;
                    case CellType.Industrial: ind++; break;
                    case CellType.Park: park++; break;
                }
        UnityEngine.Debug.Log($"住宅:{res} 高樓:{high} 工業:{ind} 公園:{park}");
    }

}
