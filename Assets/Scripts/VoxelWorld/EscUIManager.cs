using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EscUIManager : MonoBehaviour
{
    public Button continueButton;
    public Button exitToHomeButton;

    public GameObject escPanel;
    public bool isEsc;

    void Start()
    {
        escPanel.SetActive(false);

        continueButton.onClick.AddListener(ContinueButton);
        exitToHomeButton.onClick.AddListener(ExitToHomeButton);
    }

    void Update()
    {
        if (isEsc)
        {
            Time.timeScale = 0f;

            escPanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;

            escPanel.SetActive(false);
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            isEsc = true;
        }

    }
    public void ContinueButton()
    {
        isEsc = false;
    }
    public void ExitToHomeButton()
    {
        isEsc = false;
        SceneManager.LoadScene("TitleScene");
    }
    
}
