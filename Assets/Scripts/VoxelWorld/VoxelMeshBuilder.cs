using System.Collections.Generic;
using UnityEngine;

public class VoxelMeshBuilder
{
    private VoxelData data;

    public VoxelMeshBuilder(VoxelData data)
    {
        this.data = data;
    }

    public void Build(Transform parent, Material grassMat, Material dirtMat, Material waterMat)
    {
        List<Vector3> grassVerts = new List<Vector3>();
        List<int> grassTris = new List<int>();
        List<Vector3> dirtVerts = new List<Vector3>();
        List<int> dirtTris = new List<int>();
        List<Vector3> waterVerts = new List<Vector3>();
        List<int> waterTris = new List<int>();

        for (int x = 0; x < data.Width; x++)
        {
            for (int z = 0; z < data.Depth; z++)
            {
                for (int y = 0; y < data.Height; y++)
                {
                    var block = data.GetBlock(x, y, z);

                    if (block == VoxelData.BlockType.Grass)
                        AddVisibleFaces(x, y, z, grassVerts, grassTris);
                    else if (block == VoxelData.BlockType.Dirt)
                        AddVisibleFaces(x, y, z, dirtVerts, dirtTris);
                    else if (block == VoxelData.BlockType.Water)
                        AddWaterFace(x, y, z, waterVerts, waterTris);
                }
            }
        }

        if (grassVerts.Count > 0)
            CreateMeshObject("Grass", grassVerts, grassTris, grassMat, parent);
        if (dirtVerts.Count > 0)
            CreateMeshObject("Dirt", dirtVerts, dirtTris, dirtMat, parent);
        if (waterVerts.Count > 0)
            CreateMeshObject("Water", waterVerts, waterTris, waterMat, parent);
    }

    private void AddVisibleFaces(int x, int y, int z, List<Vector3> verts, List<int> tris)
    {
        Vector3 pos = new Vector3(x, y, z);

        if (!data.IsSolid(x, y + 1, z)) AddFaceTop(pos, verts, tris);
        if (!data.IsSolid(x, y - 1, z)) AddFaceBottom(pos, verts, tris);
        if (!data.IsSolid(x, y, z + 1)) AddFaceFront(pos, verts, tris);
        if (!data.IsSolid(x, y, z - 1)) AddFaceBack(pos, verts, tris);
        if (!data.IsSolid(x + 1, y, z)) AddFaceRight(pos, verts, tris);
        if (!data.IsSolid(x - 1, y, z)) AddFaceLeft(pos, verts, tris);
    }

    private void AddFaceTop(Vector3 pos, List<Vector3> verts, List<int> tris)
    {
        int i = verts.Count;
        float y = pos.y + 0.5f;
        verts.Add(new Vector3(pos.x - 0.5f, y, pos.z - 0.5f));
        verts.Add(new Vector3(pos.x - 0.5f, y, pos.z + 0.5f));
        verts.Add(new Vector3(pos.x + 0.5f, y, pos.z + 0.5f));
        verts.Add(new Vector3(pos.x + 0.5f, y, pos.z - 0.5f));
        tris.Add(i); tris.Add(i + 1); tris.Add(i + 2);
        tris.Add(i); tris.Add(i + 2); tris.Add(i + 3);
    }

    private void AddFaceBottom(Vector3 pos, List<Vector3> verts, List<int> tris)
    {
        int i = verts.Count;
        float y = pos.y - 0.5f;
        verts.Add(new Vector3(pos.x - 0.5f, y, pos.z - 0.5f));
        verts.Add(new Vector3(pos.x + 0.5f, y, pos.z - 0.5f));
        verts.Add(new Vector3(pos.x + 0.5f, y, pos.z + 0.5f));
        verts.Add(new Vector3(pos.x - 0.5f, y, pos.z + 0.5f));
        tris.Add(i); tris.Add(i + 2); tris.Add(i + 1);
        tris.Add(i); tris.Add(i + 3); tris.Add(i + 2);
    }

