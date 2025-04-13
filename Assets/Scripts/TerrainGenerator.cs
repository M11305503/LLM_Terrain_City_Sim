using UnityEngine;

[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour
{
    [Header("地形設定")]
    public int width = 256;
    public int height = 256;
    public float scale = 20f;

    [Header("種子控制")]
    public int seed = 0;                  // 種子數字（可固定）
    public bool useRandomSeed = true;     // 是否使用隨機種子
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

        // 根據最小邊取得合法的 heightmapResolution（必須為 2 的冪 + 1）
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
