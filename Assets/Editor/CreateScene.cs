using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

// Open the project and use Pizza Master > Create Playable Scene once.
public static class CreateScene
{
    [MenuItem("Pizza Master/Create Playable Scene")]
    public static void Create()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("Pizza Master Game").AddComponent<PizzaMasterGame>();
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/PizzaMaster.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/PizzaMaster.unity", true) };
        PlayerSettings.productName = "Pizza Master";
        PlayerSettings.companyName = "maddvert";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.maddvert.pizzamaster");
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.maddvert.pizzamaster");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        EditorSceneManager.OpenScene("Assets/Scenes/PizzaMaster.unity");
    }
}
