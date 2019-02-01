using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

[CustomEditor(typeof(PKFxSettings))]
public class PKFxSettingsEditor : UnityEditor.Editor
{
	GUIContent debugEffectsBoundingBoxes = new GUIContent(" Debug the effects bounding boxes");
	GUIContent enableSoftParticlesLabel = new GUIContent(" Enable soft particles");
	GUIContent enableFileLogLabel = new GUIContent(" Enable File Log");
	GUIContent enableRaycastForCollisionsLabel = new GUIContent(" Enable raycast for collisions");
	GUIContent splitDrawCallsOfSoubleSidedParticlesLabel = new GUIContent(" Split the draw calls of the particles that require disabling the back-face culling");
	GUIContent disableDynamicEffectBoundsLabel = new GUIContent(" Disable dynamic effect bounds");
	GUIContent materialFactoryLabel = new GUIContent(" Material Factory");

	GUIContent timeMultiplierLabel = new GUIContent(" Time scale for the particle simulation");
	GUIContent singleThreadedExecLabel = new GUIContent(" Run PopcornFX on a single thread to avoid visual studio hangs");
	GUIContent splitUpdateInComponentsLabel = new GUIContent(" Splits the update in 3 components");
	GUIContent waitForUpdateOnRenderThreadLabel = new GUIContent(" Wait for the end of the particle update on the render thread");
	GUIContent overrideThreadPoolConfigLabel = new GUIContent(" Override the PopcornFX thread pool configuration");
	GUIContent automaticMeshResizingLabel = new GUIContent(" Automatic mesh resizing to avoid dynamic re-alloc during the next run");
	GUIContent vertexBufferSizeMultiplicatorLabel = new GUIContent(" Vertex buffer size multiplicator");
	GUIContent indexBufferSizeMultiplicatorLabel = new GUIContent(" Index buffer size multiplicator");

	

	const string UnityAssetFolder = "Assets";

	private static void _AddSortingLayerIFN(string layerName, bool isFirst)
	{
		SerializedObject tagsAndLayersManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
		SerializedProperty sortingLayersProp = tagsAndLayersManager.FindProperty("m_SortingLayers");

		//serch if the sorting layer already exist
		for (int i = 0; i < sortingLayersProp.arraySize; ++i)
		{
			var layer = sortingLayersProp.GetArrayElementAtIndex(i);
			if (layer.FindPropertyRelative("name").stringValue == layerName)
				return;
		}

		//insert the sorting layer
		int index = 0;
		if (!isFirst)
			index = sortingLayersProp.arraySize;
		sortingLayersProp.InsertArrayElementAtIndex(index);
		var newlayer = sortingLayersProp.GetArrayElementAtIndex(index);
		newlayer.FindPropertyRelative("uniqueID").intValue = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		newlayer.FindPropertyRelative("name").stringValue = layerName;
		tagsAndLayersManager.ApplyModifiedProperties();
		string beforeOrAfter = isFirst ? "before" : "after";
		Debug.Log("[PKFX] Adding the sorting layer \"" + layerName + "\" " + beforeOrAfter + " \"Default\"");
	}

	private static void _CheckForMultipleSettingsObject(PKFxSettings instance)
	{
		string[] guids = AssetDatabase.FindAssets("t:PKFxSettings");
		if (guids.Length > 1)
		{
			string instancePath = AssetDatabase.GetAssetPath(instance);
			string objPaths = "";
			foreach (var guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				if (path != instancePath)
					objPaths += " - " + path + "\n";
			}
			Debug.Assert(false, "[PKFX] Found more than one PKFxSettings object, loading " + instancePath + "\nThis objects will not be used: " + objPaths);
		}
	}

	public static PKFxSettings GetOrCreateSettingsAsset()
	{
		PKFxSettings instance = Resources.Load(PKFxSettings.kSettingsAssetName) as PKFxSettings;

		_CheckForMultipleSettingsObject(instance);

		if (instance == null)
		{
			string targetFolder = PKFxSettings.kSettingsPath;
			// no asset found, we need to create it. 
			if (!Directory.Exists(Path.Combine(UnityAssetFolder, targetFolder)))
			{
				targetFolder = "Resources";
				if (!Directory.Exists(Path.Combine(UnityAssetFolder, targetFolder)))
					AssetDatabase.CreateFolder(UnityAssetFolder, targetFolder);
			}

			string fullPath = Path.Combine(Path.Combine(UnityAssetFolder, targetFolder),
											PKFxSettings.kSettingsAssetName + PKFxSettings.kSettingsAssetExtension);

			if (targetFolder == "Resources")
				Debug.LogWarning("[PKFX] Unable to find the PopcornFX folder, creating the settings asset to " + fullPath + ".\n You should move it into PopcornFX/Resources.");

			instance = CreateInstance<PKFxSettings>();
			AssetDatabase.CreateAsset(instance, fullPath);
			AssetDatabase.SaveAssets();
		}
		return instance;
	}

