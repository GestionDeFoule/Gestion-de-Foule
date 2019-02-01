using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ImportPKFxListEditor : EditorWindow
{
	public List<bool> m_checked = new List<bool>();
	private List<string> m_Paths;
	public List<string> Paths
	{
		get
		{
			return m_Paths;
		}
		set
		{
			m_Paths = value;
			m_checked.RemoveAll(x => true);
			for (int i = 0; i < m_Paths.Count;++i)
			{
				m_checked.Add(true);
			}
		}
	}

	public Vector2 scrollVector = new Vector2(0, 0);

	public ImportPKFxListEditor()
	{
	}

	void OnGUI()
	{

		EditorGUILayout.LabelField("Select Effects:");

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Select All"))
		{
			for (int i = 0; i < m_checked.Count; ++i)
			{
				m_checked[i] = true;
			}
		}
		if (GUILayout.Button("Select None"))
		{
			for (int i = 0; i < m_checked.Count; ++i)
			{
				m_checked[i] = false;
			}
		}
		EditorGUILayout.EndHorizontal();

		scrollVector = EditorGUILayout.BeginScrollView(scrollVector);

		for (int i = 0; i < Paths.Count; ++i)
		{
			EditorGUILayout.BeginHorizontal();
			if (EditorGUILayout.ToggleLeft(Paths[i], m_checked[i]) != m_checked[i])
				m_checked[i] = !m_checked[i];
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndScrollView();

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Bake and Import"))
		{
			Close();
			List<string> path = new List<string>();
			for (int i = 0; i < m_Paths.Count; ++i)
			{
				if (m_checked[i])
				{
					path.Add(m_Paths[i]);
				}
			}
			PKFxSettings.ReimportAssets(path);
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Cancel"))
			Close();
		EditorGUILayout.EndHorizontal();
	}

	void OnInspectorUpdate()
	{
		Repaint();
	}
}