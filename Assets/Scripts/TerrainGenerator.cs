using UnityEngine;

[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour
{
    [Header("�a�γ]�w")]
    public int width = 256;
    public int height = 256;
    public float scale = 20f;

    [Header("�ؤl����")]
    public int seed = 0;                  // �ؤl�Ʀr�]�i�T�w�^
    public bool useRandomSeed = true;     // �O�_�ϥ��H���ؤl
    private float seedOffsetX = 0f;
    private float seedOffsetY = 0f;

    private Terrain terrain;

    void Awake()
    {
        terrain = GetComponent<Terrain>();
    }

    public void Generate()
    {
        if (terrain == null)
            terrain = GetComponent<Terrain>();

        GenerateSeedOffset();

        // �ھڳ̤p����o�X�k�� heightmapResolution�]������ 2 ���� + 1�^
        int minDimension = Mathf.Min(width, height);
        int mapSize = Mathf.ClosestPowerOfTwo(minDimension);
        int resolution = mapSize + 1;

        TerrainData terrainData = terrain.terrainData;
        terrainData.heightmapResolution = resolution;
        terrainData.size = new Vector3(width, 20, height);
        terrainData.SetHeights(0, 0, GenerateHeights(mapSize));
    }

    private void GenerateSeedOffset()
    {
        if (useRandomSeed)
        {
            seedOffsetX = Random.Range(0f, 10000f);
            seedOffsetY = Random.Range(0f, 10000f);
        }
        else
        {
            Random.InitState(seed);
            seedOffsetX = Random.Range(0f, 10000f);
            seedOffsetY = Random.Range(0f, 10000f);
        }
    }

    private float[,] GenerateHeights(int mapSize)
    {
        float[,] heights = new float[mapSize, mapSize];
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                float xCoord = ((float)x / mapSize * scale) + seedOffsetX;
                float yCoord = ((float)y / mapSize * scale) + seedOffsetY;
                heights[x, y] = Mathf.PerlinNoise(xCoord, yCoord);
            }
        }
        return heights;
    }
}
