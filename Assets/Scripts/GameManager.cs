using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    [SerializeField] private int score = 0;
    [SerializeField] private bool gameActive = true;

    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    [SerializeField] private TMPro.TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI highScoreText;

    private int highScore;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadHighScore();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        gameActive = true;
        score = 0;
        UpdateScoreUI();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (!gameActive && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    public void AddScore(int points)
    {
        if (gameActive)
        {
            score += points;
            UpdateScoreUI();
        }
    }

    public void GameOver()
    {
        if (gameActive)
        {
            gameActive = false;

            if (score > highScore)
            {
                highScore = score;
                SaveHighScore();
            }

            ShowGameOverScreen();
        }
    }

    void ShowGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = "Score: " + score;
        }

        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + highScore;
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public bool IsGameActive()
    {
        return gameActive;
    }

    void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    void SaveHighScore()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
    }
}
