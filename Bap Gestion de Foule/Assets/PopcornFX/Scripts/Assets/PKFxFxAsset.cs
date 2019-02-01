using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;
using System;
using System.Runtime.InteropServices;

public class PKFxFxAsset : PKFxAsset
{
	[HideInInspector]
	public string m_FXText = "";
	[HideInInspector]
	public override byte[] m_Data
	{
		get
		{
			return _data;
		}

		set
		{
			_data = value;
			m_FXText = System.Text.Encoding.ASCII.GetString(_data);
		}
	}

	[Serializable]
	public class DependencyDesc
	{
		public string	m_Path;
		public bool		m_IsTextureLinear;
		public bool		m_IsMeshRenderer;
		public bool		m_IsMeshSampler;
		public bool		m_IsTextureSampler;

		public UnityEngine.Object m_Object;
	}

	[Serializable]
	public class AttributeDesc
	{
		// Has min, has max, has desc:
		public enum EAttrDescFlag : int
		{
			HasMin = 0x01,
			HasMax = 0x02,
			HasDesc = 0x04,
		};

		[FormerlySerializedAs("Type")]
		public PKFxManagerImpl.EBaseType m_Type;
		[FormerlySerializedAs("MinMaxFlag")]
		public int m_MinMaxFlag;
		[FormerlySerializedAs("Name")]
		public string m_Name;
		[FormerlySerializedAs("Description")]
		public string m_Description;

		// We do not store Vector4 for backward compatibility reasons:
		[FormerlySerializedAs("DefaultValue0")]
		public float m_DefaultValue0;
		[FormerlySerializedAs("DefaultValue1")]
		public float m_DefaultValue1;
		[FormerlySerializedAs("DefaultValue2")]
		public float m_DefaultValue2;
		[FormerlySerializedAs("DefaultValue3")]
		public float m_DefaultValue3;
		[FormerlySerializedAs("MinValue0")]
		public float m_MinValue0;
		[FormerlySerializedAs("MinValue1")]
		public float m_MinValue1;
		[FormerlySerializedAs("MinValue2")]
		public float m_MinValue2;
		[FormerlySerializedAs("MinValue3")]
		public float m_MinValue3;
		[FormerlySerializedAs("MaxValue0")]
		public float m_MaxValue0;
		[FormerlySerializedAs("MaxValue1")]
		public float m_MaxValue1;
		[FormerlySerializedAs("MaxValue2")]
		public float m_MaxValue2;
		[FormerlySerializedAs("MaxValue3")]
		public float m_MaxValue3;

		public AttributeDesc(PKFxManagerImpl.SNativeAttributeDesc desc)
		{
			m_Type = (PKFxManagerImpl.EBaseType)desc.m_AttributeType;
			m_MinMaxFlag = (int)desc.m_MinMaxFlag;
			m_Name = Marshal.PtrToStringAnsi(desc.m_AttributeName);
			//			if ((byte)desc.MinMaxFlag & EAttrDescFlag.HasDesc)
			m_Description = Marshal.PtrToStringAnsi(desc.m_Description);
			m_DefaultValue0 = desc.m_DefaultValueX;
			m_DefaultValue1 = desc.m_DefaultValueY;
			m_DefaultValue2 = desc.m_DefaultValueZ;
			m_DefaultValue3 = desc.m_DefaultValueW;
			m_MinValue0 = desc.m_MinValueX;
			m_MinValue1 = desc.m_MinValueY;
			m_MinValue2 = desc.m_MinValueZ;
			m_MinValue3 = desc.m_MinValueW;
			m_MaxValue0 = desc.m_MaxValueX;
			m_MaxValue1 = desc.m_MaxValueY;
			m_MaxValue2 = desc.m_MaxValueZ;
			m_MaxValue3 = desc.m_MaxValueW;
		}

		public AttributeDesc(PKFxManagerImpl.EBaseType type, IntPtr name)
		{
			m_Type = type;
			m_Name = Marshal.PtrToStringAnsi(name);
		}

		public AttributeDesc(PKFxManagerImpl.EBaseType type, string name)
		{
			m_Type = type;
			m_Name = name;
		}

		public bool HasMin()
		{
			return (m_MinMaxFlag & (int)EAttrDescFlag.HasMin) != 0;
		}

		public bool HasMax()
		{
			return (m_MinMaxFlag & (int)EAttrDescFlag.HasMax) != 0;
		}

		public Vector4		GetDefaultAttributeValueClamped()
		{
			Vector4 clampedValue = new Vector4(m_DefaultValue0, m_DefaultValue1, m_DefaultValue2, m_DefaultValue3);

			return ClampAttributeValue(clampedValue);
		}

