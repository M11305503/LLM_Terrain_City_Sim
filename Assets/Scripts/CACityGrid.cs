using System;
using System.Diagnostics;
using UnityEngine;

public enum CellType
{
    None,         // ぃ縱
    Residential,  // 
    Highrise,     // 蔼加
    Park,         // そ堕
    Industrial    // 穨
}

public class CACityGrid : MonoBehaviour
{
    [Header("灿璏簍て把计パ LLM 臱笆")]
    public CityEvolutionParams evolutionParams = new CityEvolutionParams();

    public BuildableGrid buildableGrid; // Inspector ﹚

    [Header("把计")]
    public int gridX = 25;
    public int gridZ = 25;

    [Header("篈戈")]
    public CellType[,] cityTypeGrid;

    private bool[,] _latestBuildableGrid; // 癘魁程穝縱よ獽簍て耞

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
            UnityEngine.Debug.Log("cityTypeGrid ﹍て");
        }
        else
        {
            UnityEngine.Debug.LogWarning("buildableGrid ﹟ゼ碞狐cityTypeGrid ゼ﹍て");
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
                cityTypeGrid[x, z] = (buildable != null && !buildable[x, z]) ? CellType.Park : CellType.Residential;
            }
        }
    }

    public void SetInitialSeeds()
    {
        var buildable = buildableGrid.GetBuildableGrid();
        for (int x = 0; x < gridX; x++)
            for (int z = 0; z < gridZ; z++)
                if (buildable != null && !buildable[x, z])
                    cityTypeGrid[x, z] = CellType.Park;
                else
                    cityTypeGrid[x, z] = CellType.Residential;

        // いァ蔼加
        int cx = gridX / 2, cz = gridZ / 2, highriseR = UnityEngine.Random.Range(2, 5);
        for (int x = cx - highriseR; x <= cx + highriseR; x++)
            for (int z = cz - highriseR; z <= cz + highriseR; z++)
                if (IsBuildable(x, z) && UnityEngine.Random.value < 0.85f)
                    cityTypeGrid[x, z] = CellType.Highrise;

        // 繦诀穨娩絫
        for (int x = 0; x < gridX; x++)
            for (int z = 0; z < gridZ; z++)
                if (IsBuildable(x, z) && (x < 2 || z < 2 || x >= gridX - 2 || z >= gridZ - 2) && UnityEngine.Random.value < 0.7f)
                    cityTypeGrid[x, z] = CellType.Industrial;

        // 繦诀そ堕
        for (int i = 0; i < gridX * gridZ / 16; i++)
        {
            int rx = UnityEngine.Random.Range(0, gridX), rz = UnityEngine.Random.Range(0, gridZ);
            if (IsBuildable(rx, rz) && UnityEngine.Random.value < 0.5f)
                cityTypeGrid[rx, rz] = CellType.Park;
        }
    }

    private bool IsBuildable(int x, int z)
    {
        return x >= 0 && z >= 0 && x < gridX && z < gridZ && _latestBuildableGrid != null && _latestBuildableGrid[x, z];
    }

    public void ApplyLLMParameters(string json)
    {
        CityEvolutionParams p = JsonUtility.FromJson<CityEvolutionParams>(json);
        evolutionParams = p;
        UnityEngine.Debug.Log("甅ノ LLM 把计" + json);
    }

    [ContextMenu("ノ絛ㄒ把计甅ノ LLM")]
    public void ApplyExampleLLMParams()
    {
        string json = "{\"baseHighriseProb\":0.18,\"baseIndustrialProb\":0.07,\"highriseClusterBoost\":0.15,\"industrialClusterBoost\":0.09,\"parkSpreadProb\":0.5}";
        ApplyLLMParameters(json);
    }

    public void StepAutomaton()
    {
        var nextGrid = new CellType[gridX, gridZ];
        var buildable = buildableGrid.GetBuildableGrid();

        // 眖 evolutionParams 弄把计
        float baseHighriseProb = evolutionParams.baseHighriseProb;
        float baseIndustrialProb = evolutionParams.baseIndustrialProb;
        float highriseClusterBoost = evolutionParams.highriseClusterBoost;
        float industrialClusterBoost = evolutionParams.industrialClusterBoost;
        float parkSpreadProb = evolutionParams.parkSpreadProb;

        for (int x = 0; x < gridX; x++)
        {
            for (int z = 0; z < gridZ; z++)
            {
                // ぃ縱ッ环そ堕
                if (buildable != null && !buildable[x, z])
                {
                    nextGrid[x, z] = CellType.Park;
                    continue;
                }

                // そ堕籈栋蒋┑砞璸把计北
                int parkNeighbors = CountNeighbors(x, z, CellType.Park);
                if (parkNeighbors >= 3 && UnityEngine.Random.value < parkSpreadProb)
                {
                    nextGrid[x, z] = CellType.Park;
                    continue;
                }

                int highriseNeighbors = CountNeighbors(x, z, CellType.Highrise);
                int industrialNeighbors = CountNeighbors(x, z, CellType.Industrial);

                float highriseProb = baseHighriseProb + highriseClusterBoost * highriseNeighbors;
                float industrialProb = baseIndustrialProb + industrialClusterBoost * industrialNeighbors;
                float residentialProb = 1f - highriseProb - industrialProb;

                float r = UnityEngine.Random.value;
                if (r < highriseProb)
                    nextGrid[x, z] = CellType.Highrise;
                else if (r < highriseProb + industrialProb)
                    nextGrid[x, z] = CellType.Industrial;
                else
                    nextGrid[x, z] = CellType.Residential;
            }
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
        UnityEngine.Debug.Log("笆簍て10˙");
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
        UnityEngine.Debug.Log($":{res} 蔼加:{high} 穨:{ind} そ堕:{park}");
    }
}