using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PKFxManager : PKFxManagerImpl
{
	// Public ReadOnly Properties
	//----------------------------------------------------------------------------

	public static string[] CustomFileTypes		{ get { return s_CustomFileTypes; } }
	public static string[] TextureFileTypes		{ get { return s_TexFileTypes; } }
	public static string[] SoundFileTypes		{ get { return s_SoundFileTypes; } }
	public static string[] SimcacheFileTypes	{ get { return s_SimcacheFileTypes; } }
	
	public static Dictionary<int, List<PKFxFxAsset>> DependenciesLoading { get { return m_DependenciesLoading; } }

	public static string ImportedAssetName		{ get; set; }
	// PopcornFX General State With Native Interop
	//----------------------------------------------------------------------------

	public static bool IsDllLoaded()
	{
		return _IsDllLoaded();
	}

	public static void StartupPopcorn(bool isInPlayMode)
	{
		if (!m_IsStarted)
		{
			Startup();
			SetupPopcornFxSettings(isInPlayMode);
			m_IsStartedAsPlayMode = isInPlayMode;
		}
		else if (m_IsStartedAsPlayMode != isInPlayMode)
		{
			SetupPopcornFxSettings(isInPlayMode);
			m_IsStartedAsPlayMode = isInPlayMode;
		}
	}

	public static void ShutdownPopcorn()
	{
		PopcornFXShutdown();
	}

	public static void ResetAllEffects()
	{
		Reset();
	}

	public static void ResetAndUnloadAllEffects()
	{
		DeepReset();
	}

	public static bool IsSupportedTextureExtension(string extension)
	{
		return arrayContains(TextureFileTypes, extension);
	}

	new public static void ClearAllCallbacks()
	{
		PKFxManagerImpl.ClearAllCallbacks();
	}

	new public static void ClearRenderers()
	{
		PKFxManagerImpl.ClearRenderers();
	}

	public static void SetSceneMesh(PKFxMeshAsset sceneMesh)
	{
		if (sceneMesh == null)
		{
			SceneMeshClear();
		}
		else
		{
			PreloadSceneMesh(sceneMesh);
			if (LoadPkmmAsSceneMesh(sceneMesh.m_AssetName))
				Debug.Log("[PKFX] Scene Mesh loaded");
			else
				Debug.LogError("[PKFX] Failed to load mesh " + sceneMesh + " as scene mesh");
			UnloadAllFxDependencies();
		}
	}

	public static bool AddMeshToSceneMesh(Mesh mesh, Transform transform)
	{
		return PKFxManagerImpl.AddMeshToSceneMesh(mesh, transform.localToWorldMatrix);
	}

	new public static bool AddMeshToSceneMesh(Mesh mesh,  Matrix4x4 localToWorldMatrix)
	{
		return PKFxManagerImpl.AddMeshToSceneMesh(mesh, localToWorldMatrix);
	}

	new public static int SceneMeshBuild(string outFile)
	{
		return PKFxManagerImpl.SceneMeshBuild(outFile);
	}

	public static void SetupPopcornFxSettings(bool isInPlayMode)
	{
		SPopcornFxSettings settings = new SPopcornFxSettings();

		settings.m_EnableRaycastForCollisions = PKFxSettings.EnableRaycastForCollisions;
		settings.m_SplitDrawCallsOfSoubleSidedParticles = PKFxSettings.SplitDrawCallsOfSoubleSidedParticles;
		settings.m_WaitForUpdateOnRenderThread = PKFxSettings.WaitForUpdateOnRenderThread;
		settings.m_DisableDynamicEffectBounds = PKFxSettings.DisableDynamicEffectBounds;

		if (!isInPlayMode)
		{
			settings.m_SingleThreadedExecution = true;
			settings.m_OverrideThreadPool = false;
			settings.m_WorkerCount = 0;
			settings.m_WorkerAffinities = IntPtr.Zero;
		}
		else
		{
			settings.m_SingleThreadedExecution = PKFxSettings.SingleThreadedExecution;

			if (!PKFxSettings.SingleThreadedExecution && PKFxSettings.OverrideThreadPoolConfig)
			{
				// Create pool with correct affinities:
				int[] affinities = new int[PKFxSettings.ThreadsAffinity.Count];
				IntPtr workersAffinities = Marshal.AllocHGlobal(PKFxSettings.ThreadsAffinity.Count * sizeof(int));

				PKFxSettings.ThreadsAffinity.CopyTo(affinities);
				Marshal.Copy(affinities, 0, workersAffinities, PKFxSettings.ThreadsAffinity.Count);

				settings.m_OverrideThreadPool = true;
				settings.m_WorkerCount = PKFxSettings.ThreadsAffinity.Count;
				settings.m_WorkerAffinities = workersAffinities;
			}
			else
			{
				settings.m_OverrideThreadPool = false;
				settings.m_WorkerCount = 0;
				settings.m_WorkerAffinities = IntPtr.Zero;
			}
		}
		PopcornFXStartup(ref settings);
		if (settings.m_WorkerAffinities != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(settings.m_WorkerAffinities);
		}
	}

	// Editor Only
	//----------------------------------------------------------------------------
#region editor

#if UNITY_EDITOR
	new public static void StartupPopcornFileWatcher()
	{
		PKFxManagerImpl.StartupPopcornFileWatcher();
	}

	new public static void LockPackWatcherChanges()
	{
		PKFxManagerImpl.LockPackWatcherChanges();
	}

	new public static void UnlockPackWatcherChanges()
	{
		PKFxManagerImpl.UnlockPackWatcherChanges();
	}

	public static bool PullPackWatcherChanges(out int remainingChanges)
	{
		return UnstackPackWatcherChanges(out remainingChanges);
	}

	new public static void CancelPackWatcherChanges()
	{
		PKFxManagerImpl.CancelPackWatcherChanges();
	}

	new public static void RestartPackWatcher()
	{
		PKFxManagerImpl.RestartPackWatcher();
	}

	new public static void PausePackWatcher()
	{
		PKFxManagerImpl.PausePackWatcher();
	}

	new public static bool BrowseEffectContent(IntPtr pkfxContentPtr, int contentByteSize, string path)
	{
		return PKFxManagerImpl.BrowseEffectContent(pkfxContentPtr, contentByteSize, path);
	}

	new public static void GetAllAssetPath()
	{
		PKFxManagerImpl.GetAllAssetPath();
	}

	new public static void ReimportAllAssets()
	{
		PKFxManagerImpl.ReimportAllAssets();
	}

	public static void ReimportAssets(List<string> assets)
	{
		PKFxManagerImpl.ReimportAssets(assets.Count, assets.ToArray());
	}
#endif

	#endregion editor

	// Frame Update
	//----------------------------------------------------------------------------
#region FrameUpdateApi

	public static void UpdateLogic(float dt)
	{
		LogicalUpdate(dt);
	}

	public static void UpdateCamera(int GUID, ref SCamDesc desc)
	{
		UpdateCamDesc(GUID, ref desc, false);
	}

	public static void UpdateParticles()
	{
		// Sync previous frame IFN:
		SyncParticlesSimulation();

		// Update the FX transforms and attributes:
		foreach (PKFxFX fx in PKFxFX.g_PlayingEffectsToUpdate)
		{
			fx.UpdateEffectTransforms();
			fx.UpdateSamplers(false);
		}

		float frameDt = Time.smoothDeltaTime * PKFxSettings.TimeMultiplier;

		// Update the spawn of the particles:
		UpdateParticlesSpawn(frameDt);

		// Update the particles:
		UpdateParticles(frameDt);
	}

	public static void UpdateParticlesSpawn()
	{
		// Sync previous frame IFN:
		SyncParticlesSimulation();

		// Update the FX transforms and attributes:
		foreach (PKFxFX fx in PKFxFX.g_PlayingEffectsToUpdate)
		{
			fx.UpdateEffectTransforms();
			fx.UpdateSamplers(false);
		}

		// Update the spawn of the particles:
		float frameDt = Time.smoothDeltaTime * PKFxSettings.TimeMultiplier;
		UpdateParticlesSpawn(frameDt);
	}

	public static void UpdateParticlesEvolve()
	{
		// Update the spawn of the particles:
		float frameDt = Time.smoothDeltaTime * PKFxSettings.TimeMultiplier;
		UpdateParticles(frameDt);
	}

	public static void Render(short cameraID)
	{
		_Render(cameraID);
	}
#endregion FrameUpdateApi

	// Effect Related API With Native Interop
	//----------------------------------------------------------------------------
#region effectsApi
	public static int InstantiateEffect(ref SFxDesc effectDescription)
	{
		return InstantiateFx(ref effectDescription);
	}

	public static int StartEffect(int effectGUID, float dt = 0.0f)
	{
		if (effectGUID >= 0)
			return StartFx(effectGUID, dt);
		return -1;
	}

	public static bool StopEffect(int effectGUID, float dt = 0.0f)
	{
		if (effectGUID >= 0)
			return StopFx(effectGUID, dt);
		return false;
	}

	public static bool TerminateEffect(int effectGUID, float dt = 0.0f)
	{
		if (effectGUID >= 0)
			return TerminateFx(effectGUID, dt);
		return false;
	}

	public static bool KillEffect(int effectGUID, float dt = 0.0f)
	{
		if (effectGUID >= 0)
			return KillFx(effectGUID, dt);
		return false;
	}
	
	public static bool PreloadEffectDependencies(PKFxFxAsset fxAsset)
	{
		return _PreloadFxDependencies(fxAsset);
	}

	public static void PreloadEffectIFN(string path, bool useMeshRenderer)
	{
		PreloadFxIFN(path, useMeshRenderer ? 1 : 0);
	}

	public static bool SetEffectTransform(int effectGUID, Transform transform)
	{
		Matrix4x4 m = transform.localToWorldMatrix;
		return EffectSetTransforms(effectGUID, m);
	}

	public static IntPtr GetTextureSamplerToFill(int byteSize)
	{
		return GetTextureSamplerData(byteSize);
	}
	#endregion effectsApi

	// Attributes Buffers
	//----------------------------------------------------------------------------
#region Attributes Buffers
	public static IntPtr GetAttributesBuffer(int effectGUID)
	{
		return EffectGetAttributesBuffer(effectGUID);
	}

	public static bool UpdateAttributesBuffer(int effectGUID)
	{
		return EffectUpdateAttributes(effectGUID);
	}
#endregion Attributes Buffers

	// Effect Samplers
	//----------------------------------------------------------------------------
#region Samplers

	// Default Samplers
	public static bool SetDefaultSampler(int effectGUID, int samplerId)
	{
		return EffectResetDefaultSampler(effectGUID, samplerId);
	}

	// Mesh Samplers
	public static bool SetMeshSampler(int effectGUID, int samplerId, IntPtr meshSampler, Vector3 size)
	{
		return EffectSetMeshSampler(effectGUID, samplerId, meshSampler, size);
	}

	public static IntPtr GetMeshSamplerToFill(int effectGUID, int samplerId,int bonesCount, int samplingInfo)
	{
		return GetMeshSamplerData(effectGUID, samplerId, bonesCount, samplingInfo);
	}

	// Skinning Samplers
	public static bool BeginUpdateSamplerSkinning(int effectGUID, int samplerId, float dt)
	{
		return EffectBeginUpdateSamplerSkinning(effectGUID, samplerId, dt);
	}

	public static bool EndUpdateSamplerSkinning(int effectGUID, int samplerId)
	{
		return EffectEndUpdateSamplerSkinning(effectGUID, samplerId);
	}

	public static IntPtr UpdateSamplerSkinningSetMatrices(int effectGUID, int samplerId)
	{
		return EffectUpdateSamplerSkinningSetMatrices(effectGUID, samplerId);
	}

	// Shape Samplers
	public static bool SetSamplerShape(int effectGUID, int samplerId, PKFxFX.Sampler.EShapeType shapeType, Vector3 size)
	{
		return EffectSetSamplerShape(effectGUID, samplerId, shapeType, size);
	}

	public static bool SetSamplerShapeTransform(int effectGUID, int samplerId, Matrix4x4 transform)
	{
		return EffectSetSamplerShapeTransform(effectGUID, samplerId, transform);
	}

	// Curve Samplers
	public static bool SetCurveSampler(int effectGUID, int samplerId, IntPtr curveSampler)
	{
		return EffectSetCurveSampler(effectGUID, samplerId, curveSampler);
	}

	public static IntPtr GetCurveSamplerToFill(int keyPointsCount, int curveDimension)
	{
		return GetCurveSamplerData(keyPointsCount, curveDimension);
	}

	// Texture Samplers
	public static bool SetTextureSampler(int effectGUID, int samplerId, IntPtr textureSampler)
	{
		return EffectSetTextureSampler(effectGUID, samplerId, textureSampler);
	}

	// Text Samplers 
	public static bool SetTextSampler(int effectGUID, int samplerId, string text)
	{
		return EffectSetTextSampler(effectGUID, samplerId, text);
	}
	#endregion Samplers

	// Getter Setters
	//----------------------------------------------------------------------------
#region Get/Set
	public static PKFxFxAsset GetBuiltAsset()
	{
		return m_CurrentlyBuildAsset;
	}

	public static void SetBuiltAsset(PKFxFxAsset value)
	{
		m_CurrentlyBuildAsset = value;
	}

	public static PKFxFxAsset GetImportedAsset()
	{
		return m_CurrentlyImportedAsset;
	}

	public static void SetImportedAsset(PKFxFxAsset value)
	{
		m_CurrentlyImportedAsset = value;
	}

	public static string GetImportedAssetName()
	{
		return ImportedAssetName;
	}
#endregion Get/Set

	// Profiler Management
	//----------------------------------------------------------------------------

	public static void ProfilerEnable(bool onOff)
	{
		ProfilerSetEnable(onOff);
	}

	public static void ProfilerWriteReport(string path)
	{
		WriteProfileReport(path);
	}

}
