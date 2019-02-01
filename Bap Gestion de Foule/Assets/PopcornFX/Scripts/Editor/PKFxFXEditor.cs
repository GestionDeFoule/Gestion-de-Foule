//----------------------------------------------------------------------------
// Created on Tue Sep 2 18:09:33 2014 Raphael Thoulouze
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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(PKFxFX))]
[CanEditMultipleObjects]
public class PKFxFXEditor : Editor
{
    const int PlaybackControlsMaxHeight = 20;
    const int PlaybackControlsMaxWidth = 75;

	GUIStyle m_oddStyle = new GUIStyle();
	GUIStyle m_pairStyle = new GUIStyle();


	SerializedProperty m_FxName;
	SerializedProperty	m_FxAsset;

	SerializedProperty	m_FxAttributesStartValues;
	SerializedProperty	m_FxAttributesDesc;

	SerializedProperty	m_FxSamplers;
	SerializedProperty	m_PlayOnStart;
    SerializedProperty  m_TriggerAndForget;

    Texture2D m_CtrlPlayIcon;
    Texture2D m_CtrlStopIcon;
    Texture2D m_CtrlRestartIcon;
    Texture2D m_CtrlKillIcon;


	bool	m_RequiresApplyModifiedProperties = false;

	//----------------------------------------------------------------------------

	void OnEnable()
	{
		m_FxName = serializedObject.FindProperty("m_FxName");
		m_FxAsset = serializedObject.FindProperty("m_FxAsset");
		m_FxSamplers = serializedObject.FindProperty("m_FxSamplersList");
		m_PlayOnStart = serializedObject.FindProperty("m_PlayOnStart");
		m_TriggerAndForget = serializedObject.FindProperty("m_TriggerAndForget");
		m_FxAttributesStartValues = serializedObject.FindProperty("m_FxAttributesStartValues");
		m_FxAttributesDesc = serializedObject.FindProperty("m_FxAttributesDesc");
        
        m_CtrlPlayIcon = Resources.Load<Texture2D>("Icons/CtrlPlay");
        m_CtrlStopIcon = Resources.Load<Texture2D>("Icons/CtrlStop");
        m_CtrlRestartIcon = Resources.Load<Texture2D>("Icons/CtrlRestart");
        m_CtrlKillIcon = Resources.Load<Texture2D>("Icons/CtrlKill");

		Color pairLineBackgroundColor = PKFxEditorTools.EditorBoxBackgroundColor;
		m_pairStyle.normal.background = MakeTex(1, 1, pairLineBackgroundColor);
	}

	private void ForceSerializedObjectUpdate(PKFxFX fx)
	{
		SerializedObject updatedObject = new SerializedObject(fx);

		serializedObject.CopyFromSerializedProperty(updatedObject.FindProperty("m_FxAsset"));
		serializedObject.CopyFromSerializedProperty(updatedObject.FindProperty("m_FxName"));
		serializedObject.CopyFromSerializedProperty(updatedObject.FindProperty("m_FxSamplersList"));
		serializedObject.CopyFromSerializedProperty(updatedObject.FindProperty("m_FxAttributesStartValues"));
		serializedObject.CopyFromSerializedProperty(updatedObject.FindProperty("m_FxAttributesDesc"));
		serializedObject.CopyFromSerializedProperty(updatedObject.FindProperty("m_FxAttributesDescHash"));
		serializedObject.CopyFromSerializedProperty(updatedObject.FindProperty("m_FxSamplersDescHash"));
#if !PK_REMOVE_OBSOLETE_CODE
		serializedObject.CopyFromSerializedProperty(updatedObject.FindProperty("m_FxAttributesList"));
#endif
		m_RequiresApplyModifiedProperties = true;
	}

	//----------------------------------------------------------------------------

	public void OnSceneGUI()
	{
		if (target != null)
		{
			PKFxFX fx = target as PKFxFX;

			if (fx != null && fx.m_FxSamplersList != null)
			{
				for (int i = 0; i < fx.m_FxSamplersList.Count; i++)
				{
					PKFxFX.Sampler sampler = fx.m_FxSamplersList[i];
					if (sampler != null && sampler.m_Descriptor.m_Type == (int)PKFxFxAsset.SamplerDesc.ESamplerType.SamplerShape)
					{
						// Show the shapes in world space instead of local space (just for the editor workflow, does not change the actual sampling)
						bool worldSpaceSampling = sampler.m_WorldSpaceSampling;
					
						if (sampler.m_ShapeType == PKFxFX.Sampler.EShapeType.BoxShape)
							DrawCube(i, sampler, worldSpaceSampling);
						else if (sampler.m_ShapeType == PKFxFX.Sampler.EShapeType.SphereShape)
							DrawSphere(i, sampler, worldSpaceSampling);
						else if (sampler.m_ShapeType == PKFxFX.Sampler.EShapeType.CylinderShape)
							DrawCylinder(i, sampler, worldSpaceSampling);
						else if (sampler.m_ShapeType == PKFxFX.Sampler.EShapeType.CapsuleShape)
							DrawCapsule(i, sampler, worldSpaceSampling);
					}
				}
			}
		}
	}

	//----------------------------------------------------------------------------

	public override void OnInspectorGUI()
	{
		if (serializedObject.targetObjects.Length == 1)
		{
			EditorGUILayout.PropertyField(m_FxAsset);
			serializedObject.ApplyModifiedProperties();
			serializedObject.Update();

			PKFxFX fx = target as PKFxFX;
			if (EditorUpdatePkFX(fx, false))
			{
				ForceSerializedObjectUpdate(fx);
			}
		}

		bool isFxAssetSet = (m_FxAsset.objectReferenceValue != null);

		if (isFxAssetSet)
            DrawPlaybackControls();

		DrawEmitterProperties();

		if (serializedObject.targetObjects.Length == 1)
		{
			if (isFxAssetSet)
				DrawAttributes();
		}

		if (m_RequiresApplyModifiedProperties)
		{
	        serializedObject.ApplyModifiedProperties();
			m_RequiresApplyModifiedProperties = false;
		}
	}

