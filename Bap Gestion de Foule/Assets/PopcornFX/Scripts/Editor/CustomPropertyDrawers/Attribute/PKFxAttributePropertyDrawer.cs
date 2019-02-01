using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public partial class PKFxAttributePropertyDrawer
{
    public const int IconSize = 13;

	public static bool Vector4IntAreEqual(Vector4 value1, Vector4 value2)
	{
		int val1X = PKFxManagerImpl.Float2Int(value1.x);
		int val1Y = PKFxManagerImpl.Float2Int(value1.y);
		int val1Z = PKFxManagerImpl.Float2Int(value1.z);
		int val1W = PKFxManagerImpl.Float2Int(value1.w);

		int val2X = PKFxManagerImpl.Float2Int(value2.x);
		int val2Y = PKFxManagerImpl.Float2Int(value2.y);
		int val2Z = PKFxManagerImpl.Float2Int(value2.z);
		int val2W = PKFxManagerImpl.Float2Int(value2.w);

		return	val1X == val2X && val1Y == val2Y && val1Z == val2Z && val1W == val2W;
	}

	public static bool DrawAttribute(SerializedProperty attrDesc, ref Vector4 value)
	{
		Vector4				oldValue = value;
		SerializedProperty	m_Type = attrDesc.FindPropertyRelative("m_Type");

		++EditorGUI.indentLevel;
		switch ((PKFxManagerImpl.EBaseType)m_Type.intValue)
		{
			case PKFxManagerImpl.EBaseType.Float:
				DrawFloatAttribute(attrDesc, ref value.x);
				break;
			case PKFxManagerImpl.EBaseType.Float2:
				DrawFloat2Attribute(attrDesc, ref value.x, ref value.y);
				break;
			case PKFxManagerImpl.EBaseType.Float3:
				DrawFloat3Attribute(attrDesc, ref value.x, ref value.y, ref value.z);
				break;
			case PKFxManagerImpl.EBaseType.Float4:
				DrawFloat4Attribute(attrDesc, ref value.x, ref value.y, ref value.z, ref value.w);
				break;

			case PKFxManagerImpl.EBaseType.Int:
				DrawIntAttribute(attrDesc, ref value.x);
				break;
			case PKFxManagerImpl.EBaseType.Int2:
				DrawInt2Attribute(attrDesc, ref value.x, ref value.y);
				break;
			case PKFxManagerImpl.EBaseType.Int3:
				DrawInt3Attribute(attrDesc, ref value.x, ref value.y, ref value.z);
				break;
			case PKFxManagerImpl.EBaseType.Int4:
				DrawInt4Attribute(attrDesc, ref value.x, ref value.y, ref value.z, ref value.w);
				break;
		}
		--EditorGUI.indentLevel;
		return !Vector4IntAreEqual(oldValue, value);
	}

	public static bool DrawAttribute(SerializedProperty attrDesc, SerializedProperty propX, SerializedProperty propY, SerializedProperty propZ, SerializedProperty propW)
    {
		Vector4 value = new Vector4(propX.floatValue, propY.floatValue, propZ.floatValue, propW.floatValue);

		bool	wasModified = DrawAttribute(attrDesc, ref value);

		propX.floatValue = value.x;
		propY.floatValue = value.y;
		propZ.floatValue = value.z;
		propW.floatValue = value.w;
		return wasModified;
	}

	/// <summary>
	/// Draw attribute name with icon and tooltip
	/// </summary>
	/// <returns>If not folding out, the position returned by PrefixLabel</returns>
	public static void DrawAttributeName(SerializedProperty attrDesc, bool canFoldOut = false)
    {
        GUIContent content = CreateGUIContentFromAttribute(attrDesc);

        if (canFoldOut)
        {
            Rect iconRect = new Rect(0, 0, IconSize, IconSize);
            GUI.DrawTexture(iconRect, content.image, ScaleMode.ScaleToFit);

            content.image = null;
			attrDesc.isExpanded = EditorGUILayout.Foldout(attrDesc.isExpanded, content);
        }
        else
        {
            EditorGUILayout.PrefixLabel(content);
        }
    }

    public static GUIContent CreateGUIContentFromAttribute(SerializedProperty attrDesc)
    {
        SerializedProperty nameProp = attrDesc.FindPropertyRelative("m_Name");
        SerializedProperty descriptionProp = attrDesc.FindPropertyRelative("m_Description");
        SerializedProperty typeProp = attrDesc.FindPropertyRelative("m_Type");

        Texture2D icon = LoadTextureByAttributeType((PKFxManagerImpl.EBaseType)typeProp.intValue);
        string description = descriptionProp.stringValue.Replace("\\n", "\n");
        return new GUIContent(nameProp.stringValue, icon, description);
    }

    //----------------------------------------------------------------------------

    private static Texture2D LoadTextureByAttributeType(PKFxManagerImpl.EBaseType AttributeType)
    {
        string iconPath;
        switch (AttributeType)
        {
            case PKFxManagerImpl.EBaseType.Int:
                iconPath = "AttributeI1";
                break;
            case PKFxManagerImpl.EBaseType.Int2:
                iconPath = "AttributeI2";
                break;
            case PKFxManagerImpl.EBaseType.Int3:
                iconPath = "AttributeI3";
                break;
            case PKFxManagerImpl.EBaseType.Int4:
                iconPath = "AttributeI4";
                break;
            case PKFxManagerImpl.EBaseType.Float:
                iconPath = "AttributeF1";
                break;
            case PKFxManagerImpl.EBaseType.Float2:
                iconPath = "AttributeF2";
                break;
            case PKFxManagerImpl.EBaseType.Float3:
                iconPath = "AttributeF3";
                break;
            case PKFxManagerImpl.EBaseType.Float4:
                iconPath = "AttributeF4";
                break;
            default:
                return (null);
        }

        return (Resources.Load<Texture2D>(PKFxEditorTools.IconsRootDirectory + "/" + iconPath));
    }

    private float GetHeightForControls(int numControls)
    {
        return (EditorGUIUtility.singleLineHeight * numControls + EditorGUIUtility.standardVerticalSpacing * (numControls - 1));
    }
}