		public Vector4		ClampAttributeValue(Vector4 attributeValue)
		{
			if (HasMin())
			{
				if (m_Type == PKFxManagerImpl.EBaseType.Float ||
					m_Type == PKFxManagerImpl.EBaseType.Float2 ||
					m_Type == PKFxManagerImpl.EBaseType.Float3 ||
					m_Type == PKFxManagerImpl.EBaseType.Float4)
				{
					attributeValue.x = Mathf.Max(m_MinValue0, attributeValue.x);
					attributeValue.y = Mathf.Max(m_MinValue1, attributeValue.y);
					attributeValue.z = Mathf.Max(m_MinValue2, attributeValue.z);
					attributeValue.w = Mathf.Max(m_MinValue3, attributeValue.w);
				}
				else if (	m_Type == PKFxManagerImpl.EBaseType.Int ||
							m_Type == PKFxManagerImpl.EBaseType.Int2 ||
							m_Type == PKFxManagerImpl.EBaseType.Int3 ||
							m_Type == PKFxManagerImpl.EBaseType.Int4)
				{
					attributeValue.x = PKFxManagerImpl.Int2Float(Mathf.Max(PKFxManagerImpl.Float2Int(m_MinValue0), PKFxManagerImpl.Float2Int(attributeValue.x)));
					attributeValue.y = PKFxManagerImpl.Int2Float(Mathf.Max(PKFxManagerImpl.Float2Int(m_MinValue1), PKFxManagerImpl.Float2Int(attributeValue.y)));
					attributeValue.z = PKFxManagerImpl.Int2Float(Mathf.Max(PKFxManagerImpl.Float2Int(m_MinValue2), PKFxManagerImpl.Float2Int(attributeValue.z)));
					attributeValue.w = PKFxManagerImpl.Int2Float(Mathf.Max(PKFxManagerImpl.Float2Int(m_MinValue3), PKFxManagerImpl.Float2Int(attributeValue.w)));
				}
			}
			if (HasMax())
			{
				if (m_Type == PKFxManagerImpl.EBaseType.Float ||
					m_Type == PKFxManagerImpl.EBaseType.Float2 ||
					m_Type == PKFxManagerImpl.EBaseType.Float3 ||
					m_Type == PKFxManagerImpl.EBaseType.Float4)
				{
					attributeValue.x = Mathf.Min(m_MaxValue0, attributeValue.x);
					attributeValue.y = Mathf.Min(m_MaxValue1, attributeValue.y);
					attributeValue.z = Mathf.Min(m_MaxValue2, attributeValue.z);
					attributeValue.w = Mathf.Min(m_MaxValue3, attributeValue.w);
				}
				else if (	m_Type == PKFxManagerImpl.EBaseType.Int ||
							m_Type == PKFxManagerImpl.EBaseType.Int2 ||
							m_Type == PKFxManagerImpl.EBaseType.Int3 ||
							m_Type == PKFxManagerImpl.EBaseType.Int4)
				{
					attributeValue.x = PKFxManagerImpl.Int2Float(Mathf.Min(PKFxManagerImpl.Float2Int(m_MaxValue0), PKFxManagerImpl.Float2Int(attributeValue.x)));
					attributeValue.y = PKFxManagerImpl.Int2Float(Mathf.Min(PKFxManagerImpl.Float2Int(m_MaxValue1), PKFxManagerImpl.Float2Int(attributeValue.y)));
					attributeValue.z = PKFxManagerImpl.Int2Float(Mathf.Min(PKFxManagerImpl.Float2Int(m_MaxValue2), PKFxManagerImpl.Float2Int(attributeValue.z)));
					attributeValue.w = PKFxManagerImpl.Int2Float(Mathf.Min(PKFxManagerImpl.Float2Int(m_MaxValue3), PKFxManagerImpl.Float2Int(attributeValue.w)));
				}
			}
			return attributeValue;
		}

		public override string	ToString()
		{
			// Just the type and the name are taken into account when mix and matching the attributes:
			return m_Type.ToString() + ";" + m_Name + ";";
		}
	}

	[Serializable]
	public class ShapeTransform
	{
		public Vector3 m_Position;
		public Quaternion m_Rotation;
		public Vector3 m_Scale;

		public ShapeTransform()
		{
			m_Position = Vector3.zero;
			m_Rotation = Quaternion.identity;
			m_Scale = Vector3.one;
		}

		public ShapeTransform(Vector3 position, Quaternion rotation, Vector3 scale)
		{
			m_Position = position;
			m_Rotation = rotation;
			m_Scale = scale;
		}

		public Matrix4x4 transform
		{
			get
			{
				Matrix4x4 mat = new Matrix4x4();
				mat.SetTRS(m_Position, m_Rotation, m_Scale);
				return mat;
			}
		}

		public static ShapeTransform identity
		{
			get
			{
				return new ShapeTransform();
			}
		}
	}

	[Serializable]
	public class CurveDefaultValue
	{
		public AnimationCurve[] m_Curves;

