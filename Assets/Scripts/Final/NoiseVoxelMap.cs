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
    public int width = 20;
    public int depth = 20;
    public int maxHeight = 16;
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
                        DirtPlace(x, y, z);
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
        go.name = $"B_{x}_{y}_{z}";
    }
    void GrassPlace(int x, int y, int z)
    {
        var go = Instantiate(grassBlockPrefab, new Vector3(x, y, z), Quaternion.identity, transform);
        go.name = $"B_{x}_{y}_{z}";
    }
    void WaterPlace(int x, int y, int z)
    {
        var go = Instantiate(waterBlockPrefab, new Vector3(x, y, z), Quaternion.identity, transform);
        go.name = $"B_{x}_{y}_{z}";
    }
}
