using UnityEngine;
using UnityEditor;
using System.IO;
using System;

[InitializeOnLoad]
public class PKFxFilePackManager : AssetPostprocessor
{
	static private bool	m_DllLoaded = false;

	//----------------------------------------------------------------------------

	static PKFxFilePackManager()
	{
		EditorApplication.update += Update;

		//Compatible only with 2018 and higher
		//EditorApplication.quitting += Quit;
		//
		//if (PlayerSettings.allowUnsafeCode == false)
		//{
		//	Debug.LogWarning("PopcornFX plugin rely on unsafe code to be activated. We took the permission to allow it. To revert, change your player settings.");
		//	PlayerSettings.allowUnsafeCode = true;
		//}
	}

	static bool Initialize()
	{
		if (PKFxManager.IsDllLoaded() && !Application.isPlaying)
		{
			PKFxManager.StartupPopcorn(false);
			PKFxManager.StartupPopcornFileWatcher();
			m_DllLoaded = true;

			if (!Directory.Exists("Assets" + PKFxSettings.UnityPackFxPath))
			{
				string DirName = PKFxSettings.UnityPackFxPath;
				if (DirName.StartsWith("/"))
					DirName = DirName.Substring(1);
				if (DirName.EndsWith("/"))
					DirName = DirName.Substring(DirName.Length - 1, 1);
				AssetDatabase.CreateFolder("Assets", DirName);
			}

			return true;
		}
		return false;
	}

	//----------------------------------------------------------------------------

	static void Update()
	{
		if (m_DllLoaded == false)
		{
			if (Initialize() == false)
				return;
		}
		if (EditorApplication.isPlaying == true)
			return;

		PKFxManager.LockPackWatcherChanges();

		int		totalChanges = 0;
		int		remainingChanges = 0;
		bool	unstackChangesSuccess = true;

		unstackChangesSuccess = PKFxManager.PullPackWatcherChanges(out totalChanges);
		remainingChanges = totalChanges;
		while (remainingChanges > 0 && unstackChangesSuccess)
		{
			unstackChangesSuccess = PKFxManager.PullPackWatcherChanges(out remainingChanges);

			if (EditorUtility.DisplayCancelableProgressBar(	"Baking and importing PopcornFX effects",
															"Importing \'" + PKFxManager.GetImportedAssetName() + "\' and its dependencies.",
															(float)(totalChanges - remainingChanges) / (float)totalChanges))
			{
				remainingChanges = 0;
				PKFxManager.CancelPackWatcherChanges();
			}
		}

		EditorUtility.ClearProgressBar();

		if (unstackChangesSuccess == false)
		{
			Debug.LogWarning("PackWatcher Unstack issue");
		}
		PKFxManager.UnlockPackWatcherChanges();
	}

	//----------------------------------------------------------------------------

	static void Quit()
	{
		EditorApplication.update -= Update;
		//EditorApplication.quitting -= Quit;
	}

	//----------------------------------------------------------------------------

	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		AssetDatabase.StartAssetEditing();
		foreach (string str in importedAssets)
		{
			PKFxAssetCreationUtils.NotifyAssetPostProcess(str);
		}
		AssetDatabase.StopAssetEditing();
	}
}
