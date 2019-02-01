//----------------------------------------------------------------------------
// Created on Thu Oct 31 11:53:00 2013 Maxime Dumas
//
// This program is the property of Persistant Studios SARL.
//
// You may not redistribute it and/or modify it under any conditions
// without written permission from Persistant Studios SARL, unless
// otherwise stated in the latest Persistant Studios Code License.
//
// See the Persistant Studios Code License for further details.
//----------------------------------------------------------------------------

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

//----------------------------------------------------------------------------

public partial class PKFxManagerImpl : object
{
	//----------------------------------------------------------------------------

	public const UInt32 POPCORN_MAGIC_NUMBER = 0x5AFE0000;

	//----------------------------------------------------------------------------
	// Data structures:
	//----------------------------------------------------------------------------

	[StructLayout(LayoutKind.Sequential)]
	public struct SPopcornFxSettings
	{
		public int		m_DeviceType;

		public bool		m_EnableRaycastForCollisions;
		public bool		m_SplitDrawCallsOfSoubleSidedParticles;
		public bool		m_DisableDynamicEffectBounds;

		// Threading
		public bool		m_WaitForUpdateOnRenderThread;
		public bool		m_SingleThreadedExecution;
		public bool		m_OverrideThreadPool;
		public int		m_WorkerCount;
		public IntPtr	m_WorkerAffinities;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SCamDesc
	{
		public Matrix4x4	m_ViewMatrix;
		public Matrix4x4	m_ProjectionMatrix;
		public uint			m_ViewportWidth;
		public uint			m_ViewportHeight;
		public int			m_RenderPass;
		public float		m_NearClip;
		public float		m_FarClip;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public struct SFxDesc
	{
		public string		m_FxPath;
		public Matrix4x4	m_Transforms;
		public bool			m_UsesMeshRenderer;
		public bool			m_AutoDestroyEffect;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SMeshSamplerToFill
	{
		// Ptr to the mesh new
		private IntPtr		m_MeshNew;

		// ptr to the vertex/index buffers
		public int			m_IdxCount;
		public IntPtr		m_Indices;
		public int			m_VtxCount;
		public IntPtr		m_Positions;
		public IntPtr		m_Normals;
		public IntPtr		m_Tangents;
		public IntPtr		m_UV;
		public IntPtr		m_VertexColor;

		// Skinning info
		public int			m_BonesCount;
		public IntPtr		m_VertexBonesWeights;
		public IntPtr		m_VertexBonesIndices;

		// Sampling flags
		public int			m_SamplingInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SCurveSamplerToFill
	{
		public IntPtr		m_KeyPoints;
		public int			m_CurveDimension;
		public int			m_KeyPointsCount;
	};

	[StructLayout(LayoutKind.Sequential)]
	public struct STextureSamplerToFill
	{
		public IntPtr		m_TextureData;
		public int			m_Width;
		public int			m_Height;
		public int			m_SizeInBytes;
		public int			m_PixelFormat;
		public int			m_WrapMode;
	};

	[StructLayout(LayoutKind.Sequential)]
	public struct SStats
	{
		public float m_UpdateTime;
		public float m_RenderTime;
		public int m_TotalMemoryFootprint;
		public int m_TotalParticleMemory;
		public int m_UnusedParticleMemory;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SMirrorPackFxSettings
	{
		public string m_PopcornPackFxPath;
		public string m_UnityPackFxPath;
	}

	//----------------------------------------------------------------------------

	#region plugin name and calling convention

#if (UNITY_IOS || UNITY_SWITCH) && !UNITY_EDITOR
	private const string kPopcornPluginName = "__Internal";
#elif UNITY_XBOXONE && !UNITY_EDITOR
	private const string kPopcornPluginName = "PK-UnityPlugin_XBoxOne";
#else
	private const string kPopcornPluginName = "PK-UnityPlugin";
#	endif

#	if (UNITY_WINRT || UNITY_SWITCH) && !UNITY_EDITOR
	public const CallingConvention kCallingConvention = CallingConvention.Cdecl;
#	else
	public const CallingConvention kCallingConvention = CallingConvention.Winapi;
#endif
	#endregion

#if UNITY_SWITCH && !UNITY_EDITOR
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void RegisterPlugin();
#endif
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern IntPtr GetRenderEventFunc();
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern int PopcornFXDllLoaded();
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void PopcornFXStartup(ref SPopcornFxSettings settings);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void PopcornFXShutdown();
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void UpdateCamDesc(int camID, ref SCamDesc desc, bool update);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void UpdateParticlesSpawn(float DT);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void UpdateParticles(float DT);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void SyncParticlesSimulation();
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void TransformAllParticles(Matrix4x4 transform);

	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void GetStats(ref SStats stats);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void PreloadFxIFN(string path, int usesMeshRenderer);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern int InstantiateFx(ref SFxDesc fxDesc);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern bool TerminateFx(int guid, float dt);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern int StartFx(int guid, float dt);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern bool StopFx(int guid, float dt);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern bool KillFx(int guid, float dt);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void DestroyFx(int guid);

	// For the attributes:
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern IntPtr EffectGetAttributesBuffer(int guid);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern bool EffectUpdateAttributes(int guid);

	// For the samplers shape:
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern bool EffectSetSamplerShapeTransform(int guid, int samplerId, Matrix4x4 transform);
	// Mesh sampler:
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern IntPtr GetMeshSamplerData(int vertexCount, int indexCount, int bonesCount, int samplingInfo);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern bool EffectSetMeshSampler(int guid, int samplerId, IntPtr meshSampler, Vector3 size);
	// Skinning:
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern IntPtr EffectUpdateSamplerSkinningSetMatrices(int guid, int samplerId);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern bool EffectBeginUpdateSamplerSkinning(int guid, int samplerId, float dt);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern bool EffectEndUpdateSamplerSkinning(int guid, int samplerId);
	// Other sampler shapes:
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern bool EffectSetSamplerShape(int guid, int samplerId, PKFxFX.Sampler.EShapeType shapeType, Vector3 size);
	// Sampler curve:
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern IntPtr GetCurveSamplerData(int keyPointsCount, int curveDimension);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern bool EffectSetCurveSampler(int guid, int samplerId, IntPtr curveSampler);
	// Sampler texture:
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern IntPtr GetTextureSamplerData(int byteSize);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern bool EffectSetTextureSampler(int guid, int samplerId, IntPtr textureSampler);
	// Sampler text:
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern bool EffectSetTextSampler(int guid, int samplerId, string text);
	// Any sampler:
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern bool EffectResetDefaultSampler(int guid, int samplerId);
	// ----------------------------------

	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern bool EffectSetTransforms(int guid, Matrix4x4 tranforms);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern bool UnloadEffect(string path);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void LogicalUpdate(float dt);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern bool LoadPkmmAsSceneMesh(string pkmmVirtualPath);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void SceneMeshClear();
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern bool SceneMeshAddRawMesh(int indicesCount, int[] indices, int verticesCount, Vector3[] vertices, Vector3[] normals, Matrix4x4 MeshMatrix);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern int SceneMeshBuild(string outputPkmmVirtualPath);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void WriteProfileReport(string path);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void ProfilerSetEnable(bool enable);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void Reset();
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void DeepReset();
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void ClearAllCallbacks();

#if		UNITY_EDITOR
	// Browse an effect content to create the Unity asset:
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern bool BrowseEffectContent(IntPtr pkfxContentPtr, int contentByteSize, string path);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern IntPtr GetRuntimeVersion();
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern bool StartPackWatcher(ref SMirrorPackFxSettings settings);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void PausePackWatcher();
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void RestartPackWatcher();
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern bool UnstackPackWatcherChanges(out int assetRemaining);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void CancelPackWatcherChanges();
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void LockPackWatcher();
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void UnlockPackWatcher();
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void GetAllAssetPath();
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void ReimportAllAssets(bool refresh);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	protected static extern void ReimportAssets(int size, string[] ar);
#endif

	//----------------------------------------------------------------------------

	public static int Float2Int(float fff)
	{
		return BitConverter.ToInt32(BitConverter.GetBytes(fff), 0);
	}
	
	public static float Int2Float(int i)
	{
		return BitConverter.ToSingle(BitConverter.GetBytes(i), 0);
	}

	//----------------------------------------------------------------------------

	public enum EImageFormat : int
	{
		Invalid = 0,
		BGR8 = 3,
		BGRA8,
		BGRA8_sRGB,
		DXT1 = 8,
		DXT1_sRGB,
		DXT3,
		DXT3_sRGB,
		DXT5,
		DXT5_sRGB,
		RGB8_ETC1 = 16,
		RGB8_ETC2,
		RGBA8_ETC2,
		RGB8A1_ETC2,
		RGB4_PVRTC1,
		RGB2_PVRTC1,
		RGBA4_PVRTC1,
		RGBA2_PVRTC1,
		Fp32_RGBA = 26
	}

	//----------------------------------------------------------------------------


	private const string			m_UnityVersion = "Unity 2017.4 and up";
	public const string				m_PluginVersion = "3.0 for " + m_UnityVersion;
#if		UNITY_EDITOR
	public static string			m_CurrentVersionString = "";
#endif
	public static string			m_LogFilePath = Path.GetFullPath(Path.Combine(Application.dataPath, "../popcorn.htm"));
	public static bool				m_IsStarted = false;
	public static bool				m_IsStartedAsPlayMode = false;
	public static int				m_DistortionLayer = 31;

	private static GameObject		m_RenderersRoot = null;
	private static int				m_CurrentRenderersGUID = 0;
	public static List<SMeshDesc>	m_Renderers = new List<SMeshDesc>();
	private static GameObject		m_RuntimeAssetsRoot = null;
	public static List<GameObject>	m_RuntimeAssets = new List<GameObject>();

	private static float[]			m_Samples;
	private static GCHandle			m_SamplesHandle;
	private static bool				m_HasFileLogging = false;

	//----------------------------------------------------------------------------

	protected static string[]	s_CustomFileTypes = { ".pkat", ".pkfx", ".pkmm", ".pkfm", ".pksc", ".pkan" };
	protected static string[]	s_TexFileTypes = { ".dds", ".png", ".jpg", ".jpeg", ".tga", ".exr", ".hdr", ".tif", ".tiff", ".pkm", ".pvr" };
	protected static string[]	s_SoundFileTypes = { ".mp3", ".wav", ".ogg" };
	protected static string[]	s_SimcacheFileTypes = { ".pksc" };

	//----------------------------------------------------------------------------

	// Resource dependencies:
	private static List<PKFxAsset>						m_Dependencies = new List<PKFxAsset>();
	private static List<Texture2D>						m_TexDependencies = new List<Texture2D>();
	private static List<PinnedData>						m_NeedsFreeing;

	protected static PKFxFxAsset						m_CurrentlyImportedAsset = null;
	protected static PKFxFxAsset						m_CurrentlyBuildAsset = null;

	protected static Dictionary<int, List<PKFxFxAsset>>	m_DependenciesLoading = new Dictionary<int, List<PKFxFxAsset>>();



	//----------------------------------------------------------------------------

	protected static bool AddMeshToSceneMesh(Mesh mesh, Matrix4x4 localToWorldMatrix)
	{
		int		subMeshCount = mesh.subMeshCount;
		if (subMeshCount <= 0)
		{
			Debug.LogError("[PKFX] Mesh doesn't have sub meshes");
			return false;
		}
		int		verticesCount = mesh.vertexCount;
		if (mesh.subMeshCount > 1)
			Debug.LogWarning("[PKFX] Mesh has more than 1 submesh: non opti");
		for (int subMeshId = 0; subMeshId < mesh.subMeshCount; subMeshId++)
		{
			int	indicesCount = mesh.GetIndices(subMeshId).Length;
			if (!Application.isPlaying) // don't pollute the log at runtime.
				Debug.Log("[PKFX] Mesh (" + (subMeshId + 1).ToString() + "/" + subMeshCount.ToString() + ") idx:" + indicesCount.ToString() + " v:" + verticesCount.ToString() + " v:" + mesh.vertices.Length.ToString() + " n:" + mesh.normals.Length.ToString() + " uv:" + mesh.uv.Length.ToString());
			if (mesh.vertices.Length != verticesCount ||
				mesh.normals.Length != verticesCount)
			{
				Debug.LogError("[PKFX] Invalid mesh");
			}

			// Load the mesh (after the brush)
			if (!SceneMeshAddRawMesh(
				indicesCount, mesh.GetIndices(subMeshId),
				verticesCount, mesh.vertices, mesh.normals,
				localToWorldMatrix
				))
			{
				Debug.LogError("[PKFX] Fail to load raw mesh");
			}
		}
		return true;
	}

	//----------------------------------------------------------------------------

	protected static void _Render(short cameraID)
	{
		if (cameraID >= 0)
		{
			UInt32 eventID = ((UInt32)cameraID | POPCORN_MAGIC_NUMBER);

			GL.IssuePluginEvent(GetRenderEventFunc(), (int)eventID);
		}
		else
			Debug.LogError("[PKFX] PKFxManager: invalid cameraID for rendering " + cameraID);
	}

	//----------------------------------------------------------------------------

	static PKFxManagerImpl()
	{
		EnableFileLoggingIFN(PKFxSettings.EnableFileLog);

#if PKFX_CHECK_SIGNATURES
		PKFxCrypto.SetupCrypto();
#endif
	}

	//----------------------------------------------------------------------------

	protected static bool _IsDllLoaded()
	{
		try
		{
			PopcornFXDllLoaded();
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}
	
	protected static void Startup()
	{
		PKFxDelegateHandler delegateHandler = PKFxDelegateHandler.Instance;
#		if UNITY_IOS

		//if (Application.platform == RuntimePlatform.IPhonePlayer)
		//{
			if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Metal)
				IOSPluginLoad(IntPtr.Zero, (int)UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2);
			else 
				IOSPluginLoad(IntPtr.Zero, (int)UnityEngine.Rendering.GraphicsDeviceType.Metal);
		//}
#		endif
		
#		if UNITY_SWITCH && !UNITY_EDITOR
			RegisterPlugin();
#		endif

		delegateHandler.DiscardAllDelegatesRefs();

		SetDelegateOnResourceLoad(delegateHandler.DelegateToFunctionPointer(new ResourceLoadCallback(OnResourceLoad)));
		SetDelegateOnResourceWrite(delegateHandler.DelegateToFunctionPointer(new ResourceWriteCallback(OnResourceWrite)));
		// Not in native to managed file
		SetDelegateOnRaiseEvent(delegateHandler.DelegateToFunctionPointer(new PKFxEventManager.StartEventCallback(PKFxEventManager.OnRaiseEvent)));
		SetDelegateOnRaycastPack(delegateHandler.DelegateToFunctionPointer(new PKFxRaycasts.RaycastPackCallback(PKFxRaycasts.OnRaycastPack)));
		SetDelegateOnStartSound(delegateHandler.DelegateToFunctionPointer(new PKFxSoundManager.StartSoundCallback(PKFxSoundManager.OnStartSound)));

		SetDelegateOnFxStopped(delegateHandler.DelegateToFunctionPointer(new FxCallback(OnFxStopped)));
		SetDelegateOnAudioWaveformData(delegateHandler.DelegateToFunctionPointer(new AudioCallback(OnAudioWaveformData)));
		SetDelegateOnAudioSpectrumData(delegateHandler.DelegateToFunctionPointer(new AudioCallback(OnAudioSpectrumData)));
		SetDelegateOnSetupNewBillboardRenderer(delegateHandler.DelegateToFunctionPointer(new RendererSetupCallback(OnNewBillboardRendererSetup)));
		SetDelegateOnSetupNewRibbonRenderer(delegateHandler.DelegateToFunctionPointer(new RendererSetupCallback(OnNewRibbonRendererSetup)));
		SetDelegateOnSetupNewMeshRenderer(delegateHandler.DelegateToFunctionPointer(new RendererSetupCallback(OnNewMeshRendererSetup)));
		SetDelegateOnResizeRenderer(delegateHandler.DelegateToFunctionPointer(new RendererResizeCallback(OnRendererResize)));
		SetDelegateOnSetRendererActive(delegateHandler.DelegateToFunctionPointer(new SetRendererActiveCallback(OnSetRendererActive)));
		SetDelegateOnSetMeshInstancesCount(delegateHandler.DelegateToFunctionPointer(new SetMeshInstancesCountCallback(OnSetMeshInstancesCount)));
		SetDelegateOnSetMeshInstancesBuffer(delegateHandler.DelegateToFunctionPointer(new SetMeshInstancesBufferCallback(OnSetMeshInstancesBuffer)));
		SetDelegateOnRetrieveRendererBufferInfo(delegateHandler.DelegateToFunctionPointer(new RetrieveRendererBufferInfoCallback(OnRetrieveRendererBufferInfo)));
		SetDelegateOnUpdateRendererBounds(delegateHandler.DelegateToFunctionPointer(new RendererBoundsUpdateCallback(OnRendererBoundsUpdate)));

#if UNITY_EDITOR
		SetDelegateOnAssetChange(delegateHandler.DelegateToFunctionPointer(new AssetChangeCallback(OnAssetChange)));
		SetDelegateOnImportError(delegateHandler.DelegateToFunctionPointer(new ImportErrorCallback(OnImportError)));
		SetDelegateOnEffectDependencyFound(delegateHandler.DelegateToFunctionPointer(new EffectDependencyFoundCallback(OnEffectDependencyFound)));
		SetDelegateOnEffectAttributeFound(delegateHandler.DelegateToFunctionPointer(new EffectAttributeFoundCallback(OnEffectAttributeFound)));
		SetDelegateOnEffectSamplerFound(delegateHandler.DelegateToFunctionPointer(new EffectSamplerFoundCallback(OnEffectSamplerFound)));
		SetDelegateOnGetEffectInfo(delegateHandler.DelegateToFunctionPointer(new GetEffectInfoCallback(OnGetEffectInfo)));
		SetDelegateOnGetAllAssetPath(delegateHandler.DelegateToFunctionPointer(new GetAllAssetPathCallback(OnGetAllAssetPath)));
		SetDelegateOnRendererNamesAdded(delegateHandler.DelegateToFunctionPointer(new RendererNamesAddedCallback(OnRendererNamesAdded)));
#endif

		m_Samples = new float[1024];
		m_SamplesHandle = GCHandle.Alloc(m_Samples, GCHandleType.Pinned);
#if		UNITY_EDITOR
		m_CurrentVersionString = Marshal.PtrToStringAnsi(GetRuntimeVersion());
#endif
		m_IsStarted = true;
	}

#if UNITY_EDITOR
	protected static void StartupPopcornFileWatcher()
	{
		SMirrorPackFxSettings packFxSettings;

		if (string.IsNullOrEmpty(PKFxSettings.PopcornPackFxPath))
			packFxSettings.m_PopcornPackFxPath = null;
		else
			packFxSettings.m_PopcornPackFxPath = Path.GetFullPath(PKFxSettings.PopcornPackFxPath);
		if (string.IsNullOrEmpty(PKFxSettings.PopcornPackFxPath))
			packFxSettings.m_UnityPackFxPath = null;
		else
			packFxSettings.m_UnityPackFxPath = PKFxSettings.UnityPackFxPath;
		StartPackWatcher(ref packFxSettings);
	}

	protected static void LockPackWatcherChanges()
	{
		LockPackWatcher();
	}

	protected static void UnlockPackWatcherChanges()
	{
		UnlockPackWatcher();
	}

	protected static void ReimportAllAssets()
	{
		//Code Editor
		ReimportAllAssets(true);
		AssetDatabase.Refresh();
	}
#endif

	//----------------------------------------------------------------------------

	// Allocates a pinned buffer and frees it when destroyed
	class PinnedData
	{
		public IntPtr m_PinnedData = IntPtr.Zero;

		public void PinData(byte[] rawData, int size)
		{
			m_PinnedData = Marshal.AllocHGlobal(size);
			Marshal.Copy(rawData, 0, m_PinnedData, size);
		}

		public void FreePinnedData()
		{
			if (m_PinnedData != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(m_PinnedData);
				m_PinnedData = IntPtr.Zero;
			}
		}

		~PinnedData()
		{
			FreePinnedData();
		}
	}

	//----------------------------------------------------------------------------

	class PinnedAssetData : PinnedData
	{
		public PKFxAsset	m_Asset;

		public PinnedAssetData(PKFxAsset asset)
		{
			Debug.Assert(asset.m_Data.Length > 0, asset.m_AssetName + " no data");
			int size = Marshal.SizeOf(asset.m_Data[0]) * asset.m_Data.Length;
			m_PinnedData = Marshal.AllocHGlobal(size);
			Marshal.Copy(asset.m_Data, 0, m_PinnedData, size);
			//PinData(asset.m_Data, size);
			m_Asset = asset;
		}
	};

	//----------------------------------------------------------------------------

	class PinnedRuntimeMeshAssetData : PinnedData
	{
		public PKFxRuntimeMeshAsset m_Asset;

		public PinnedRuntimeMeshAssetData(PKFxRuntimeMeshAsset asset)
		{
			Debug.Assert(asset.m_Data.Length > 0, asset.m_AssetName + " no data");
			int size = Marshal.SizeOf(asset.m_Data[0]) * asset.m_Data.Length;
			m_PinnedData = Marshal.AllocHGlobal(size);
			Marshal.Copy(asset.m_Data, 0, m_PinnedData, size);
			//PinData(asset.m_Data, size);
			m_Asset = asset;
		}
	};

	//----------------------------------------------------------------------------

	class PinnedTexData : PinnedData
	{
		public Texture2D	m_Asset;
		public ulong		m_DataLength;

		public PinnedTexData(Texture2D t)
		{
			m_Asset = t;
			byte[] header = new byte[16];
			byte[] width = BitConverter.GetBytes(m_Asset.width);
			byte[] height = BitConverter.GetBytes(m_Asset.height);
			byte[] imgData = m_Asset.GetRawTextureData();
			byte[] size = BitConverter.GetBytes(imgData.Length);
			byte[] format = BitConverter.GetBytes((int)PKFxFX.ResolveImageFormat(m_Asset, ref imgData));

			Array.Copy(width, header, width.Length);
			Array.Copy(height, 0, header, width.Length, height.Length);
			Array.Copy(size, 0, header, width.Length + height.Length, size.Length);
			Array.Copy(format, 0, header, width.Length + height.Length + size.Length, format.Length);

			byte[] data = new byte[header.Length + imgData.Length];
			Array.Copy(header, data, header.Length);
			Array.Copy(imgData, 0, data, header.Length, imgData.Length);

#if NETFX_CORE
				m_DataLength = (ulong)data.Length;
#else
				m_DataLength = (ulong)data.LongLength;
#endif

			if (data.Length != 0)
				PinData(data, data.Length);
			else
				Debug.LogError("[PKFX] Sampler " + m_Asset.name + " : Could not get raw texture data.");
		}
	};

	//----------------------------------------------------------------------------

	protected static IntPtr PinAssetData(PKFxAsset asset)
	{
		var pinnedData = new PinnedAssetData(asset);
		if (m_NeedsFreeing == null)
			m_NeedsFreeing = new List<PinnedData>();
		m_NeedsFreeing.Add(pinnedData);
		return pinnedData.m_PinnedData;
	}

	//----------------------------------------------------------------------------

	protected static IntPtr PinAssetData(PKFxRuntimeMeshAsset asset)
	{
		var pinnedData = new PinnedRuntimeMeshAssetData(asset);
		if (m_NeedsFreeing == null)
			m_NeedsFreeing = new List<PinnedData>();
		m_NeedsFreeing.Add(pinnedData);
		return pinnedData.m_PinnedData;
	}

	//----------------------------------------------------------------------------

	protected static ulong PinTextureData(Texture2D tex, ref IntPtr handler)
	{
		var pinnedData = new PinnedTexData(tex);
		if (m_NeedsFreeing == null)
			m_NeedsFreeing = new List<PinnedData>();
		m_NeedsFreeing.Add(pinnedData);
		handler = pinnedData.m_PinnedData;
		return pinnedData.m_DataLength;
	}
	
	//----------------------------------------------------------------------------

	private static void WalkDependencies(PKFxFxAsset fxAsset)
	{

		foreach (PKFxFxAsset.DependencyDesc depDesc in fxAsset.m_Dependencies)
		{
			PKFxFxAsset depAsset = depDesc.m_Object as PKFxFxAsset;
			Texture2D depTex = depDesc.m_Object as Texture2D;
			AudioClip depAudio = depDesc.m_Object as AudioClip;
			PKFxFontMetricsAsset depFontMetrics = depDesc.m_Object as PKFxFontMetricsAsset;
			PKFxSimCacheAsset depSimCache = depDesc.m_Object as PKFxSimCacheAsset;
			PKFxAnimationAsset depAnimation = depDesc.m_Object as PKFxAnimationAsset;
			PKFxAtlasAsset depAtlas = depDesc.m_Object as PKFxAtlasAsset;
			PKFxMeshAsset depMesh = depDesc.m_Object as PKFxMeshAsset;

			if (depAsset != null && !m_Dependencies.Contains(depAsset))
			{
				m_Dependencies.Add(depAsset);
				WalkDependencies(depAsset);
			}
			else if (depTex != null && !m_TexDependencies.Contains(depTex))
				m_TexDependencies.Add(depTex);
			else if (depAudio != null)
				PKFxSoundManager.AddSound(depDesc);
			else if (depFontMetrics != null && !m_Dependencies.Contains(depFontMetrics))
				m_Dependencies.Add(depFontMetrics);
			else if (depSimCache != null && !m_Dependencies.Contains(depSimCache))
				m_Dependencies.Add(depSimCache);
			else if (depAnimation != null && !m_Dependencies.Contains(depAnimation))
				m_Dependencies.Add(depAnimation);
			else if (depAtlas != null && !m_Dependencies.Contains(depAtlas))
				m_Dependencies.Add(depAtlas);
			else if (depMesh != null && !m_Dependencies.Contains(depMesh))
				m_Dependencies.Add(depMesh);
		}
	}

	//----------------------------------------------------------------------------

	protected static bool _PreloadFxDependencies(PKFxFxAsset fxToPreload)
	{
		if (fxToPreload != null)
		{
			var loading = fxToPreload as PKFxAsset;
			Debug.Assert(fxToPreload.m_Data.Length > 0);
			Debug.Assert(loading.m_Data.Length > 0);
			if (!m_Dependencies.Contains(loading))
			{
				m_Dependencies.Add(loading);
				WalkDependencies(fxToPreload);
			}
			return true;
		}
		else
			Debug.LogError("[PKFX] Attempting to load null asset dependency.");
		return false;
	}

	//----------------------------------------------------------------------------

	protected static bool PreloadSceneMesh(PKFxMeshAsset mesh)
	{
		if (mesh != null)
		{
			var loading = mesh as PKFxAsset;
			Debug.Assert(mesh.m_Data.Length > 0, "scene mesh " + mesh.m_AssetName + " len " + mesh.m_Data.Length);
			if (!m_Dependencies.Contains(loading))
			{
				m_Dependencies.Add(loading);
			}
			return true;
		}
		else
			Debug.LogError("[PKFX] Attempting to load null scene mesh.");
		return false;
	}

	//----------------------------------------------------------------------------

	protected static void UnloadAllFxDependencies()
	{
		// May be null when recompiling C#
		if (m_NeedsFreeing != null)
			m_NeedsFreeing.Clear();
		if (m_Dependencies != null)
			m_Dependencies.Clear();
		if (m_TexDependencies != null)
			m_TexDependencies.Clear();
	}

	//----------------------------------------------------------------------------

	protected static bool UnloadRuntimeResource(string assetName)
	{
		foreach (GameObject go in m_RuntimeAssets)
		{
			PKFxAsset a = go.GetComponent<PKFxAsset>();
			if (a.m_AssetName == assetName)
			{
				m_RuntimeAssets.Remove(go);
				return true;
			}
		}
		return false;
	}

	//----------------------------------------------------------------------------

	protected static void FlushRuntimeResource(string assetName)
	{
		m_RuntimeAssets.Clear();
	}

	//----------------------------------------------------------------------------
	//----------------------------------------------------------------------------

	// We Are matching the material in the FrameData here:
	public enum	EMaterialFlags : int
	{
		Has_RibbonComplex = (1 << 0),
		Has_AnimBlend = (1 << 1),
		Has_AlphaRemap = (1 << 2),
		Has_Lighting = (1 << 3),
		Has_Soft = (1 << 4),
		Has_Distortion = (1 << 5),
		Has_Color = (1 << 6),
		Has_Diffuse = (1 << 7),
		Has_DoubleSided = (1 << 8),
		Has_CastShadow = (1 << 9),
	};

	public enum EUniformFlags : int
	{
		Is_AdditiveAlphaBlend = (1 << 0),
		Is_Additive = (1 << 1),
		Is_AdditiveNoAlpha = (1 << 2),
		Is_RotateTexture = (1 << 3),
		Is_OrthoCam = (1 << 4),
	};

	public enum ERendererType
	{
		Billboard,
		Ribbon,
		Mesh,
	};

	public class SBatchDesc
	{
		public ERendererType	m_Type;
		public int				m_MaterialFlags;
		public int				m_UniformFlags;
		public int				m_DrawOrder;
		public string			m_UserData;
		public string			m_DiffuseMap;
		public string			m_NormalMap;
		// Billboards/Ribbons:
		public string			m_AlphaRemap;
		//  For meshes:
		public string			m_MeshAsset;
		public string			m_SpecularMap;
		public int				m_SubMeshID;

		public float			m_InvSofnessDistance;

		public string			m_GeneratedName;

		public string			m_EffectNames;

		public SBatchDesc(ERendererType type, SPopcornRendererDesc desc)
		{
			string userDataStr = null;
			string diffuseStr = null;
			string normalStr = null;
			string alphaRemapStr = null;
			string effectNamesStr = null;

			if (desc.m_UserData != IntPtr.Zero)
				userDataStr = Marshal.PtrToStringAnsi(desc.m_UserData);
			if (desc.m_DiffuseMap != IntPtr.Zero)
				diffuseStr = PKFxSettings.UnityPackFxPath + "/" + Marshal.PtrToStringAnsi(desc.m_DiffuseMap);
			if (desc.m_NormalMap != IntPtr.Zero)
				normalStr = PKFxSettings.UnityPackFxPath + "/" + Marshal.PtrToStringAnsi(desc.m_NormalMap);
			if (desc.m_AlphaRemap != IntPtr.Zero)
				alphaRemapStr = PKFxSettings.UnityPackFxPath + "/" + Marshal.PtrToStringAnsi(desc.m_AlphaRemap);
			if (desc.m_EffectNames != IntPtr.Zero)
				effectNamesStr = Marshal.PtrToStringAnsi(desc.m_EffectNames);

			m_Type = type;
			m_MaterialFlags = desc.m_MaterialFlags;
			m_UniformFlags = desc.m_UniformFlags;
			m_DrawOrder = desc.m_DrawOrder;
			m_UserData = userDataStr;
			m_DiffuseMap = diffuseStr;
			m_NormalMap = normalStr;
			m_AlphaRemap = alphaRemapStr;
			m_InvSofnessDistance = desc.m_InvSofnessDistance;
			m_EffectNames = effectNamesStr;

			m_GeneratedName = GenerateNameFromDescription();
		}

		public SBatchDesc(SMeshRendererDesc desc)
		{
			string userDataStr = null;
			string diffuseStr = null;
			string normalStr = null;
			string specularStr = null;
			string effectNamesStr = null;
			string meshAssetStr = null;

			if (desc.m_UserData != IntPtr.Zero)
				userDataStr = Marshal.PtrToStringAnsi(desc.m_UserData);
			if (desc.m_DiffuseMap != IntPtr.Zero)
				diffuseStr = PKFxSettings.UnityPackFxPath + "/" + Marshal.PtrToStringAnsi(desc.m_DiffuseMap);
			if (desc.m_NormalMap != IntPtr.Zero)
				normalStr = PKFxSettings.UnityPackFxPath + "/" + Marshal.PtrToStringAnsi(desc.m_NormalMap);
			if (desc.m_SpecularMap != IntPtr.Zero)
				specularStr = PKFxSettings.UnityPackFxPath + "/" + Marshal.PtrToStringAnsi(desc.m_SpecularMap);
			if (desc.m_EffectNames != IntPtr.Zero)
				effectNamesStr = Marshal.PtrToStringAnsi(desc.m_EffectNames);
			if (desc.m_MeshAsset != IntPtr.Zero)
			{
				meshAssetStr = Path.ChangeExtension(PKFxSettings.UnityPackFxPath + "/" + Marshal.PtrToStringAnsi(desc.m_MeshAsset), ".fbx");
			}
				

			m_Type = ERendererType.Mesh;
			m_MaterialFlags = desc.m_MaterialFlags;
			m_UniformFlags = desc.m_UniformFlags;
			m_DrawOrder = desc.m_DrawOrder;
			m_UserData = userDataStr;
			m_DiffuseMap = diffuseStr;
			m_NormalMap = normalStr;
			m_SpecularMap = specularStr;
			m_EffectNames = effectNamesStr;
			m_MeshAsset = meshAssetStr;
			m_SubMeshID = desc.m_SubMeshID;

			m_GeneratedName = GenerateNameFromDescription();
		}

		public bool HasMaterialFlag(EMaterialFlags flag)
		{
			return (m_MaterialFlags & (int)flag) == (int)flag;
		}

		public bool HasUniformFlag(EUniformFlags flag)
		{
			return (m_UniformFlags & (int)flag) == (int)flag;
		}

		// All of those functions are used to generate a description for the renderer:
		public static string MaterialFlagsToString(int materialFlags)
		{
			string finalName = "";

			if ((materialFlags & (int)EMaterialFlags.Has_RibbonComplex) != 0)
				finalName += " RibbonComplex";
			if ((materialFlags & (int)EMaterialFlags.Has_AnimBlend) != 0)
				finalName += " AnimBlend";
			if ((materialFlags & (int)EMaterialFlags.Has_AlphaRemap) != 0)
				finalName += " AlphaRemap";
			if ((materialFlags & (int)EMaterialFlags.Has_Lighting) != 0)
				finalName += " Lighting";
			if ((materialFlags & (int)EMaterialFlags.Has_Soft) != 0)
				finalName += " Soft";
			if ((materialFlags & (int)EMaterialFlags.Has_Distortion) != 0)
				finalName += " Distortion";
			if ((materialFlags & (int)EMaterialFlags.Has_Color) != 0)
				finalName += " Color";
			if ((materialFlags & (int)EMaterialFlags.Has_Diffuse) != 0)
				finalName += " Diffuse";
			if ((materialFlags & (int)EMaterialFlags.Has_DoubleSided) != 0)
				finalName += " DoubleSided";
			if ((materialFlags & (int)EMaterialFlags.Has_CastShadow) != 0)
				finalName += " CastShadow";
			return finalName.Length == 0 ? " MaterialBasic" : finalName;
		}

		public static string UniformFlagsToString(int uniformFlags)
		{
			string finalName = "";

			if ((uniformFlags & (int)EUniformFlags.Is_AdditiveAlphaBlend) != 0)
				finalName += " AdditiveAlphaBlend";
			if ((uniformFlags & (int)EUniformFlags.Is_Additive) != 0)
				finalName += " Additive";
			if ((uniformFlags & (int)EUniformFlags.Is_AdditiveNoAlpha) != 0)
				finalName += " AdditiveNoAlpha";
			if ((uniformFlags & (int)EUniformFlags.Is_RotateTexture) != 0)
				finalName += " RotateTexture";
			if ((uniformFlags & (int)EUniformFlags.Is_OrthoCam) != 0)
				finalName += " OrthoCam";
			return finalName.Length == 0 ? " AlphaBlend" : finalName;
		}

		public string		GenerateNameFromDescription()
		{
			string	finalName;

			if (m_Type == ERendererType.Billboard)
				finalName = "Billboard";
			else if (m_Type == ERendererType.Ribbon)
				finalName = "Ribbon";
			else if (m_Type == ERendererType.Mesh)
				finalName = "Mesh";
			else
				finalName = "Unknown";
			finalName += MaterialFlagsToString(m_MaterialFlags);
			finalName += UniformFlagsToString(m_UniformFlags);
			finalName += " ";
			finalName += m_UserData == null ? "(none)" : m_UserData;
			finalName += " ";
			finalName += m_DiffuseMap == null ? "(none)" : m_DiffuseMap;
			finalName += " ";
			finalName += m_NormalMap == null ? "(none)" : m_NormalMap;
			if (m_Type != ERendererType.Mesh)
			{
				finalName += " ";
				finalName += m_AlphaRemap == null ? "(none)" : m_AlphaRemap;
				finalName += " ";
				finalName += m_InvSofnessDistance;
			}
			else
			{
				finalName += " ";
				finalName += m_SpecularMap == null ? "(none)" : m_SpecularMap;
			}
			return finalName;
		}
	}

	public class SMeshDesc
	{
		public MeshFilter		m_Slice;
		public Material			m_Material;
		public GameObject		m_RenderingObject;

		public SBatchDesc		m_BatchDesc;

		public PKFxMeshInstancesRenderer m_InstancesRenderer;

#if UNITY_EDITOR
		public Color			m_BoundsDebugColor;
#endif

		public SMeshDesc(Material mat, SBatchDesc batchDesc, GameObject renderingObject)
		{
			m_Slice = null;
			m_RenderingObject = renderingObject;
			m_Material = mat;
			m_BatchDesc = batchDesc;
			m_InstancesRenderer = null;
#if		UNITY_EDITOR
			m_BoundsDebugColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
#endif
		}

		public SMeshDesc(MeshFilter m, Material mat, SBatchDesc batchDesc, PKFxMeshInstancesRenderer mir, GameObject renderingObject)
		{
			m_Slice = m;
			m_RenderingObject = renderingObject;
			m_Material = mat;
			m_BatchDesc = batchDesc;
			m_InstancesRenderer = mir;
#if UNITY_EDITOR
			m_BoundsDebugColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
#endif
		}

		public bool HasMaterialFlag(EMaterialFlags flag)
		{
			return (m_BatchDesc.m_MaterialFlags & (int)flag) == (int)flag;
		}

		public bool HasUniformFlag(EUniformFlags flag)
		{
			return (m_BatchDesc.m_UniformFlags & (int)flag) == (int)flag;
		}
	}

	//----------------------------------------------------------------------------

	protected static void ClearRenderers()
	{
		m_Renderers.Clear();
		if (m_RenderersRoot != null)
		{
			foreach (Transform t in m_RenderersRoot.transform)
			{
				Transform.DestroyImmediate(t.gameObject);
			}
			Transform.DestroyImmediate(m_RenderersRoot);
			m_RenderersRoot = null;
		}
	}


	//----------------------------------------------------------------------------

	private static GameObject GetNewRenderingObject(string name)
	{
		if (m_RenderersRoot == null)
		{
			m_RenderersRoot = new GameObject("PopcornFX Renderers");
			m_RenderersRoot.transform.position = Vector3.zero;
			UnityEngine.Object.DontDestroyOnLoad(m_RenderersRoot);
		}
		GameObject gameObject = new GameObject(name);
		gameObject.transform.parent = m_RenderersRoot.transform;
		return gameObject;
	}

	//----------------------------------------------------------------------------

	private static void SetupSliceInRenderingObject(SMeshDesc d)
	{
		GameObject gameObject = d.m_RenderingObject;

		gameObject.SetActive(false);

		d.m_Slice = d.m_RenderingObject.AddComponent<MeshFilter>();
		Mesh		mesh = d.m_Slice.mesh;

		mesh.MarkDynamic();
		mesh.Clear();

		// Load the default size configuration
		string particleMeshGeneratedName = d.m_BatchDesc.m_GeneratedName;
		PKFxSettings.SParticleMeshDefaultSize meshSizeToUpdate = GetParticleMeshDefaultSizeSettings(particleMeshGeneratedName);

		if (meshSizeToUpdate != null)
		{
			uint vertexCount = (uint)meshSizeToUpdate.m_DefaultVertexBufferSize;
			uint indexCount = (uint)meshSizeToUpdate.m_DefaultIndexBufferSize;
			bool useLargeIndices = vertexCount > UInt16.MaxValue;

			ResizeParticleMeshBuffer(mesh, d, vertexCount, indexCount, vertexCount, indexCount, useLargeIndices);
		}

		var renderer = gameObject.AddComponent<MeshRenderer>();
		renderer.material = d.m_Material;

		PKFxSettings.MaterialFactory.SetupRenderer(d.m_BatchDesc, gameObject, renderer);
	}

	//----------------------------------------------------------------------------
	// Sets up the root gameobject for the different mesh "slices" with the same material.
	private static int SetupRenderingObject(GameObject renderingObject,
											SBatchDesc batchDesc,
											Material mat)
	{
		int newId = m_CurrentRenderersGUID++;

		renderingObject.name += " " + newId;

		var meshDesc = new SMeshDesc(mat, batchDesc, renderingObject);

		SetupSliceInRenderingObject(meshDesc);

		m_Renderers.Add(meshDesc);

		return newId;
	}

	//----------------------------------------------------------------------------

	private static int SetupMeshRenderingObject(GameObject renderingObject, SBatchDesc batchDesc, Material mat)
	{
		int newId = m_CurrentRenderersGUID++;

		renderingObject.name += " " + newId;
		var renderer = renderingObject.AddComponent<PKFxMeshInstancesRenderer>();
		renderer.m_Material = mat;

		PKFxSettings.MaterialFactory.SetupMeshRenderer(batchDesc, renderingObject, renderer);

		var filter = renderingObject.AddComponent<MeshFilter>();
		filter.mesh = renderer.m_Meshes[0];
		
		m_Renderers.Add(new SMeshDesc(filter, mat, batchDesc, renderer, renderingObject));
		Debug.Assert(m_Renderers[newId].m_Slice.mesh == filter.mesh);

		return newId;
	}
	
	//----------------------------------------------------------------------------

	private static PKFxSettings.SParticleMeshDefaultSize	GetParticleMeshDefaultSizeSettings(string meshName)
	{
		foreach (PKFxSettings.SParticleMeshDefaultSize meshConf in PKFxSettings.MeshesDefaultSize)
		{
			if (meshConf.m_GeneratedName == meshName)
			{
				return meshConf; // The configuration already exists
			}
		}
		return null;
	}

	//----------------------------------------------------------------------------


	#region Conf file stuff

	private static void EnableFileLoggingIFN(bool enable)
	{
		if (Application.platform == RuntimePlatform.Android ||
		    Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.Switch)
		{
			m_HasFileLogging = false;
			return ;
		}
		try
		{
			m_HasFileLogging = enable;
			if (enable && !File.Exists(m_LogFilePath))
			{
				FileStream fs = File.Create(m_LogFilePath);
#if !NETFX_CORE
					fs.Close();
#endif
			}
			if (!enable && File.Exists(m_LogFilePath))
				File.Delete(m_LogFilePath);
		}
		catch
		{
			Debug.LogError("[PKFX] Setting up file logging failed.");
		}
	}

	public static bool FileLoggingEnabled()
	{
		return m_HasFileLogging;
	}
	#endregion

	#region Utils
	//----------------------------------------------------------------------------
	//	Utils
	//----------------------------------------------------------------------------

	protected static bool arrayContains(string[] array, string s)
	{
		foreach (string _s in array)
		{
			if (_s == s)
				return true;
		}
		return false;
	}

	//----------------------------------------------------------------------------

	private static T[] ForFill<T>(T value, int nbElts)
	{
		var a = new T[nbElts];
		for (int i = 0; i < nbElts; ++i)
		{
			a[i] = value;
		}
		return a;
	}
	#endregion

}
