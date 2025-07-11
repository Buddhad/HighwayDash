using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject pausePanel;

    private bool isPaused = false;

    void Start()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame); // ✅ This was missing

        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // important if game was paused
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
        Debug.Log("ResumeGame Called"); // ← Add this to test
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }
}