	[MenuItem("Edit/Project Settings/PopcornFX")]
	public static void Edit()
	{
		Selection.activeObject = GetOrCreateSettingsAsset();

		ShowInspector();
	}

	void OnDisable()
	{
		// make sure the runtime code will load the Asset from Resources when it next tries to access this. 
		PKFxSettings.SetInstance(null);
	}

	[InitializeOnLoadMethod]
	private static void CreateSettingsAssetIFN()
	{
		if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
		{
			return;
		}
		GetOrCreateSettingsAsset();
		_AddSortingLayerIFN("PopcornFX", true);
		_AddSortingLayerIFN("PopcornFXUI", false);
	}

	public override void OnInspectorGUI()
	{
		PKFxSettings settings = (PKFxSettings)target;
		PKFxSettings.SetInstance(settings);
		
		using (var category = new PKFxEditorCategory(() => EditorGUILayout.Foldout(PKFxSettings.GeneralCategory, "General") ))
		{
			PKFxSettings.GeneralCategory = category.IsExpanded();
			if (category.IsExpanded())
			{
				DisplayGeneralCategory();
			}
		}
		using (var category = new PKFxEditorCategory(() => EditorGUILayout.Foldout(PKFxSettings.RenderingCategory, "Rendering")))
		{
			PKFxSettings.RenderingCategory = category.IsExpanded();
			if (category.IsExpanded())
			{
				DisplayRenderingCategory();
			}
		}
		using (var category = new PKFxEditorCategory(() => EditorGUILayout.Foldout(PKFxSettings.ThreadingCategory, "Multithreading")))
		{
			PKFxSettings.ThreadingCategory = category.IsExpanded();
			if (category.IsExpanded())
			{
				DisplayThreadingCategory();
			}
		}
		
		if (PKFxSettings.EnableFileLog != PKFxManagerImpl.FileLoggingEnabled())
			EditorGUILayout.HelpBox("At least one of the changes requires a restart of Unity to be effective.", MessageType.Warning, true);

		if (GUI.changed)
			EditorUtility.SetDirty(settings);
	}

