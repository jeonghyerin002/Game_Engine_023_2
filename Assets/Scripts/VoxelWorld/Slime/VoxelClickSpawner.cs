using UnityEngine;

public class VoxelClickSpawner : MonoBehaviour
{
    [Header("필수 레퍼런스")]
    public VoxelWorld voxelWorld;      // 월드 오브젝트
    public SlimeBuildUI buildUI;       // 현재 모드 정보

    [Header("프리팹")]
    public GameObject[] orePrefabs;    // 0~4 => 광석1~5 (금, 은, 동, 구리, 철)
    public GameObject slimeSpawnerPrefab;

    [Header("정리용 부모")]
    public Transform oreParent;
    public Transform spawnerParent;

    void Start()
    {
        if (oreParent == null)
        {
            var o = new GameObject("OreObjects");
            oreParent = o.transform;
        }

        if (spawnerParent == null)
        {
            var s = new GameObject("SlimeSpawners");
            spawnerParent = s.transform;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (voxelWorld == null || voxelWorld.Picker == null) return;
            if (buildUI == null || buildUI.currentMode == SlimeBuildMode.None) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var placePos = voxelWorld.Picker.GetPlacementPosition(ray);
            if (!placePos.HasValue) return;

            Vector3Int blockPos = placePos.Value;
            Vector3 worldPos = new Vector3(blockPos.x, blockPos.y, blockPos.z);

            switch (buildUI.currentMode)
            {
                case SlimeBuildMode.Ore1:
                case SlimeBuildMode.Ore2:
                case SlimeBuildMode.Ore3:
                case SlimeBuildMode.Ore4:
                case SlimeBuildMode.Ore5:
                    int idx = (int)buildUI.currentMode - (int)SlimeBuildMode.Ore1;
                    SpawnOre(idx, worldPos);
                    break;

                case SlimeBuildMode.SlimeSpawner:
                    {
                        GameObject spawnerObj = null;

                        // 1) 프리팹이 있으면 프리팹 사용
                        if (slimeSpawnerPrefab != null)
                        {
                            spawnerObj = Instantiate(
                                slimeSpawnerPrefab,
                                worldPos + Vector3.up * 0.5f,
                                Quaternion.identity,
                                spawnerParent
                            );
                        }
                        else
                        {
                            // 2) 프리팹 없으면 핑크 상자 자동 생성
                            spawnerObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            spawnerObj.transform.position = worldPos + Vector3.up * 0.5f;
                            spawnerObj.transform.localScale = Vector3.one * 0.85f;
                            spawnerObj.transform.SetParent(spawnerParent);

                            var renderer = spawnerObj.GetComponent<MeshRenderer>();
                            if (renderer != null)
                            {
                                Material mat = new Material(Shader.Find("Standard"));
                                mat.color = new Color(1f, 0.6f, 1f);  // 파스텔 핑크
                                mat.SetFloat("_Metallic", 0.2f);
                                mat.SetFloat("_Glossiness", 0.7f);
                                renderer.material = mat;
                            }
                        }

                        spawnerObj.name = "SlimeSpawner";

                        var spawnerComp = spawnerObj.GetComponent<SlimeSpawner>();
                        if (spawnerComp == null)
                            spawnerComp = spawnerObj.AddComponent<SlimeSpawner>();

                        break;
                    }
            }
        }

        void SpawnOre(int idx, Vector3 worldPos)
        {
            GameObject ore = null;

            // 1) 프리팹이 있으면 프리팹 우선
            if (orePrefabs != null &&
                idx >= 0 &&
                idx < orePrefabs.Length &&
                orePrefabs[idx] != null)
            {
                ore = Instantiate(orePrefabs[idx], worldPos, Quaternion.identity, oreParent);
            }
            else
            {
                // 2) 프리팹이 없으면 Primitive Cube + 색깔로 생성
                ore = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ore.transform.position = worldPos;
                ore.transform.localScale = Vector3.one * 0.9f;
                ore.transform.SetParent(oreParent);

                // 머티리얼 생성해서 색 지정
                var renderer = ore.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    var mat = new Material(Shader.Find("Standard"));
                    Color c = Color.white;
                    float metallic = 0.8f;
                    float smoothness = 0.6f;

                    switch (idx)
                    {
                        case 0: // 금
                            c = new Color(1.0f, 0.84f, 0.0f); // Gold-ish
                            metallic = 1.0f;
                            smoothness = 0.9f;
                            break;
                        case 1: // 은
                            c = new Color(0.9f, 0.9f, 0.95f);
                            metallic = 0.95f;
                            smoothness = 0.9f;
                            break;
                        case 2: // 동
                            c = new Color(0.8f, 0.5f, 0.2f);
                            metallic = 0.8f;
                            smoothness = 0.7f;
                            break;
                        case 3: // 구리
                            c = new Color(0.9f, 0.45f, 0.2f);
                            metallic = 0.9f;
                            smoothness = 0.75f;
                            break;
                        case 4: // 철
                            c = new Color(0.6f, 0.6f, 0.65f);
                            metallic = 0.9f;
                            smoothness = 0.6f;
                            break;
                        default:
                            c = Color.gray;
                            metallic = 0.5f;
                            smoothness = 0.5f;
                            break;
                    }

                    mat.color = c;
                    mat.SetFloat("_Metallic", metallic);
                    mat.SetFloat("_Glossiness", smoothness);
                    renderer.material = mat;
                }
            }

            ore.name = $"OreType{idx + 1}";
            ore.tag = $"Ore{idx + 1}"; // Ore1~Ore5 태그
        }
    }
}
