using System;
using System.Diagnostics;
using UnityEngine;

public enum CellType
{
    None,         // ���i�ؿv
    Residential,  // ��v
    Highrise,     // ����
    Park,         // ����
    Industrial    // �u�~
}

public class CACityGrid : MonoBehaviour
{
    public BuildableGrid buildableGrid; // Inspector ���w

    [Header("��l�Ѽ�")]
    public int gridX = 25;
    public int gridZ = 25;

    [Header("���A���")]
    public CellType[,] cityTypeGrid;

    private bool[,] _latestBuildableGrid; // �O���̷s�i�ؿv��A��K�t�ƮɧP�_

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
            UnityEngine.Debug.Log("cityTypeGrid �w��l��");
        }
        else
        {
            UnityEngine.Debug.LogWarning("buildableGrid �|���N���AcityTypeGrid ����l��");
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
                // �u�n����ؿv�A�����]������
                cityTypeGrid[x, z] = (buildable != null && !buildable[x, z]) ? CellType.Park : CellType.Residential;
            }
        }
    }

    public void SetInitialSeeds()
    {
        // ��l�ƮɫO�ҩҦ����i�ؿv�泣�]�� Park
        var buildable = buildableGrid.GetBuildableGrid();
        for (int x = 0; x < gridX; x++)
            for (int z = 0; z < gridZ; z++)
                if (buildable != null && !buildable[x, z])
                    cityTypeGrid[x, z] = CellType.Park;
                else
                    cityTypeGrid[x, z] = CellType.Residential;

        // ��l��l�i�� seed �����]�U��ާ@���u�ʥi�ؿv��^

        // ��������
        int cx = gridX / 2, cz = gridZ / 2, highriseR = UnityEngine.Random.Range(2, 5);
        for (int x = cx - highriseR; x <= cx + highriseR; x++)
            for (int z = cz - highriseR; z <= cz + highriseR; z++)
                if (IsBuildable(x, z) && UnityEngine.Random.value < 0.85f)
                    cityTypeGrid[x, z] = CellType.Highrise;

        // �H���u�~�]��t�^
        for (int x = 0; x < gridX; x++)
            for (int z = 0; z < gridZ; z++)
                if (IsBuildable(x, z) && (x < 2 || z < 2 || x >= gridX - 2 || z >= gridZ - 2) && UnityEngine.Random.value < 0.7f)
                    cityTypeGrid[x, z] = CellType.Industrial;

        // �H������]�i�ؿv�Ϥ~���i��A�X�{�p�q���^
        for (int i = 0; i < gridX * gridZ / 16; i++)
        {
            int rx = UnityEngine.Random.Range(0, gridX), rz = UnityEngine.Random.Range(0, gridZ);
            if (IsBuildable(rx, rz) && UnityEngine.Random.value < 0.5f)
                cityTypeGrid[rx, rz] = CellType.Park;
        }
    }

    // �P�_�o���I�O���O�i�ؿv�]�B���V�ɡ^
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
                // ���i�ؿv��û�����
                if (buildable != null && !buildable[x, z])
                {
                    nextGrid[x, z] = CellType.Park;
                    continue;
                }

                // �p�G�ۤv���ӴN�O����A�åB������F�~�A�i�H�����v�O�d���骬�A
                int parkNeighbors = CountNeighbors(x, z, CellType.Park);

                // �p�G�P�򤽶�h�A�����v�ܦ�����
                if (parkNeighbors >= 3 && UnityEngine.Random.value < 0.08f)
                {
                    nextGrid[x, z] = CellType.Park;
                    continue;
                }

                // ���ӻE������
                int highriseNeighbors = CountNeighbors(x, z, CellType.Highrise);
                int residentialNeighbors = CountNeighbors(x, z, CellType.Residential);
                int industrialNeighbors = CountNeighbors(x, z, CellType.Industrial);

                float highriseProb = 0.10f + 0.1f * highriseNeighbors; // �P�򰪼ӶV�h�A�ܰ��Ӿ��v�V�j
                float industrialProb = 0.05f + 0.08f * industrialNeighbors; // �u�~�Ϥ]���E���ɦV
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
        UnityEngine.Debug.Log("�w�۰ʺt��10�B");
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
        UnityEngine.Debug.Log($"��v:{res} ����:{high} �u�~:{ind} ����:{park}");
    }

}
