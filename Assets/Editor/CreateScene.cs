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
        // Keep a real shader reference in a Resources material: Android builds can strip
        // shaders that are referenced only by Shader.Find at runtime.
        System.IO.Directory.CreateDirectory("Assets/Resources");
        const string materialPath = "Assets/Resources/PizzaMasterBase.mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(materialPath) == null)
        {
            var shader = Shader.Find("Standard");
            if (shader == null) throw new System.Exception("Unity Standard shader is unavailable");
            AssetDatabase.CreateAsset(new Material(shader), materialPath);
            AssetDatabase.SaveAssets();
        }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 12;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.7f, .86f, .9f);
        cameraObject.transform.position = new Vector3(0, 17, -12);
        cameraObject.transform.rotation = Quaternion.Euler(53, 0, 0);
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
