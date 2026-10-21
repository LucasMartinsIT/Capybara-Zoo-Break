using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject MainMenuCanvas;
    public GameObject GameUI;

    [Header("GameUI Children Panels")]
    public GameObject PauseMenuPanel;  
    public GameObject GameOverPanel;
    public GameObject countDownObject;
    public Text countDownText;

    [Header("Text Elements")]
    public Text coinText;
    public Text scoreText;
    public Text highScoreText;
    public Text MainMenuHighScoreText;
    public Text MainMenuTotalCoinText;

    [Header("Data Reference")]
    public ScoreAndDataManager dataManager;

    void OnEnable()
    {
        GameManager.OnGameStart += ShowGameUI;
        GameManager.OnGameOver  += HandleGameOver;
    }

    void OnDisable()
    {
        GameManager.OnGameStart -= ShowGameUI;
        GameManager.OnGameOver -= HandleGameOver;
    }

    void Start()
    {
        MainMenuCanvas.SetActive(true);
        GameUI.SetActive(false);
        PauseMenuPanel.SetActive(false);
        GameOverPanel.SetActive(false);
        UpdateUI();
    }

    void Update()
    {
        // Se o jogo estiver rodando, atualiza a interface a cada frame
        if (GameManager.Instance.gameStarted)
        {
            UpdateUI();
        }
    }

    void ShowGameUI()
    {
        MainMenuCanvas.SetActive(false);
        GameUI.SetActive(true);
    }

    public void ShowPauseMenu()
    {
        Time.timeScale = 0f;
        PauseMenuPanel.SetActive(true);
    }

    public void ResumeGame()
    {
        StartCoroutine(ResumeGameCountdown());
    }

    private IEnumerator ResumeGameCountdown()
    {
        PauseMenuPanel.SetActive(false);

        countDownObject.SetActive(true);

        int timer = 3;
        while(timer > 0)
        {
            countDownText.text = timer.ToString();

            yield return new WaitForSecondsRealtime(1f);

            timer--;
        }

        countDownObject.SetActive(false);

        Time.timeScale = 1f;
    }

    private void HandleGameOver()
    {
        GameOverPanel.SetActive(true);
    }

    void UpdateUI()
    {
        if (dataManager == null) return;

        coinText.text = dataManager.currentCoin.ToString();
        scoreText.text = dataManager.currentScore.ToString();
        highScoreText.text = dataManager.highScore.ToString();

        MainMenuHighScoreText.text = dataManager.highScore.ToString();
        MainMenuTotalCoinText.text = dataManager.totalCoins.ToString();
    }

}