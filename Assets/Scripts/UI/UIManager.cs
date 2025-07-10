using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject pausePanel;

    private bool isPaused = false;

    void Start()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    public void RestartGame()
    {
        GameManager.Instance.RestartGame();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            if (pausePanel != null)
                pausePanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }
}
