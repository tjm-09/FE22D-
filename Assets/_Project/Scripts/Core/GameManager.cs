using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    // Singleton Pattern
    private static GameManager instance;
    /* static modifyer
        means the thing belongs to the class itself and no
        individual object 
        */
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<GameManager>();
                if (instance == null)
                {
                    GameObject gameManagerObject = new GameObject("GameManager");
                    instance = gameManagerObject.AddComponent<GameManager>();
                    DontDestroyOnLoad(gameManagerObject);
                }
            }
            return instance;
        }
    }

    // Game States
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        LevelComplete,
        Loading
    }

    // Feilds
    private GameState currentState = GameState.MainMenu;
    public GameState CurrentState
    {
        get { return currentState; }
        private set { currentState = value; }
    }

    public int CurrentLevel { get; private set; } = 1;
    public float TotalPlayTime { get; private set; } = 0f;
    public bool IsGameStarted { get; private set; } = false;

    // Events
    public event Action<GameState> OnStateChanged;
    public event Action<int> OnLevelChanged;

    void Awake()
    {
        // Singleton enforcement
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

    }

    void Update() //update is called once per frame
    {
        // Track total play time
        if (IsGameStarted && CurrentState == GameState.Playing)
        {
            TotalPlayTime += Time.deltaTime;
        }

        // Check for pause input
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // ----- PUBLIC METHODS -----

    public void StartGame()
    {
        CurrentState = GameState.Playing;
        IsGameStarted = true;
        TotalPlayTime = 0f;
        OnStateChanged?.Invoke(CurrentState);
        Debug.Log("Game Started!");
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
        {
            CurrentState = GameState.Paused;
            Time.timeScale = 0f;
            OnStateChanged?.Invoke(CurrentState);
            Debug.Log("Game Paused");
        }
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            CurrentState = GameState.Playing;
            Time.timeScale = 1f;
            OnStateChanged?.Invoke(CurrentState);
            Debug.Log("Game Resumed");
        }
    }

    public void TogglePause()
    {
        if (CurrentState == GameState.Playing)
        {
            PauseGame();
        }
        else if (CurrentState == GameState.Paused)
        {
            ResumeGame();
        }
    }

    public void GameOver()
    {
        CurrentState = GameState.GameOver;
        Time.timeScale = 0f;
        OnStateChanged?.Invoke(CurrentState);
        Debug.Log("Game Over!");
    }

    public void LevelComplete()
    {
        CurrentState = GameState.LevelComplete;
        Time.timeScale = 0f;
        OnStateChanged?.Invoke(CurrentState);
        Debug.Log($"Level {CurrentLevel} Complete!");
    }

    public void LoadNextLevel()
    {
        CurrentLevel++;
        OnLevelChanged?.Invoke(CurrentLevel);
        StartGame();
        Debug.Log($"Loading Level {CurrentLevel}");
    }

    public void LoadLevel(int levelId)
    {
        CurrentLevel = levelId;
        OnLevelChanged?.Invoke(CurrentLevel);
        StartGame();
        Debug.Log($"Loading Level {CurrentLevel}");
        SceneLoader.Instance.LoadScene($"Level{CurrentLevel}");
    }

    public void ReturnToMainMenu()
    {
        CurrentState = GameState.MainMenu;
        IsGameStarted = false;
        Time.timeScale = 1f;
        OnStateChanged?.Invoke(CurrentState);
        Debug.Log("Returned to Main Menu");
        SceneLoader.Instance.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public bool IsPlaying()
    {
        return CurrentState == GameState.Playing;
    }

    public bool IsPaused()
    {
        return CurrentState == GameState.Paused;
    }

    public string GetStateName()
    {
        return CurrentState.ToString();
    }

    // ----- PRIVATE HELPERS -----

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && IsPlaying())
        {
            PauseGame();
        }
    }

    private void OnDestroy()
    {
        // Clean up singleton
        if (instance == this)
        {
            instance = null;
        }
    }
}
