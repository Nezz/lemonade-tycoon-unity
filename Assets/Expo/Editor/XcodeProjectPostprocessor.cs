using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace Yousician.Expo.Editor
{
	/// <summary>
	/// Post-processes the Xcode project after an iOS build to configure native interop:
	/// - Moves the Data folder to the UnityFramework target
	/// - Adds required framework references
	/// - Exposes native headers (.h files) as public headers on UnityFramework
	/// - Sets the data bundle ID in main.mm
	/// - Disables the keyboard delegate in UnityAppController.mm
	/// </summary>
	public class XcodeProjectPostprocessor : IPostprocessBuildWithReport
	{
		private const string NativeHeadersPath = "Assets/Expo";

		public int callbackOrder => 0;

		public void OnPostprocessBuild(BuildReport report)
		{
			if (report.summary.platform == BuildTarget.iOS)
			{
				OnPostprocessBuild(report.summary.outputPath);
			}
		}

#if UNITY_IOS
		private static void OnPostprocessBuild(string path)
		{
			var pbxProjectPath = PBXProject.GetPBXProjectPath(path);
			var pbxProject = new PBXProject();
			pbxProject.ReadFromString(File.ReadAllText(pbxProjectPath));

			var unityFrameworkTargetGuid = pbxProject.GetUnityFrameworkTargetGuid();
			var unityMainTargetGuid = pbxProject.GetUnityMainTargetGuid();

			// Move Data folder target membership from Unity-iPhone to UnityFramework
			var dataGuid = pbxProject.FindFileGuidByProjectPath("Data");
			if (dataGuid != null)
			{
				pbxProject.RemoveFileFromBuild(unityMainTargetGuid, dataGuid);
				pbxProject.AddFileToBuild(unityFrameworkTargetGuid, dataGuid);
				Debug.Log("[XcodePostprocessor] Moved Data folder to UnityFramework target");
			}
			else
			{
				Debug.LogWarning("[XcodePostprocessor] Could not find Data folder in Xcode project");
			}

			// Expose native headers as public headers on the UnityFramework target
			var nativeHeaders = Directory.GetFiles(NativeHeadersPath, "*.h");
			foreach (var header in nativeHeaders)
			{
				// Unity copies Assets/ files into Libraries/ in the Xcode project
				var projectPath = $"Libraries/{header.Substring(header.IndexOf("Assets/", StringComparison.Ordinal) + "Assets/".Length)}";
				var fileGuid = pbxProject.FindFileGuidByProjectPath(projectPath);

				if (fileGuid != null)
				{
					pbxProject.AddPublicHeaderToBuild(unityFrameworkTargetGuid, fileGuid);
					Debug.Log($"[XcodePostprocessor] Added public header: {projectPath}");
				}
				else
				{
					Debug.LogWarning($"[XcodePostprocessor] Could not find header in Xcode project: {projectPath}");
				}
			}

			pbxProject.WriteToFile(pbxProjectPath);

			UpdateMain(path);
			DisableKeyboardDelegate(path);

			Debug.Log("[XcodePostprocessor] Xcode project post-processing complete");
		}

		private static void UpdateMain(string path)
		{
			// Point the Unity-iPhone target to the correct Data bundle location inside UnityFramework
			const string bundleString = "[ufw setDataBundleId: \"com.unity3d.framework\"];";
			var mainPath = Path.Combine(path, "MainApp", "main.mm");

			if (!File.Exists(mainPath))
			{
				Debug.LogWarning($"[XcodePostprocessor] main.mm not found at {mainPath}");
				return;
			}

			var contents = File.ReadAllText(mainPath);

			if (contents.Contains(bundleString))
			{
				return;
			}

			const string insertAfter = "UnityFrameworkLoad();";
			var index = contents.IndexOf(insertAfter, StringComparison.Ordinal);
			if (index < 0)
			{
				throw new BuildFailedException($"[XcodePostprocessor] Couldn't find '{insertAfter}' in main.mm");
			}

			index += insertAfter.Length + 1;
			contents = contents.Insert(index, $"\t\t{bundleString}\n");
			File.WriteAllText(mainPath, contents);

			Debug.Log("[XcodePostprocessor] Added bundle ID setting to main.mm");
		}

		private static void DisableKeyboardDelegate(string path)
		{
			const string fileName = "UnityAppController.mm";
			var filePath = Path.Combine(path, "Classes", fileName);

			if (!File.Exists(filePath))
			{
				throw new BuildFailedException($"[XcodePostprocessor] {fileName} not found at {filePath}");
			}

			const string keyboardDelegateInit = "[KeyboardDelegate Initialize]";
			var contents = File.ReadAllText(filePath);

			if (!contents.Contains(keyboardDelegateInit))
			{
				throw new BuildFailedException(
					$"[XcodePostprocessor] Couldn't find '{keyboardDelegateInit}' in {fileName}");
			}

			contents = contents.Replace(keyboardDelegateInit, $"//{keyboardDelegateInit}");
			File.WriteAllText(filePath, contents);

			Debug.Log($"[XcodePostprocessor] Commented out {keyboardDelegateInit} in {fileName}");
		}
#else
		private static void OnPostprocessBuild(string path)
		{
			// No-op on non-iOS platforms; the entry point already gates on BuildTarget.iOS
		}
#endif
	}
}
