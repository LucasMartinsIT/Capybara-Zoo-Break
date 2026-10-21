using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public static event Action OnGameStart;
    public static event Action OnGameOver;

    public bool gameStarted = false;
    private bool gameOver = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (!gameStarted)
        {
            // Check touch input first (works on mobile builds AND Unity Device Simulator)
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                {
                    StartGame();
                    return;
                }
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            // Fallback for regular mouse clicks in the Game View
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
                {
                    StartGame();
                }
            }
#endif
        }
    }

    public void StartGame()
    {
        if (gameStarted) return;
        gameStarted = true;
        gameOver = false;
        OnGameStart?.Invoke();
    }

    public void Die()
    {
        if (gameOver) return;
        gameOver = true;
        gameStarted = false;
        OnGameOver?.Invoke();
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}