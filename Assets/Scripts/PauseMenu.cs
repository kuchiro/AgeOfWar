using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public static bool isPaused = false;
    public GameObject pauseMenuUI;

    public TextMeshProUGUI victoryText;

    public GameObject buttonUIPause;


    public GameObject gameOverPanel;

    public bool isGameOver = false;

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }


    public void ShowGameOver(int winningTeam)
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 0f; // Met le jeu en pause
        
         if (winningTeam == 0)
        {
            victoryText.text = "Victoire du joueur";
        } else
        {
            victoryText.text = "Défaite du joueur";
        }
        

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }


    public void Resume()
    {

        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        buttonUIPause.SetActive(true);
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        buttonUIPause.SetActive(false);
    }

    public void Home()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
