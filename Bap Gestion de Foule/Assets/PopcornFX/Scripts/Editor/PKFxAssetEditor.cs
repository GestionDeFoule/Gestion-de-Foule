using System.Collections;
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
			SerializedProperty minMaxFlag = attrDesc.FindPropertyRelative("m_MinMaxFlag");
			PKFxManager.EBaseType baseType = (PKFxManager.EBaseType)attrType.intValue;

			string minValDesc = "";
			string maxValDesc = "";

			if ((minMaxFlag.intValue & (int)PKFxFxAsset.AttributeDesc.EAttrDescFlag.HasMin) != 0)
			{
				Vector4 minVal = GetMinFValue(attrDesc);
				minValDesc = FormatLimitValue(minVal, baseType);
			}
			else
			{
				minValDesc = "[-infinity]";
			}
			if ((minMaxFlag.intValue & (int)PKFxFxAsset.AttributeDesc.EAttrDescFlag.HasMax) != 0)
			{
				Vector4 maxVal = GetMaxFValue(attrDesc);
				maxValDesc = FormatLimitValue(maxVal, baseType);
			}
			else
			{
				maxValDesc = "[+infinity]";
			}

			Vector4 defaultVal = GetDefaultFValue(attrDesc);
			string defaultValStr = FormatLimitValue(defaultVal, baseType);

			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField(attrName.stringValue);
			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField(attrType.enumNames[attrType.enumValueIndex]);
			EditorGUILayout.LabelField("Min/Max: " + minValDesc + "-" + maxValDesc);
			EditorGUILayout.LabelField("Default: " + defaultValStr);
			EditorGUI.indentLevel--;
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

	private Vector4 GetMinFValue(SerializedProperty attrDesc)
	{
		SerializedProperty minValue0 = attrDesc.FindPropertyRelative("m_MinValue0");
		SerializedProperty minValue1 = attrDesc.FindPropertyRelative("m_MinValue1");
		SerializedProperty minValue2 = attrDesc.FindPropertyRelative("m_MinValue2");
		SerializedProperty minValue3 = attrDesc.FindPropertyRelative("m_MinValue3");

		return new Vector4(minValue0.floatValue, minValue1.floatValue, minValue2.floatValue, minValue3.floatValue);
	}

	private Vector4 GetMaxFValue(SerializedProperty attrDesc)
	{
		SerializedProperty maxValue0 = attrDesc.FindPropertyRelative("m_MaxValue0");
		SerializedProperty maxValue1 = attrDesc.FindPropertyRelative("m_MaxValue1");
		SerializedProperty maxValue2 = attrDesc.FindPropertyRelative("m_MaxValue2");
		SerializedProperty maxValue3 = attrDesc.FindPropertyRelative("m_MaxValue3");

		return new Vector4(maxValue0.floatValue, maxValue1.floatValue, maxValue2.floatValue, maxValue3.floatValue);
	}

	private Vector4 GetDefaultFValue(SerializedProperty attrDesc)
	{
		SerializedProperty defaultValue0 = attrDesc.FindPropertyRelative("m_DefaultValue0");
		SerializedProperty defaultValue1 = attrDesc.FindPropertyRelative("m_DefaultValue1");
		SerializedProperty defaultValue2 = attrDesc.FindPropertyRelative("m_DefaultValue2");
		SerializedProperty defaultValue3 = attrDesc.FindPropertyRelative("m_DefaultValue3");

		return new Vector4(defaultValue0.floatValue, defaultValue1.floatValue, defaultValue2.floatValue, defaultValue3.floatValue);
	}

	private string FormatLimitValue(Vector4 value, PKFxManager.EBaseType baseType)
	{
		string retStr = "";

		if (baseType == PKFxManagerImpl.EBaseType.Float)
		{
			retStr = "[" + value.x + "]";
		}
		else if (baseType == PKFxManagerImpl.EBaseType.Float2)
		{
			retStr = "[" + value.x + "," + value.y + "]";
		}
		else if (baseType == PKFxManagerImpl.EBaseType.Float3)
		{
			retStr = "[" + value.x + "," + value.y + "," + value.z + "]";
		}
		else if (baseType == PKFxManagerImpl.EBaseType.Float4)
		{
			retStr = "[" + value.x + "," + value.y + "," + value.z + "," + value.w + "]";
		}
		else if (baseType == PKFxManagerImpl.EBaseType.Int)
		{
			retStr = "[" + PKFxManager.Float2Int(value.x) + "]";
		}
		else if (baseType == PKFxManagerImpl.EBaseType.Int2)
		{
			retStr = "[" + PKFxManager.Float2Int(value.x) + "," + PKFxManager.Float2Int(value.y) + "]";
		}
		else if (baseType == PKFxManagerImpl.EBaseType.Int3)
		{
			retStr = "[" + PKFxManager.Float2Int(value.x) + "," + PKFxManager.Float2Int(value.y) + "," + PKFxManager.Float2Int(value.z) + "]";
		}
		else if (baseType == PKFxManagerImpl.EBaseType.Int4)
		{
			retStr = "[" + PKFxManager.Float2Int(value.x) + "," + PKFxManager.Float2Int(value.y) + "," + PKFxManager.Float2Int(value.z) + "," + PKFxManager.Float2Int(value.w) + "]";
		}
		return retStr;
	}
}
