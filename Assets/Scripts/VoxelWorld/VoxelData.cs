using UnityEngine;

public class VoxelData
{
    // -------------------------
    // Block 정의
    // -------------------------
    public enum BlockType
    {
        Air,
        Grass,
        Dirt,
        Water
    }

    // -------------------------
    // 월드 크기 정보
    // -------------------------
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Depth { get; private set; }
    public int Seed { get; private set; }

    // -------------------------
    // 데이터
    // -------------------------
    private BlockType[,,] blocks;
    private int[,] heightMap;

    // -------------------------
    // 생성자
    // -------------------------
    public VoxelData(int width, int height, int depth, int seed = -1)
    {
        Width = Mathf.Max(1, width);
        Height = Mathf.Max(1, height);
        Depth = Mathf.Max(1, depth);
        Seed = seed == -1 ? Random.Range(0, int.MaxValue) : seed;

        blocks = new BlockType[Width, Height, Depth];
        heightMap = new int[Width, Depth];

        ClearAll(); // 안전 초기화
    }

    // -------------------------
    // 월드 생성 (IndexOutOfRange 방지)
    // -------------------------
    public void Generate(int maxHeight, int waterLevel, float noiseScale)
    {
        Random.InitState(Seed);

        float offsetX = Random.Range(-9999f, 9999f);
        float offsetZ = Random.Range(-9999f, 9999f);

        int maxY = Height - 1;

        // 방어: 입력값 클램프
        int clampedMaxHeight = Mathf.Clamp(maxHeight, 0, maxY);
        int clampedWater = Mathf.Clamp(waterLevel, 0, maxY);
        float safeNoiseScale = Mathf.Max(0.0001f, noiseScale);

        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Depth; z++)
            {
                float nx = (x + offsetX) / safeNoiseScale;
                float nz = (z + offsetZ) / safeNoiseScale;

                int h = Mathf.FloorToInt(Mathf.PerlinNoise(nx, nz) * clampedMaxHeight);
                h = Mathf.Clamp(h, 0, maxY);

                heightMap[x, z] = h;

                // 0 ~ h : 지면
                for (int y = 0; y <= h; y++)
                {
                    blocks[x, y, z] = (y == h) ? BlockType.Grass : BlockType.Dirt;
                }

                // h+1 ~ water : 물
                if (clampedWater > h)
                {
                    int waterTop = Mathf.Min(clampedWater, maxY);
                    for (int y = h + 1; y <= waterTop; y++)
                    {
                        blocks[x, y, z] = BlockType.Water;
                    }
                }

                // 나머지 위쪽은 Air (이전 데이터 잔상 제거)
                int airStart = Mathf.Max(h + 1, clampedWater + 1);
                for (int y = airStart; y <= maxY; y++)
                {
                    blocks[x, y, z] = BlockType.Air;
                }
            }
        }
    }

    // -------------------------
    // 접근 API
    // -------------------------
    public BlockType GetBlock(int x, int y, int z)
    {
        if (!InRange(x, y, z))
            return BlockType.Air;

        return blocks[x, y, z];
    }

    public void SetBlock(int x, int y, int z, BlockType type)
    {
        if (!InRange(x, y, z))
            return;

        blocks[x, y, z] = type;
    }

    public int GetSurfaceHeight(int x, int z)
    {
        if (x < 0 || x >= Width || z < 0 || z >= Depth)
            return 0;

        return heightMap[x, z];
    }

    public bool IsSolid(int x, int y, int z)
    {
        var block = GetBlock(x, y, z);
        return block != BlockType.Air && block != BlockType.Water;
    }

    // -------------------------
    // 내부 유틸
    // -------------------------
    bool InRange(int x, int y, int z)
    {
        return
            x >= 0 && x < Width &&
            y >= 0 && y < Height &&
            z >= 0 && z < Depth;
    }

    void ClearAll()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int z = 0; z < Depth; z++)
                {
                    blocks[x, y, z] = BlockType.Air;
                }
            }
        }

        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Depth; z++)
            {
                heightMap[x, z] = 0;
            }
        }
    }
}
