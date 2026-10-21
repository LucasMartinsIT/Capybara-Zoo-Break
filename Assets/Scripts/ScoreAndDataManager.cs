using UnityEngine;
using System;

public class ScoreAndDataManager : MonoBehaviour
{
    [Header("Coin & Score Settings")]
    public int currentCoin = 0;
    public int totalCoins = 0;
    public float scorePerSec = 10f;
    public int currentScore;
    public int highScore = 0;
    private float scoreTimer = 0f;
    private bool isScoring = false;

    public static event Action<int> OnCoinCollected;

    void OnEnable()
    {
        GameManager.OnGameStart += StartScoring;
        GameManager.OnGameOver += GameOver;
        OnCoinCollected += AddCoins;
    }

    void OnDisable()
    {
        GameManager.OnGameStart -= StartScoring;
        GameManager.OnGameOver -= GameOver;
        OnCoinCollected -= AddCoins;
    }

    void Start()
    {
        // Carrega os dados salvos imediatamente quando o app abre
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    void Update()
    {
        if (isScoring)
        {
            scoreTimer += Time.deltaTime;
            float interval = 1f / scorePerSec;

            while (scoreTimer >= interval)
            {
                currentScore += 1;
                scoreTimer -= interval;

                if (currentScore > highScore)
                {
                    highScore = currentScore;
                }
            }
        }
    }

    public void AddCoins(int amount)
    {
        currentCoin += amount;
        totalCoins += amount;
    }

    public void StartScoring()
    {
        currentCoin = 0;
        currentScore = 0;
        scoreTimer = 0f;
        isScoring = true;
    }

    public void GameOver()
    {
        isScoring = false;
        PlayerPrefs.SetInt("LastRunCoins", currentCoin);
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
    }

    public static void TriggerCoinCollected(int amount)
    {
        OnCoinCollected?.Invoke(amount);
    }
}