	private void DisplayGeneralCategory()
	{
		GUIStyle boldStyleRed = new GUIStyle();
		boldStyleRed.fontStyle = FontStyle.Bold;
		boldStyleRed.normal.textColor = Color.red;
		boldStyleRed.hover.textColor = Color.red;

		EditorGUILayout.BeginHorizontal();
		PKFxSettings.TimeMultiplier = EditorGUILayout.Slider(timeMultiplierLabel, PKFxSettings.TimeMultiplier, 0.0f, 10.0f);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		PKFxSettings.EnableFileLog = EditorGUILayout.ToggleLeft(enableFileLogLabel, PKFxSettings.EnableFileLog);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		PKFxSettings.EnableRaycastForCollisions = EditorGUILayout.ToggleLeft(enableRaycastForCollisionsLabel, PKFxSettings.EnableRaycastForCollisions);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		
		if (!string.IsNullOrEmpty(PKFxSettings.PopcornPackFxPath))
		{
			EditorGUILayout.LabelField(PKFxSettings.PopcornPackFxPath);
		}
		else
		{
			EditorGUILayout.LabelField("<empty>", boldStyleRed);
		}
		if (GUILayout.Button("Set Pack Fx Path"))
		{
			string path = EditorUtility.OpenFolderPanel("Choose PopcornFx Asset Folder", "", "");
			if (Directory.Exists(path))
			{
				var fileUri = new Uri(path);
				var referenceUri = new Uri(Application.dataPath);
				PKFxSettings.PopcornPackFxPath = referenceUri.MakeRelativeUri(fileUri).ToString();
			}
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		if (!string.IsNullOrEmpty(PKFxSettings.UnityPackFxPath))
		{
			EditorGUILayout.LabelField(PKFxSettings.UnityPackFxPath);
		}
		else
		{
			EditorGUILayout.LabelField("<empty>", boldStyleRed);
		}
		
		if (GUILayout.Button("Set Unity Fx Path"))
		{
			string path = EditorUtility.OpenFolderPanel("Choose Unity Fx Assets Folder", "Resources", "");
			if (Directory.Exists(path))
			{
				PKFxSettings.UnityPackFxPath = path.Substring(Application.dataPath.Length);
			}
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Reimport Pack");
		if (GUILayout.Button("Choose files"))
		{
			PKFxSettings.GetAllAssetPath();
			ImportPKFxListEditor window = ScriptableObject.CreateInstance(typeof(ImportPKFxListEditor)) as ImportPKFxListEditor;
			window.Paths = PKFxSettings.AssetPathList;
			window.ShowUtility();
		}
		if (GUILayout.Button("All"))
		{
			PKFxSettings.ReimportAllAssets();
		}
		EditorGUILayout.EndHorizontal();
	}

	private void DisplayRenderingCategory()
	{
		EditorGUILayout.BeginHorizontal();
		PKFxSettings.DebugEffectsBoundingBoxes = EditorGUILayout.ToggleLeft(debugEffectsBoundingBoxes, PKFxSettings.DebugEffectsBoundingBoxes);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		PKFxSettings.MaterialFactory = EditorGUILayout.ObjectField(materialFactoryLabel, PKFxSettings.MaterialFactory, typeof(PKFxMaterialFactory), false) as PKFxMaterialFactory;
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		PKFxSettings.EnableSoftParticles = EditorGUILayout.ToggleLeft(enableSoftParticlesLabel, PKFxSettings.EnableSoftParticles);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		PKFxSettings.SplitDrawCallsOfSoubleSidedParticles = EditorGUILayout.ToggleLeft(splitDrawCallsOfSoubleSidedParticlesLabel, PKFxSettings.SplitDrawCallsOfSoubleSidedParticles);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		PKFxSettings.DisableDynamicEffectBounds = EditorGUILayout.ToggleLeft(disableDynamicEffectBoundsLabel, PKFxSettings.DisableDynamicEffectBounds);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		PKFxSettings.AutomaticMeshResizing = EditorGUILayout.ToggleLeft(automaticMeshResizingLabel, PKFxSettings.AutomaticMeshResizing);
		EditorGUILayout.EndHorizontal();

		using (new EditorGUI.DisabledScope(!PKFxSettings.AutomaticMeshResizing))
		{
			EditorGUI.indentLevel = 2;

			EditorGUILayout.BeginHorizontal();
			PKFxSettings.VertexBufferSizeMultiplicator = EditorGUILayout.FloatField(vertexBufferSizeMultiplicatorLabel, PKFxSettings.VertexBufferSizeMultiplicator);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			PKFxSettings.IndexBufferSizeMultiplicator = EditorGUILayout.FloatField(indexBufferSizeMultiplicatorLabel, PKFxSettings.IndexBufferSizeMultiplicator);
			EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel = 0;
		}

		DisplayPopcornFxRenderers();
	}

	private void DisplayThreadingCategory()
	{
		EditorGUILayout.BeginHorizontal();
		PKFxSettings.SingleThreadedExecution = EditorGUILayout.ToggleLeft(singleThreadedExecLabel, PKFxSettings.SingleThreadedExecution);
		EditorGUILayout.EndHorizontal();

		using (new EditorGUI.DisabledScope(PKFxSettings.SingleThreadedExecution))
		{
			using (new EditorGUI.DisabledScope(PKFxSettings.EnableRaycastForCollisions))
			{
				EditorGUILayout.BeginHorizontal();
				PKFxSettings.WaitForUpdateOnRenderThread = EditorGUILayout.ToggleLeft(waitForUpdateOnRenderThreadLabel, PKFxSettings.WaitForUpdateOnRenderThread);
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.BeginHorizontal();
			PKFxSettings.SplitUpdateInComponents = EditorGUILayout.ToggleLeft(splitUpdateInComponentsLabel, PKFxSettings.SplitUpdateInComponents);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			PKFxSettings.OverrideThreadPoolConfig = EditorGUILayout.ToggleLeft(overrideThreadPoolConfigLabel, PKFxSettings.OverrideThreadPoolConfig);
			EditorGUILayout.EndHorizontal();
		}
		DisplayThreadAffinities();
	}

	private Vector2 m_AffinityScrollPosition = new Vector2();

	private void DisplayThreadAffinities()
	{
		GUIStyle scrollViewStyle = new GUIStyle();
		scrollViewStyle.stretchHeight = true;
		scrollViewStyle.stretchWidth = true;

		GUIStyle boldStyleWhite = new GUIStyle();
		boldStyleWhite.fontStyle = FontStyle.Bold;
		boldStyleWhite.normal.textColor = Color.white;
		boldStyleWhite.hover.textColor = Color.white;

		GUIStyle boldStyleGreen = new GUIStyle();
		boldStyleGreen.fontStyle = FontStyle.Bold;
		boldStyleGreen.normal.textColor = Color.green;
		boldStyleGreen.hover.textColor = Color.green;

		GUIStyle boldStyleRed = new GUIStyle();
		boldStyleRed.fontStyle = FontStyle.Bold;
		boldStyleRed.normal.textColor = Color.red;
		boldStyleRed.hover.textColor = Color.red;

		using (new EditorGUI.DisabledScope(PKFxSettings.SingleThreadedExecution || !PKFxSettings.OverrideThreadPoolConfig))
		{
			int workerThreads = Mathf.Max(EditorGUILayout.DelayedIntField("Worker threads count", PKFxSettings.ThreadsAffinity.Count), 0);

			while (workerThreads > PKFxSettings.ThreadsAffinity.Count)
				PKFxSettings.ThreadsAffinity.Add(~0);
			while (workerThreads < PKFxSettings.ThreadsAffinity.Count)
				PKFxSettings.ThreadsAffinity.RemoveAt(PKFxSettings.ThreadsAffinity.Count - 1);

			m_AffinityScrollPosition = EditorGUILayout.BeginScrollView(m_AffinityScrollPosition, scrollViewStyle);

			for (int i = 0; i < PKFxSettings.ThreadsAffinity.Count; ++i)
			{
				int affinity = PKFxSettings.ThreadsAffinity[i];

				EditorGUILayout.LabelField("Affinity for Worker " + i + ":", affinity == 0 ? boldStyleRed : boldStyleGreen);

				EditorGUILayout.BeginHorizontal();

				if (EditorGUILayout.ToggleLeft("Full affinity", affinity == ~0, boldStyleWhite, GUILayout.Width(100)))
					affinity = ~0;
				else if (affinity == ~0)
					affinity = 0;

				for (int b = 0; b < 32; ++b)
				{
					bool curValue = (affinity & (1 << b)) != 0;
					bool nextValue = EditorGUILayout.ToggleLeft(b.ToString(), curValue, GUILayout.Width(35));

					if (nextValue)
					{
						affinity |= (1 << b);
					}
					else if (curValue)
					{
						affinity ^= (1 << b);
					}
				}
				EditorGUILayout.EndHorizontal();

				PKFxSettings.ThreadsAffinity[i] = affinity;
			}
			EditorGUILayout.EndScrollView();
		}
	}

	private Vector2 m_BuffersScrollPosition = new Vector2();

	private void DisplayPopcornFxRenderers()
	{
		GUIStyle scrollViewStyle = new GUIStyle();
		scrollViewStyle.stretchHeight = true;
		scrollViewStyle.stretchWidth = true;

		GUIStyle boldStyleBlack = new GUIStyle();
		boldStyleBlack.fontStyle = FontStyle.Bold;

		GUIStyle italicStyle = new GUIStyle();
		italicStyle.fontStyle = FontStyle.Italic;

		if (GUILayout.Button("Clear all particle meshes"))
		{
			PKFxSettings.MeshesDefaultSize.Clear();
		}

		m_BuffersScrollPosition = EditorGUILayout.BeginScrollView(m_BuffersScrollPosition, scrollViewStyle);

		using (new EditorGUI.DisabledScope(true))
		{
			foreach (PKFxSettings.SParticleMeshDefaultSize particleMesh in PKFxSettings.MeshesDefaultSize)
			{
				EditorGUI.indentLevel = 0;
				EditorGUILayout.LabelField(particleMesh.m_GeneratedName, boldStyleBlack);
				EditorGUI.indentLevel = 2;

				string[] effectNames = particleMesh.m_EffectNames.Split(';');

				foreach (string effectName in effectNames)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(effectName, italicStyle);
					EditorGUILayout.EndHorizontal();
				}

				EditorGUI.indentLevel = 4;

				EditorGUILayout.BeginVertical();
				particleMesh.m_DefaultVertexBufferSize = EditorGUILayout.IntField("Vertex count", particleMesh.m_DefaultVertexBufferSize);
				particleMesh.m_DefaultIndexBufferSize = EditorGUILayout.IntField("Index count", particleMesh.m_DefaultIndexBufferSize);

				if (PKFxSettings.DisableDynamicEffectBounds)
				{
					particleMesh.m_StaticWorldBounds.center = EditorGUILayout.Vector3Field("Static world bounds center", particleMesh.m_StaticWorldBounds.center);
					particleMesh.m_StaticWorldBounds.size = EditorGUILayout.Vector3Field("Static world bounds size", particleMesh.m_StaticWorldBounds.size);
				}

				EditorGUILayout.EndVertical();
			}
			EditorGUI.indentLevel = 0;
		}
		EditorGUILayout.EndScrollView();
	}

	private static void ShowInspector()
	{
		try
		{
			var editorAsm = typeof(UnityEditor.Editor).Assembly;
			var type = editorAsm.GetType("UnityEditor.InspectorWindow");
			UnityEngine.Object[] findObjectsOfTypeAll = Resources.FindObjectsOfTypeAll(type);

			if (findObjectsOfTypeAll.Length > 0)
			{
				((EditorWindow)findObjectsOfTypeAll [0]).Focus();
			}
			else
			{
				EditorWindow.GetWindow(type);
			}
		}
		catch
		{
		}
	}
}
