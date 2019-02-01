using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class PKFxAttributePropertyDrawer : PropertyDrawer
{
    private static void DrawFloatAttribute(SerializedProperty attrDesc, ref float valueX)
    {
        GUIContent content = CreateGUIContentFromAttribute(attrDesc);

		SerializedProperty m_MinMaxFlag = attrDesc.FindPropertyRelative("m_MinMaxFlag");
		SerializedProperty m_MinValue0 = attrDesc.FindPropertyRelative("m_MinValue0");
		SerializedProperty m_MaxValue0 = attrDesc.FindPropertyRelative("m_MaxValue0");

		if (PKFxEditorTools.HasMin(m_MinMaxFlag.intValue) && PKFxEditorTools.HasMax(m_MinMaxFlag.intValue))
			valueX = PKFxEditorTools.FloatSlider(valueX, m_MinValue0, m_MaxValue0, content);
        else if (PKFxEditorTools.HasMin(m_MinMaxFlag.intValue))
            valueX = PKFxEditorTools.MinFloatField(valueX, m_MinValue0, content);
        else if (PKFxEditorTools.HasMax(m_MinMaxFlag.intValue))
			valueX = PKFxEditorTools.MaxFloatField(valueX, m_MaxValue0, content);
        else
			valueX = EditorGUILayout.FloatField(content, valueX);
    }

    //----------------------------------------------------------------------------

    private static void DrawFloat2Attribute(SerializedProperty attrDesc, ref float valueX, ref float valueY)
    {
		SerializedProperty m_MinMaxFlag = attrDesc.FindPropertyRelative("m_MinMaxFlag");
		SerializedProperty m_MinValue0 = attrDesc.FindPropertyRelative("m_MinValue0");
		SerializedProperty m_MaxValue0 = attrDesc.FindPropertyRelative("m_MaxValue0");
		SerializedProperty m_MinValue1 = attrDesc.FindPropertyRelative("m_MinValue1");
		SerializedProperty m_MaxValue1 = attrDesc.FindPropertyRelative("m_MaxValue1");

		if (PKFxEditorTools.HasMin(m_MinMaxFlag.intValue) && PKFxEditorTools.HasMax(m_MinMaxFlag.intValue))
        {
            DrawAttributeName(attrDesc, true);
            if (attrDesc.isExpanded)
            {
                ++EditorGUI.indentLevel;
                {
                    valueX = PKFxEditorTools.FloatSlider(valueX, m_MinValue0, m_MaxValue0, new GUIContent("X"));
                    valueY = PKFxEditorTools.FloatSlider(valueY, m_MinValue1, m_MaxValue1, new GUIContent("Y"));
                }
                --EditorGUI.indentLevel;
            }
        }
        else
        {
            DrawAttributeName(attrDesc);
            Vector2 tmp2 = new Vector2(valueX, valueY);
            tmp2 = EditorGUILayout.Vector2Field(GUIContent.none, tmp2);

            if (PKFxEditorTools.HasMin(m_MinMaxFlag.intValue))
				tmp2 = PKFxEditorTools.Maxf(tmp2, m_MinValue0, m_MinValue1);
            if (PKFxEditorTools.HasMax(m_MinMaxFlag.intValue))
				tmp2 = PKFxEditorTools.Minf(tmp2, m_MaxValue0, m_MaxValue1);

            valueX = tmp2.x;
			valueY = tmp2.y;
        }
    }

    //----------------------------------------------------------------------------

    private static void DrawFloat3Attribute(SerializedProperty attrDesc, ref float valueX, ref float valueY, ref float valueZ)
    {
		SerializedProperty m_MinMaxFlag = attrDesc.FindPropertyRelative("m_MinMaxFlag");
		SerializedProperty m_MinValue0 = attrDesc.FindPropertyRelative("m_MinValue0");
		SerializedProperty m_MaxValue0 = attrDesc.FindPropertyRelative("m_MaxValue0");
		SerializedProperty m_MinValue1 = attrDesc.FindPropertyRelative("m_MinValue1");
		SerializedProperty m_MaxValue1 = attrDesc.FindPropertyRelative("m_MaxValue1");
		SerializedProperty m_MinValue2 = attrDesc.FindPropertyRelative("m_MinValue2");
		SerializedProperty m_MaxValue2 = attrDesc.FindPropertyRelative("m_MaxValue2");

        if (PKFxEditorTools.HasMin(m_MinMaxFlag.intValue) && PKFxEditorTools.HasMax(m_MinMaxFlag.intValue))
        {
            DrawAttributeName(attrDesc, true);
            PKFxEditorTools.ColorPicker(GUIContent.none, ref valueX, ref valueY, ref valueZ);

			if (attrDesc.isExpanded)
            {
                ++EditorGUI.indentLevel;
                {
					valueX = PKFxEditorTools.FloatSlider(valueX, m_MinValue0, m_MaxValue0, new GUIContent("X"));
					valueY = PKFxEditorTools.FloatSlider(valueY, m_MinValue1, m_MaxValue1, new GUIContent("Y"));
					valueZ = PKFxEditorTools.FloatSlider(valueZ, m_MinValue2, m_MaxValue2, new GUIContent("Z"));
                }
                --EditorGUI.indentLevel;
            }
        }
        else
        {
            DrawAttributeName(attrDesc);
			Vector3 tmp3 = new Vector3(valueX, valueY, valueZ);
            tmp3 = EditorGUILayout.Vector3Field(GUIContent.none, tmp3);

            {
                if (PKFxEditorTools.HasMin(m_MinMaxFlag.intValue))
                    tmp3 = PKFxEditorTools.Maxf(tmp3, m_MinValue0, m_MinValue1, m_MinValue2);
                if (PKFxEditorTools.HasMax(m_MinMaxFlag.intValue))
                    tmp3 = PKFxEditorTools.Minf(tmp3, m_MaxValue0, m_MaxValue1, m_MaxValue2);

                valueX = tmp3.x;
                valueY = tmp3.y;
                valueZ = tmp3.z;
            }
            PKFxEditorTools.ColorPicker(GUIContent.none, ref valueX, ref valueY, ref valueZ);
        }

		// Re-clamp between min and max at the end:
		{
			Vector3 tmp3 = new Vector3(valueX, valueY, valueZ);

			if (PKFxEditorTools.HasMin(m_MinMaxFlag.intValue))
				tmp3 = PKFxEditorTools.Maxf(tmp3, m_MinValue0, m_MinValue1, m_MinValue2);
			if (PKFxEditorTools.HasMax(m_MinMaxFlag.intValue))
				tmp3 = PKFxEditorTools.Minf(tmp3, m_MaxValue0, m_MaxValue1, m_MaxValue2);

			valueX = tmp3.x;
			valueY = tmp3.y;
			valueZ = tmp3.z;
		}
	}

	//----------------------------------------------------------------------------

	private static void DrawFloat4Attribute(SerializedProperty attrDesc, ref float valueX, ref float valueY, ref float valueZ, ref float valueW)
    {
		SerializedProperty m_MinMaxFlag = attrDesc.FindPropertyRelative("m_MinMaxFlag");
		SerializedProperty m_MinValue0 = attrDesc.FindPropertyRelative("m_MinValue0");
		SerializedProperty m_MaxValue0 = attrDesc.FindPropertyRelative("m_MaxValue0");
		SerializedProperty m_MinValue1 = attrDesc.FindPropertyRelative("m_MinValue1");
		SerializedProperty m_MaxValue1 = attrDesc.FindPropertyRelative("m_MaxValue1");
		SerializedProperty m_MinValue2 = attrDesc.FindPropertyRelative("m_MinValue2");
		SerializedProperty m_MaxValue2 = attrDesc.FindPropertyRelative("m_MaxValue2");
		SerializedProperty m_MinValue3 = attrDesc.FindPropertyRelative("m_MinValue3");
		SerializedProperty m_MaxValue3 = attrDesc.FindPropertyRelative("m_MaxValue3");

		if (PKFxEditorTools.HasMin(m_MinMaxFlag.intValue) && PKFxEditorTools.HasMax(m_MinMaxFlag.intValue))
		{
			DrawAttributeName(attrDesc, true);
			PKFxEditorTools.ColorPicker(GUIContent.none, ref valueX, ref valueY, ref valueZ, ref valueW);

			if (attrDesc.isExpanded)
			{
				++EditorGUI.indentLevel;
				{
					valueX = PKFxEditorTools.FloatSlider(valueX, m_MinValue0, m_MaxValue0, new GUIContent("X"));
					valueY = PKFxEditorTools.FloatSlider(valueY, m_MinValue1, m_MaxValue1, new GUIContent("Y"));
					valueZ = PKFxEditorTools.FloatSlider(valueZ, m_MinValue2, m_MaxValue2, new GUIContent("Z"));
					valueW = PKFxEditorTools.FloatSlider(valueW, m_MinValue3, m_MaxValue3, new GUIContent("W"));
				}
				--EditorGUI.indentLevel;
			}
		}
		else
		{
			DrawAttributeName(attrDesc);
			Vector4 tmp4 = new Vector4(valueX, valueY, valueZ, valueW);
			tmp4 = EditorGUILayout.Vector4Field(GUIContent.none, tmp4);

			{
				if (PKFxEditorTools.HasMin(m_MinMaxFlag.intValue))
					tmp4 = PKFxEditorTools.Maxf(tmp4, m_MinValue0, m_MinValue1, m_MinValue2, m_MinValue3);
				if (PKFxEditorTools.HasMax(m_MinMaxFlag.intValue))
					tmp4 = PKFxEditorTools.Minf(tmp4, m_MaxValue0, m_MaxValue1, m_MaxValue2, m_MaxValue3);

				valueX = tmp4.x;
				valueY = tmp4.y;
				valueZ = tmp4.z;
				valueW = tmp4.w;
			}
			PKFxEditorTools.ColorPicker(GUIContent.none, ref valueX, ref valueY, ref valueZ, ref valueW);
		}

		// Re-clamp between min and max at the end:
		{
			Vector4 tmp4 = new Vector4(valueX, valueY, valueZ, valueW);

			if (PKFxEditorTools.HasMin(m_MinMaxFlag.intValue))
				tmp4 = PKFxEditorTools.Maxf(tmp4, m_MinValue0, m_MinValue1, m_MinValue2, m_MinValue3);
			if (PKFxEditorTools.HasMax(m_MinMaxFlag.intValue))
				tmp4 = PKFxEditorTools.Minf(tmp4, m_MaxValue0, m_MaxValue1, m_MaxValue2, m_MaxValue3);

			valueX = tmp4.x;
			valueY = tmp4.y;
			valueZ = tmp4.z;
			valueW = tmp4.w;
		}
	}

}
