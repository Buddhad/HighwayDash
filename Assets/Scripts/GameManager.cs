using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;  

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    [SerializeField] private int score = 0;
    [SerializeField] private bool gameActive = true;
    
    [Header("Score Settings")]
    [SerializeField] private float scoreMultiplier = 2f; // Multiplier when boost is active

    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    [SerializeField] private TMPro.TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI highScoreText;

    private int highScore;
    private PlayerController playerController;

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

        // Find the player controller
        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("GameManager: PlayerController not found!");
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
    }

    void Update()
    {

    }

    public void AddScore(int basePoints)
    {
        if (gameActive)
        {
            int pointsToAdd = basePoints;
            
            // Check if score multiplier is active
            if (playerController != null && playerController.IsScoreBoosted())
            {
                pointsToAdd = Mathf.RoundToInt(basePoints * scoreMultiplier);
                Debug.Log($"Score Multiplier Active! {basePoints} x {scoreMultiplier} = {pointsToAdd}");
            }
            
            score += pointsToAdd;
            Debug.Log($"Score: {score} (Added: {pointsToAdd})");
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
        Time.timeScale = 1f; // <-- important
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