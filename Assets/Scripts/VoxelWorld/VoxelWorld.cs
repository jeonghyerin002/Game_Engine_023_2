using UnityEngine;

public class VoxelWorld : MonoBehaviour
{
    [Header("Materials")]
    public Material grassMaterial;
    public Material dirtMaterial;
    public Material waterMaterial;

    [Header("World Settings")]
    public int width = 50;
    public int depth = 50;
    public int maxHeight = 16;
    public int waterLevel = 4;
    public float noiseScale = 20f;
    public int seed = -1; // -1이면 랜덤

    [Header("Debug")]
    public bool showPickedBlock = true;

    // 외부 접근용
    public VoxelData Data { get; private set; }
    public VoxelPicker Picker { get; private set; }

    [Header("Layers")]
    public string groundLayerName = "Ground";


    void Start()
    {
        // 데이터 생성
        Data = new VoxelData(width, maxHeight + waterLevel + 1, depth, seed);
        Data.Generate(maxHeight, waterLevel, noiseScale);

        // 메시 빌드
        var builder = new VoxelMeshBuilder(Data);
        builder.Build(transform, grassMaterial, dirtMaterial, waterMaterial);
        ApplyLayerToWorldMeshes();
        // 피커 초기화
        Picker = new VoxelPicker(Data);

        Debug.Log($"World generated with seed: {Data.Seed}");
    }

    void Update()
    {
        if (showPickedBlock && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var hitInfo = Picker.GetHitInfo(ray);

            if (hitInfo.HasValue)
            {
                Debug.Log($"Block: {hitInfo.Value.BlockType} at {hitInfo.Value.BlockPosition}");
            }
        }
    }

    // 같은 시드로 재생성
    public void Regenerate()
    {
        // 기존 메시 삭제
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        Data.Generate(maxHeight, waterLevel, noiseScale);
        var builder = new VoxelMeshBuilder(Data);
        builder.Build(transform, grassMaterial, dirtMaterial, waterMaterial);
        ApplyLayerToWorldMeshes();
    }

    // 새 시드로 재생성
    public void RegenerateWithNewSeed()
    {
        seed = Random.Range(0, int.MaxValue);
        Data = new VoxelData(width, maxHeight + waterLevel + 1, depth, seed);
        Regenerate();
    }

    void ApplyLayerToWorldMeshes()
    {
        int groundLayer = LayerMask.NameToLayer(groundLayerName);
        if (groundLayer == -1)
        {
            Debug.LogWarning($"Layer '{groundLayerName}' 가 없습니다.");
            return;
        }

        foreach (Transform child in transform)
        {
            SetLayerRecursively(child.gameObject, groundLayer);
        }
    }

    static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            SetLayerRecursively(obj.transform.GetChild(i).gameObject, layer);
        }
    }
}