    private void DrawEmitterProperties()
    {
		using (var category = new PKFxEditorCategory(m_PlayOnStart, "Emitter"))
        {
            if (category.IsExpanded())
            {
				EditorGUI.BeginChangeCheck();
				{
					EditorGUILayout.PropertyField(m_PlayOnStart);
					EditorGUILayout.PropertyField(m_TriggerAndForget);
				}
				if (EditorGUI.EndChangeCheck())
				{
					m_RequiresApplyModifiedProperties = true;
				}
           }
        }
    }

    private void DrawAttributes()
    {
		if (m_FxAttributesDesc.arraySize == 0 && m_FxSamplers.arraySize == 0)
			return;
		using (var category = new PKFxEditorCategory(DrawAttributesHeader))
		{
			if (!category.IsExpanded())
				return;
			EditorGUI.BeginChangeCheck();
			{
				int numLine = 0;

				if (serializedObject.targetObjects.Length == 1)
				{
					PKFxFX fx = target as PKFxFX;
					bool attributesChanged = false;
					PKFxAttributesContainer attribContainer = fx.m_AttributesContainer;

					for (int i = 0; i < m_FxAttributesDesc.arraySize; i++)
					{
						SerializedProperty attrDesc = m_FxAttributesDesc.GetArrayElementAtIndex(i);

						SerializedProperty valueX = m_FxAttributesStartValues.GetArrayElementAtIndex(i * 4 + 0);
						SerializedProperty valueY = m_FxAttributesStartValues.GetArrayElementAtIndex(i * 4 + 1);
						SerializedProperty valueZ = m_FxAttributesStartValues.GetArrayElementAtIndex(i * 4 + 2);
						SerializedProperty valueW = m_FxAttributesStartValues.GetArrayElementAtIndex(i * 4 + 3);

						SetColorBackgroundByParity(numLine);
						if (PKFxAttributePropertyDrawer.DrawAttribute(attrDesc, valueX, valueY, valueZ, valueW))
						{
							if (attribContainer != null) // The FX is not started
							{
								attributesChanged = true;
								attribContainer.SetAttributeUnsafe(i, valueX.floatValue, valueY.floatValue, valueZ.floatValue, valueW.floatValue);
							}
						}
						EditorGUILayout.EndVertical();
						++numLine;
					}

					if (attributesChanged)
					{
						attribContainer.UpdateAttributes();
					}

					if (m_FxAttributesDesc.arraySize > 0 && m_FxSamplers.arraySize > 0)
					{
						PkFxEditorSplitter.Splitter(0);
					}

					for (int i = 0; i < m_FxSamplers.arraySize; i++)
					{
						SerializedProperty smp = m_FxSamplers.GetArrayElementAtIndex(i);
						SamplerField(smp);
					}
				}

				//Clear FxName, Attributes and Samplers if the asset is None
				if (m_FxAsset.objectReferenceInstanceIDValue == 0)
				{
					m_FxAttributesDesc.ClearArray();
					m_FxAttributesStartValues.ClearArray();
					m_FxSamplers.ClearArray();
					m_FxName.stringValue = "";
				}
			}

			if (EditorGUI.EndChangeCheck())
			{
				m_RequiresApplyModifiedProperties = true;
			}
		}
	}

