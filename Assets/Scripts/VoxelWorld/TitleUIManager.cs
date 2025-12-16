using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleUIManager : MonoBehaviour
{
    public Button startButton;
    public Button exitButton;


    void Update()
    {
        startButton.onClick.AddListener(StartButton);
        exitButton.onClick.AddListener(ExitButton);
    }
    public void StartButton()
    {
        SceneManager.LoadScene("TestSceneDev");
    }
    public void ExitButton()
    {
        Application.Quit();
    }
}
