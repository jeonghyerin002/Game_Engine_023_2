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

    [Header("Debug")]
    public bool showPickedBlock = true;

    public VoxelData Data { get; private set; }
    public VoxelPicker Picker { get; private set; }

    void Start()
    {
        Regenerate();
    }

    public void Regenerate()
    {
        int seed = (PlanetManager.Instance != null && PlanetManager.Instance.CurrentPlanet != null)
            ? PlanetManager.Instance.CurrentPlanet.seed
            : 12345;

        // 기존 메시 삭제
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        // 데이터 생성
        Data = new VoxelData(width, maxHeight + waterLevel + 1, depth, seed);
        Data.Generate(maxHeight, waterLevel, noiseScale);

        // 메시 빌드
        var builder = new VoxelMeshBuilder(Data);
        builder.Build(transform, grassMaterial, dirtMaterial, waterMaterial);

        // 피커
        Picker = new VoxelPicker(Data);

        Debug.Log($"[VoxelWorld] Regenerated with seed={seed}");
    }
}
