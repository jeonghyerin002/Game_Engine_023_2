using UnityEngine;

public class VoxelPicker
{
    private VoxelData data;

    public VoxelPicker(VoxelData data)
    {
        this.data = data;
    }

    // 클릭한 블록 좌표
    public Vector3Int? GetBlockPosition(Ray ray, float maxDistance = 100f)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            Vector3 point = hit.point - hit.normal * 0.1f;
            return WorldToBlock(point);
        }
        return null;
    }

    // 블록 위에 배치할 좌표
    public Vector3Int? GetPlacementPosition(Ray ray, float maxDistance = 100f)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            Vector3 point = hit.point + hit.normal * 0.1f;
            return WorldToBlock(point);
        }
        return null;
    }

    // 클릭한 블록 타입
    public VoxelData.BlockType? GetBlockType(Ray ray, float maxDistance = 100f)
    {
        var pos = GetBlockPosition(ray, maxDistance);
        if (pos.HasValue)
        {
            return data.GetBlock(pos.Value.x, pos.Value.y, pos.Value.z);
        }
        return null;
    }

    // 히트 정보 전체 반환
    public VoxelHitInfo? GetHitInfo(Ray ray, float maxDistance = 100f)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            Vector3Int blockPos = WorldToBlock(hit.point - hit.normal * 0.1f);
            Vector3Int placePos = WorldToBlock(hit.point + hit.normal * 0.1f);

            return new VoxelHitInfo
            {
                BlockPosition = blockPos,
                PlacementPosition = placePos,
                BlockType = data.GetBlock(blockPos.x, blockPos.y, blockPos.z),
                Normal = hit.normal,
                Point = hit.point
            };
        }
        return null;
    }

    private Vector3Int WorldToBlock(Vector3 worldPos)
    {
        return new Vector3Int(
            Mathf.FloorToInt(worldPos.x + 0.5f),
            Mathf.FloorToInt(worldPos.y + 0.5f),
            Mathf.FloorToInt(worldPos.z + 0.5f)
        );
    }
}

public struct VoxelHitInfo
{
    public Vector3Int BlockPosition;
    public Vector3Int PlacementPosition;
    public VoxelData.BlockType BlockType;
    public Vector3 Normal;
    public Vector3 Point;
}