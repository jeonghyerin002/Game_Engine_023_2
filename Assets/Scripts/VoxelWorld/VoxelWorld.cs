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

    public string groundLayer = "Ground";

    void Awake()
    {
        // 유니티 창이 백그라운드여도 계속 실행되게 함
        Application.runInBackground = true;
    }

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

        ApplyGroundLayerToChildren();

        // 피커
        Picker = new VoxelPicker(Data);

        Debug.Log($"[VoxelWorld] Regenerated with seed={seed}");
    }

    void ApplyGroundLayerToChildren()
    {
        int layer = LayerMask.NameToLayer(groundLayer);
        if (layer == -1)
        {
            Debug.LogWarning($"[VoxelWorld] Layer '{groundLayer}' does not exist.");
            return;
        }

        foreach (Transform child in transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            SetLayerRecursively(obj.transform.GetChild(i).gameObject, layer);
        }
    }
}
