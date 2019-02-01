//----------------------------------------------------------------------------
// Created on Fri Dec 27 14:47:38 2013 Raphael Thoulouze
//
// This program is the property of Persistant Studios SARL.
//
// You may not redistribute it and/or modify it under any conditions
// without written permission from Persistant Studios SARL, unless
// otherwise stated in the latest Persistant Studios Code License.
//
// See the Persistant Studios Code License for further details.
//----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PKFxMenus : MonoBehaviour
{
	private static void WarnRestart()
	{
		UnityEngine.Debug.LogWarning("[PKFX] Settings : For this setting to take effect, you'll need to re-open your project.");
	}

	public delegate bool UpdatePKFxFXComponent(PKFxFX fx);

	private static bool UpdateFX(PKFxFX fx)
	{
		bool	loadingFailed = false;

		fx.UpdateAssetPathIFN();
		if (fx.m_FxAsset == null && !string.IsNullOrEmpty(fx.m_FxName))
		{
			string parentfolderPath = "Assets" + PKFxSettings.UnityPackFxPath;
			string assetPath = parentfolderPath + "/" + fx.m_FxName;

			fx.m_FxAsset = (PKFxFxAsset)AssetDatabase.LoadAssetAtPath(assetPath, typeof(PKFxFxAsset));

			if (fx.m_FxAsset != null)
			{
				Debug.Log("Reloading effect '" + fx.m_FxName + "'");
			}
			else
			{
				loadingFailed = true;
			}
		}
		fx.UpdatePkFXComponent(fx.m_FxAsset, loadingFailed == false);
		if (loadingFailed)
		{
			fx.ClearAllAttributesAndSamplers();
		}
		return true;
	}

	public static void	UpdateFxsOnAllScenesAndPrefabs(UpdatePKFxFXComponent updateComponent)
	{
		Scene startingScene = EditorSceneManager.GetActiveScene();
		string startingScenePath = null;

		if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			return; //abort

		if (!string.IsNullOrEmpty(startingScene.path))
			startingScenePath = startingScene.path;

		string[] foundScenes = AssetDatabase.FindAssets("t:SceneAsset", null);
		string[] foundPrefabs = AssetDatabase.FindAssets("t:Prefab");

		int referenceUpdateCount = 0;
		int totalElemCount = foundScenes.Length + foundPrefabs.Length;
		int currElemCount = 0;

		foreach (var guid in foundScenes)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);

			EditorUtility.DisplayProgressBar("Update PKFxFX References", path, (float)currElemCount / (float)totalElemCount);

			EditorSceneManager.OpenScene(path);
			Object[] objs = Resources.FindObjectsOfTypeAll(typeof(PKFxFX));
			bool sceneUpdated = false;
			foreach (var obj in objs)
			{
				PKFxFX fx = obj as PKFxFX;
				if (fx != null)
				{
					if (updateComponent(fx))
					{
						referenceUpdateCount++;
						sceneUpdated = true;
					}
				}
			}
			if (sceneUpdated)
				EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
			currElemCount++;
		}
		Resources.UnloadUnusedAssets();

		//restore the starting scene
		if (startingScenePath != null)
			EditorSceneManager.OpenScene(startingScenePath);

		bool prefabsUpdated = false;
		List<string> filesToReimport = new List<string>();

		foreach (string guid in foundPrefabs)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);

			EditorUtility.DisplayProgressBar("Update PKFxFX References", path, (float)currElemCount / (float)totalElemCount);

			Object[] objs = AssetDatabase.LoadAllAssetsAtPath(path);
			foreach (var obj in objs)
			{
				PKFxFX fx = obj as PKFxFX;
				if (fx != null)
				{
					if (updateComponent(fx))
					{
						referenceUpdateCount++;
						prefabsUpdated = true;
						EditorUtility.SetDirty(fx.gameObject);
						filesToReimport.Add(path);
					}
				}
			}
			currElemCount++;
		}
		if (prefabsUpdated)
			AssetDatabase.SaveAssets();

		EditorUtility.ClearProgressBar();

		foreach (var path in filesToReimport)
		{
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
		}

		Debug.Log("[PKFX] " + referenceUpdateCount + " PKFxFX references updated.");
	}

	[MenuItem("Assets/PopcornFX/Update PKFxFX References")]
	static void UpdatePKFxFXReferences()
	{
		UpdateFxsOnAllScenesAndPrefabs(UpdateFX);
		RemoveObsoleteCode();
	}

	private static void AddObsoleteCodeDefine(BuildTargetGroup targetGroup)
	{
		string defSymbolfs = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

		if (defSymbolfs == null || !defSymbolfs.Contains("PK_REMOVE_OBSOLETE_CODE"))
		{
				if (string.IsNullOrEmpty(defSymbolfs))
					defSymbolfs = "PK_REMOVE_OBSOLETE_CODE";
				else
					defSymbolfs += ";PK_REMOVE_OBSOLETE_CODE";
		}
		PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defSymbolfs);
	}

	private static void RemoveObsoleteCodeDefine(BuildTargetGroup targetGroup)
	{
		string defSymbolfs = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
		if (defSymbolfs.Contains(";PK_UNSAFE_CODE_ENABLED"))
			defSymbolfs = defSymbolfs.Replace(";PK_UNSAFE_CODE_ENABLED", "");
		else if (defSymbolfs.Contains("PK_UNSAFE_CODE_ENABLED;"))
			defSymbolfs = defSymbolfs.Replace("PK_UNSAFE_CODE_ENABLED;", "");
		else if (defSymbolfs.Contains("PK_UNSAFE_CODE_ENABLED"))
			defSymbolfs = defSymbolfs.Replace("PK_UNSAFE_CODE_ENABLED", "");
		PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defSymbolfs);
	}

	[MenuItem("Assets/PopcornFX/Remove Obsolete Code")]
	static void RemoveObsoleteCode()
	{
		AddObsoleteCodeDefine(BuildTargetGroup.Standalone);
		AddObsoleteCodeDefine(BuildTargetGroup.iOS);
		AddObsoleteCodeDefine(BuildTargetGroup.Android);
		AddObsoleteCodeDefine(BuildTargetGroup.PS4);
		AddObsoleteCodeDefine(BuildTargetGroup.XboxOne);
		AddObsoleteCodeDefine(BuildTargetGroup.Switch);
	}

	[MenuItem("Assets/PopcornFX/Import Effect Pack")]
	static void ImportEffectPack()
	{
		PKFxSettings.GetAllAssetPath();
		ImportPKFxListEditor window = ScriptableObject.CreateInstance(typeof(ImportPKFxListEditor)) as ImportPKFxListEditor;
		window.Paths = PKFxSettings.AssetPathList;
		window.ShowUtility();
	}

	//============ GLOBAL SETTINGS MOVED TO PKFxSettings.cs =========

	//========== CUSTOM GAMEOBJECTS CREATION ==============================
	//
	//
	//			
	//
	//
	//=====================================================================

	[MenuItem("GameObject/Create PopcornFX/Effect", false, 10)]
	static void CreateEmptyFX(MenuCommand menuCommand) {
		// Create a custom game object
		GameObject go = new GameObject("Fx");
		go.AddComponent<PKFxFX>();
		// TODO : redo this with new resource loader
		//PKFxFX fx = go.AddComponent<PKFxFX>();
		//string[] pkfxs = Directory.GetFiles("Assets/StreamingAssets/PackFx/", "*.pkfx", SearchOption.AllDirectories);
		//if (pkfxs.Length > 0)
		//{
		//	fx.m_FxName = pkfxs[0].Substring("Assets/StreamingAssets/PackFx/".Length);
		//	UnityEngine.Debug.Log(fx.m_FxName);
		//}
		// Ensure it gets reparented if this was a context click (otherwise does 
		GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
		// Register the creation in the undo system
		Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
		Selection.activeObject = go;
	}

	//----------------------------------------------------------------------------

	[MenuItem("GameObject/Create PopcornFX/Camera", false, 10)]
	static void CreatePKCamera(MenuCommand menuCommand) {
		// Create a custom game object
		GameObject go = new GameObject("PKFxCamera");
		go.AddComponent<Camera>();
		go.AddComponent<PKFxRenderingPlugin>();
		// Ensure it gets reparented if this was a context click (otherwise does 
		//GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
		// Register the creation in the undo system
		Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
		Selection.activeObject = go;
	}

	//========== HELP =====================================================
	//
	//
	//
	//
	//
	//=====================================================================

	[MenuItem("Help/PopcornFX Wiki")]
	static void LinkToWiki()
	{
		Application.OpenURL("http://wiki.popcornfx.com/index.php/Unity_V3");
	}

	//----------------------------------------------------------------------------

	[MenuItem("Help/Open PopcornFX Log")]
	static void OpenLog()
	{
		if (PKFxManagerImpl.FileLoggingEnabled() && File.Exists(PKFxManagerImpl.m_LogFilePath))
			Application.OpenURL(PKFxManagerImpl.m_LogFilePath);
		else
			UnityEngine.Debug.LogError("[PKFX] Log file not found, try enabling the log in the PopcornFX Preferences and/or restarting Unity.");
	}

	//----------------------------------------------------------------------------

}
