using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PKFxMaterialFactoryDefault : PKFxMaterialFactory
{
	public Shader	m_ParticleShader;
	public Shader	m_OpaqueMeshShader;
	public Shader 	m_TransparentMeshShader;
	public int		m_RenderQueue = 3500;
	public bool		m_UseSortingLayers = false;

#if UNITY_EDITOR
	[MenuItem("Assets/Create/PopcornFX/Default Material Factory")]
	static void CreateMaterialFactoryDefault()
	{
		string folderPath = GetSelectedPathOrFallback();
		string path = Path.Combine(folderPath, "DefaultMaterialFactory.asset");

		PKFxMaterialFactoryDefault materialFactoryDefault = ScriptableObject.CreateInstance<PKFxMaterialFactoryDefault>();

		materialFactoryDefault.SetupShaders();

		AssetDatabase.CreateAsset(materialFactoryDefault, path);
		AssetDatabase.SaveAssets();

		EditorUtility.FocusProjectWindow();

		Selection.activeObject = materialFactoryDefault;
	}
	
	public override Shader[] GetMeshShaders()
	{
		return new Shader[2] { m_OpaqueMeshShader, m_TransparentMeshShader };
	}
#endif

	public void		SetupShaders()
	{
		// Find the shaders:
		m_ParticleShader = Shader.Find("PopcornFX/PKFxParticleShader");
		m_OpaqueMeshShader = Shader.Find("PopcornFX/PKFxMeshInstanced");
		m_TransparentMeshShader = Shader.Find("PopcornFX/PKFxMeshInstancedTransparent");
	}

	public override void		SetupRenderer(PKFxManagerImpl.SBatchDesc batchDesc, GameObject gameObject, MeshRenderer meshRenderer)
	{
		if (batchDesc.HasMaterialFlag(PKFxManagerImpl.EMaterialFlags.Has_Distortion))
		{
			gameObject.layer = PKFxManagerImpl.m_DistortionLayer;
		}
		if (m_UseSortingLayers)
		{
			if (IsUI(batchDesc.m_UserData))
				meshRenderer.sortingLayerName = "PopcornFXUI";
			else
				meshRenderer.sortingLayerName = "PopcornFX";
		}
	}

	public override void		SetupMeshRenderer(PKFxManagerImpl.SBatchDesc batchDesc, GameObject gameObject, PKFxMeshInstancesRenderer meshRenderer)
	{
		if (batchDesc.HasMaterialFlag(PKFxManagerImpl.EMaterialFlags.Has_Distortion))
		{
			gameObject.layer = PKFxManagerImpl.m_DistortionLayer;
		}
		
		PKFxFxAsset.DependencyDesc DepDesc = PKFxManager.GetBuiltAsset().m_Dependencies.Find(x => batchDesc.m_MeshAsset.Contains(x.m_Path));
		if (DepDesc != null)
		{
			GameObject		meshGO= DepDesc.m_Object as GameObject;
			List<Mesh>		meshes = new List<Mesh>();
			List<Matrix4x4>	trans = new List<Matrix4x4>();

			MeshFilter meshFilter = meshGO.GetComponent<MeshFilter>();
			if (meshFilter != null)
			{
				meshes.Add(meshFilter.sharedMesh);
				trans.Add(meshGO.transform.localToWorldMatrix);
			}
			if (meshes.Count == 0)
			{
				MeshFilter[] meshFilters = meshGO.GetComponentsInChildren<MeshFilter>();
				if (batchDesc.m_SubMeshID == -1)
				{
					for (int i = 0; i < meshFilters.Length; ++i)
					{
						meshes.Add(meshFilters[i].sharedMesh);
						trans.Add(meshGO.transform.localToWorldMatrix * meshFilters[i].transform.localToWorldMatrix);
					}
				}
				else if (meshFilters.Length > batchDesc.m_SubMeshID)
				{
					meshes.Add(meshFilters[batchDesc.m_SubMeshID].sharedMesh);
					trans.Add(meshGO.transform.localToWorldMatrix * meshFilters[batchDesc.m_SubMeshID].transform.localToWorldMatrix);
				}
			}

			meshRenderer.m_MeshesImportTransform = trans.ToArray();
			meshRenderer.m_Meshes = meshes.ToArray();
		}
		meshRenderer.m_CastShadow = batchDesc.HasMaterialFlag(PKFxManagerImpl.EMaterialFlags.Has_CastShadow);
	}

	public override Material	ResolveParticleMaterial(PKFxManagerImpl.SBatchDesc batchDesc)
	{
		Material customMat = TryFindAndInstantiateCustomMaterial(batchDesc);

		if (customMat != null)
			return customMat;

		Shader shader = ResolveShader(batchDesc);
		Debug.Assert(shader != null);
		Material				material = new Material(shader);

		// Set the diffuse texture:
		Texture diffuseTexture = GetTextureDiffuse(batchDesc);
		material.mainTexture = diffuseTexture;

		// Set the material uniforms:
		material.SetInt("_UniformFlags", batchDesc.m_UniformFlags);

		// We can stop here if the material is for a mesh particle:
		if (batchDesc.m_Type == PKFxManagerImpl.ERendererType.Mesh)
		{
			return material;
		}

		// Set the alpha remap texture
		if (batchDesc.m_AlphaRemap != null)
		{
			string path = Path.GetDirectoryName(batchDesc.m_AlphaRemap) + "/" + Path.GetFileNameWithoutExtension(batchDesc.m_AlphaRemap) + "_linear" + Path.GetExtension(batchDesc.m_AlphaRemap);
			PKFxFxAsset.DependencyDesc DepDesc = PKFxManager.GetBuiltAsset().m_Dependencies.Find(x => path.Contains(x.m_Path));
			if (DepDesc != null)
			{
				Texture alphaRemapTexture = DepDesc.m_Object as Texture;
				alphaRemapTexture.wrapMode = TextureWrapMode.Clamp;
				Debug.Assert(alphaRemapTexture != null);
				material.SetTexture("_AlphaMap", alphaRemapTexture);
			}
		}

		// Set the blend mode:
		int srcMode = 0;
		int dstMode = 0;

		// Additive and distortion
		if (batchDesc.HasUniformFlag(PKFxManagerImpl.EUniformFlags.Is_Additive) ||
			batchDesc.HasUniformFlag(PKFxManagerImpl.EUniformFlags.Is_AdditiveNoAlpha) ||
			batchDesc.HasMaterialFlag(PKFxManagerImpl.EMaterialFlags.Has_Distortion))
		{
			srcMode = (int)UnityEngine.Rendering.BlendMode.One;
			dstMode = (int)UnityEngine.Rendering.BlendMode.One;
		}
		else if (batchDesc.HasUniformFlag(PKFxManagerImpl.EUniformFlags.Is_AdditiveAlphaBlend))
		{
			srcMode = (int)UnityEngine.Rendering.BlendMode.One;
			dstMode = (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
		}
		else // Alpha blended
		{
			srcMode = (int)UnityEngine.Rendering.BlendMode.SrcAlpha;
			dstMode = (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
		}

		material.SetInt("_SrcBlendMode", srcMode);
		material.SetInt("_DstBlendMode", dstMode);

		if (IsUI(batchDesc.m_UserData))
			material.SetInt("_ZTestMode", (int)UnityEngine.Rendering.CompareFunction.Always);
		else
			material.SetInt("_ZTestMode", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
		
		// Set the shader variation:
		if (batchDesc.HasMaterialFlag(PKFxManagerImpl.EMaterialFlags.Has_AlphaRemap))
			material.EnableKeyword("PK_HAS_ALPHA_REMAP");
		if (batchDesc.HasMaterialFlag(PKFxManagerImpl.EMaterialFlags.Has_AnimBlend))
			material.EnableKeyword("PK_HAS_ANIM_BLEND");
		if (batchDesc.HasMaterialFlag(PKFxManagerImpl.EMaterialFlags.Has_Distortion))
			material.EnableKeyword("PK_HAS_DISTORTION");
		if (batchDesc.HasMaterialFlag(PKFxManagerImpl.EMaterialFlags.Has_Lighting))
			material.EnableKeyword("PK_HAS_LIGHTING");
		if (batchDesc.HasMaterialFlag(PKFxManagerImpl.EMaterialFlags.Has_RibbonComplex))
			material.EnableKeyword("PK_HAS_RIBBON_COMPLEX");
		if (batchDesc.HasMaterialFlag(PKFxManagerImpl.EMaterialFlags.Has_Color))
			material.EnableKeyword("PK_HAS_COLOR");
		if (batchDesc.HasMaterialFlag(PKFxManagerImpl.EMaterialFlags.Has_Soft) && PKFxSettings.EnableSoftParticles)
		{
			material.EnableKeyword("PK_HAS_SOFT");
			material.SetFloat("_InvSoftnessDistance", batchDesc.m_InvSofnessDistance);
		}
		// Set the render queue:
		material.renderQueue = m_RenderQueue + batchDesc.m_DrawOrder;
		return material;
	}

	private Shader				ResolveShader(PKFxManagerImpl.SBatchDesc batchDesc)
	{
		Shader					shader = null;
		string					shaderPath = ExtractShaderPath(batchDesc.m_UserData);

		if (!string.IsNullOrEmpty(shaderPath))
		{
			shader = Shader.Find(shaderPath);
		}
		if (shader == null)
		{
			if (batchDesc.m_Type != PKFxManagerImpl.ERendererType.Mesh)
			{
				shader = m_ParticleShader;
			}
			else
			{
				if (batchDesc.HasUniformFlag(PKFxManagerImpl.EUniformFlags.Is_Additive) ||
					batchDesc.HasUniformFlag(PKFxManagerImpl.EUniformFlags.Is_AdditiveNoAlpha))
				{
					shader = m_TransparentMeshShader;
				}
				else
				{
					shader = m_OpaqueMeshShader;
				}
			}
		}
		return shader;
	}
}
