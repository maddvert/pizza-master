using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

// Open the project and use Food Street > Create Playable Scene once.
public static class CreateScene
{
    [MenuItem("Food Street/Create Playable Scene")]
    public static void Create()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("Food Street Game").AddComponent<FoodStreetGame>();
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/FoodStreet.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/FoodStreet.unity", true) };
        PlayerSettings.productName = "Food Street Prototype";
        PlayerSettings.companyName = "maddvert";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.maddvert.foodstreetprototype");
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.maddvert.foodstreetprototype");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        EditorSceneManager.OpenScene("Assets/Scenes/FoodStreet.unity");
    }
}