	private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];

        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }

    private void SetColorBackgroundByParity(int numLine)
    {
        EditorGUILayout.BeginVertical((numLine % 2 == 0) ? m_oddStyle : m_pairStyle);
    }

	private bool DrawAttributesHeader()
	{
		EditorGUILayout.BeginHorizontal();
		m_FxAttributesDesc.isExpanded = EditorGUILayout.Foldout(m_FxAttributesDesc.isExpanded, "Attributes", true);

		EditorGUILayout.Separator();

		if (serializedObject.targetObjects.Length == 1)
		{
			if (GUILayout.Button(new GUIContent("Reset All", "Reset all attributes to default values"), GUILayout.Width(70)))
			{
				Object[] effects = serializedObject.targetObjects;
				foreach (PKFxFX fx in effects)
				{
					if (EditorUpdatePkFX(fx, true))
					{
						ForceSerializedObjectUpdate(fx);
					}
				}
			}
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Separator();
		return (m_FxAttributesDesc.isExpanded);
	}

    //----------------------------------------------------------------------------

    private void DrawPlaybackControls()
	{
        Object[]	effects = serializedObject.targetObjects;
		bool		effectsPlaying = true;

		foreach (PKFxFX fx in effects)
		{
			if (!fx.Alive())
				effectsPlaying = false;
		}

		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);

		if (!effectsPlaying)
        {
			if (GUILayout.Button(
                new GUIContent(m_CtrlPlayIcon, "Start emitter"), 
                EditorStyles.miniButtonLeft, 
                GUILayout.MaxHeight(PlaybackControlsMaxHeight)))
            {
                foreach (PKFxFX fx in effects)
                {
					if (!fx.Alive())
						fx.StartEffect();
                }
            }
        }
        else
        {
            if (GUILayout.Button(
                new GUIContent(m_CtrlStopIcon, "Stop emitter"), 
                EditorStyles.miniButtonLeft,
                GUILayout.MaxHeight(PlaybackControlsMaxHeight)))
            {
                foreach (PKFxFX fx in effects)
                {
                    fx.StopEffect();
                }
            }
        }

		if (GUILayout.Button(
            new GUIContent(m_CtrlRestartIcon, "Restart emitter"), 
            EditorStyles.miniButtonMid,
            GUILayout.MaxHeight(PlaybackControlsMaxHeight)))
        {
            foreach (PKFxFX fx in effects)
            {
                fx.TerminateEffect();
                fx.StartEffect();
            }
        }

        if (GUILayout.Button(
            new GUIContent(m_CtrlKillIcon, "Stop emitter and destroy particles"), 
            EditorStyles.miniButtonRight,
            GUILayout.MaxHeight(PlaybackControlsMaxHeight)))
        {
            foreach (PKFxFX fx in effects)
            {
                fx.KillEffect();
            }
        }

		EditorGUI.EndDisabledGroup();
		EditorGUILayout.EndHorizontal();
	}

	//----------------------------------------------------------------------------

	private SerializedProperty SamplerField(SerializedProperty sampler)
	{
		bool				hasChanged = false;
		SerializedProperty	samplerName = sampler.FindPropertyRelative("m_Descriptor.m_Name");
		SerializedProperty	samplerType = sampler.FindPropertyRelative("m_Descriptor.m_Type");

		EditorGUI.indentLevel++;
		sampler.isExpanded = EditorGUILayout.Foldout(sampler.isExpanded, samplerName.stringValue, true);
		if (!sampler.isExpanded)
		{
			EditorGUI.indentLevel--;
			return sampler;
		}
			

		if (samplerType.intValue == (int)PKFxFxAsset.SamplerDesc.ESamplerType.SamplerShape)
		{
			SerializedProperty m_ShapeType = sampler.FindPropertyRelative("m_ShapeType");

			int		oldType = m_ShapeType.intValue;

			m_ShapeType.intValue = EditorGUILayout.Popup(m_ShapeType.intValue, ShapeTypes);

			bool shapeHasChanged = oldType != m_ShapeType.intValue;
			bool meshHasChanged = false;

			SerializedProperty worldSpaceSampling = sampler.FindPropertyRelative("m_WorldSpaceSampling");

			// Show world space:
			worldSpaceSampling.boolValue = EditorGUILayout.Toggle("Show in world space", worldSpaceSampling.boolValue);

			if (m_ShapeType.intValue == (int)PKFxFX.Sampler.EShapeType.BoxShape)
				BoxField(sampler, shapeHasChanged);
			else if (m_ShapeType.intValue == (int)PKFxFX.Sampler.EShapeType.SphereShape)
				SphereField(sampler, shapeHasChanged);
			else if (m_ShapeType.intValue == (int)PKFxFX.Sampler.EShapeType.CylinderShape)
				CylinderField(sampler, shapeHasChanged);
			else if (m_ShapeType.intValue == (int)PKFxFX.Sampler.EShapeType.CapsuleShape)
				CapsuleField(sampler, shapeHasChanged);
			else if (m_ShapeType.intValue == (int)PKFxFX.Sampler.EShapeType.MeshShape)
				meshHasChanged = MeshField(sampler, shapeHasChanged);
			else if (m_ShapeType.intValue == (int)PKFxFX.Sampler.EShapeType.MeshFilterShape)
				meshHasChanged = MeshFilterField(sampler, shapeHasChanged);
			else if (m_ShapeType.intValue == (int)PKFxFX.Sampler.EShapeType.SkinnedMeshShape)
				meshHasChanged = SkinnedMeshField(sampler, shapeHasChanged);
			else
			{
				SerializedProperty m_Transform = sampler.FindPropertyRelative("m_ShapeTransform");
				DrawShapeTransform(m_Transform);
			}

			hasChanged = shapeHasChanged || meshHasChanged;
		}
		else if (samplerType.intValue == (int)PKFxFxAsset.SamplerDesc.ESamplerType.SamplerCurve)
		{
			SerializedProperty m_CurvesArray = sampler.FindPropertyRelative("m_CurvesArray");
			SerializedProperty m_CurvesTimeKeys = sampler.FindPropertyRelative("m_CurvesTimeKeys");
			SerializedProperty m_CurveIsOverride = sampler.FindPropertyRelative("m_CurveIsOverride");

			hasChanged = MultipleCurvesEditor(m_CurvesArray);

			if (hasChanged)
			{
				m_CurveIsOverride.boolValue = true;
			}

			if (m_CurvesArray.arraySize != 0)
			{
				int iKey = 0;
				m_CurvesTimeKeys.arraySize = m_CurvesArray.GetArrayElementAtIndex(0).animationCurveValue.keys.Length;

				Keyframe[] keyframes = m_CurvesArray.GetArrayElementAtIndex(0).animationCurveValue.keys;

				foreach (var key in keyframes)
				{
					m_CurvesTimeKeys.GetArrayElementAtIndex(iKey++).floatValue = key.time;
				}
			}
			else
				m_CurvesTimeKeys.arraySize = 0;
		}
		else if (samplerType.intValue == (int)PKFxFxAsset.SamplerDesc.ESamplerType.SamplerImage)
		{
			SerializedProperty m_Tex = sampler.FindPropertyRelative("m_Texture");
			SerializedProperty m_TextureTexcoordMode = sampler.FindPropertyRelative("m_TextureTexcoordMode");

			Texture2D newTex = (Texture2D)EditorGUILayout.ObjectField("Texture", m_Tex.objectReferenceValue, typeof(Texture2D), false);

			if (newTex != m_Tex.objectReferenceValue)
			{
				m_Tex.objectReferenceValue = newTex;
				hasChanged = true;
			}
			EditorGUILayout.LabelField("Texcoord Mode");
			PKFxFX.Sampler.ETexcoordMode newType = (PKFxFX.Sampler.ETexcoordMode)EditorGUILayout.EnumPopup((PKFxFX.Sampler.ETexcoordMode)m_TextureTexcoordMode.intValue);
			m_TextureTexcoordMode.intValue = (int)newType;
		}
		else if (samplerType.intValue == (int)PKFxFxAsset.SamplerDesc.ESamplerType.SamplerText)
		{
			SerializedProperty m_Text = sampler.FindPropertyRelative("m_Text");

			string	newValue = EditorGUILayout.TextField(m_Text.stringValue);

			if (m_Text.stringValue != newValue)
			{
				m_Text.stringValue = newValue;
				hasChanged = true;
			}
		}
		else if (samplerType.intValue == (int)PKFxFxAsset.SamplerDesc.ESamplerType.SamplerUnsupported)
		{
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.LabelField("Edits not supported");
			EditorGUI.EndDisabledGroup();
		}

		if (hasChanged)
		{
			SerializedProperty wasModified = sampler.FindPropertyRelative("m_WasModified");
			wasModified.boolValue = true;
			m_RequiresApplyModifiedProperties = true;
		}
		EditorGUI.indentLevel--;
		return sampler;
	}

	public bool MultipleCurvesEditor(SerializedProperty curvesArray)
	{
		bool	hasChanged = false;

		for (int i = 0; i < curvesArray.arraySize; ++i)
		{
			Keyframe[] keysCache = curvesArray.GetArrayElementAtIndex(i).animationCurveValue.keys;
			EditorGUILayout.PropertyField(curvesArray.GetArrayElementAtIndex(i), new GUIContent(CurveDimensionsNames[i]));
			hasChanged |= MultipleCurvesCheckModify(curvesArray, i, keysCache);
		}
		return hasChanged;
	}

	public bool MultipleCurvesCheckModify(SerializedProperty currentKeysProp, int curveIndex, Keyframe[] oldKeys)
	{
		bool hasChanged = false;

		AnimationCurve curve = currentKeysProp.GetArrayElementAtIndex(curveIndex).animationCurveValue;
		if (curve.keys.Length > 0)
		{
			if (curve.keys.Length < 2)
			{
				hasChanged = true;
				curve.keys = oldKeys;
				currentKeysProp.GetArrayElementAtIndex(curveIndex).animationCurveValue = curve;
			}
			currentKeysProp.GetArrayElementAtIndex(curveIndex).animationCurveValue = curve;

			List<float> changedOldKeys;
			List<float> changedNewKeys;

			MultipleCurvesFindKeysChanges(curve.keys, oldKeys, out changedOldKeys, out changedNewKeys);

			for (int i = 0; i < changedOldKeys.Count; ++i)
			{
				int idx = changedNewKeys.FindIndex(x => x == changedOldKeys[i]);

				if (idx == -1)
				{
					MultipleCurvesDeleteKey(currentKeysProp, curveIndex, changedOldKeys[i]);
					hasChanged = true;
				}
				else
				{
					MultipleCurvesChangeKey(currentKeysProp, curveIndex, changedOldKeys[i], changedNewKeys[idx]);
					hasChanged = true;
				}
			}
			// No need to change the keys here, all the key that should gave changed are now updated:
			for (int i = 0; i < changedNewKeys.Count; ++i)
			{
				if (!changedOldKeys.Exists(x => x == changedNewKeys[i]))
				{
					MultipleCurvesAddKey(currentKeysProp, curveIndex, changedNewKeys[i]);
					hasChanged = true;
				}
			}
		}
		return hasChanged;
	}

	public void MultipleCurvesFindKeysCountChanges(Keyframe[] refKeys, Keyframe[] compKeys, List<float> diffKeys)
	{
		foreach (var key in refKeys)
		{
			bool found = false;
			foreach (var othkey in compKeys)
			{
				if (key.time == othkey.time)
				{
					found = true;
					break;
				}
			}
			if (!found)
				diffKeys.Add(key.time);
		}
	}

	public void MultipleCurvesFindKeysChanges(Keyframe[] actualKeys, Keyframe[] cacheKeys, out List<float> oldKeys, out List<float> newKeys)
	{
		List<float> addedKeys = new List<float>();
		List<float> deletedKeys = new List<float>();
		MultipleCurvesFindKeysCountChanges(actualKeys, cacheKeys, addedKeys);
		MultipleCurvesFindKeysCountChanges(cacheKeys, actualKeys, deletedKeys);
		if (addedKeys.Count != 0 && addedKeys.Count == deletedKeys.Count)
		{
			addedKeys.Sort();
			deletedKeys.Sort();
		}
		oldKeys = deletedKeys;
		newKeys = addedKeys;
	}

	public void MultipleCurvesAddKey(SerializedProperty curvesArray, int sourceCurveIndex, float time)
	{
		for (int i = 0; i < curvesArray.arraySize; ++i)
		{
			if (i == sourceCurveIndex)
				continue;
			AnimationCurve curve = curvesArray.GetArrayElementAtIndex(i).animationCurveValue;
			curve.AddKey(time, curve.Evaluate(time));
			curvesArray.GetArrayElementAtIndex(i).animationCurveValue = curve;
		}
	}

	public void MultipleCurvesDeleteKey(SerializedProperty curvesArray, int sourceCurveIndex, float time)
	{
		for (int i = 0; i < curvesArray.arraySize; ++i)
		{
			if (i == sourceCurveIndex)
				continue;
			AnimationCurve curve = curvesArray.GetArrayElementAtIndex(i).animationCurveValue;
			for (int iKey = 0; iKey < curve.keys.Length; ++iKey)
			{
				if (curve.keys[iKey].time == time)
				{
					curve.RemoveKey(iKey);
					curvesArray.GetArrayElementAtIndex(i).animationCurveValue = curve;
					break;
				}
			}
		}
	}

	public void MultipleCurvesChangeKey(SerializedProperty curvesArray, int sourceCurveIndex, float oldTime, float newTime)
	{
		for (int i = 0; i < curvesArray.arraySize; ++i)
		{
			if (i == sourceCurveIndex)
				continue;
			AnimationCurve curve = curvesArray.GetArrayElementAtIndex(i).animationCurveValue;
			int iKey;
			for (iKey = 0; iKey < curve.keys.Length; ++iKey)
			{
				if (curve.keys[iKey].time == oldTime)
				{
					Keyframe keyframe = curve.keys[iKey];
					keyframe.time = newTime;
					curve.RemoveKey(iKey);
					curve.AddKey(keyframe);
					curvesArray.GetArrayElementAtIndex(i).animationCurveValue = curve;
					break;
				}
			}
		}
	}

	//----------------------------------------------------------------------------

	private static readonly string[] ShapeTypes = { "BOX", "SPHERE", "CYLINDER", "CAPSULE", "MESH", "MESHFILTER", "SKINNEDMESH", "Default" };
	private static readonly string[] CurveDimensionsNames = { "X", "Y", "Z", "W" };

	private static void 				BoxField(SerializedProperty sampler, bool shapeHasChanged)
	{
		SerializedProperty m_Transform = sampler.FindPropertyRelative("m_ShapeTransform");
		SerializedProperty m_Dimensions = sampler.FindPropertyRelative("m_Dimensions");

		// Reset the default dimension:
		if (shapeHasChanged)
		{
			m_Dimensions.vector3Value = new Vector3(1, 1, 1);
		}

		EditorGUILayout.PropertyField(m_Dimensions);
		DrawShapeTransform(m_Transform);
	}

	private static Transform transform;

	private static void 				SphereField(SerializedProperty sampler, bool shapeHasChanged)
	{
		SerializedProperty m_Transform = sampler.FindPropertyRelative("m_ShapeTransform");
		SerializedProperty m_Dimensions = sampler.FindPropertyRelative("m_Dimensions");

		// Reset the default dimension:
		if (shapeHasChanged)
		{
			m_Dimensions.vector3Value = new Vector3(1, 0, 0);
		}

		Vector3 tmp = m_Dimensions.vector3Value;
		tmp.y = EditorGUILayout.FloatField("Inner Radius", Mathf.Min(tmp.x, tmp.y));
		tmp.x = EditorGUILayout.FloatField("Radius", Mathf.Max(tmp.x, tmp.y));
		m_Dimensions.vector3Value = tmp;
		DrawShapeTransform(m_Transform);
	}

	private static void 				CylinderField(SerializedProperty sampler, bool shapeHasChanged)
	{
		SerializedProperty m_Transform = sampler.FindPropertyRelative("m_ShapeTransform");
		SerializedProperty m_Dimensions = sampler.FindPropertyRelative("m_Dimensions");

		// Reset the default dimension:
		if (shapeHasChanged)
		{
			m_Dimensions.vector3Value = new Vector3(1, 0, 1);
		}

		Vector3 tmp = m_Dimensions.vector3Value;
		tmp.y = EditorGUILayout.FloatField("Inner Radius", Mathf.Min(tmp.x, tmp.y));
		tmp.x = EditorGUILayout.FloatField("Radius", Mathf.Max(tmp.x, tmp.y));
		tmp.z = EditorGUILayout.FloatField("Height", tmp.z);
		m_Dimensions.vector3Value = tmp;
		DrawShapeTransform(m_Transform);
	}

	private static void 				CapsuleField(SerializedProperty sampler, bool shapeHasChanged)
	{
		SerializedProperty m_Transform = sampler.FindPropertyRelative("m_ShapeTransform");
		SerializedProperty m_Dimensions = sampler.FindPropertyRelative("m_Dimensions");

		if (shapeHasChanged)
		{
			m_Dimensions.vector3Value = new Vector3(1, 0, 1);
		}

		Vector3 tmp = m_Dimensions.vector3Value;
		tmp.y = EditorGUILayout.FloatField("Inner Radius", Mathf.Min(tmp.x, tmp.y));
		tmp.x = EditorGUILayout.FloatField("Radius", Mathf.Max(tmp.x, tmp.y));
		tmp.z = EditorGUILayout.FloatField("Height", tmp.z);
		m_Dimensions.vector3Value = tmp;
		DrawShapeTransform(m_Transform);
	}

	private static void SamplingChannelsFields(SerializedProperty sampler, bool enableVelocity)
	{
		SerializedProperty m_SamplingInfo = sampler.FindPropertyRelative("m_SamplingInfo");

		EditorGUILayout.Toggle("Sample Positions", true);

		if (EditorGUILayout.Toggle("Sample Normals", (m_SamplingInfo.intValue & (int)PKFxFX.Sampler.EMeshSamplingInfo.Info_ChannelNormal) != 0))
			m_SamplingInfo.intValue |= (int)PKFxFX.Sampler.EMeshSamplingInfo.Info_ChannelNormal;
		else
			m_SamplingInfo.intValue &= ~(int)PKFxFX.Sampler.EMeshSamplingInfo.Info_ChannelNormal;
		if (EditorGUILayout.Toggle("Sample Tangents", (m_SamplingInfo.intValue & (int)PKFxFX.Sampler.EMeshSamplingInfo.Info_ChannelTangent) != 0))
			m_SamplingInfo.intValue |= (int)PKFxFX.Sampler.EMeshSamplingInfo.Info_ChannelTangent;
		else
			m_SamplingInfo.intValue &= ~(int)PKFxFX.Sampler.EMeshSamplingInfo.Info_ChannelTangent;
		if (!enableVelocity)
			m_SamplingInfo.intValue &= ~(int)PKFxFX.Sampler.EMeshSamplingInfo.Info_ChannelVelocity;
		else
		{
			if (EditorGUILayout.Toggle("Sample Velocity", (m_SamplingInfo.intValue & (int)PKFxFX.Sampler.EMeshSamplingInfo.Info_ChannelVelocity) != 0))
				m_SamplingInfo.intValue |= (int)PKFxFX.Sampler.EMeshSamplingInfo.Info_ChannelVelocity;
			else
				m_SamplingInfo.intValue &= ~(int)PKFxFX.Sampler.EMeshSamplingInfo.Info_ChannelVelocity;
		}
		if (EditorGUILayout.Toggle("Sample UVs", (m_SamplingInfo.intValue & (int)PKFxFX.Sampler.EMeshSamplingInfo.Info_ChannelUV) != 0))
			m_SamplingInfo.intValue |= (int)PKFxFX.Sampler.EMeshSamplingInfo.Info_ChannelUV;
		else
			m_SamplingInfo.intValue &= ~(int)PKFxFX.Sampler.EMeshSamplingInfo.Info_ChannelUV;
		if (EditorGUILayout.Toggle("Sample Vertex Color", (m_SamplingInfo.intValue & (int)PKFxFX.Sampler.EMeshSamplingInfo.Info_ChannelVertexColor) != 0))
			m_SamplingInfo.intValue |= (int)PKFxFX.Sampler.EMeshSamplingInfo.Info_ChannelVertexColor;
		else
			m_SamplingInfo.intValue &= ~(int)PKFxFX.Sampler.EMeshSamplingInfo.Info_ChannelVertexColor;
		if (EditorGUILayout.Toggle("Build Mesh Sampling Info", (m_SamplingInfo.intValue & (int)PKFxFX.Sampler.EMeshSamplingInfo.Info_NeedBuildSamplingInfo) != 0))
			m_SamplingInfo.intValue |= (int)PKFxFX.Sampler.EMeshSamplingInfo.Info_NeedBuildSamplingInfo;
		else
			m_SamplingInfo.intValue &= ~(int)PKFxFX.Sampler.EMeshSamplingInfo.Info_NeedBuildSamplingInfo;
		if (EditorGUILayout.Toggle("Build Mesh KD Tree (for the Projection and Collision evolvers)", (m_SamplingInfo.intValue & (int)PKFxFX.Sampler.EMeshSamplingInfo.Info_NeedBuildKdTree) != 0))
			m_SamplingInfo.intValue |= (int)PKFxFX.Sampler.EMeshSamplingInfo.Info_NeedBuildKdTree;
		else
			m_SamplingInfo.intValue &= ~(int)PKFxFX.Sampler.EMeshSamplingInfo.Info_NeedBuildKdTree;
	}

	private static bool MeshField(SerializedProperty sampler, bool shapeHasChanged)
	{
		SerializedProperty m_Transform = sampler.FindPropertyRelative("m_ShapeTransform");
		SerializedProperty m_Dimensions = sampler.FindPropertyRelative("m_Dimensions");
		SerializedProperty m_SkinnedMeshRenderer = sampler.FindPropertyRelative("m_SkinnedMeshRenderer");
		SerializedProperty m_MeshFilter = sampler.FindPropertyRelative("m_MeshFilter");
		SerializedProperty m_Mesh = sampler.FindPropertyRelative("m_Mesh");

		if (shapeHasChanged)
		{
			m_Dimensions.vector3Value = Vector3.one;
		}

		int		oldNameHash = 0;
		int		newNameHash = 0;

		if (m_Mesh.objectReferenceValue != null)
		{
			oldNameHash = (m_Mesh.objectReferenceValue as Mesh).name.GetHashCode();
		}

		EditorGUILayout.PropertyField(m_Mesh);

		if (m_Mesh.objectReferenceValue != null)
		{
			newNameHash = (m_Mesh.objectReferenceValue as Mesh).name.GetHashCode();
		}

		m_SkinnedMeshRenderer.objectReferenceValue = null;
		m_MeshFilter.objectReferenceValue = null;
		EditorGUILayout.PropertyField(m_Dimensions);
		DrawShapeTransform(m_Transform);
		SamplingChannelsFields(sampler, false);
		return oldNameHash != newNameHash;
	}

	private static bool MeshFilterField(SerializedProperty sampler, bool shapeHasChanged)
	{
		SerializedProperty m_Transform = sampler.FindPropertyRelative("m_ShapeTransform");
		SerializedProperty m_Dimensions = sampler.FindPropertyRelative("m_Dimensions");
		SerializedProperty m_SkinnedMeshRenderer = sampler.FindPropertyRelative("m_SkinnedMeshRenderer");
		SerializedProperty m_MeshFilter = sampler.FindPropertyRelative("m_MeshFilter");
		SerializedProperty m_Mesh = sampler.FindPropertyRelative("m_Mesh");

		if (shapeHasChanged)
		{
			m_Dimensions.vector3Value = Vector3.one;
		}

		int oldNameHash = 0;
		int newNameHash = 0;

		if (m_Mesh.objectReferenceValue != null)
		{
			oldNameHash = (m_Mesh.objectReferenceValue as Mesh).name.GetHashCode();
		}

		EditorGUILayout.PropertyField(m_MeshFilter);

		if (m_MeshFilter.objectReferenceValue != null)
		{
			m_Mesh.objectReferenceValue = (m_MeshFilter.objectReferenceValue as MeshFilter).sharedMesh;
			if (m_Mesh.objectReferenceValue != null)
			{
				newNameHash = (m_Mesh.objectReferenceValue as Mesh).name.GetHashCode();
			}
		}
		else
		{
			m_Mesh.objectReferenceValue = null;
		}

		m_SkinnedMeshRenderer.objectReferenceValue = null;
		EditorGUILayout.PropertyField(m_Dimensions);
		DrawShapeTransform(m_Transform);
		SamplingChannelsFields(sampler, false);
		return oldNameHash != newNameHash;
	}

	private static bool SkinnedMeshField(SerializedProperty sampler, bool shapeHasChanged)
	{
		SerializedProperty m_Transform = sampler.FindPropertyRelative("m_ShapeTransform");
		SerializedProperty m_Dimensions = sampler.FindPropertyRelative("m_Dimensions");
		SerializedProperty m_SkinnedMeshRenderer = sampler.FindPropertyRelative("m_SkinnedMeshRenderer");
		SerializedProperty m_MeshFilter = sampler.FindPropertyRelative("m_MeshFilter");
		SerializedProperty m_Mesh = sampler.FindPropertyRelative("m_Mesh");

		if (shapeHasChanged)
		{
			m_Dimensions.vector3Value = Vector3.one;
		}

		int oldNameHash = 0;
		int newNameHash = 0;

		if (m_Mesh.objectReferenceValue != null)
		{
			oldNameHash = (m_Mesh.objectReferenceValue as Mesh).name.GetHashCode();
		}

		EditorGUILayout.PropertyField(m_SkinnedMeshRenderer);

		if (m_SkinnedMeshRenderer.objectReferenceValue != null)
		{
			Mesh mesh = (m_SkinnedMeshRenderer.objectReferenceValue as SkinnedMeshRenderer).sharedMesh;
			if (mesh != null)
			{
				m_Mesh.objectReferenceValue = mesh;
			}
			else
			{
				m_Mesh.objectReferenceValue = null;
			}
		}
		else
		{
			m_Mesh.objectReferenceValue = null;
		}

		if (m_Mesh.objectReferenceValue != null)
		{
			newNameHash = (m_Mesh.objectReferenceValue as Mesh).name.GetHashCode();
		}

		m_MeshFilter.objectReferenceValue = null;
		EditorGUILayout.PropertyField(m_Dimensions);
		DrawShapeTransform(m_Transform);
		SamplingChannelsFields(sampler, false);
		return oldNameHash != newNameHash;

	}

	//----------------------------------------------------------------------------

	public void DrawSphere(int i, PKFxFX.Sampler sampler, bool worldSpaceSampling)
	{
		PKFxFX fx = (PKFxFX)target;
		float radius = sampler.m_Dimensions.x;
		float innerRadius = sampler.m_Dimensions.y;
		Matrix4x4 shapeTransform = sampler.m_ShapeTransform.transform;

		if (!worldSpaceSampling)
		{
			shapeTransform = ((GameObject)fx.gameObject).transform.localToWorldMatrix * shapeTransform;
		}

		Vector3		position = shapeTransform.GetColumn(3);
		Quaternion	rotation = shapeTransform.rotation;
		Vector3		scale = shapeTransform.lossyScale;

		Handles.color = Color.blue;
		innerRadius = Handles.RadiusHandle(rotation, position, Mathf.Min(radius, innerRadius));
		Handles.color = Color.cyan;
		radius = Handles.RadiusHandle(rotation, position, Mathf.Max(radius, innerRadius));
	}

	//----------------------------------------------------------------------------

	private void _PrimitiveCapsule(float radius, Vector2 minMax, float height, Vector3 center, Quaternion rotation)
	{
		Vector3 topCenter = center + new Vector3(0f, height/2f, 0f);
		Vector3 lowCenter = center - new Vector3(0f, height/2f, 0f);

		Vector3 dir = topCenter - center;
		dir = rotation * dir;
		topCenter = center + dir;
		
		dir = lowCenter - center;
		dir = rotation * dir;
		lowCenter = center + dir;

		if (minMax.x != -1)
		{
			radius = Handles.RadiusHandle(rotation, topCenter, Mathf.Max(radius, minMax.x));
			radius = Handles.RadiusHandle(rotation, lowCenter, Mathf.Max(radius, minMax.x));
		}
		else if (minMax.y != -1)
		{
			radius = Handles.RadiusHandle(rotation, topCenter, Mathf.Min(radius, minMax.y));
			radius = Handles.RadiusHandle(rotation, lowCenter, Mathf.Min(radius, minMax.y));
		}
		Handles.DrawLine(topCenter + rotation * new Vector3(radius,0f,0f), lowCenter + rotation * new Vector3(radius,0f,0f));
		Handles.DrawLine(topCenter - rotation * new Vector3(radius,0f,0f), lowCenter - rotation * new Vector3(radius,0f,0f));
		Handles.DrawLine(topCenter + rotation * new Vector3(0f,0f,radius), lowCenter + rotation * new Vector3(0f,0f,radius));
		Handles.DrawLine(topCenter - rotation * new Vector3(0f,0f,radius), lowCenter - rotation * new Vector3(0f,0f,radius));
	}

	public void DrawCapsule(int i, PKFxFX.Sampler sampler, bool worldSpaceSampling)
	{
		PKFxFX fx = (PKFxFX)target;

		float radius = sampler.m_Dimensions.x;
		float innerRadius = sampler.m_Dimensions.y;
		float height = sampler.m_Dimensions.z;
		Matrix4x4 shapeTransform = sampler.m_ShapeTransform.transform;

		if (!worldSpaceSampling)
		{
			shapeTransform = ((GameObject)fx.gameObject).transform.localToWorldMatrix * shapeTransform;
		}

		Vector3		position = shapeTransform.GetColumn(3);
		Quaternion	rotation = shapeTransform.rotation;

		Handles.color = Color.blue;
		_PrimitiveCapsule(innerRadius, new Vector2(-1, radius), height, position, rotation);

		Handles.color = Color.cyan;
		_PrimitiveCapsule(radius, new Vector2(innerRadius, -1), height, position, rotation);
	}

	//----------------------------------------------------------------------------

	public void _PrimitiveCylinder(float radius, float height, Vector3 center, Quaternion rotation)
	{
		Vector3 topCenter = center + new Vector3(0f, height/2f, 0f);
		Vector3 lowCenter = center - new Vector3(0f, height/2f, 0f);

		Vector3 dir = topCenter - center;
		dir = rotation * dir;
		topCenter = center + dir;

		dir = lowCenter - center;
		dir = rotation * dir;
		lowCenter = center + dir;

#if UNITY_5_6_OR_NEWER
		Handles.CircleHandleCap(0, topCenter, rotation * Quaternion.FromToRotation(Vector3.forward, Vector3.up), radius, EventType.Repaint);
		Handles.CircleHandleCap(0, lowCenter, rotation * Quaternion.FromToRotation(Vector3.forward, Vector3.up), radius, EventType.Repaint);
#else
		Handles.CircleCap(0, topCenter, rotation * Quaternion.FromToRotation(Vector3.forward, Vector3.up), radius);
		Handles.CircleCap(0, lowCenter, rotation * Quaternion.FromToRotation(Vector3.forward, Vector3.up), radius);
#endif
		Handles.DrawLine(topCenter + rotation * new Vector3(radius,0f,0f), lowCenter + rotation * new Vector3(radius,0f,0f));
		Handles.DrawLine(topCenter - rotation * new Vector3(radius,0f,0f), lowCenter - rotation * new Vector3(radius,0f,0f));
		Handles.DrawLine(topCenter + rotation * new Vector3(0f,0f,radius), lowCenter + rotation * new Vector3(0f,0f,radius));
		Handles.DrawLine(topCenter - rotation * new Vector3(0f,0f,radius), lowCenter - rotation * new Vector3(0f,0f,radius));
	}

	public void DrawCylinder(int i, PKFxFX.Sampler sampler, bool worldSpaceSampling)
	{
		PKFxFX fx = (PKFxFX)target;
		float radius = sampler.m_Dimensions.x;
		float innerRadius = sampler.m_Dimensions.y;
		float height = sampler.m_Dimensions.z;
		Matrix4x4 shapeTransform = sampler.m_ShapeTransform.transform;

		if (!worldSpaceSampling)
		{
			shapeTransform = ((GameObject)fx.gameObject).transform.localToWorldMatrix * shapeTransform;
		}

		Vector3		position = shapeTransform.GetColumn(3);
		Quaternion	rotation = shapeTransform.rotation;

		Handles.color = Color.blue;
		_PrimitiveCylinder(innerRadius, height, position, rotation);

		Handles.color = Color.cyan;
		_PrimitiveCylinder(radius, height, position, rotation);
	}

	//----------------------------------------------------------------------------

	public void DrawCube(int i, PKFxFX.Sampler sampler, bool worldSpaceSampling)
	{
		PKFxFX fx = (PKFxFX)target;

		Matrix4x4 shapeTransform = sampler.m_ShapeTransform.transform;

		if (!worldSpaceSampling)
		{
			shapeTransform = ((GameObject)fx.gameObject).transform.localToWorldMatrix * shapeTransform;
		}

		Vector3		position = shapeTransform.GetColumn(3);
		Quaternion	rotation = shapeTransform.rotation;

		Vector3		size = sampler.m_Dimensions;
		Vector3		A = position + rotation * new Vector3(-size.x/2, size.y/2, size.z/2);
		Vector3		B = position + rotation * new Vector3(size.x/2, size.y/2, size.z/2);
		Vector3		C = position + rotation * new Vector3(size.x/2, -size.y/2, size.z/2);
		Vector3		D = position + rotation * new Vector3(-size.x/2, -size.y/2, size.z/2);
		Vector3		E = position + rotation * new Vector3(-size.x/2, size.y/2, -size.z/2);
		Vector3		F = position + rotation * new Vector3(size.x/2, size.y/2,  -size.z/2);
		Vector3		G = position + rotation * new Vector3(size.x/2, -size.y/2, -size.z/2);
		Vector3		H = position + rotation * new Vector3(-size.x/2, -size.y/2, -size.z/2);
		Vector3[]	face = new Vector3[5];

		Handles.color = Color.cyan;
		face[0] = A;
		face[1] = B;
		face[2] = C;
		face[3] = D;
		face[4] = A;
		Handles.DrawPolyLine(face);
		face[0] = A;
		face[1] = E;
		face[2] = H;
		face[3] = D;
		face[4] = A;
		Handles.DrawPolyLine(face);
		face[0] = B;
		face[1] = F;
		face[2] = G;
		face[3] = C;
		face[4] = B;
		Handles.DrawPolyLine(face);
		face[0] = E;
		face[1] = F;
		face[2] = G;
		face[3] = H;
		face[4] = E;
		Handles.DrawPolyLine(face);
	}

	public static void			DrawShapeTransform(SerializedProperty shapeTransform)
	{
		++EditorGUI.indentLevel;
		shapeTransform.isExpanded = EditorGUILayout.Foldout(shapeTransform.isExpanded, "Transform");

		if (shapeTransform.isExpanded)
		{
			SerializedProperty positionProp = shapeTransform.FindPropertyRelative("m_Position");
			SerializedProperty rotationProp = shapeTransform.FindPropertyRelative("m_Rotation");
			SerializedProperty scaleProp = shapeTransform.FindPropertyRelative("m_Scale");

			positionProp.vector3Value = EditorGUILayout.Vector3Field("Position", positionProp.vector3Value);
			rotationProp.quaternionValue = Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation", rotationProp.quaternionValue.eulerAngles));
			scaleProp.vector3Value = EditorGUILayout.Vector3Field("Scale", scaleProp.vector3Value);
		}
		--EditorGUI.indentLevel;
	}

	public static bool EditorUpdatePkFX(PKFxFX fx, bool resetAllAttributes)
	{
		bool fxWasUpdated = false;
		bool failedLoading = false;

		fx.UpdateAssetPathIFN();
		if (fx.m_FxAsset == null && !string.IsNullOrEmpty(fx.m_FxName))
		{
			string parentfolderPath = "Assets" + PKFxSettings.UnityPackFxPath;
			string assetPath = Path.Combine(parentfolderPath, fx.m_FxName);

			fxWasUpdated = true;
			fx.m_FxAsset = (PKFxFxAsset)AssetDatabase.LoadAssetAtPath(assetPath, typeof(PKFxFxAsset));

			if (fx.m_FxAsset == null)
			{
				failedLoading = true;
				fx.ClearAllAttributesAndSamplers();
			}
		}
		// We update the attributes and samplers IFN:
		if (!failedLoading && fx.UpdateEffectAsset(fx.m_FxAsset, resetAllAttributes))
		{
			fxWasUpdated = true;
		}
		return fxWasUpdated;
	}
}
