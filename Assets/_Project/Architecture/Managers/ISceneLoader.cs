using System;
using System.Threading;
using UnityEngine;

public interface ISceneLoader
{
    bool IsLoading { get; }
    string CurrentActiveSceneName { get; }

    event Action OnLoadStarted;
    event Action<float> OnProgressUpdated;
    event Action<string> OnSceneLoaded;
    event Action<string> OnSceneUnloaded;
    event Action OnLoadCompleted;

    Awaitable<bool> SwitchSceneAsync(string sceneName, bool cleanUnusedAssets = true, CancellationToken cancellationToken = default);
    Awaitable<bool> SwitchSceneAsync(SceneField sceneField, bool cleanUnusedAssets = true, CancellationToken cancellationToken = default);

    Awaitable<bool> LoadAdditiveAsync(string sceneName, bool setActive = false, CancellationToken cancellationToken = default);
    Awaitable<bool> LoadAdditiveAsync(SceneField sceneField, bool setActive = false, CancellationToken cancellationToken = default);

    Awaitable<bool> UnloadSceneAsync(string sceneName, bool cleanUnusedAssets = true, CancellationToken cancellationToken = default);
    Awaitable<bool> UnloadSceneAsync(SceneField sceneField, bool cleanUnusedAssets = true, CancellationToken cancellationToken = default);

    bool IsSceneLoaded(string sceneName);
    bool IsSceneLoaded(SceneField sceneField);
}
