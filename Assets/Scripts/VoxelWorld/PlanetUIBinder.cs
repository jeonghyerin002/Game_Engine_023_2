using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlanetUIBinder : MonoBehaviour
{
    [Header("Refs")]
    public PlanetManager planetManager;
    public SlimePlanetSaveManager planetSave; // 행성별 세이브/로드 담당

    [Header("Buttons")]
    public Button btnNew;
    public Button btnPrev;
    public Button btnNext;
    public Button btnDelete;

    [Header("UI")]
    public TMP_Text infoText;

    void Start()
    {
        btnNew.onClick.AddListener(OnNewPlanet);
        btnPrev.onClick.AddListener(OnPrev);
        btnNext.onClick.AddListener(OnNext);
        btnDelete.onClick.AddListener(OnDelete);

        // 시작 시 현재 행성 로드
        planetSave.LoadCurrentPlanet();
        Refresh();
    }

    void OnNewPlanet()
    {
        planetManager.CreatePlanet();
        planetSave.LoadCurrentPlanet(); // 새 행성의 “전용 파일” 로드(처음은 비어있음)
        Refresh();
    }

    void OnPrev()
    {
        planetManager.PrevPlanet();
        planetSave.LoadCurrentPlanet(); // 그 행성 파일 로드 + 복원
        Refresh();
    }

    void OnNext()
    {
        planetManager.NextPlanet();
        planetSave.LoadCurrentPlanet();
        Refresh();
    }

    void OnDelete()
    {
        planetManager.DeletePlanet();
        planetSave.LoadCurrentPlanet();
        Refresh();
    }

    void Refresh()
    {
        var p = planetManager.CurrentPlanet;
        if (p == null) return;

        infoText.text = $"{planetManager.currentIndex + 1}/{planetManager.planets.Count}  {p.name}\nSeed: {p.seed}";
    }
}
