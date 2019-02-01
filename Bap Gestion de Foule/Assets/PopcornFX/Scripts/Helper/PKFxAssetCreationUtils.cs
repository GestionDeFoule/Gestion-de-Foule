
//----------------------------------------------------------------------------
// Unity Editor Only class.
// Exposed in runtime due to the limitation of importing symbols from editor
//	folder into runtime classes.
//----------------------------------------------------------------------------
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PKFxAssetCreationUtils : object
{
	//----------------------------------------------------------------------------
	// Public Methods
	//----------------------------------------------------------------------------

	public static bool CreatePKFXAsset(PKFxManagerImpl.SAssetChangesDesc assetChange)
	{
		return ProcessAssetCreationData(assetChange);
	}

	//----------------------------------------------------------------------------

	public static bool UpdatePKFXAsset(PKFxFxAsset fxAsset, string path)
	{
		fxAsset.m_AssetName = path;
		if (ProcessAssetChangeData(fxAsset, path) == false)
			return false;
		UpdateAssetDependency(fxAsset);
		EditorUtility.SetDirty(fxAsset); // Seems like the asset is never serialized if we do not flag it dirty
		return true;
	}

	//----------------------------------------------------------------------------

	public static bool UpdateAndRenamePKFXAsset(PKFxFxAsset fxAsset, string oldPath, string newPath)
	{
		fxAsset.m_AssetName = newPath;
		if (ProcessAssetChangeData(fxAsset, oldPath) == false)
			return false;
		UpdateAssetDependency(fxAsset);
		return true;
	}

	//----------------------------------------------------------------------------

	public static void NotifyAssetPostProcess(string path)
	{
		int key = path.ToLowerInvariant().GetHashCode();
		if (PKFxManager.DependenciesLoading.ContainsKey(key))
		{
			UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
			List<PKFxFxAsset> assets = PKFxManager.DependenciesLoading[key];
			foreach (PKFxFxAsset asset in assets)
			{
				var dependency = asset.m_Dependencies.Find(x => path.ToLowerInvariant().Contains(x.m_Path.ToLowerInvariant()));
				if (dependency != null)
				{
					dependency.m_Object = obj;
					ApplyPKImportSetting(dependency, path);
				}
			}
			PKFxManager.DependenciesLoading.Remove(key);
		}
	}

	//----------------------------------------------------------------------------
	// Private Methods
	//----------------------------------------------------------------------------

	public static void CreateAssetFolderUpToPath(string path)
	{
		string parentfolderPath = "Assets" + PKFxSettings.UnityPackFxPath;
		char[] separators = { '/', '\\' };

		if (AssetDatabase.IsValidFolder(parentfolderPath) == false)
		{
			string root = PKFxSettings.UnityPackFxPath;
			if (root.StartsWith("/"))
				root = root.Substring(1);
			Debug.Log("AssetDatabase.CreateFolder " + root);
			AssetDatabase.CreateFolder("Assets", root);
		}

		path = Path.GetDirectoryName(path);

		string[] folders = path.Split(separators, StringSplitOptions.RemoveEmptyEntries);
		foreach (string folder in folders)
		{
			if (AssetDatabase.IsValidFolder(Path.Combine(parentfolderPath, folder)) == false)
				AssetDatabase.CreateFolder(parentfolderPath, folder);
			parentfolderPath = parentfolderPath + "/" + folder;
		}
	}
	//----------------------------------------------------------------------------

	private static bool ProcessAssetCreationData(PKFxManagerImpl.SAssetChangesDesc assetChange)
	{
		PKFxFxAsset fxAsset = ScriptableObject.CreateInstance<PKFxFxAsset>();

		if (fxAsset != null)
		{
			fxAsset.m_Data = File.ReadAllBytes("Temp/PopcornFx/Baked/" + assetChange.m_Path);
			fxAsset.m_AssetName = assetChange.m_Path;
			if (ProcessAssetChangeData(fxAsset, assetChange.m_Path) == false)
				return false;
			CreateAssetFolderUpToPath(assetChange.m_Path);
			AssetDatabase.CreateAsset(fxAsset, Path.Combine("Assets" + PKFxSettings.UnityPackFxPath, assetChange.m_Path + ".asset"));
		}
		return true;
	}

	//----------------------------------------------------------------------------

	private static bool ProcessAssetChangeData(PKFxFxAsset fxAsset, string fxPathToPatch)
	{
		GCHandle	handle = GCHandle.Alloc(fxAsset.m_Data, GCHandleType.Pinned);
		IntPtr		fileContentPtr = handle.AddrOfPinnedObject();

		PKFxManager.SetImportedAsset(fxAsset);

		if (PKFxManager.BrowseEffectContent(fileContentPtr, fxAsset.m_Data.Length, "Temp/PopcornFx/Baked/" + fxAsset.m_AssetName) == false)
		{
			handle.Free();
			Debug.LogError("Reimport of " + fxAsset.m_AssetName + "failed");
			return false;
		}

		fxAsset.ComputeAttributesAndSamplersHash();

		// Fix all references in the current scene:
		bool					sceneHasChanged = false;
		UnityEngine.Object[]	effects = UnityEngine.Object.FindObjectsOfType(typeof(PKFxFX));

		foreach (UnityEngine.Object obj in effects)
		{
			PKFxFX effect = obj as PKFxFX;

			if (effect.m_FxName == fxPathToPatch) // Sometimes the effect.m_FxAsset is null here, so we test against the m_FxName
			{
				if (effect.UpdateEffectAsset(fxAsset, false, false))
					sceneHasChanged = true;
			}
		}

		if (sceneHasChanged)
		{
			Scene currentScene = SceneManager.GetActiveScene();
			EditorSceneManager.MarkSceneDirty(currentScene);
		}

		PKFxManager.SetImportedAsset(null);
		handle.Free();

		ImportAssetDependencies(fxAsset);
		return true;
	}

	//----------------------------------------------------------------------------

	private static void UpdateAssetDependency(PKFxFxAsset fxAsset)
	{
		foreach (var dependency in fxAsset.m_Dependencies)
		{
			string path = "Assets" + dependency.m_Path;
			UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
			if (obj)
			{
				dependency.m_Object = obj;
				ApplyPKImportSetting(dependency, path);
			}
		}
	}

	//----------------------------------------------------------------------------

	private static void ImportAssetDependencies(PKFxFxAsset fxAsset)
	{
		foreach (PKFxFxAsset.DependencyDesc dependency in fxAsset.m_Dependencies)
		{
			dependency.m_Path = dependency.m_Path.Replace("\\", "/");
			string pkDependencyRelativePath = dependency.m_Path.Substring(PKFxSettings.UnityPackFxPath.Length);
			string sourceFile;

			CreateAssetFolderUpToPath(pkDependencyRelativePath);
			//Cases where we need originals assets without bake.
			if (dependency.m_IsTextureLinear)
			{
				sourceFile = PKFxSettings.PopcornPackFxPath + pkDependencyRelativePath;
				dependency.m_Path = Path.GetDirectoryName(dependency.m_Path) + "/" + Path.GetFileNameWithoutExtension(dependency.m_Path) + "_linear" + Path.GetExtension(dependency.m_Path);
				CreateDependencyAsset(fxAsset, "Assets" + PKFxSettings.UnityPackFxPath, dependency.m_Path.Substring(PKFxSettings.UnityPackFxPath.Length), sourceFile);
			}
			else if (dependency.m_IsMeshRenderer)
			{
				sourceFile = PKFxSettings.PopcornPackFxPath + pkDependencyRelativePath;
				CreateDependencyAsset(fxAsset, "Assets" + PKFxSettings.UnityPackFxPath, pkDependencyRelativePath, sourceFile);
			}
			else// if (dependency.m_IsMeshSampler)
			{
				sourceFile = "Temp/PopcornFx/Baked" + pkDependencyRelativePath;
				CreateDependencyAsset(fxAsset, "Assets" + PKFxSettings.UnityPackFxPath, pkDependencyRelativePath, sourceFile);
			}
		}
	}

	//----------------------------------------------------------------------------

	private static void CreateDependencyAsset(PKFxFxAsset fxAsset, string dstPath, string dstFile, string srcFile)
	{
		//Case where PopedV1 keep shadow dependencies.
		if (!File.Exists(srcFile))
			return;

		bool fileExist = File.Exists(dstPath + dstFile);
		if (fileExist)
			FileUtil.ReplaceFile(srcFile, dstPath + dstFile);
		else
			FileUtil.CopyFileOrDirectory(srcFile, dstPath + dstFile);
		
		string keyPath = "Assets" + PKFxSettings.UnityPackFxPath + dstFile;
		int key = keyPath.ToLowerInvariant().GetHashCode();
		if (!PKFxManager.DependenciesLoading.ContainsKey(key))
		{
			PKFxManager.DependenciesLoading.Add(key, new List<PKFxFxAsset>());
			PKFxManager.DependenciesLoading[key].Add(fxAsset);
			AssetDatabase.ImportAsset(dstPath + dstFile, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
		}
		else if (!PKFxManager.DependenciesLoading[key].Contains(fxAsset))
			PKFxManager.DependenciesLoading[key].Add(fxAsset);
		//AssetDatabase.ImportAsset(dstPath + dstFile, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
	}

	//----------------------------------------------------------------------------

	private static void ApplyPKImportSetting(PKFxFxAsset.DependencyDesc dependency, string path)
	{
		string fExt = Path.GetExtension(path);
		if (PKFxManager.IsSupportedTextureExtension(fExt))
		{
			TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
			bool reimport = false;
			if (importer == null)
				return;
			if (importer.sRGBTexture != !dependency.m_IsTextureLinear)
			{
				importer.sRGBTexture = !dependency.m_IsTextureLinear;
				reimport = true;
			}
			if (dependency.m_IsTextureSampler)
			{
				importer.isReadable = true;
				reimport = true;
			}
			if (reimport)
				importer.SaveAndReimport();
		}
	}

	//----------------------------------------------------------------------------
}

#endif