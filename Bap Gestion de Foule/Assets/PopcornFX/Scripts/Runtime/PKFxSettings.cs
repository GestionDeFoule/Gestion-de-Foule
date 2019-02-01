using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class PKFxSettings : ScriptableObject
{
	public const string			kSettingsAssetName = "PKFxSettings";
	public const string			kSettingsPath = "PopcornFX/Resources";
	public const string			kSettingsAssetExtension = ".asset";
	private static PKFxSettings g_Instance;

	public static void SetInstance(PKFxSettings settings)
	{
		g_Instance = settings;
	}

	public static PKFxSettings Instance
	{
		get
		{
			if (g_Instance == null)
			{
				g_Instance = Resources.Load(kSettingsAssetName) as PKFxSettings;
				if (g_Instance == null)
				{
					Debug.LogWarning("[PKFX] Can't load PKFxSettings asset, using default values...");
					g_Instance = CreateInstance<PKFxSettings>();
				}
				g_Instance.Setup();
			}
			return g_Instance;
		}
	}

	// General:
	[SerializeField]
	private bool							m_GeneralCategory = true;
	[SerializeField]
	private float							m_TimeMultiplier = 1.0f;
	[SerializeField]
	private bool							m_EnableFileLog = true;
	[SerializeField]
	private bool							m_EnableRaycastForCollisions = false;
	[SerializeField]
	private string							m_PopcornPackFxPath = null;
	[SerializeField]
	private string							m_UnityPackFxPath = "/PopcornFXAssets";

	public static bool GeneralCategory
	{
		get { return Instance.m_GeneralCategory; }
		set { Instance.m_GeneralCategory = value; }
	}

	public static float TimeMultiplier
	{
		get { return Instance.m_TimeMultiplier; }
		set { Instance.m_TimeMultiplier = value; }
	}

	public static bool EnableFileLog
	{
		get { return Instance.m_EnableFileLog; }
		set { Instance.m_EnableFileLog = value; }
	}

	public static bool EnableRaycastForCollisions
	{
		get { return Instance.m_EnableRaycastForCollisions; }
		set { Instance.m_EnableRaycastForCollisions = value; }
	}

	public static string PopcornPackFxPath
	{
		get { return Instance.m_PopcornPackFxPath; }
		set {
			if (value.CompareTo(Instance.m_PopcornPackFxPath) != 0)
			{
				Instance.m_PopcornPackFxPath = value;
#if UNITY_EDITOR
				PKFxManager.StartupPopcornFileWatcher();
#endif
			}
		}
	}

	public static string UnityPackFxPath
	{
		get { return Instance.m_UnityPackFxPath; }
		set
		{
			if (value.CompareTo(Instance.m_UnityPackFxPath) != 0)
			{
				Instance.m_UnityPackFxPath = value;
#if UNITY_EDITOR
				PKFxManager.StartupPopcornFileWatcher();
#endif
			}
		}
	}
	public static List<string> AssetPathList
	{
		get; set;
	}

#if UNITY_EDITOR
	public static void GetAllAssetPath()
	{
		PKFxManager.GetAllAssetPath();
	}

	public static void ReimportAllAssets()
	{
		PKFxManager.ReimportAllAssets();
	}

	public static void ReimportAssets(List<string> assetsList)
	{
		PKFxManager.ReimportAssets(assetsList);
	}
#endif

	// Rendering
	[System.Serializable]
	public class SParticleMeshDefaultSize
	{
		public string	m_EffectNames; // Just for ease to read in the editor, does not need to be serialized
		public string	m_GeneratedName;
		public int		m_DefaultVertexBufferSize;
		public int		m_DefaultIndexBufferSize;

		public Bounds m_StaticWorldBounds = new Bounds(Vector3.zero, new Vector3(100, 100, 100));
	}

	[SerializeField]
	private bool							m_RenderingCategory = false;
	[SerializeField]
	private bool							m_DebugEffectsBoundingBoxes = false;
	[SerializeField]
	private PKFxMaterialFactory				m_MaterialFactory = null;
	[SerializeField]
	private bool							m_EnableSoftParticles = true;
	[SerializeField]
	private bool							m_SplitDrawCallsOfSoubleSidedParticles = false;
	[SerializeField]
	private bool							m_DisableDynamicEffectBounds = false;
	[SerializeField]
	private bool							m_AutomaticMeshResizing = true;
	[SerializeField]
	private float							m_VertexBufferSizeMultiplicator = 0.5f;
	[SerializeField]
	private float							m_IndexBufferSizeMultiplicator = 0.5f;
	[SerializeField]
	private List<SParticleMeshDefaultSize>	m_MeshesDefaultSize = new List<SParticleMeshDefaultSize>();

	public static bool RenderingCategory
	{
		get { return Instance.m_RenderingCategory; }
		set { Instance.m_RenderingCategory = value; }
	}

	public static bool DebugEffectsBoundingBoxes
	{
		get { return Instance.m_DebugEffectsBoundingBoxes; }
		set { Instance.m_DebugEffectsBoundingBoxes = value; }
	}

	public static PKFxMaterialFactory MaterialFactory
	{
		get
		{
			if (Instance.m_MaterialFactory == null)
			{
				PKFxMaterialFactoryDefault materialFactoryDefault = ScriptableObject.CreateInstance<PKFxMaterialFactoryDefault>();
				materialFactoryDefault.SetupShaders();
				Instance.m_MaterialFactory = materialFactoryDefault;
			}
			return Instance.m_MaterialFactory;
		}
		set { Instance.m_MaterialFactory = value; }
	}

	public static bool EnableSoftParticles
	{
		get { return Instance.m_EnableSoftParticles; }
		set { Instance.m_EnableSoftParticles = value; }
	}

	public static bool SplitDrawCallsOfSoubleSidedParticles
	{
		get { return Instance.m_SplitDrawCallsOfSoubleSidedParticles; }
		set { Instance.m_SplitDrawCallsOfSoubleSidedParticles = value; }
	}

	public static bool DisableDynamicEffectBounds
	{
		get { return Instance.m_DisableDynamicEffectBounds; }
		set { Instance.m_DisableDynamicEffectBounds = value; }
	}

	public static bool AutomaticMeshResizing
	{
		get { return Instance.m_AutomaticMeshResizing; }
		set { Instance.m_AutomaticMeshResizing = value; }
	}

	public static float VertexBufferSizeMultiplicator
	{
		get { return Instance.m_VertexBufferSizeMultiplicator; }
		set { Instance.m_VertexBufferSizeMultiplicator = value; }
	}

	public static float IndexBufferSizeMultiplicator
	{
		get { return Instance.m_IndexBufferSizeMultiplicator; }
		set { Instance.m_IndexBufferSizeMultiplicator = value; }
	}

	public static List<SParticleMeshDefaultSize> MeshesDefaultSize
	{
		get { return Instance.m_MeshesDefaultSize; }
		set { Instance.m_MeshesDefaultSize = value; }
	}

	// Threading:
	[SerializeField]
	private bool							m_ThreadingCategory = false;
	[SerializeField]
	private bool							m_SingleThreadedExecution = false;
	[SerializeField]
	private bool							m_WaitForUpdateOnRenderThread = false;
	[SerializeField]
	private bool							m_SplitUpdateInComponents = false;
	[SerializeField]
	private bool							m_OverrideThreadPoolConfig = false;
	[SerializeField]
	private List<int>						m_ThreadsAffinity = new List<int>();

	public static bool ThreadingCategory
	{
		get { return Instance.m_ThreadingCategory; }
		set { Instance.m_ThreadingCategory = value; }
	}

	public static bool SingleThreadedExecution
	{
		get { return Instance.m_SingleThreadedExecution; }
		set { Instance.m_SingleThreadedExecution = value; }
	}

	public static bool WaitForUpdateOnRenderThread
	{
		get { return Instance.m_WaitForUpdateOnRenderThread; }
		set { Instance.m_WaitForUpdateOnRenderThread = value; }
	}

	public static bool SplitUpdateInComponents
	{
		get { return Instance.m_SplitUpdateInComponents; }
		set { Instance.m_SplitUpdateInComponents = value; }
	}

	public static bool OverrideThreadPoolConfig
	{
		get { return Instance.m_OverrideThreadPoolConfig; }
		set { Instance.m_OverrideThreadPoolConfig = value; }
	}

	public static List<int> ThreadsAffinity
	{
		get { return Instance.m_ThreadsAffinity; }
		set { Instance.m_ThreadsAffinity = value; }
	}

	public void Setup()
	{
		if (PopcornPackFxPath == null || !Directory.Exists(PopcornPackFxPath))
		{
			Debug.LogWarning("[PKFX]Source Pack path is required to import your FXs", this);
		}
	}
}
