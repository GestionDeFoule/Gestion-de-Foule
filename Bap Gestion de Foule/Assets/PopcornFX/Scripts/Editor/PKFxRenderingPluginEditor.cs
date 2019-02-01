//----------------------------------------------------------------------------
// Created on Thu Jun 26 15:28:42 2014 Raphael Thoulouze
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
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(PKFxRenderingPlugin))]
[CanEditMultipleObjects]
public class PKFxRenderingPluginEditor : Editor
{
	SerializedProperty		m_ShowAdvanced;
	SerializedProperty		m_EnableDistortion;
	SerializedProperty		m_EnableBlur;
	SerializedProperty		m_BlurFactor;
	SerializedProperty		m_UseSceneMesh;
	SerializedProperty		m_SceneMesh;

	//----------------------------------------------------------------------------

	void OnEnable()
	{
		m_EnableDistortion = serializedObject.FindProperty("m_EnableDistortion");
		m_EnableBlur = serializedObject.FindProperty("m_EnableBlur");
		m_BlurFactor = serializedObject.FindProperty("m_BlurFactor");
		m_UseSceneMesh = serializedObject.FindProperty("m_UseSceneMesh");
		m_SceneMesh = serializedObject.FindProperty("m_SceneMesh");
	}

	//----------------------------------------------------------------------------

	public override void OnInspectorGUI()
	{
		EditorGUILayout.LabelField("PopcornFX plugin "
									+ PKFxManagerImpl.m_PluginVersion
									+ " (Build "
									+ PKFxManagerImpl.m_CurrentVersionString + ")");

		DrawDefaultInspector();

		EditorGUILayout.PropertyField(m_EnableDistortion);
		EditorGUILayout.PropertyField(m_EnableBlur);
		if (m_EnableBlur.boolValue)
		{
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(m_BlurFactor);
			m_BlurFactor.floatValue = Mathf.Clamp(m_BlurFactor.floatValue, 0.0f, 1.0f);
			EditorGUI.indentLevel--;
		}
		HandleSceneMesh();

		serializedObject.ApplyModifiedProperties();
	}

	//----------------------------------------------------------------------------

	private void HandleSceneMesh()
	{
		EditorGUILayout.PropertyField(m_UseSceneMesh);
		if (m_UseSceneMesh.boolValue)
		{
			EditorGUI.indentLevel++;
			if (PKFxSettings.EnableRaycastForCollisions)
				EditorGUILayout.HelpBox("Can't enable Scene Mesh if raycast for collisions is enabled in settings.", MessageType.Warning, true);
			else
				EditorGUILayout.PropertyField(m_SceneMesh, new GUIContent("Mesh Asset"));
			EditorGUI.indentLevel--;
		}
	}

	//----------------------------------------------------------------------------

	[InitializeOnLoad]
	public class PlayModeChangeWatcher
	{
		static PlayModeChangeWatcher()
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
			{
				PKFxManager.SetSceneMesh(null);
				PKFxManager.ResetAndUnloadAllEffects();
			}
			EditorApplication.playModeStateChanged += PlaymodeStateChanged;
		}

		private static void PlaymodeStateChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.ExitingPlayMode)
			{
				// Start by clearing the FXs in the C# so they don't get updated
				foreach (KeyValuePair<int, PKFxFX> effect in PKFxFX.g_ListEffects)
				{
					effect.Value.DestroyIFN(false);
				}
				PKFxFX.g_ListEffects.Clear();
				// Then clear the native side:
				PKFxManager.ClearRenderers();
				PKFxManager.SetSceneMesh(null);
				PKFxManager.ResetAndUnloadAllEffects();

				// We also save the configuration file:
				if (PKFxSettings.AutomaticMeshResizing)
					EditorUtility.SetDirty(PKFxSettings.Instance);
				PKFxManager.StartupPopcorn(false);
				PKFxManager.RestartPackWatcher();
			}
			else if (state == PlayModeStateChange.EnteredPlayMode)
			{
				PKFxManager.PausePackWatcher();
			}
		}
	}
}
