using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("무전기")]
    public bool isRogerOpen = true;
    public GameObject rogerPanel;
    public Button openStore;

    [Header("상점")]
    public GameObject storePanel;
    public bool isStoreOpen = true;
    public Button ore1;
    public Button ore2;
    public Button ore3;
    public Button ore4;
    public Button ore5;
    public Button slimeSpawer;

    [Header("코인")]
    public int coin;
    public int currentCoin;
    public TextMeshProUGUI playerCoin;


    void Start()
    {
        currentCoin = coin;

        isRogerOpen = false;
        isStoreOpen = false;
    }
    void Update()
    {
        RogerSetting();
        openStore.onClick.AddListener(StoreSetting);
    }
    void RogerSetting()
    {
        if (!isRogerOpen)
        {
            rogerPanel.SetActive(false);
            Debug.Log("무전기가 비활성화 상태입니다.");
        }
        else
        {
            isRogerOpen = true;
            rogerPanel.SetActive(true);
            Debug.Log("무전기가 활성화 상태입니다.");
        }

        if(!isStoreOpen)
        {
            storePanel.SetActive(false);
            Debug.Log("상점창이 닫혀있습니다.");
        }
        else
        {
            isStoreOpen = true;
            Debug.Log("상점창이 열려있습니다.");
        }

        if (Input.GetKeyDown(KeyCode.E) && !isRogerOpen)
        {
            isRogerOpen = true;
        }
        else if (Input.GetKeyDown(KeyCode.E) && isRogerOpen)
        {
            isRogerOpen = false;
        }

    }
    void StoreSetting()
    {
        storePanel.SetActive(true);
    }
}
