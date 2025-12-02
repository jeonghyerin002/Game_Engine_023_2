using UnityEngine;

public class VoxelData
{
    public enum BlockType { Air, Grass, Dirt, Water }

    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Depth { get; private set; }
    public int Seed { get; private set; }

    private BlockType[,,] blocks;
    private int[,] heightMap;

    public VoxelData(int width, int height, int depth, int seed = -1)
    {
        Width = width;
        Height = height;
        Depth = depth;
        Seed = seed == -1 ? Random.Range(0, int.MaxValue) : seed;

        blocks = new BlockType[width, height, depth];
        heightMap = new int[width, depth];
    }

    public void Generate(int maxHeight, int waterLevel, float noiseScale)
    {
        Random.InitState(Seed);
        float offsetX = Random.Range(-9999f, 9999f);
        float offsetZ = Random.Range(-9999f, 9999f);

        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Depth; z++)
            {
                float nx = (x + offsetX) / noiseScale;
                float nz = (z + offsetZ) / noiseScale;
                int h = Mathf.FloorToInt(Mathf.PerlinNoise(nx, nz) * maxHeight);
                heightMap[x, z] = h;

                for (int y = 0; y <= h; y++)
                {
                    blocks[x, y, z] = (y == h) ? BlockType.Grass : BlockType.Dirt;
                }
                for (int y = h + 1; y <= waterLevel; y++)
                {
                    blocks[x, y, z] = BlockType.Water;
                }
            }
        }
    }

    public BlockType GetBlock(int x, int y, int z)
    {
        if (x < 0 || x >= Width || z < 0 || z >= Depth || y < 0 || y >= Height)
            return BlockType.Air;
        return blocks[x, y, z];
    }

    public void SetBlock(int x, int y, int z, BlockType type)
    {
        if (x < 0 || x >= Width || z < 0 || z >= Depth || y < 0 || y >= Height)
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
}