using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class SceneField
{
    [SerializeField] private UnityEngine.Object sceneAsset;
    [SerializeField] private string sceneName = "";

    public string SceneName => sceneName;

    public static implicit operator string(SceneField sceneField)
    {
        return sceneField.SceneName;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SceneField))]
public class SceneFieldPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        var sceneAssetProp = property.FindPropertyRelative("sceneAsset");
        var sceneNameProp = property.FindPropertyRelative("sceneName");

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        if (sceneAssetProp != null)
        {
            EditorGUI.BeginChangeCheck();
            var value = EditorGUI.ObjectField(position, sceneAssetProp.objectReferenceValue, typeof(SceneAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                sceneAssetProp.objectReferenceValue = value;
                if (value != null)
                {
                    sceneNameProp.stringValue = ((SceneAsset)value).name;
                }
            }
        }
        EditorGUI.EndProperty();
    }
}
#endif