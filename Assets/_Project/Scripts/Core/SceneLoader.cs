using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Handles all scene loading with smooth transitions and asynchronous loading.
/// Singleton pattern ensures only one instance exists.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    // ----- SINGLETON -----
    private static SceneLoader instance;
    public static SceneLoader Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<SceneLoader>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("SceneLoader");
                    instance = obj.AddComponent<SceneLoader>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }

    // ----- PROPERTIES -----
    [Header("Transition Settings")]
    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private float transitionTime = 1f;
    [SerializeField] private string loadingSceneName = "Loading";

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // Private variables
    private bool isLoading = false;
    private string currentSceneName = "";
    private string targetSceneName = "";

    // Events
    public System.Action<string> OnSceneLoadStarted;
    public System.Action<string> OnSceneLoadComplete;
    public System.Action<float> OnSceneLoadProgress;

    // ----- UNITY LIFECYCLE -----
    private void Awake()
    {
        // Singleton enforcement
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Store current scene name
        currentSceneName = SceneManager.GetActiveScene().name;

        if (showDebugLogs)
            Debug.Log($"SceneLoader initialized. Current scene: {currentSceneName}");
    }

    private void OnEnable()
    {
        // Subscribe to scene load events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from scene load events
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ----- PUBLIC METHODS -----

    /// <summary>
    /// Load a scene by name with transition
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (isLoading)
        {
            Debug.LogWarning($"Already loading a scene. Cannot load {sceneName}");
            return;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Cannot load scene: Scene name is null or empty");
            return;
        }

        if (showDebugLogs)
            Debug.Log($"Loading scene: {sceneName}");

        StartCoroutine(LoadSceneAsync(sceneName));
    }

    /// <summary>
    /// Load a scene immediately without transition
    /// </summary>
    public void LoadSceneDirect(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Cannot load scene: Scene name is null or empty");
            return;
        }

        if (showDebugLogs)
            Debug.Log($"Loading scene directly: {sceneName}");

        SceneManager.LoadScene(sceneName);
        currentSceneName = sceneName;
        OnSceneLoadComplete?.Invoke(sceneName);
    }

    /// <summary>
    /// Load a scene by build index with transition
    /// </summary>
    public void LoadScene(int sceneIndex)
    {
        string sceneName = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"No scene found at build index {sceneIndex}");
            return;
        }
        LoadScene(sceneName);
    }

    /// <summary>
    /// Reload the currently active scene
    /// </summary>
    public void ReloadCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (showDebugLogs)
            Debug.Log($"Reloading scene: {currentScene}");

        LoadScene(currentScene);
    }

    /// <summary>
    /// Load the next scene in build order
    /// </summary>
    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;
        int totalScenes = SceneManager.sceneCountInBuildSettings;

        if (nextIndex >= totalScenes)
        {
            Debug.Log("No more scenes to load. You've reached the last scene!");
            return;
        }

        LoadScene(nextIndex);
    }

    /// <summary>
    /// Load the previous scene in build order
    /// </summary>
    public void LoadPreviousScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int previousIndex = currentIndex - 1;

        if (previousIndex < 0)
        {
            Debug.Log("No previous scene to load. You're at the first scene!");
            return;
        }

        LoadScene(previousIndex);
    }

    /// <summary>
    /// Quit the game
    /// </summary>
    public void QuitGame()
    {
        if (showDebugLogs)
            Debug.Log("Quitting game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Check if a scene exists in the build
    /// </summary>
    public bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Get the name of the current scene
    /// </summary>
    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    /// <summary>
    /// Get the build index of the current scene
    /// </summary>
    public int GetCurrentSceneIndex()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }

    /// <summary>
    /// Check if a scene is currently being loaded
    /// </summary>
    public bool IsLoading()
    {
        return isLoading;
    }

    // ----- PRIVATE METHODS -----

    /// <summary>
    /// Coroutine that handles async scene loading with transition
    /// </summary>
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        isLoading = true;
        targetSceneName = sceneName;

        // Trigger start event
        OnSceneLoadStarted?.Invoke(sceneName);

        // ---- PHASE 1: FADE OUT ----
        if (transitionAnimator != null)
        {
            transitionAnimator.SetTrigger("FadeOut");
            yield return new WaitForSeconds(transitionTime);
        }

        // ---- PHASE 2: LOAD SCENE ----
        // Check if we need to show a loading scene
        bool showLoadingScene = !string.IsNullOrEmpty(loadingSceneName) &&
                                SceneExists(loadingSceneName) &&
                                sceneName != loadingSceneName;

        if (showLoadingScene)
        {
            // Load loading scene additively
            AsyncOperation loadLoadingOp = SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);
            while (!loadLoadingOp.isDone)
            {
                yield return null;
            }

            // Start async load of target scene
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);

            // Check if asyncOperation is null (scene might not exist)
            if (asyncOperation == null)
            {
                Debug.LogError($"Failed to load scene: {sceneName}. Make sure it's in Build Settings.");
                isLoading = false;
                yield break;
            }

            asyncOperation.allowSceneActivation = false;

            // While loading, update progress
            while (asyncOperation.progress < 0.9f)
            {
                // Only call if there are subscribers
                if (OnSceneLoadProgress != null)
                {
                    OnSceneLoadProgress?.Invoke(asyncOperation.progress);
                }
                yield return null;
            }

            // Wait for any additional time
            yield return new WaitForSeconds(0.5f);

            // Allow scene activation
            asyncOperation.allowSceneActivation = true;

            // Wait for scene to fully load
            while (!asyncOperation.isDone)
            {
                yield return null;
            }

            // Unload loading scene - add null check
            if (!string.IsNullOrEmpty(loadingSceneName))
            {
                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(loadingSceneName);
                if (unloadOp != null)
                {
                    while (!unloadOp.isDone)
                    {
                        yield return null;
                    }
                }
            }
        }
        else
        {
            // Direct async load without loading scene
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);

            // Check if asyncOperation is null
            if (asyncOperation == null)
            {
                Debug.LogError($"Failed to load scene: {sceneName}. Make sure it's in Build Settings.");
                isLoading = false;
                yield break;
            }

            // Wait for load to complete
            while (!asyncOperation.isDone)
            {
                // Only call if there are subscribers
                if (OnSceneLoadProgress != null)
                {
                    OnSceneLoadProgress?.Invoke(asyncOperation.progress);
                }
                yield return null;
            }
        }

        // ---- PHASE 3: FADE IN ----
        if (transitionAnimator != null)
        {
            transitionAnimator.SetTrigger("FadeIn");
            yield return new WaitForSeconds(transitionTime);
        }

        // ---- COMPLETE ----
        currentSceneName = sceneName;
        isLoading = false;

        // Trigger completion event - null check added
        OnSceneLoadComplete?.Invoke(sceneName);

        if (showDebugLogs)
            Debug.Log($"Scene loaded successfully: {sceneName}");
    }

    /// <summary>
    /// Called when a scene has been loaded
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;

        if (showDebugLogs)
            Debug.Log($"Scene loaded event: {scene.name} (mode: {mode})");
    }

    // ----- DEBUG METHODS -----

    private void OnValidate()
    {
        // Check if transition animator is assigned
        if (transitionAnimator == null)
        {
            // This is fine - transitions are optional
        }
    }

    private void OnDestroy()
    {
        // Clean up events
        OnSceneLoadStarted = null;
        OnSceneLoadComplete = null;
        OnSceneLoadProgress = null;
    }
}