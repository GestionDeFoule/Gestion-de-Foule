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

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public partial class PKFxFX : MonoBehaviour
{
#if		!PK_REMOVE_OBSOLETE_CODE
	[Serializable]
	[System.Obsolete("Old representation of the attributes")]
	public class Attribute
	{
		public PKFxFxAsset.AttributeDesc m_Descriptor;
		public float m_Value0;
		public float m_Value1;
		public float m_Value2;
		public float m_Value3;
	}

	private Vector4		ClampAttributeValue(Attribute attrib)
	{
		Vector4 clampedValue = new Vector4(attrib.m_Value0, attrib.m_Value1, attrib.m_Value2, attrib.m_Value3);

		clampedValue = attrib.m_Descriptor.ClampAttributeValue(clampedValue);
		return clampedValue;
	}
#endif

	#region Shape Sampler Descriptors

	public class SamplerDescShapeBox
	{
		public PKFxFxAsset.ShapeTransform	m_Transform;
		public Vector3						m_Dimensions;

		public SamplerDescShapeBox()
		{
			m_Transform = PKFxFxAsset.ShapeTransform.identity;
			m_Dimensions = Vector3.one;
		}

		public SamplerDescShapeBox(Vector3 dimension, PKFxFxAsset.ShapeTransform transform)
		{
			m_Transform = transform;
			m_Dimensions = dimension;
		}
	}

	public class SamplerDescShapeSphere
	{
		public PKFxFxAsset.ShapeTransform	m_Transform;
		public float						m_InnerRadius;
		public float						m_Radius;

		public SamplerDescShapeSphere()
		{
			m_Transform = PKFxFxAsset.ShapeTransform.identity;
			m_InnerRadius = 0.0f;
			m_Radius = 1.0f;
		}

		public SamplerDescShapeSphere(float radius, float innerRadius, PKFxFxAsset.ShapeTransform transform)
		{
			m_Transform = transform;
			m_InnerRadius = innerRadius;
			m_Radius = radius;
		}
	}

	public class SamplerDescShapeCylinder
	{
		public PKFxFxAsset.ShapeTransform	m_Transform;
		public float						m_InnerRadius;
		public float						m_Radius;
		public float						m_Height;

		public SamplerDescShapeCylinder()
		{
			m_Transform = PKFxFxAsset.ShapeTransform.identity;
			m_InnerRadius = 1.0f;
			m_Radius = 1.0f;
			m_Height = 1.0f;
		}

		public SamplerDescShapeCylinder(float radius, float innerRadius, float height, PKFxFxAsset.ShapeTransform transform)
		{
			m_Transform = transform;
			m_InnerRadius = innerRadius;
			m_Radius = radius;
			m_Height = height;
		}
	}

	public class SamplerDescShapeCapsule
	{
		public PKFxFxAsset.ShapeTransform	m_Transform;
		public float						m_InnerRadius;
		public float						m_Radius;
		public float						m_Height;

		public SamplerDescShapeCapsule()
		{
			m_Transform = PKFxFxAsset.ShapeTransform.identity;
			m_InnerRadius = 1.0f;
			m_Radius = 1.0f;
			m_Height = 1.0f;
		}

		public SamplerDescShapeCapsule(float radius, float innerRadius, float height, PKFxFxAsset.ShapeTransform transform)
		{
			m_Transform = transform;
			m_InnerRadius = innerRadius;
			m_Radius = radius;
			m_Height = height;
		}
	}

	public class SamplerDescShapeMesh
	{
		public PKFxFxAsset.ShapeTransform	m_Transform;
		public Vector3						m_Dimensions;
		public Mesh							m_Mesh;
		public int							m_SamplingInfo;

		public SamplerDescShapeMesh()
		{
			m_Transform = PKFxFxAsset.ShapeTransform.identity;
			m_Dimensions = Vector3.one;
			m_Mesh = null;
			m_SamplingInfo = 0;
		}

		public SamplerDescShapeMesh(Vector3 dimension, PKFxFxAsset.ShapeTransform transform, Mesh mesh, int samplingInfo)
		{
			m_Transform = transform;
			m_Dimensions = dimension;
			m_Mesh = mesh;
			m_SamplingInfo = samplingInfo;
		}
	}

	public class SamplerDescShapeMeshFilter
	{
		public PKFxFxAsset.ShapeTransform	m_Transform;
		public Vector3						m_Dimensions;
		public MeshFilter					m_MeshFilter;
		public int							m_SamplingInfo;

		public SamplerDescShapeMeshFilter()
		{
			m_Transform = PKFxFxAsset.ShapeTransform.identity;
			m_Dimensions = Vector3.one;
			m_MeshFilter = null;
			m_SamplingInfo = 0;
		}

		public SamplerDescShapeMeshFilter(Vector3 dimension, PKFxFxAsset.ShapeTransform transform, MeshFilter mesh, int samplingInfo)
		{
			m_Transform = transform;
			m_Dimensions = dimension;
			m_MeshFilter = mesh;
			m_SamplingInfo = samplingInfo;
		}
	}

	public class SamplerDescShapeSkinnedMesh
	{
		public PKFxFxAsset.ShapeTransform	m_Transform;
		public Vector3						m_Dimensions;
		public SkinnedMeshRenderer			m_SkinnedMesh;
		public int							m_SamplingInfo;

		public SamplerDescShapeSkinnedMesh()
		{
			m_Transform = PKFxFxAsset.ShapeTransform.identity;
			m_Dimensions = Vector3.one;
			m_SkinnedMesh = null;
			m_SamplingInfo = 0;
		}

		public SamplerDescShapeSkinnedMesh(Vector3 dimension, PKFxFxAsset.ShapeTransform transform, SkinnedMeshRenderer skinnedMesh, int samplingInfo)
		{
			m_Dimensions = dimension;
			m_Transform = transform;
			m_SkinnedMesh = skinnedMesh;
			m_SamplingInfo = samplingInfo;
		}
	}

	#endregion

	[Serializable]
	public class Sampler
	{
		public enum ETexcoordMode : int
		{
			Clamp = 0,
			Wrap
		}

#if		!PK_REMOVE_OBSOLETE_CODE
		public int				m_EditorShapeType = -1;
		public Vector3			m_ShapeCenter = Vector3.zero;
		public Vector3			m_EulerOrientation = Vector3.zero;

		public void				UpgradeSampler()
		{
			// Update the shape type:
			if (m_EditorShapeType == -1)
				m_ShapeType = EShapeType.ShapeUnsupported;
			else
			{
				m_ShapeType = (EShapeType)m_EditorShapeType;
			}
			// Update sampler transforms:
			m_ShapeTransform.m_Position = m_ShapeCenter;
			m_ShapeTransform.m_Rotation = Quaternion.Euler(m_EulerOrientation);
			m_ShapeTransform.m_Scale = Vector3.one;
		}
#endif

		public enum EShapeType : int
		{
			BoxShape = 0,
			SphereShape,
			CylinderShape,
			CapsuleShape,
			MeshShape,
			MeshFilterShape,
			SkinnedMeshShape,
			ShapeUnsupported
		}

		public enum EMeshSamplingInfo : int
		{
			Info_ChannelNormal = (1 << 1),
			Info_ChannelTangent = (1 << 2),
			Info_ChannelVelocity = (1 << 3),
			Info_ChannelUV = (1 << 4),
			Info_ChannelVertexColor = (1 << 5),
			Info_NeedBuildSamplingInfo = (1 << 6),
			Info_NeedBuildKdTree = (1 << 7),
		};

		public PKFxFxAsset.SamplerDesc	m_Descriptor;
		// For sampler shape:
		public EShapeType				m_ShapeType = EShapeType.ShapeUnsupported;

		// Shape transform:
		public PKFxFxAsset.ShapeTransform	m_ShapeTransform;
		public Vector3						m_Dimensions = Vector3.one;

		// For shape mesh:
		public Mesh						m_Mesh;
		public MeshFilter				m_MeshFilter;
		public SkinnedMeshRenderer		m_SkinnedMeshRenderer;
		public int						m_SamplingInfo;

		// Is this sampler in world space? (evolve script not in local space)
		public bool						m_WorldSpaceSampling = false;

		// For sampler image:
		public Texture2D				m_Texture;
		public ETexcoordMode			m_TextureTexcoordMode;

		// For sampler curve:
		public bool						m_CurveIsOverride = false; // We have the m_CurveIsOverride because the curve is always filled
		public AnimationCurve[]			m_CurvesArray;
		public float[]					m_CurvesTimeKeys;

		// For sampler text:
		public string					m_Text = "";

		// Was modified:
		public bool						m_WasModified = true;

		public void Copy(Sampler other)
		{
			m_ShapeTransform = other.m_ShapeTransform;
			m_ShapeType = other.m_ShapeType;
			m_MeshFilter = other.m_MeshFilter;
			m_Mesh = other.m_Mesh;
			m_SkinnedMeshRenderer = other.m_SkinnedMeshRenderer;
			m_SamplingInfo = other.m_SamplingInfo;
			m_Dimensions = other.m_Dimensions;
			m_Texture = other.m_Texture;
			m_TextureTexcoordMode = other.m_TextureTexcoordMode;
			m_CurvesArray = other.m_CurvesArray;
			m_CurvesTimeKeys = other.m_CurvesTimeKeys;
			m_Text = other.m_Text;
			m_WorldSpaceSampling = other.m_WorldSpaceSampling;
		}

		public Sampler(PKFxFxAsset.SamplerDesc dsc)
		{
			m_Descriptor = dsc;
			m_ShapeType = EShapeType.ShapeUnsupported;
			m_SamplingInfo = (int)EMeshSamplingInfo.Info_NeedBuildSamplingInfo;
			m_ShapeTransform = dsc.m_ShapeDefaultTransform;
			// Set the curve default values:
			m_CurveIsOverride = false;
			UpdateDefaultCurveValueIFN(dsc);
		}

		public Sampler(string name, SamplerDescShapeBox dsc)
		{
			m_Descriptor = new PKFxFxAsset.SamplerDesc(name, (int)PKFxFxAsset.SamplerDesc.ESamplerType.SamplerShape);
			m_Dimensions = dsc.m_Dimensions;
			m_ShapeTransform = dsc.m_Transform;
			m_ShapeType = (int)EShapeType.BoxShape;
		}

		public Sampler(string name, SamplerDescShapeSphere dsc)
		{
			m_Descriptor = new PKFxFxAsset.SamplerDesc(name, (int)PKFxFxAsset.SamplerDesc.ESamplerType.SamplerShape);
			m_Dimensions = new Vector3(dsc.m_Radius, dsc.m_InnerRadius);
			m_Dimensions.y = Mathf.Min(m_Dimensions.x, m_Dimensions.y);
			m_Dimensions.x = Mathf.Max(m_Dimensions.x, m_Dimensions.y);
			m_ShapeTransform = dsc.m_Transform;
			m_ShapeType = EShapeType.SphereShape;
		}

		public Sampler(string name, SamplerDescShapeCylinder dsc)
		{
			m_Descriptor = new PKFxFxAsset.SamplerDesc(name, (int)PKFxFxAsset.SamplerDesc.ESamplerType.SamplerShape);
			m_ShapeTransform = dsc.m_Transform;
			m_Dimensions = new Vector3(dsc.m_Radius, dsc.m_InnerRadius, dsc.m_Height);
			m_Dimensions.y = Mathf.Min(m_Dimensions.x, m_Dimensions.y);
			m_Dimensions.x = Mathf.Max(m_Dimensions.x, m_Dimensions.y);
			m_ShapeType = EShapeType.CylinderShape;
		}

		public Sampler(string name, SamplerDescShapeCapsule dsc)
		{
			m_Descriptor = new PKFxFxAsset.SamplerDesc(name, (int)PKFxFxAsset.SamplerDesc.ESamplerType.SamplerShape);
			m_ShapeTransform = dsc.m_Transform;
			m_Dimensions = new Vector3(dsc.m_Radius, dsc.m_InnerRadius, dsc.m_Height);
			m_Dimensions.y = Mathf.Min(m_Dimensions.x, m_Dimensions.y);
			m_Dimensions.x = Mathf.Max(m_Dimensions.x, m_Dimensions.y);
			m_ShapeType = EShapeType.CapsuleShape;
		}

		public Sampler(string name, SamplerDescShapeMesh dsc)
		{
			m_Descriptor = new PKFxFxAsset.SamplerDesc(name, (int)PKFxFxAsset.SamplerDesc.ESamplerType.SamplerShape);
			m_Dimensions = dsc.m_Dimensions;
			m_Mesh = dsc.m_Mesh;
			m_MeshFilter = null;
			m_SkinnedMeshRenderer = null;
			m_SamplingInfo = dsc.m_SamplingInfo;
			m_ShapeTransform = dsc.m_Transform;
			m_ShapeType = EShapeType.MeshShape;
		}

		public Sampler(string name, SamplerDescShapeMeshFilter dsc)
		{
			m_Descriptor = new PKFxFxAsset.SamplerDesc(name, (int)PKFxFxAsset.SamplerDesc.ESamplerType.SamplerShape);
			m_Dimensions = dsc.m_Dimensions;
			m_MeshFilter = dsc.m_MeshFilter;
			m_Mesh = m_MeshFilter.sharedMesh;
			m_SkinnedMeshRenderer = null;
			m_SamplingInfo = dsc.m_SamplingInfo;
			m_ShapeTransform = dsc.m_Transform;
			m_ShapeType = EShapeType.MeshFilterShape;
		}

		public Sampler(string name, SamplerDescShapeSkinnedMesh dsc)
		{
			m_Descriptor = new PKFxFxAsset.SamplerDesc(name, PKFxFxAsset.SamplerDesc.ESamplerType.SamplerShape);
			m_Dimensions = dsc.m_Dimensions;
			m_SkinnedMeshRenderer = dsc.m_SkinnedMesh;
			m_Mesh = dsc.m_SkinnedMesh.sharedMesh;
			m_MeshFilter = null;
			m_SamplingInfo = dsc.m_SamplingInfo;
			m_ShapeTransform = dsc.m_Transform;
			m_ShapeType = EShapeType.SkinnedMeshShape;
		}

		public Sampler(string name, AnimationCurve[] curvesArray)
		{
			m_Descriptor = new PKFxFxAsset.SamplerDesc(name, PKFxFxAsset.SamplerDesc.ESamplerType.SamplerCurve);
			m_CurvesArray = curvesArray;
			if (m_CurvesArray.Length != 0)
			{
				int iKey = 0;
				m_CurvesTimeKeys = new float[m_CurvesArray[0].keys.Length];
				foreach (var key in m_CurvesArray[0].keys)
				{
					m_CurvesTimeKeys[iKey++] = key.time;
				}
			}
			m_ShapeType = EShapeType.ShapeUnsupported;
			m_CurveIsOverride = true;
		}

		public Sampler(string name, Texture2D texture, ETexcoordMode texcoordMode)
		{
			m_Descriptor = new PKFxFxAsset.SamplerDesc(name, PKFxFxAsset.SamplerDesc.ESamplerType.SamplerImage);
			m_Texture = texture;
			m_TextureTexcoordMode = texcoordMode;
			m_ShapeType = EShapeType.ShapeUnsupported;
		}

		public Sampler(string name, string text)
		{
			m_Descriptor = new PKFxFxAsset.SamplerDesc(name, PKFxFxAsset.SamplerDesc.ESamplerType.SamplerText);
			m_Text = text;
			m_ShapeType = EShapeType.ShapeUnsupported;
		}

		public void		UpdateDefaultCurveValueIFN(PKFxFxAsset.SamplerDesc dsc)
		{
			if (!m_CurveIsOverride)
			{
				if (dsc.m_CurveDefaultValue == null || dsc.m_CurveDefaultValue.m_Curves == null)
				{
					Debug.LogWarning("Curve asset has no default value: you should re-import the effect asset that was not created correctly");
					m_CurveIsOverride = false;
					return;
				}
				int curveDimension = dsc.m_CurveDefaultValue.m_Curves.Length;
				if (curveDimension != 0)
				{
					m_CurvesArray = new AnimationCurve[curveDimension];
					for (int i = 0; i < m_CurvesArray.Length; ++i)
					{
						m_CurvesArray[i] = new AnimationCurve(dsc.m_CurveDefaultValue.m_Curves[i].keys);
					}
					m_CurvesTimeKeys = new float[m_CurvesArray[0].keys.Length];
					for (int i = 0; i < m_CurvesTimeKeys.Length; ++i)
					{
						m_CurvesTimeKeys[i] = m_CurvesArray[0].keys[i].time;
					}
					m_CurveIsOverride = false;
				}
			}
		}
	}

	private int		m_FXGUID = -1;
	private bool	m_AskedToStart = false;
	private bool	m_IsStopped = false;
	private bool	m_Awaked = false;

	public int FXGUID
	{
		get { return m_FXGUID; }
	}


#if !PK_REMOVE_OBSOLETE_CODE
	public List<Attribute>					m_FxAttributesList; // OLD version of the attribute list
#endif

	public List<PKFxFxAsset.AttributeDesc>	m_FxAttributesDesc = null;
	public int								m_FxAttributesDescHash = 0;
	public float[]							m_FxAttributesStartValues = null; // New version of the attributes
	public PKFxAttributesContainer			m_AttributesContainer = null;

	public List<Sampler>					m_FxSamplersList = new List<Sampler>();
	public int								m_FxSamplersDescHash = 0;
	public bool								m_PlayOnStart = true;
	public bool								m_TriggerAndForget = false;

	static public Dictionary<int, PKFxFX>	g_ListEffects = new Dictionary<int, PKFxFX>();
	static public List<PKFxFX>				g_PlayingEffectsToUpdate = new List<PKFxFX>();

	[SerializeField]
	private bool							m_IsPlaying = false;

	public delegate void OnFxStoppedDelegate(PKFxFX component);
	public OnFxStoppedDelegate				m_OnFxStopped = null;
	public UnityEngine.Object				m_BoundFx;
	// redundant with m_FxAsset.m_AssetName, kept to reload attributes and samplers as sometimes the m_FxAsset seems to be null during the import:
	public string							m_FxName = "";
	public PKFxFxAsset						m_FxAsset;

	//----------------------------------------------------------------------------

	private void SetPlayingState(bool isPlaying)
	{
		if (!m_TriggerAndForget && isPlaying != m_IsPlaying)
		{
			if (isPlaying)
			{
				if (!g_PlayingEffectsToUpdate.Contains(this))
				{
					g_PlayingEffectsToUpdate.Add(this);
				}
			}
			else
			{
				g_PlayingEffectsToUpdate.Remove(this);
			}
		}
		m_IsPlaying = isPlaying;
		m_IsStopped = !isPlaying;
	}

	//----------------------------------------------------------------------------

	public void InstantiateIFN()
	{
		if (m_FXGUID == -1 && m_FxAsset != null)
		{
			// Preload the FX:
			PreloadEffectFromAsset(m_FxAsset);
			// Instantiate the FX:
			m_FXGUID = CreateEffect(m_FxAsset, transform, false);

			if (m_FXGUID != -1)
			{
				// Add the effect to the list of all the effects components:
				if (g_ListEffects.ContainsKey(m_FXGUID))
				{
					g_ListEffects[m_FXGUID] = this;
				}
				else
				{
					g_ListEffects.Add(m_FXGUID, this);
				}

				// We are checking if the FX component is up to date (mix and match the attributes and samplers IFN):
				UpdateEffectAsset(m_FxAsset, false, true);

				m_AttributesContainer = new PKFxAttributesContainer(m_FxAsset, m_FXGUID);
				// Setup the default value for the attributes:
				if (m_FxAttributesStartValues != null && m_FxAttributesStartValues.Length != 0)
				{
					m_AttributesContainer.SetAllAttributes(m_FxAttributesStartValues);
					m_AttributesContainer.UpdateAttributes();
				}
				// Setup the default values for the samplers:
				UpdateSamplers(true);
			}
			else
			{
				Debug.LogWarning("Could not instantiate the effect", this);
			}
		}
	}

	public void DestroyIFN(bool removeFromFxList)
	{
		if (m_FXGUID == -1)
			return;
		SetPlayingState(false);				// Remove from the currently playing FXs list
		if (removeFromFxList)
			g_ListEffects.Remove(m_FXGUID);	// Remove the effect from the FXs list
		m_AttributesContainer = null;		// The attribute container is only valid while the effect is instantiated
		m_FXGUID = -1;						// Set the fxId to -1
	}

	//----------------------------------------------------------------------------

	void Awake()
	{
		PKFxManager.StartupPopcorn(true);
		m_Awaked = true;
	}

	//----------------------------------------------------------------------------

	void Start()
	{
		InstantiateIFN();
		if (m_PlayOnStart || m_AskedToStart)
			StartEffect();
		m_AskedToStart = false;
	}

	//----------------------------------------------------------------------------

	void OnDrawGizmos()
	{
		Gizmos.DrawIcon(transform.position, "FX.png", true);
	}

	//----------------------------------------------------------------------------

	public void OnDestroy()
	{
		DestroyIFN(true);
	}

	//----------------------------------------------------------------------------

	public void StartEffect()
	{
		StartEffectWithDt(0);
	}

	//----------------------------------------------------------------------------
	
	public void StartEffectWithDt(float dt)
	{
		if (!m_Awaked)
		{
			m_AskedToStart = true;
			return;
		}

		// instantiate the FX IFN:
		InstantiateIFN();

		if (m_FXGUID == -1)
		{
			Debug.LogWarning("Could not start effect as the effect was not instantiated", this);
		}
		else if (Alive())
		{
			Debug.LogWarning("Effect already started", this);
		}
		else
		{
			PKFxManager.StartEffect(m_FXGUID, dt);		// Start the FX in the native plugin
			SetPlayingState(true);						// Set the m_IsPlaying to true
		}
	}

	//----------------------------------------------------------------------------

	public void TerminateEffect()
	{
		TerminateEffectWithDt(0);
	}

	public void TerminateEffectWithDt(float dt)
	{
		// TODO: Do a proper terminate effect here that will let the effect play until the end and recycle the m_FXGUID:
		if (!m_IsPlaying || m_IsStopped || m_FXGUID == -1) // Cannot terminate an effect that was stopped
			return;
		PKFxManager.TerminateEffect(m_FXGUID, dt);
		// Destroy the FX to be able to restart it:
		DestroyIFN(true);
	}

	//----------------------------------------------------------------------------

	public void StopEffect()
	{
		StopEffectWithDt(0);
	}

	public void StopEffectWithDt(float dt)
	{
		if (!m_IsPlaying || m_FXGUID == -1)
			return;
		PKFxManager.StopEffect(m_FXGUID, dt);
		m_IsStopped = true;
	}

	//----------------------------------------------------------------------------

	public void OnFxStopPlaying(int guid)
	{
		Debug.Assert(guid == m_FXGUID);
		DestroyIFN(true);
	}

	//----------------------------------------------------------------------------

	public void KillEffect()
	{
		KillEffectWithDt(0);
	}

	public void KillEffectWithDt(float dt)
	{
		if (m_FXGUID == -1)
			return;
		PKFxManager.KillEffect(m_FXGUID, dt);
	}

	//----------------------------------------------------------------------------

	public bool Alive()
	{
		return m_IsPlaying;
	}

	//----------------------------------------------------------------------------

	public void UpdateEffectTransforms()
	{
		UpdateTransformEffect(m_FXGUID, transform);
	}

#region /!\ Attributes /!\

	//----------------------------------------------------------------------------

	// Will not actually change the asset, just retrieve the attributes and samplers from it
	public bool UpdateEffectAsset(PKFxFxAsset updatedAsset, bool resetAllAttributes, bool mismatchAttribsWarning = false)
	{
		bool	hasChanged = false;

		if (updatedAsset == null)
		{
			if (!string.IsNullOrEmpty(m_FxName))
			{
				hasChanged = true;
			}
			m_FxName = "";
			ClearAllAttributesAndSamplers();
		}
		else
		{
			if (m_FxName != updatedAsset.m_AssetName)
			{
				m_FxName = updatedAsset.m_AssetName;
				hasChanged = true;
			}
			if (updatedAsset.m_AttributeDescsHash != m_FxAttributesDescHash || resetAllAttributes)
			{
				if (mismatchAttribsWarning)
				{
					Debug.LogWarning("Performance warning: the attributes are not up to date on this PkFX component (" + m_FxAsset.m_AssetName + ")", this);
					Debug.LogWarning("Right click on the Project explorer > PopcornFX > Update PKFxFX References", this);
				}
				LoadAttributes(updatedAsset.m_AttributeDescs, resetAllAttributes);
				m_FxAttributesDescHash = updatedAsset.m_AttributeDescsHash;
				hasChanged = true;
			}
			if (updatedAsset.m_SamplerDescsHash != m_FxSamplersDescHash || resetAllAttributes)
			{
				if (mismatchAttribsWarning)
				{
					Debug.LogWarning("Performance warning: the samplers are not up to date on this PkFX component (" + m_FxAsset.m_AssetName + ")", this);
					Debug.LogWarning("Right click on the Project explorer > PopcornFX > Update PKFxFX References", this);
				}
				LoadSamplers(updatedAsset.m_SamplerDescs, resetAllAttributes);
				m_FxSamplersDescHash = updatedAsset.m_SamplerDescsHash;
				hasChanged = true;
			}
		}
		return hasChanged;
	}

	public void ClearAllAttributesAndSamplers()
	{
		if (m_FxSamplersList != null)
			m_FxSamplersList.Clear();
		m_FxAttributesStartValues = null;
		if (m_FxAttributesDesc != null)
			m_FxAttributesDesc.Clear();
		m_FxAttributesDescHash = -1;
		m_FxSamplersDescHash = -1;
#if !PK_REMOVE_OBSOLETE_CODE
		// DEPRECATED, need to clear it:
		if (m_FxAttributesList != null)
			m_FxAttributesList.Clear();
#endif
	}

	private Sampler GetSamplerFromDesc(PKFxFxAsset.SamplerDesc desc)
	{
		foreach (Sampler attr in m_FxSamplersList)
		{
			if (attr.m_Descriptor.m_Name == desc.m_Name && attr.m_Descriptor.m_Type == desc.m_Type)
				return attr;
		}
		return null;
	}

	private void LoadSamplers(List<PKFxFxAsset.SamplerDesc> samplersFromPkfxFile, bool resetAllToDefault)
	{
		List<Sampler> newList = new List<Sampler>();

		foreach (PKFxFxAsset.SamplerDesc desc in samplersFromPkfxFile)
		{
			Sampler sampler = resetAllToDefault ? null : GetSamplerFromDesc(desc);

			if (sampler == null)
			{
				newList.Add(new Sampler(desc));
			}
			else
			{
				sampler.UpdateDefaultCurveValueIFN(desc);
				newList.Add(sampler);
			}
		}
		m_FxSamplersList = newList;
	}

	private int GetAttributeIdFromDesc(PKFxFxAsset.AttributeDesc desc)
	{
		int		attribId = 0;

		foreach (PKFxFxAsset.AttributeDesc curDesc in m_FxAttributesDesc)
		{
			if (curDesc.m_Name == desc.m_Name && curDesc.m_Type == desc.m_Type)
				return attribId;
			++attribId;
		}
		return -1;
	}

	private void LoadAttributes(List<PKFxFxAsset.AttributeDesc> attributesFromPkfxFile, bool resetAllToDefault)
	{
		float[]		attributesValues = new float[attributesFromPkfxFile.Count * 4]; // 4 values per attribute
		int			currentAttributeId = 0;

		foreach (PKFxFxAsset.AttributeDesc desc in attributesFromPkfxFile)
		{
			bool attribIsSet = false;

			if (!resetAllToDefault && m_FxAttributesStartValues != null && m_FxAttributesDesc != null)
			{
				int attribId = GetAttributeIdFromDesc(desc);

				if (attribId != -1)
				{
					attributesValues[currentAttributeId * 4 + 0] = m_FxAttributesStartValues[attribId * 4 + 0];
					attributesValues[currentAttributeId * 4 + 1] = m_FxAttributesStartValues[attribId * 4 + 1];
					attributesValues[currentAttributeId * 4 + 2] = m_FxAttributesStartValues[attribId * 4 + 2];
					attributesValues[currentAttributeId * 4 + 3] = m_FxAttributesStartValues[attribId * 4 + 3];
					attribIsSet = true;
				}
			}
			if (!attribIsSet)
			{
				Vector4 clampedAttribute = desc.GetDefaultAttributeValueClamped();

				attributesValues[currentAttributeId * 4 + 0] = clampedAttribute.x;
				attributesValues[currentAttributeId * 4 + 1] = clampedAttribute.y;
				attributesValues[currentAttributeId * 4 + 2] = clampedAttribute.z;
				attributesValues[currentAttributeId * 4 + 3] = clampedAttribute.w;
			}
			++currentAttributeId;
		}
		m_FxAttributesDesc = attributesFromPkfxFile;
		m_FxAttributesStartValues = attributesValues;
	}

	//----------------------------------------------------------------------------

	public void UpdateSamplers(bool forceUpdate)
	{
		if (m_AttributesContainer != null)
		{
			m_AttributesContainer.UpdateAttributes();
		}
		for (int i = 0; i < m_FxSamplersList.Count; i++)
		{
			int						samplerId = i;
			Sampler					curSampler = m_FxSamplersList[i];

			// for the sampler shape:
			if (curSampler.m_Descriptor.m_Type == PKFxFxAsset.SamplerDesc.ESamplerType.SamplerShape)
			{
				// Sampler shape mesh:
				if (curSampler.m_ShapeType == Sampler.EShapeType.MeshShape ||
					curSampler.m_ShapeType == Sampler.EShapeType.MeshFilterShape ||
					curSampler.m_ShapeType == Sampler.EShapeType.SkinnedMeshShape)
				{
					bool isSkinned = curSampler.m_ShapeType == Sampler.EShapeType.SkinnedMeshShape;
					// Upload the mesh data to the PopcornFX plugin IFN:
					if (forceUpdate || curSampler.m_WasModified)
					{
						// Setup the mesh from skinned mesh renderer or mesh filter:
						if (curSampler.m_SkinnedMeshRenderer != null)
						{
							curSampler.m_Mesh = curSampler.m_SkinnedMeshRenderer.sharedMesh;
						}
						if (curSampler.m_MeshFilter != null)
						{
							curSampler.m_Mesh = curSampler.m_MeshFilter.sharedMesh;
						}
						if (curSampler.m_Mesh != null) // Setup the mesh here...
						{
							// Feed the mesh to the PopcornFX runtime:
							IntPtr meshToFillPtr = GetMeshToFill(curSampler.m_Mesh, curSampler.m_SamplingInfo, isSkinned);
							UpdateMeshToFill(meshToFillPtr, curSampler.m_Mesh);
							PKFxManager.SetMeshSampler(m_FXGUID, samplerId, meshToFillPtr, curSampler.m_Dimensions);
						}
						else
						{
							PKFxManager.SetDefaultSampler(m_FXGUID, samplerId);
						}
					}
					// Update the skinning:
					if (isSkinned && curSampler.m_SkinnedMeshRenderer != null)
					{
						UpdateMeshBones(m_FXGUID, samplerId, curSampler.m_SkinnedMeshRenderer);
						// Update skinning should be async:
						PKFxManager.BeginUpdateSamplerSkinning(m_FXGUID, samplerId, Time.deltaTime);
						PKFxManager.EndUpdateSamplerSkinning(m_FXGUID, samplerId);
					}
				}
				else
				{
					if (forceUpdate || curSampler.m_WasModified)
					{
						if (curSampler.m_ShapeType != Sampler.EShapeType.ShapeUnsupported)
						{
							PKFxManager.SetSamplerShape(m_FXGUID, samplerId, curSampler.m_ShapeType, curSampler.m_Dimensions);
						}
						else
						{
							PKFxManager.SetDefaultSampler(m_FXGUID, samplerId);
						}
					}
				}
				Matrix4x4 shapeTransform = curSampler.m_ShapeTransform.transform;

				PKFxManager.SetSamplerShapeTransform(m_FXGUID, samplerId, shapeTransform);
			}
			else if (curSampler.m_Descriptor.m_Type == PKFxFxAsset.SamplerDesc.ESamplerType.SamplerCurve)
			{
				if (forceUpdate || curSampler.m_WasModified)
				{
					if (curSampler.m_CurveIsOverride)
					{
						int keyPointsCount = curSampler.m_CurvesTimeKeys == null ? 0 : curSampler.m_CurvesTimeKeys.Length;
						int curvesCount = curSampler.m_CurvesArray == null ? 0 : curSampler.m_CurvesArray.Length;
						IntPtr curveToFill = PKFxManager.GetCurveSamplerToFill(keyPointsCount, curvesCount);
						UpdateCurveToFill(curveToFill, curSampler.m_CurvesArray, curSampler.m_CurvesTimeKeys);
						PKFxManager.SetCurveSampler(m_FXGUID, samplerId, curveToFill);
					}
					else
					{
						PKFxManager.SetDefaultSampler(m_FXGUID, samplerId);
					}
				}
			}
			else if (curSampler.m_Descriptor.m_Type == PKFxFxAsset.SamplerDesc.ESamplerType.SamplerImage)
			{
				if (forceUpdate || curSampler.m_WasModified)
				{
					if (curSampler.m_Texture != null) 
					{
						int textureByteSize = curSampler.m_Texture.GetRawTextureData().Length;
						IntPtr	textureToFill = GetAndUpdateTextureToFill(curSampler.m_Texture, curSampler.m_TextureTexcoordMode);
						PKFxManager.SetTextureSampler(m_FXGUID, samplerId, textureToFill);
					}
					else
					{
						PKFxManager.SetDefaultSampler(m_FXGUID, samplerId);
					}
				}
			}
			else if (curSampler.m_Descriptor.m_Type == PKFxFxAsset.SamplerDesc.ESamplerType.SamplerText)
			{
				if (forceUpdate || curSampler.m_WasModified)
				{
					if (!string.IsNullOrEmpty(curSampler.m_Text))
					{
						PKFxManager.SetTextSampler(m_FXGUID, samplerId, curSampler.m_Text);
					}
					else
					{
						PKFxManager.SetDefaultSampler(m_FXGUID, samplerId);
					}
				}
			}
			// The sampler is now up to date:
			curSampler.m_WasModified = false;
		}
	}

	//----------------------------------------------------------------------------

	#endregion

	public void			UpdateAssetPathIFN()
	{
		if (!string.IsNullOrEmpty(m_FxName))
		{
			int split = m_FxName.IndexOf("PackFx");
			// We remove the leading "PackFx" folder from the asset path (the user can now name the folder as they want):
			m_FxName = (split == 0) ? m_FxName.Substring(7) : m_FxName;
			if (Path.GetExtension(m_FxName) == ".pkfx")
			{
				m_FxName += ".asset";
			}
		}
	}

	public void			UpdatePkFXComponent(PKFxFxAsset updatedAsset, bool updateAsset)
	{
#if !PK_REMOVE_OBSOLETE_CODE
		// Update the attribute default values:
		if (m_FxAttributesList != null && m_FxAttributesList.Count > 0)
		{
			int			currentAttributeId = 0;

			if (m_FxAttributesStartValues != null || m_FxAttributesStartValues.Length != 0 ||
				m_FxAttributesDesc != null || m_FxAttributesDesc.Count != 0)
			{
				Debug.LogWarning("Effect should not have old and new attribute values", this);
			}
			// We setup the effect as if it was created with the new plugin:
			m_FxAttributesStartValues = new float[m_FxAttributesList.Count * 4];
			m_FxAttributesDesc = new List<PKFxFxAsset.AttributeDesc>();

			foreach (Attribute attrib in m_FxAttributesList)
			{
				m_FxAttributesDesc.Add(attrib.m_Descriptor);

				Vector4 clampedValue = ClampAttributeValue(attrib);

				// We need to re-clamp the attribute (the previous version of the plugin did not store the clamped attribute values)
				m_FxAttributesStartValues[currentAttributeId * 4 + 0] = clampedValue.x;
				m_FxAttributesStartValues[currentAttributeId * 4 + 1] = clampedValue.y;
				m_FxAttributesStartValues[currentAttributeId * 4 + 2] = clampedValue.z;
				m_FxAttributesStartValues[currentAttributeId * 4 + 3] = clampedValue.w;
				++currentAttributeId;
			}
			m_FxAttributesList.Clear();
		}
		// Update the samplers default values:
		foreach (Sampler sampler in m_FxSamplersList)
		{
			sampler.UpgradeSampler();
		}
#endif
		// Just in case we reset those to false: they should never be true when the editor is not running
		m_IsPlaying = false;
		m_IsStopped = false;
		if (updateAsset)
		{
			// Update effect asset:
			UpdateEffectAsset(updatedAsset, false);
		}
	}

	public void SetSamplerSafe(Sampler sampler)
	{
		int samplerId = m_FxSamplersList.FindIndex(x => x.m_Descriptor == sampler.m_Descriptor);

		if (samplerId != -1)
		{
			sampler.m_WasModified = true;
			m_FxSamplersList[samplerId] = sampler;
		}
	}

	// Attributes setters:
	#region SetAttributeSafe from name

	public void SetAttributeSafe(string name, float valueX)
	{
		int attribId = m_FxAsset.AttributeIdFromName(name);

		if (attribId != -1)
		{
			SetAttributeSafe(attribId, valueX);
		}
	}

	public void SetAttributeSafe(string name, float valueX, float valueY)
	{
		int attribId = m_FxAsset.AttributeIdFromName(name);

		if (attribId != -1)
		{
			SetAttributeSafe(attribId, valueX, valueY);
		}
	}

	public void SetAttributeSafe(string name, float valueX, float valueY, float valueZ)
	{
		int attribId = m_FxAsset.AttributeIdFromName(name);

		if (attribId != -1)
		{
			SetAttributeSafe(attribId, valueX, valueY, valueZ);
		}
	}

	public void SetAttributeSafe(string name, float valueX, float valueY, float valueZ, float valueW)
	{
		int attribId = m_FxAsset.AttributeIdFromName(name);

		if (attribId != -1)
		{
			SetAttributeSafe(attribId, valueX, valueY, valueZ, valueW);
		}
	}

	public void SetAttributeSafe(string name, int valueX)
	{
		int attribId = m_FxAsset.AttributeIdFromName(name);

		if (attribId != -1)
		{
			SetAttributeSafe(attribId, valueX);
		}
	}

	public void SetAttributeSafe(string name, int valueX, int valueY)
	{
		int attribId = m_FxAsset.AttributeIdFromName(name);

		if (attribId != -1)
		{
			SetAttributeSafe(attribId, valueX, valueY);
		}
	}

	public void SetAttributeSafe(string name, int valueX, int valueY, int valueZ)
	{
		int attribId = m_FxAsset.AttributeIdFromName(name);

		if (attribId != -1)
		{
			SetAttributeSafe(attribId, valueX, valueY, valueZ);
		}
	}

	public void SetAttributeSafe(string name, int valueX, int valueY, int valueZ, int valueW)
	{
		int attribId = m_FxAsset.AttributeIdFromName(name);

		if (attribId != -1)
		{
			SetAttributeSafe(attribId, valueX, valueY, valueZ, valueW);
		}
	}

	#endregion

	#region SetAttributeSafe from ID

	public void SetAttributeSafe(int attribId, float valueX)
	{
		PKFxFxAsset.AttributeDesc attributeDesc = m_FxAsset.m_AttributeDescs[attribId];

		if (attributeDesc.m_Type != PKFxManagerImpl.EBaseType.Float)
		{
			Debug.LogError("Type mismatch for the attribute", this);
			return;
		}
		if (attributeDesc.HasMin())
		{
			valueX = Mathf.Max(attributeDesc.m_MinValue0, valueX);
		}
		if (attributeDesc.HasMax())
		{
			valueX = Mathf.Min(attributeDesc.m_MaxValue0, valueX);
		}
		SetAttributeUnsafe(attribId, valueX);
	}

	public void SetAttributeSafe(int attribId, float valueX, float valueY)
	{
		PKFxFxAsset.AttributeDesc attributeDesc = m_FxAsset.m_AttributeDescs[attribId];

		if (attributeDesc.m_Type != PKFxManagerImpl.EBaseType.Float2)
		{
			Debug.LogError("Type mismatch for the attribute", this);
			return;
		}
		if (attributeDesc.HasMin())
		{
			valueX = Mathf.Max(attributeDesc.m_MinValue0, valueX);
			valueY = Mathf.Max(attributeDesc.m_MinValue1, valueY);
		}
		if (attributeDesc.HasMax())
		{
			valueX = Mathf.Min(attributeDesc.m_MaxValue0, valueX);
			valueY = Mathf.Min(attributeDesc.m_MaxValue1, valueY);
		}
		SetAttributeUnsafe(attribId, valueX, valueY);
	}

	public void SetAttributeSafe(int attribId, float valueX, float valueY, float valueZ)
	{
		PKFxFxAsset.AttributeDesc attributeDesc = m_FxAsset.m_AttributeDescs[attribId];

		if (attributeDesc.m_Type != PKFxManagerImpl.EBaseType.Float3)
		{
			Debug.LogError("Type mismatch for the attribute", this);
			return;
		}
		if (attributeDesc.HasMin())
		{
			valueX = Mathf.Max(attributeDesc.m_MinValue0, valueX);
			valueY = Mathf.Max(attributeDesc.m_MinValue1, valueY);
			valueZ = Mathf.Max(attributeDesc.m_MinValue2, valueZ);
		}
		if (attributeDesc.HasMax())
		{
			valueX = Mathf.Min(attributeDesc.m_MaxValue0, valueX);
			valueY = Mathf.Min(attributeDesc.m_MaxValue1, valueY);
			valueZ = Mathf.Min(attributeDesc.m_MaxValue2, valueZ);
		}
		SetAttributeUnsafe(attribId, valueX, valueY, valueZ);
	}

	public void SetAttributeSafe(int attribId, float valueX, float valueY, float valueZ, float valueW)
	{
		PKFxFxAsset.AttributeDesc attributeDesc = m_FxAsset.m_AttributeDescs[attribId];

		if (attributeDesc.m_Type != PKFxManagerImpl.EBaseType.Float4)
		{
			Debug.LogError("Type mismatch for the attribute", this);
			return;
		}
		if (attributeDesc.HasMin())
		{
			valueX = Mathf.Max(attributeDesc.m_MinValue0, valueX);
			valueY = Mathf.Max(attributeDesc.m_MinValue1, valueY);
			valueZ = Mathf.Max(attributeDesc.m_MinValue2, valueZ);
			valueW = Mathf.Max(attributeDesc.m_MinValue3, valueW);
		}
		if (attributeDesc.HasMax())
		{
			valueX = Mathf.Min(attributeDesc.m_MaxValue0, valueX);
			valueY = Mathf.Min(attributeDesc.m_MaxValue1, valueY);
			valueZ = Mathf.Min(attributeDesc.m_MaxValue2, valueZ);
			valueW = Mathf.Min(attributeDesc.m_MaxValue3, valueW);
		}
		SetAttributeUnsafe(attribId, valueX, valueY, valueZ, valueW);
	}

	public void SetAttributeSafe(int attribId, int valueX)
	{
		PKFxFxAsset.AttributeDesc attributeDesc = m_FxAsset.m_AttributeDescs[attribId];

		if (attributeDesc.m_Type != PKFxManagerImpl.EBaseType.Int)
		{
			Debug.LogError("Type mismatch for the attribute", this);
			return;
		}
		if (attributeDesc.HasMin())
		{
			valueX = Mathf.Max(PKFxManagerImpl.Float2Int(attributeDesc.m_MinValue0), valueX);
		}
		if (attributeDesc.HasMax())
		{
			valueX = Mathf.Min(PKFxManagerImpl.Float2Int(attributeDesc.m_MaxValue0), valueX);
		}
		SetAttributeUnsafe(attribId, valueX);
	}

	public void SetAttributeSafe(int attribId, int valueX, int valueY)
	{
		PKFxFxAsset.AttributeDesc attributeDesc = m_FxAsset.m_AttributeDescs[attribId];

		if (attributeDesc.m_Type != PKFxManagerImpl.EBaseType.Int2)
		{
			Debug.LogError("Type mismatch for the attribute", this);
			return;
		}
		if (attributeDesc.HasMin())
		{
			valueX = Mathf.Max(PKFxManagerImpl.Float2Int(attributeDesc.m_MinValue0), valueX);
			valueY = Mathf.Max(PKFxManagerImpl.Float2Int(attributeDesc.m_MinValue1), valueY);
		}
		if (attributeDesc.HasMax())
		{
			valueX = Mathf.Min(PKFxManagerImpl.Float2Int(attributeDesc.m_MaxValue0), valueX);
			valueY = Mathf.Min(PKFxManagerImpl.Float2Int(attributeDesc.m_MaxValue1), valueY);
		}
		SetAttributeUnsafe(attribId, valueX, valueY);
	}

	public void SetAttributeSafe(int attribId, int valueX, int valueY, int valueZ)
	{
		PKFxFxAsset.AttributeDesc attributeDesc = m_FxAsset.m_AttributeDescs[attribId];

		if (attributeDesc.m_Type != PKFxManagerImpl.EBaseType.Int3)
		{
			Debug.LogError("Type mismatch for the attribute", this);
			return;
		}
		if (attributeDesc.HasMin())
		{
			valueX = Mathf.Max(PKFxManagerImpl.Float2Int(attributeDesc.m_MinValue0), valueX);
			valueY = Mathf.Max(PKFxManagerImpl.Float2Int(attributeDesc.m_MinValue1), valueY);
			valueZ = Mathf.Max(PKFxManagerImpl.Float2Int(attributeDesc.m_MinValue2), valueZ);
		}
		if (attributeDesc.HasMax())
		{
			valueX = Mathf.Min(PKFxManagerImpl.Float2Int(attributeDesc.m_MaxValue0), valueX);
			valueY = Mathf.Min(PKFxManagerImpl.Float2Int(attributeDesc.m_MaxValue1), valueY);
			valueZ = Mathf.Min(PKFxManagerImpl.Float2Int(attributeDesc.m_MaxValue2), valueZ);
		}
		SetAttributeUnsafe(attribId, valueX, valueY, valueZ);
	}

	public void SetAttributeSafe(int attribId, int valueX, int valueY, int valueZ, int valueW)
	{
		PKFxFxAsset.AttributeDesc attributeDesc = m_FxAsset.m_AttributeDescs[attribId];

		if (attributeDesc.m_Type != PKFxManagerImpl.EBaseType.Int4)
		{
			Debug.LogError("Type mismatch for the attribute", this);
			return;
		}
		if (attributeDesc.HasMin())
		{
			valueX = Mathf.Max(PKFxManagerImpl.Float2Int(attributeDesc.m_MinValue0), valueX);
			valueY = Mathf.Max(PKFxManagerImpl.Float2Int(attributeDesc.m_MinValue1), valueY);
			valueZ = Mathf.Max(PKFxManagerImpl.Float2Int(attributeDesc.m_MinValue2), valueZ);
			valueW = Mathf.Max(PKFxManagerImpl.Float2Int(attributeDesc.m_MinValue3), valueW);
		}
		if (attributeDesc.HasMax())
		{
			valueX = Mathf.Min(PKFxManagerImpl.Float2Int(attributeDesc.m_MaxValue0), valueX);
			valueY = Mathf.Min(PKFxManagerImpl.Float2Int(attributeDesc.m_MaxValue1), valueY);
			valueZ = Mathf.Min(PKFxManagerImpl.Float2Int(attributeDesc.m_MaxValue2), valueZ);
			valueW = Mathf.Min(PKFxManagerImpl.Float2Int(attributeDesc.m_MaxValue3), valueW);
		}
		SetAttributeUnsafe(attribId, valueX, valueY, valueZ, valueW);
	}

	#endregion

	#region SetAttributeUnsafe

	public void SetAttributeUnsafe(int attribId, float valueX)
	{
		m_FxAttributesStartValues[attribId * 4 + 0] = valueX;
		if (m_AttributesContainer != null)
		{
			m_AttributesContainer.SetAttributeUnsafe(attribId, valueX);
		}
	}

	public void SetAttributeUnsafe(int attribId, float valueX, float valueY)
	{
		m_FxAttributesStartValues[attribId * 4 + 0] = valueX;
		m_FxAttributesStartValues[attribId * 4 + 1] = valueY;
		if (m_AttributesContainer != null)
		{
			m_AttributesContainer.SetAttributeUnsafe(attribId, valueX, valueY);
		}
	}

	public void SetAttributeUnsafe(int attribId, float valueX, float valueY, float valueZ)
	{
		m_FxAttributesStartValues[attribId * 4 + 0] = valueX;
		m_FxAttributesStartValues[attribId * 4 + 1] = valueY;
		m_FxAttributesStartValues[attribId * 4 + 2] = valueZ;
		if (m_AttributesContainer != null)
		{
			m_AttributesContainer.SetAttributeUnsafe(attribId, valueX, valueY, valueZ);
		}
	}

	public void SetAttributeUnsafe(int attribId, float valueX, float valueY, float valueZ, float valueW)
	{
		m_FxAttributesStartValues[attribId * 4 + 0] = valueX;
		m_FxAttributesStartValues[attribId * 4 + 1] = valueY;
		m_FxAttributesStartValues[attribId * 4 + 2] = valueZ;
		m_FxAttributesStartValues[attribId * 4 + 3] = valueW;
		if (m_AttributesContainer != null)
		{
			m_AttributesContainer.SetAttributeUnsafe(attribId, valueX, valueY, valueZ, valueW);
		}
	}

	public void SetAttributeUnsafe(int attribId, Vector4 value)
	{
		m_FxAttributesStartValues[attribId * 4 + 0] = value.x;
		m_FxAttributesStartValues[attribId * 4 + 1] = value.y;
		m_FxAttributesStartValues[attribId * 4 + 2] = value.z;
		m_FxAttributesStartValues[attribId * 4 + 3] = value.w;
		if (m_AttributesContainer != null)
		{
			m_AttributesContainer.SetAttributeUnsafe(attribId, value);
		}
	}

	public void SetAttributeUnsafe(int attribId, int valueX)
	{
		m_FxAttributesStartValues[attribId * 4 + 0] = PKFxManagerImpl.Int2Float(valueX);
		if (m_AttributesContainer != null)
		{
			m_AttributesContainer.SetAttributeUnsafe(attribId, valueX);
		}
	}

	public void SetAttributeUnsafe(int attribId, int valueX, int valueY)
	{
		m_FxAttributesStartValues[attribId * 4 + 0] = PKFxManagerImpl.Int2Float(valueX);
		m_FxAttributesStartValues[attribId * 4 + 1] = PKFxManagerImpl.Int2Float(valueY);
		if (m_AttributesContainer != null)
		{
			m_AttributesContainer.SetAttributeUnsafe(attribId, valueX, valueY);
		}
	}

	public void SetAttributeUnsafe(int attribId, int valueX, int valueY, int valueZ)
	{
		m_FxAttributesStartValues[attribId * 4 + 0] = PKFxManagerImpl.Int2Float(valueX);
		m_FxAttributesStartValues[attribId * 4 + 1] = PKFxManagerImpl.Int2Float(valueY);
		m_FxAttributesStartValues[attribId * 4 + 2] = PKFxManagerImpl.Int2Float(valueZ);
		if (m_AttributesContainer != null)
		{
			m_AttributesContainer.SetAttributeUnsafe(attribId, valueX, valueY, valueZ);
		}
	}

	public void SetAttributeUnsafe(int attribId, int valueX, int valueY, int valueZ, int valueW)
	{
		m_FxAttributesStartValues[attribId * 4 + 0] = PKFxManagerImpl.Int2Float(valueX);
		m_FxAttributesStartValues[attribId * 4 + 1] = PKFxManagerImpl.Int2Float(valueY);
		m_FxAttributesStartValues[attribId * 4 + 2] = PKFxManagerImpl.Int2Float(valueZ);
		m_FxAttributesStartValues[attribId * 4 + 3] = PKFxManagerImpl.Int2Float(valueW);
		if (m_AttributesContainer != null)
		{
			m_AttributesContainer.SetAttributeUnsafe(attribId, valueX, valueY, valueZ, valueW);
		}
	}

	#endregion
}
