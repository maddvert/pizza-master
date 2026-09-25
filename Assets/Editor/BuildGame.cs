using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildGame
{
    // Unity command line: -batchmode -quit -executeMethod BuildGame.Android
    public static void Android() => Build(BuildTarget.Android, "build/FoodStreet-test.apk");
    public static void Ios() => Build(BuildTarget.iOS, "build/ios");

    static void Build(BuildTarget target, string path)
    {
        CreateScene.Create();
        PlayerSettings.SetApplicationIdentifier(
            target == BuildTarget.Android ? NamedBuildTarget.Android : NamedBuildTarget.iOS,
            "com.maddvert.foodstreetprototype");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.Android.useCustomKeystore = false;
        EditorUserBuildSettings.buildAppBundle = false;
        Directory.CreateDirectory("build");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/FoodStreet.unity" },
            locationPathName = path,
            target = target,
            options = BuildOptions.None
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception("Build failed: " + report.summary.result + " (" + report.summary.totalErrors + " errors)");
        Debug.Log("Build succeeded: " + path);
    }
}