    private void AddFaceFront(Vector3 pos, List<Vector3> verts, List<int> tris)
    {
        int i = verts.Count;
        float z = pos.z + 0.5f;
        verts.Add(new Vector3(pos.x - 0.5f, pos.y - 0.5f, z));
        verts.Add(new Vector3(pos.x + 0.5f, pos.y - 0.5f, z));
        verts.Add(new Vector3(pos.x + 0.5f, pos.y + 0.5f, z));
        verts.Add(new Vector3(pos.x - 0.5f, pos.y + 0.5f, z));
        tris.Add(i); tris.Add(i + 2); tris.Add(i + 1);
        tris.Add(i); tris.Add(i + 3); tris.Add(i + 2);
    }

    private void AddFaceBack(Vector3 pos, List<Vector3> verts, List<int> tris)
    {
        int i = verts.Count;
        float z = pos.z - 0.5f;
        verts.Add(new Vector3(pos.x - 0.5f, pos.y - 0.5f, z));
        verts.Add(new Vector3(pos.x - 0.5f, pos.y + 0.5f, z));
        verts.Add(new Vector3(pos.x + 0.5f, pos.y + 0.5f, z));
        verts.Add(new Vector3(pos.x + 0.5f, pos.y - 0.5f, z));
        tris.Add(i); tris.Add(i + 1); tris.Add(i + 2);
        tris.Add(i); tris.Add(i + 2); tris.Add(i + 3);
    }

    private void AddFaceRight(Vector3 pos, List<Vector3> verts, List<int> tris)
    {
        int i = verts.Count;
        float x = pos.x + 0.5f;
        verts.Add(new Vector3(x, pos.y - 0.5f, pos.z - 0.5f));
        verts.Add(new Vector3(x, pos.y - 0.5f, pos.z + 0.5f));
        verts.Add(new Vector3(x, pos.y + 0.5f, pos.z + 0.5f));
        verts.Add(new Vector3(x, pos.y + 0.5f, pos.z - 0.5f));
        tris.Add(i); tris.Add(i + 2); tris.Add(i + 1);
        tris.Add(i); tris.Add(i + 3); tris.Add(i + 2);
    }

    private void AddFaceLeft(Vector3 pos, List<Vector3> verts, List<int> tris)
    {
        int i = verts.Count;
        float x = pos.x - 0.5f;
        verts.Add(new Vector3(x, pos.y - 0.5f, pos.z - 0.5f));
        verts.Add(new Vector3(x, pos.y + 0.5f, pos.z - 0.5f));
        verts.Add(new Vector3(x, pos.y + 0.5f, pos.z + 0.5f));
        verts.Add(new Vector3(x, pos.y - 0.5f, pos.z + 0.5f));
        tris.Add(i); tris.Add(i + 1); tris.Add(i + 2);
        tris.Add(i); tris.Add(i + 2); tris.Add(i + 3);
    }

    private void AddWaterFace(int x, int y, int z, List<Vector3> verts, List<int> tris)
    {
        int i = verts.Count;
        float yPos = y + 0.5f;
        verts.Add(new Vector3(x - 0.5f, yPos, z - 0.5f));
        verts.Add(new Vector3(x - 0.5f, yPos, z + 0.5f));
        verts.Add(new Vector3(x + 0.5f, yPos, z + 0.5f));
        verts.Add(new Vector3(x + 0.5f, yPos, z - 0.5f));
        tris.Add(i); tris.Add(i + 1); tris.Add(i + 2);
        tris.Add(i); tris.Add(i + 2); tris.Add(i + 3);
    }

    private void CreateMeshObject(string name, List<Vector3> verts, List<int> tris, Material mat, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);

        MeshFilter mf = obj.AddComponent<MeshFilter>();
        MeshRenderer mr = obj.AddComponent<MeshRenderer>();
        MeshCollider mc = obj.AddComponent<MeshCollider>();

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        mf.mesh = mesh;
        mr.material = mat;
        mc.sharedMesh = mesh;
    }
}