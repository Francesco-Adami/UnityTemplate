using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private string _currentActiveSceneName;
    private bool _isLoading;

    public event Action OnLoadStarted;
    public event Action<float> OnProgressUpdated;
    public event Action OnLoadCompleted;

    public bool IsLoading => _isLoading;
    public string CurrentActiveSceneName => _currentActiveSceneName;

    public void LoadScene(SceneField targetScene)
    {
        if (targetScene == null)
        {
            Debug.LogError("[SceneLoader] Target SceneData is null.");
            return;
        }

        LoadScene(targetScene.SceneName);
    }

    public void LoadScene(string sceneName)
    {
        if (_isLoading)
        {
            Debug.LogWarning($"[SceneLoader] Load request for '{sceneName}' ignored: another scene is currently loading.");
            return;
        }

        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string targetSceneName)
    {
        _isLoading = true;
        OnLoadStarted?.Invoke();

        // 1. Unload the previous active scene if one exists
        if (!string.IsNullOrEmpty(_currentActiveSceneName))
        {
            Scene activeScene = SceneManager.GetSceneByName(_currentActiveSceneName);
            if (activeScene.isLoaded)
            {
                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(_currentActiveSceneName);
                while (unloadOp != null && !unloadOp.isDone)
                {
                    yield return null;
                }
            }
        }

        // 2. Load the target scene additively
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);

        if (loadOp == null)
        {
            Debug.LogError($"[SceneLoader] Failed to start loading scene '{targetSceneName}'. Verify build settings.");
            _isLoading = false;
            yield break;
        }

        while (!loadOp.isDone)
        {
            OnProgressUpdated?.Invoke(loadOp.progress);
            yield return null;
        }

        // 3. Mark the newly loaded scene as active
        Scene loadedScene = SceneManager.GetSceneByName(targetSceneName);
        if (loadedScene.IsValid())
        {
            SceneManager.SetActiveScene(loadedScene);
            _currentActiveSceneName = targetSceneName;
        }

        _isLoading = false;
        OnLoadCompleted?.Invoke();
    }
}