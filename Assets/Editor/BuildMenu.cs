using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class BuildMenu
{
    [MenuItem("Tools/Build")]
    public static void Build()
    {
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        string outputDir = "Build_" + target;
        string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", outputDir));

        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogError("[Build] No scenes enabled in Build Settings.");
            return;
        }

        // Append to existing Xcode project when possible.
        BuildOptions options = BuildOptions.None;
        if (target == BuildTarget.iOS && Directory.Exists(fullPath))
            options = BuildOptions.AcceptExternalModificationsToPlayer;

        var report = BuildPipeline.BuildPlayer(scenes, fullPath, target, options);
    }
}
