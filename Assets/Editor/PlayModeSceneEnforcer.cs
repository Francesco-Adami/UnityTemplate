#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PlayModeSceneEnforcer
{
    private const string BootstrapScenePath = "Assets/_Project/Scenes/Bootstrap.unity"; // Adjust path to your project

    static PlayModeSceneEnforcer()
    {
        SceneAsset bootstrapAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapScenePath);

        if (bootstrapAsset != null)
        {
            // Forces Play mode to always enter via the Bootstrap scene
            EditorSceneManager.playModeStartScene = bootstrapAsset;
        }
        else
        {
            Debug.LogError($"[PlayModeSceneEnforcer] Bootstrap scene not found at path: {BootstrapScenePath}");
        }
    }
}
#endif