		public CurveDefaultValue(int dimension, int keyCount, IntPtr curveTimes, IntPtr floatValues, IntPtr floatTangents)
		{
			if (curveTimes == IntPtr.Zero || floatValues == IntPtr.Zero || floatTangents == IntPtr.Zero)
				return;
			unsafe
			{
				float* curveTimesPtr = (float*)curveTimes.ToPointer();
				float* floatValuesPtr = (float*)floatValues.ToPointer();
				float* floatTangentsPtr = (float*)floatTangents.ToPointer();

				m_Curves = new AnimationCurve[dimension];

				for (int c = 0; c < dimension; ++c)
				{
					m_Curves[c] = new AnimationCurve();

					for (int k = 0; k < keyCount; ++k)
					{
						Keyframe keyframe = new Keyframe(curveTimesPtr[k], *floatValuesPtr, floatTangentsPtr[0], floatTangentsPtr[1]);
						m_Curves[c].AddKey(keyframe);

						floatValuesPtr += 1;
						floatTangentsPtr += 2;
					}
				}
			}
		}
	}

	[Serializable]
	public class SamplerDesc
	{
		public enum ESamplerType : int
		{
			SamplerShape = 0,
			SamplerCurve,
			SamplerImage,
			SamplerText,
			SamplerUnsupported
		};

		[FormerlySerializedAs("Type")]
		public ESamplerType		m_Type;
		[FormerlySerializedAs("Name")]
		public string			m_Name;
		[FormerlySerializedAs("Description")]
		public string			m_Description;

		// For shape samplers:
		public ShapeTransform		m_ShapeDefaultTransform;
		public CurveDefaultValue	m_CurveDefaultValue;

		// Copy constructor:
		public SamplerDesc(SamplerDesc desc)
		{
			m_Type = desc.m_Type;
			m_Name = desc.m_Name;
			m_Description = desc.m_Description;
			m_ShapeDefaultTransform = desc.m_ShapeDefaultTransform;
		}

		// Build from native structure:
		public SamplerDesc(PKFxManagerImpl.SNativeSamplerDesc desc)
		{
			m_Type = (ESamplerType)desc.m_SamplerType;
			m_Name = Marshal.PtrToStringAnsi(desc.m_SamplerName);
			m_Description = Marshal.PtrToStringAnsi(desc.m_Description);
			m_ShapeDefaultTransform = new ShapeTransform(desc.m_ShapePosition, desc.m_ShapeRotation, Vector3.one);
			m_CurveDefaultValue = new CurveDefaultValue(desc.m_CurveDimension,
														desc.m_CurveKeyCount,
														desc.m_CurveTimes,
														desc.m_CurveFloatValues,
														desc.m_CurveFloatTangents);
		}

		public SamplerDesc(string name, ESamplerType type)
		{
			m_Type = type;
			m_Name = name;
		}

		public override string ToString()
		{
			// Just the type and the name are taken into account when mix and matching the samplers:
			return m_Type.ToString() + ";" + m_Name + ";";
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		public override bool Equals(object other)
		{
			SamplerDesc desc = other as SamplerDesc;

			if (desc == null)
				return false;
			return this == desc;
		}

		public static bool operator ==(SamplerDesc first, SamplerDesc second)
		{
			return first.m_Type == second.m_Type && first.m_Name == second.m_Name;
		}

		public static bool operator !=(SamplerDesc first, SamplerDesc second)
		{
			return !(first == second);
		}
	}

	public void			ComputeAttributesAndSamplersHash()
	{
		string			allAttributesString = "";
		string			allSamplersString = "";

		foreach (AttributeDesc attrDesc in m_AttributeDescs)
		{
			allAttributesString += attrDesc.ToString();
		}
		foreach (SamplerDesc smpDesc in m_SamplerDescs)
		{
			allSamplersString += smpDesc.ToString();
		}
		m_AttributeDescsHash = allAttributesString.GetHashCode();
		m_SamplerDescsHash = allSamplersString.GetHashCode();
	}

	public int			AttributeIdFromName(string attribName)
	{
		for (int i = 0; i < m_AttributeDescs.Count; ++i)
		{
			if (m_AttributeDescs[i].m_Name == attribName)
			{
				return i;
			}
		}
		Debug.LogError("Cannot find the attribute " + attribName, this);
		return -1;
	}

	public override void			Clean()
	{
		base.Clean();
		m_AttributeDescs = new List<AttributeDesc>();
		m_SamplerDescs = new List<SamplerDesc>();
		m_Dependencies = new List<DependencyDesc>();
		m_AttributeDescsHash = 0;
		m_SamplerDescsHash = 0;
		m_UsesMeshRenderer = false;
	}

	public List<DependencyDesc>		m_Dependencies = new List<DependencyDesc>();
	public List<AttributeDesc>		m_AttributeDescs = new List<AttributeDesc>();
	public int						m_AttributeDescsHash = 0;
	public List<SamplerDesc>		m_SamplerDescs = new List<SamplerDesc>();
	public int						m_SamplerDescsHash = 0;

	public bool						m_UsesMeshRenderer = false;
}
