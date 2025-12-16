using UnityEngine;
public enum TotemType
{
    Swift = 0,        // 속도
    Production = 1,   // 생산
    Harvest = 2,      // 수확
    Expansion = 3,    // 범위
    Duplication = 4,  // 증식
    Stability = 5     // 안정
}

public enum TotemStackRule
{
    MaxOnly,     // 같은 효과는 가장 큰 값만 적용
    Multiply     // 배수 곱연산
}

[System.Serializable]
public class TotemData
{
    public TotemType type;
    public string displayName;
    [TextArea] public string desc;

    public long buildCostCoin = 2000;

    [Header("Aura")]
    public float radius = 6f;

    [Header("Core Values (토탬별로 쓰는 값만 사용)")]
    public float v1 = 1f;
    public float v2 = 0f;

    [Header("Stack Rule")]
    public TotemStackRule stackRule = TotemStackRule.MaxOnly;
}
