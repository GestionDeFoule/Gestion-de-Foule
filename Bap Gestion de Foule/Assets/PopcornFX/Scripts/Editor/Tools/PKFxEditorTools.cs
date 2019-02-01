using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public static class PKFxEditorTools
{
    public const string IconsRootDirectory = "Icons";

    public static readonly Color EditorBackgroundColor = EditorGUIUtility.isProSkin
        ? new Color32(56, 56, 56, 255)
        : new Color32(194, 194, 194, 255);

    public static readonly Color EditorBoxBackgroundColor = EditorGUIUtility.isProSkin
        ? new Color32(56, 56, 56, 255) // TODO : check the color on a ProSkin editor
        : new Color32(208, 208, 208, 255);

    public static bool ColorPicker(GUIContent content, ref float r, ref float g, ref float b)
    {
        Color color = new Color(r, g, b, 1.0f);
        Color newColor = EditorGUILayout.ColorField(content, color);

		if (color != newColor)
        {
            r = newColor.r;
            g = newColor.g;
            b = newColor.b;
            return true;
        }
		return false;
    }

	public static bool ColorPicker(GUIContent content, ref float r, ref float g, ref float b, ref float a)
	{
		Color color = new Color(r, g, b, a);
		Color newColor = EditorGUILayout.ColorField(content, color);

		if (color != newColor)
		{
			r = newColor.r;
			g = newColor.g;
			b = newColor.b;
			a = newColor.a;
			return true;
		}
		return false;
	}


	//----------------------------------------------------------------------------

	public static float FloatSlider(float value, SerializedProperty min, SerializedProperty max, GUIContent content = null)
    {
        if (content != null)
            return EditorGUILayout.Slider(content, value, min.floatValue, max.floatValue);
        else
            return EditorGUILayout.Slider(value, min.floatValue, max.floatValue);
    }

    //----------------------------------------------------------------------------

    public static float IntSlider(float value, SerializedProperty min, SerializedProperty max, GUIContent content = null)
    {
		int intValue = PKFxManagerImpl.Float2Int(value);

		if (content != null)
            return PKFxManagerImpl.Int2Float(EditorGUILayout.IntSlider(content, intValue, PKFxManagerImpl.Float2Int(min.floatValue), PKFxManagerImpl.Float2Int(max.floatValue)));
        else
            return PKFxManagerImpl.Int2Float(EditorGUILayout.IntSlider(intValue, PKFxManagerImpl.Float2Int(min.floatValue), PKFxManagerImpl.Float2Int(max.floatValue)));
    }

    //----------------------------------------------------------------------------

    public static float MinFloatField(float value, SerializedProperty min, GUIContent content = null)
    {
        if (content == null)
        {
            content = GUIContent.none;
        }
        return Mathf.Max(EditorGUILayout.FloatField(content, value), min.floatValue);
    }

    public static float MaxFloatField(float value, SerializedProperty max, GUIContent content = null)
    {
        if (content == null)
        {
            content = GUIContent.none;
        }
        return Mathf.Min(EditorGUILayout.FloatField(content, value), max.floatValue);
    }

    public static float MaxIntField(float value, SerializedProperty max, GUIContent content = null)
    {
		int intValue = PKFxManagerImpl.Float2Int(value);

		if (content == null)
        {
            content = GUIContent.none;
        }
        return PKFxManagerImpl.Int2Float(Mathf.Min(EditorGUILayout.IntField(content, intValue), PKFxManagerImpl.Float2Int(max.floatValue)));
    }

    public static float MinIntField(float value, SerializedProperty min, GUIContent content = null)
    {
		int intValue = PKFxManagerImpl.Float2Int(value);

		if (content == null)
        {
            content = GUIContent.none;
        }
        return PKFxManagerImpl.Int2Float(Mathf.Max(EditorGUILayout.IntField(content, intValue), PKFxManagerImpl.Float2Int(min.floatValue)));
    }

    //----------------------------------------------------------------------------

    public static float IntField(SerializedProperty v)
    {
        return PKFxManagerImpl.Int2Float(EditorGUILayout.IntField(PKFxManagerImpl.Float2Int(v.floatValue)));
    }

    //----------------------------------------------------------------------------

    public static bool HasMin(int flag)
    {
        return (flag & (int)PKFxFxAsset.AttributeDesc.EAttrDescFlag.HasMin) != 0;
    }

    //----------------------------------------------------------------------------

    public static bool HasMax(int flag)
    {
        return (flag & (int)PKFxFxAsset.AttributeDesc.EAttrDescFlag.HasMax) != 0;
    }

    //----------------------------------------------------------------------------

    public static Vector2 Minf(Vector2 v, SerializedProperty x, SerializedProperty y)
    {
        v.x = Mathf.Min(v.x, x.floatValue);
        v.y = Mathf.Min(v.y, y.floatValue);
        return v;
    }

    public static Vector3 Minf(Vector3 v, SerializedProperty x, SerializedProperty y, SerializedProperty z)
    {
        v.x = Mathf.Min(v.x, x.floatValue);
        v.y = Mathf.Min(v.y, y.floatValue);
        v.z = Mathf.Min(v.z, z.floatValue);
        return v;
    }

    public static Vector4 Minf(Vector4 v, SerializedProperty x, SerializedProperty y, SerializedProperty z, SerializedProperty w)
    {
        v.x = Mathf.Min(v.x, x.floatValue);
        v.y = Mathf.Min(v.y, y.floatValue);
        v.z = Mathf.Min(v.z, z.floatValue);
        v.w = Mathf.Min(v.w, w.floatValue);
        return v;
    }

    //----------------------------------------------------------------------------

    public static Vector2 Maxf(Vector2 v, SerializedProperty x, SerializedProperty y)
    {
        v.x = Mathf.Max(v.x, x.floatValue);
        v.y = Mathf.Max(v.y, y.floatValue);
        return v;
    }

    public static Vector3 Maxf(Vector3 v, SerializedProperty x, SerializedProperty y, SerializedProperty z)
    {
        v.x = Mathf.Max(v.x, x.floatValue);
        v.y = Mathf.Max(v.y, y.floatValue);
        v.z = Mathf.Max(v.z, z.floatValue);
        return v;
    }

    public static Vector4 Maxf(Vector4 v, SerializedProperty x, SerializedProperty y, SerializedProperty z, SerializedProperty w)
    {
        v.x = Mathf.Max(v.x, x.floatValue);
        v.y = Mathf.Max(v.y, y.floatValue);
        v.z = Mathf.Max(v.z, z.floatValue);
        v.w = Mathf.Max(v.w, w.floatValue);
        return v;
    }

    //----------------------------------------------------------------------------

    public static float Min(SerializedProperty p, SerializedProperty min)
    {
        return PKFxManagerImpl.Int2Float(Mathf.Min(PKFxManagerImpl.Float2Int(p.floatValue), PKFxManagerImpl.Float2Int(min.floatValue)));
    }

    //----------------------------------------------------------------------------

    public static float Max(SerializedProperty p, SerializedProperty max)
    {
        return PKFxManagerImpl.Int2Float(Mathf.Max(PKFxManagerImpl.Float2Int(p.floatValue), PKFxManagerImpl.Float2Int(max.floatValue)));
    }

}
