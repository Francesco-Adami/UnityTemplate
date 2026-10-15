using System;
using System.IO;
using System.Threading;
using Project.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour, ISceneLoader
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private string bootstrapSceneName = "Bootstrap";

    private string _currentActiveSceneName;
    private bool _isLoading;

    public event Action OnLoadStarted;
    public event Action<float> OnProgressUpdated;
    public event Action<string> OnSceneLoaded;
    public event Action<string> OnSceneUnloaded;
    public event Action OnLoadCompleted;

    public bool IsLoading => _isLoading;
    public string CurrentActiveSceneName => !string.IsNullOrEmpty(_currentActiveSceneName) ? _currentActiveSceneName : SceneManager.GetActiveScene().name;
    public string BootstrapSceneName => bootstrapSceneName;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ServiceLocator.Register<ISceneLoader>(this);

        if (string.IsNullOrEmpty(_currentActiveSceneName))
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid())
            {
                _currentActiveSceneName = activeScene.name;
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            ServiceLocator.Unregister<ISceneLoader>();
        }
    }

    public async Awaitable<bool> SwitchSceneAsync(SceneField sceneField, bool cleanUnusedAssets = true, CancellationToken cancellationToken = default)
    {
        if (sceneField == null)
        {
            Debug.LogError("[SceneLoader] Target SceneField is null.");
            return false;
        }

        return await SwitchSceneAsync(sceneField.SceneName, cleanUnusedAssets, cancellationToken);
    }

    public async Awaitable<bool> SwitchSceneAsync(string sceneName, bool cleanUnusedAssets = true, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return false;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneLoader] Target scene name is null or empty.");
            return false;
        }

        if (IsBootstrapScene(sceneName))
        {
            Debug.LogWarning($"[SceneLoader] Cannot switch to persistent bootstrap scene '{sceneName}'.");
            return false;
        }

        if (_isLoading)
        {
            Debug.LogWarning($"[SceneLoader] Load request for '{sceneName}' ignored: another scene operation is already in progress.");
            return false;
        }

        if (!IsSceneInBuildSettings(sceneName))
        {
            Debug.LogError($"[SceneLoader] Target scene '{sceneName}' is not in Build Settings.");
            return false;
        }

        if (Mathf.Approximately(Time.timeScale, 0f))
        {
            Debug.LogWarning("[SceneLoader] Time.timeScale is currently 0. Asynchronous loading will continue, but animated transitions or time-dependent logic might be paused.");
        }

        _isLoading = true;
        OnLoadStarted?.Invoke();
        OnProgressUpdated?.Invoke(0f);

        try
        {
            // 1. Unload previous active scene if valid and not bootstrap
            string sceneToUnload = !string.IsNullOrEmpty(_currentActiveSceneName) ? _currentActiveSceneName : SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(sceneToUnload) && !IsBootstrapScene(sceneToUnload) && IsSceneLoaded(sceneToUnload))
            {
                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneToUnload);
                if (unloadOp != null)
                {
                    await Awaitable.FromAsyncOperation(unloadOp, cancellationToken);
                    OnSceneUnloaded?.Invoke(sceneToUnload);
                }
            }

            // 2. Memory cleanup
            if (cleanUnusedAssets)
            {
                AsyncOperation cleanOp = Resources.UnloadUnusedAssets();
                if (cleanOp != null)
                {
                    await Awaitable.FromAsyncOperation(cleanOp, cancellationToken);
                }
            }

            // 3. Load target scene additively
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (loadOp == null)
            {
                Debug.LogError($"[SceneLoader] Failed to start loading scene '{sceneName}'.");
                return false;
            }

            while (!loadOp.isDone)
            {
                cancellationToken.ThrowIfCancellationRequested();
                OnProgressUpdated?.Invoke(loadOp.progress);
                await Awaitable.NextFrameAsync(cancellationToken);
            }
            OnProgressUpdated?.Invoke(1f);

            // 4. Mark newly loaded scene as active
            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            if (loadedScene.IsValid())
            {
                SceneManager.SetActiveScene(loadedScene);
                _currentActiveSceneName = loadedScene.name;
            }
            else
            {
                _currentActiveSceneName = sceneName;
            }

            OnSceneLoaded?.Invoke(_currentActiveSceneName);
            return true;
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning($"[SceneLoader] Switching to scene '{sceneName}' was cancelled.");
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return false;
        }
        finally
        {
            _isLoading = false;
            OnLoadCompleted?.Invoke();
        }
    }

    public async Awaitable<bool> LoadAdditiveAsync(SceneField sceneField, bool setActive = false, CancellationToken cancellationToken = default)
    {
        if (sceneField == null)
        {
            Debug.LogError("[SceneLoader] Target SceneField is null.");
            return false;
        }

        return await LoadAdditiveAsync(sceneField.SceneName, setActive, cancellationToken);
    }

    public async Awaitable<bool> LoadAdditiveAsync(string sceneName, bool setActive = false, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return false;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneLoader] Scene name is null or empty.");
            return false;
        }

        if (IsSceneLoaded(sceneName))
        {
            Debug.LogWarning($"[SceneLoader] Scene '{sceneName}' is already loaded.");
            return true;
        }

        if (_isLoading)
        {
            Debug.LogWarning($"[SceneLoader] LoadAdditiveAsync for '{sceneName}' ignored: another scene operation is already in progress.");
            return false;
        }

        if (!IsSceneInBuildSettings(sceneName))
        {
            Debug.LogError($"[SceneLoader] Scene '{sceneName}' is not in Build Settings.");
            return false;
        }

        if (Mathf.Approximately(Time.timeScale, 0f))
        {
            Debug.LogWarning("[SceneLoader] Time.timeScale is currently 0. Asynchronous loading will continue, but animated transitions or time-dependent logic might be paused.");
        }

        _isLoading = true;
        OnLoadStarted?.Invoke();
        OnProgressUpdated?.Invoke(0f);

        try
        {
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (loadOp == null)
            {
                Debug.LogError($"[SceneLoader] Failed to start loading scene '{sceneName}'.");
                return false;
            }

            while (!loadOp.isDone)
            {
                cancellationToken.ThrowIfCancellationRequested();
                OnProgressUpdated?.Invoke(loadOp.progress);
                await Awaitable.NextFrameAsync(cancellationToken);
            }
            OnProgressUpdated?.Invoke(1f);

            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            if (setActive && loadedScene.IsValid())
            {
                SceneManager.SetActiveScene(loadedScene);
                _currentActiveSceneName = loadedScene.name;
            }

            OnSceneLoaded?.Invoke(sceneName);
            return true;
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning($"[SceneLoader] Loading scene '{sceneName}' was cancelled.");
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return false;
        }
        finally
        {
            _isLoading = false;
            OnLoadCompleted?.Invoke();
        }
    }

    public async Awaitable<bool> UnloadSceneAsync(SceneField sceneField, bool cleanUnusedAssets = true, CancellationToken cancellationToken = default)
    {
        if (sceneField == null)
        {
            Debug.LogError("[SceneLoader] Target SceneField is null.");
            return false;
        }

        return await UnloadSceneAsync(sceneField.SceneName, cleanUnusedAssets, cancellationToken);
    }

    public async Awaitable<bool> UnloadSceneAsync(string sceneName, bool cleanUnusedAssets = true, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return false;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneLoader] Scene name is null or empty.");
            return false;
        }

        if (IsBootstrapScene(sceneName))
        {
            Debug.LogWarning($"[SceneLoader] Cannot unload persistent bootstrap scene '{sceneName}'.");
            return false;
        }

        if (!IsSceneLoaded(sceneName))
        {
            Debug.LogWarning($"[SceneLoader] Scene '{sceneName}' is not loaded.");
            return false;
        }

        if (_isLoading)
        {
            Debug.LogWarning($"[SceneLoader] UnloadSceneAsync for '{sceneName}' ignored: another scene operation is already in progress.");
            return false;
        }

        _isLoading = true;
        OnLoadStarted?.Invoke();
        OnProgressUpdated?.Invoke(0f);

        try
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneName);
            if (unloadOp == null)
            {
                Debug.LogError($"[SceneLoader] Failed to unload scene '{sceneName}'.");
                return false;
            }

            while (!unloadOp.isDone)
            {
                cancellationToken.ThrowIfCancellationRequested();
                OnProgressUpdated?.Invoke(unloadOp.progress);
                await Awaitable.NextFrameAsync(cancellationToken);
            }
            OnProgressUpdated?.Invoke(1f);

            if (string.Equals(_currentActiveSceneName, sceneName, StringComparison.OrdinalIgnoreCase))
            {
                Scene active = SceneManager.GetActiveScene();
                _currentActiveSceneName = active.IsValid() ? active.name : string.Empty;
            }

            OnSceneUnloaded?.Invoke(sceneName);

            if (cleanUnusedAssets)
            {
                AsyncOperation cleanOp = Resources.UnloadUnusedAssets();
                if (cleanOp != null)
                {
                    await Awaitable.FromAsyncOperation(cleanOp, cancellationToken);
                }
            }

            return true;
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning($"[SceneLoader] Unloading scene '{sceneName}' was cancelled.");
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return false;
        }
        finally
        {
            _isLoading = false;
            OnLoadCompleted?.Invoke();
        }
    }

    public bool IsSceneLoaded(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return false;

        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid() && scene.isLoaded)
            return true;

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && (string.Equals(s.name, sceneName, StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(s.path, sceneName, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsSceneLoaded(SceneField sceneField)
    {
        return sceneField != null && IsSceneLoaded(sceneField.SceneName);
    }

    // Inspector Helpers (Void Fire-and-Forget)
    public void SwitchScene(string sceneName) => _ = SwitchSceneAsync(sceneName);
    public void SwitchScene(SceneField sceneField) => _ = SwitchSceneAsync(sceneField);
    public void LoadAdditive(string sceneName) => _ = LoadAdditiveAsync(sceneName);
    public void LoadAdditive(SceneField sceneField) => _ = LoadAdditiveAsync(sceneField);
    public void UnloadScene(string sceneName) => _ = UnloadSceneAsync(sceneName);
    public void UnloadScene(SceneField sceneField) => _ = UnloadSceneAsync(sceneField);

    [Obsolete("Use SwitchScene or SwitchSceneAsync instead.")]
    public void LoadScene(string sceneName) => SwitchScene(sceneName);

    [Obsolete("Use SwitchScene or SwitchSceneAsync instead.")]
    public void LoadScene(SceneField targetScene) => SwitchScene(targetScene);

    private bool IsBootstrapScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return false;

        if (!string.IsNullOrEmpty(bootstrapSceneName) &&
            string.Equals(sceneName, bootstrapSceneName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (SceneManager.sceneCountInBuildSettings > 0)
        {
            string buildZeroPath = SceneUtility.GetScenePathByBuildIndex(0);
            string buildZeroName = Path.GetFileNameWithoutExtension(buildZeroPath);
            if (string.Equals(sceneName, buildZeroName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(sceneName, buildZeroPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsSceneInBuildSettings(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return false;

        if (Application.CanStreamedLevelBeLoaded(sceneName))
            return true;

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string name = Path.GetFileNameWithoutExtension(scenePath);
            if (string.Equals(name, sceneName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(scenePath, sceneName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}