using UnityEngine;

public enum SlimeBuildMode
{
    None,
    Ore1,
    Ore2,
    Ore3,
    Ore4,
    Ore5,
    SlimeSpawner
}

public class SlimeBuildUI : MonoBehaviour
{
    public SlimeBuildMode currentMode = SlimeBuildMode.None;

    public void SetMode_Ore1() => currentMode = SlimeBuildMode.Ore1;
    public void SetMode_Ore2() => currentMode = SlimeBuildMode.Ore2;
    public void SetMode_Ore3() => currentMode = SlimeBuildMode.Ore3;
    public void SetMode_Ore4() => currentMode = SlimeBuildMode.Ore4;
    public void SetMode_Ore5() => currentMode = SlimeBuildMode.Ore5;
    public void SetMode_Spawner() => currentMode = SlimeBuildMode.SlimeSpawner;
    public void SetMode_None() => currentMode = SlimeBuildMode.None;
}
