﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PKFxFxAsset))]
public class PKFxAssetEditor : Editor
{
	public override void OnInspectorGUI()
	{
		//var m_FXText = serializedObject.FindProperty("m_FXText");
		//string str = m_FXText.stringValue;
		//if (str.Length > 65535f / 4f)
		//	str = str.Substring(0, 65535 / 4 - 6) + "\n...\n";

		EditorStyles.textField.wordWrap = true;

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField(serializedObject.FindProperty("m_AssetName").stringValue);
		using (new EditorGUI.DisabledScope(Application.isPlaying))
		{
			if (GUILayout.Button("Reimport"))
			{
				PKFxManager.ReimportAssets(new List<string> { serializedObject.FindProperty("m_AssetName").stringValue });
			}
		}
		EditorGUILayout.EndHorizontal();


		//EditorGUILayout.TextArea(str);
		EditorGUILayout.LabelField("Effect info : ");
		EditorGUI.indentLevel++;
		EditorGUILayout.LabelField("usesMeshRenderer = " + serializedObject.FindProperty("m_UsesMeshRenderer").boolValue);
		EditorGUI.indentLevel--;

		EditorGUILayout.LabelField("Dependencies : ");
		EditorGUI.indentLevel++;
		SerializedProperty deps = serializedObject.FindProperty("m_Dependencies");
		int depsListSize = deps.arraySize;

		for (int i = 0; i < depsListSize; i++)
		{
			SerializedProperty depDesc = deps.GetArrayElementAtIndex(i);
			SerializedProperty depPath = depDesc.FindPropertyRelative("m_Path");
			SerializedProperty depIsLinearTexture = depDesc.FindPropertyRelative("m_IsTextureLinear");
			SerializedProperty depIsMeshRenderer = depDesc.FindPropertyRelative("m_IsMeshRenderer");
			SerializedProperty depIsMeshSampler = depDesc.FindPropertyRelative("m_IsMeshSampler");
			SerializedProperty depObject = depDesc.FindPropertyRelative("m_Object");

			EditorGUILayout.LabelField(depPath.stringValue);
			EditorGUI.indentLevel++;
			if (depIsLinearTexture.boolValue)
				EditorGUILayout.LabelField("isLinearTexture = " + depIsLinearTexture.boolValue);
			if (depIsMeshRenderer.boolValue)
				EditorGUILayout.LabelField("isMeshRenderer = " + depIsMeshRenderer.boolValue);
			if (depIsMeshSampler.boolValue)
				EditorGUILayout.LabelField("isMeshSampler = " + depIsMeshSampler.boolValue);
			using (new EditorGUI.DisabledScope(true))
				EditorGUILayout.ObjectField(depObject);
			EditorGUI.indentLevel--;
		}
		EditorGUI.indentLevel--;

		EditorGUILayout.LabelField("Attributes : ");
		EditorGUI.indentLevel++;
		SerializedProperty attrs = serializedObject.FindProperty("m_AttributeDescs");
		int attrsListSize = attrs.arraySize;

		for (int i = 0; i < attrsListSize; i++)
		{
			SerializedProperty attrDesc = attrs.GetArrayElementAtIndex(i);
			SerializedProperty attrName = attrDesc.FindPropertyRelative("m_Name");
			SerializedProperty attrType = attrDesc.FindPropertyRelative("m_Type");
			
			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField(attrType.enumNames[attrType.enumValueIndex] + " " + attrName.stringValue);
			EditorGUI.indentLevel--;
		}
		EditorGUI.indentLevel--;

		EditorGUILayout.LabelField("Samplers : ");
		EditorGUI.indentLevel++;
		SerializedProperty smplrs = serializedObject.FindProperty("m_SamplerDescs");
		int smplrsListSize = smplrs.arraySize;

		for (int i = 0; i < smplrsListSize; i++)
		{
			SerializedProperty smplrsDesc = smplrs.GetArrayElementAtIndex(i);
			SerializedProperty smplrsName = smplrsDesc.FindPropertyRelative("m_Name");
			SerializedProperty smplrsType = smplrsDesc.FindPropertyRelative("m_Type");

			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField(smplrsType.enumNames[smplrsType.enumValueIndex] + " " + smplrsName.stringValue);
			EditorGUI.indentLevel--;
		}
		EditorGUI.indentLevel--;
	}
}
