using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NoiseVoxelMap : MonoBehaviour
{
    public GameObject grassBlockPrefab;
    public GameObject dirtBlockPrefab;
    public GameObject waterBlockPrefab;
    public GameObject diamondBlockPrefab;
    public int width = 20;
    public int depth = 20;
    public int maxHeight = 16;
    public float randomBlock = 0.2f;
    [SerializeField] float noiseScale = 20f;


    void Start()
    {
        float offsetX = Random.Range(-9999f, 9999f);
        float offsetZ = Random.Range(-9999f, 9999f);

        for(int x = 0; x < width; x++)
        {
            for(int z = 0; z < depth; z++)
            {
                float nx = (x + offsetX) / noiseScale;
                float nz = (z + offsetZ) / noiseScale;

                float noise = Mathf.PerlinNoise(nx, nz);

                int h = Mathf.FloorToInt(noise * maxHeight);

                if (h <= 0) continue;

                for (int y = 0; y <= h; y++)
                {
                    if (y == h)
                    {
                        GrassPlace(x, y, z);
                    }
                    else
                    {
                        float randomP = Random.Range(0, 1);
                        if (randomBlock < randomP)
                            DiamondPlace(x, y, z);
                        else
                        {
                            DirtPlace(x, y, z);
                        }
                    }
                }
                for(int y = h+1; y <= 4; y++)
                {
                    if (waterBlockPrefab != null)
                    {
                        WaterPlace(x, y, z);
                        
                    }
                    
                }
            }
        }
    }


    void Update()
    {
        
    }
    void DirtPlace(int x, int y, int z)
    {
        var go = Instantiate(dirtBlockPrefab, new Vector3(x, y, z), Quaternion.identity, transform);
        go.name = $"Dirt_{x}_{y}_{z}";

        var b = go.GetComponent<Block>() ?? go.AddComponent<Block>();
        b.type = ItemType.Dirt;
        b.maxHP = 3;
        b.dropCount = 1;
        b.mineable = true;

    }
    void DiamondPlace(int x, int y, int z)
    {
        var go = Instantiate(diamondBlockPrefab, new Vector3(x, y, z), Quaternion.identity, transform);
        go.name = $"diamond_{x}_{y}_{z}";

        var b = go.GetComponent<Block>() ?? go.AddComponent<Block>();
        b.type = ItemType.Diamond;
        b.maxHP = 3;
        b.dropCount = 1;
        b.mineable = true;

    }
    void GrassPlace(int x, int y, int z)
    {
        var go = Instantiate(grassBlockPrefab, new Vector3(x, y, z), Quaternion.identity, transform);
        go.name = $"Grass_{x}_{y}_{z}";

        var b = go.GetComponent<Block>() ?? go.AddComponent<Block>();
        b.type = ItemType.Grass;
        b.maxHP = 3;
        b.dropCount = 1;
        b.mineable = true;
    }
    void WaterPlace(int x, int y, int z)
    {
        var go = Instantiate(waterBlockPrefab, new Vector3(x, y, z), Quaternion.identity, transform);
        go.name = $"Water_{x}_{y}_{z}";

        var b = go.GetComponent<Block>() ?? go.AddComponent<Block>();
        b.type = ItemType.Water;
        b.maxHP = 3;
        b.dropCount = 1;
        b.mineable = true;
    }

    public void PlaceTile(Vector3Int pos, ItemType type)
    {
        switch (type)
        {
            case ItemType.Dirt:
                DirtPlace(pos.x, pos.y, pos.z);
                break;
            case ItemType.Grass:
                GrassPlace(pos.x, pos.y, pos.z);
                break;
        }


    }
}
