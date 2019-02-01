using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if		UNITY_EDITOR
using UnityEditor;
#endif

public class PKFxMaterialFactoryHDRP : PKFxMaterialFactory
{
	public Material m_DistortionMaterial;
	public Material m_DoubleSidedDistortionMaterial;
	public Shader	m_ParticleShader;
	public Shader	m_MeshShader;
	public int		m_RenderQueue = 3050; // In HDRP, the objects > 3100 are not drawn
	public bool		m_UseSortingLayers = false;

#if UNITY_EDITOR
	[MenuItem("Assets/Create/PopcornFX/HDRP Material Factory")]
	static void CreateMaterialFactoryHDRP()
	{
		string folderPath = GetSelectedPathOrFallback();
		string path = Path.Combine(folderPath, "HDRPMaterialFactory.asset");

		// Create the HDRP shader (does not compile on other projects otherwise):
		AssetDatabase.CopyAsset("Assets/PopcornFX/Resources/PKFxShaderCode/PKFxParticleShaderHDRPCode.txt", "Assets/PopcornFX/Resources/PKFxParticleShaderHDRP.shader");
		
		// Create the HDRP material factory asset:
		PKFxMaterialFactoryHDRP materialFactoryHDRP = ScriptableObject.CreateInstance<PKFxMaterialFactoryHDRP>();
		
		// Find the shaders:
		materialFactoryHDRP.m_DistortionMaterial = Resources.Load<Material>("PKFx-HDRP-Distortion");
		materialFactoryHDRP.m_DoubleSidedDistortionMaterial = Resources.Load<Material>("PKFx-HDRP-Distortion-DoubleSided");
		materialFactoryHDRP.m_ParticleShader = Shader.Find("PopcornFX/PKFxParticleShaderHDRP");
		materialFactoryHDRP.m_MeshShader = Shader.Find("HDRenderPipeline/Lit");
		
		AssetDatabase.CreateAsset(materialFactoryHDRP, path);
		AssetDatabase.SaveAssets();
		
		EditorUtility.FocusProjectWindow();
		
		Selection.activeObject = materialFactoryHDRP;
	}

	public override Shader[] GetMeshShaders()
	{
		return new Shader[1] { m_MeshShader };
	}
#endif

	public override void	SetupRenderer(PKFxManagerImpl.SBatchDesc batchDesc, GameObject gameObject, MeshRenderer meshRenderer)
	{
		if (m_UseSortingLayers)
		{
			if (IsUI(batchDesc.m_UserData))
				meshRenderer.sortingLayerName = "PopcornFXUI";
			else
				meshRenderer.sortingLayerName = "PopcornFX";
		}
	}

	public override void	SetupMeshRenderer(PKFxManagerImpl.SBatchDesc batchDesc, GameObject gameObject, PKFxMeshInstancesRenderer meshRenderer)
	{
		meshRenderer.m_CastShadow = batchDesc.HasMaterialFlag(PKFxManagerImpl.EMaterialFlags.Has_CastShadow);
		
		PKFxFxAsset.DependencyDesc DepDesc = PKFxManager.GetBuiltAsset().m_Dependencies.Find(x => batchDesc.m_MeshAsset.Contains(x.m_Path));
		if (DepDesc != null)
		{
			GameObject meshGO = DepDesc.m_Object as GameObject;
			List<Mesh> meshes = new List<Mesh>();
			List<Matrix4x4> trans = new List<Matrix4x4>();

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

	}

	public override Material ResolveParticleMaterial(PKFxManagerImpl.SBatchDesc batchDesc)
	{
		Material customMat = TryFindAndInstantiateCustomMaterial(batchDesc);

		if (customMat != null)
			return customMat;


		// First we handle the distortion with the material directly:
		if (batchDesc.HasMaterialFlag(PKFxManagerImpl.EMaterialFlags.Has_Distortion))
		{
			Material distoMat = null;
			Texture distoTexture = GetTextureDiffuse(batchDesc);

			if (batchDesc.HasMaterialFlag(PKFxManagerImpl.EMaterialFlags.Has_DoubleSided))
			{
				distoMat = new Material(m_DoubleSidedDistortionMaterial);
			}
			else
			{
				distoMat = new Material(m_DistortionMaterial);
			}

			distoMat.CopyPropertiesFromMaterial(m_DistortionMaterial);
			distoMat.SetTexture("_DistortionVectorMap", distoTexture); // Set disto vector map

			return distoMat;
		}

		// Set the diffuse texture:
		Texture diffuseTexture = GetTextureDiffuse(batchDesc);

		Shader shader = ResolveShader(batchDesc);
		Debug.Assert(shader != null);
		Material material = new Material(shader);

		// Set the diffuse texture:
		material.mainTexture = diffuseTexture;

		if (batchDesc.m_Type == PKFxManagerImpl.ERendererType.Mesh)
		{
			return material;
		}

		// Set the alpha remap texture
		if (batchDesc.m_AlphaRemap != null)
		{
			string alphaRemapPath = Path.ChangeExtension(batchDesc.m_AlphaRemap, null);
			Texture alphaRemapTexture = Resources.Load<Texture>(alphaRemapPath);
			alphaRemapTexture.wrapMode = TextureWrapMode.Clamp;
			Debug.Assert(alphaRemapTexture != null);
			material.SetTexture("_AlphaMap", alphaRemapTexture);
		}

		// Set the material uniforms:
		material.SetInt("_UniformFlags", batchDesc.m_UniformFlags);

		int srcMode = 0;
		int dstMode = 0;

		// Additive and distortion
		if (batchDesc.HasUniformFlag(PKFxManagerImpl.EUniformFlags.Is_Additive) ||
			batchDesc.HasUniformFlag(PKFxManagerImpl.EUniformFlags.Is_AdditiveNoAlpha))
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
		material.renderQueue = m_RenderQueue + batchDesc.m_DrawOrder;
		return material;
	}

	private Shader ResolveShader(PKFxManagerImpl.SBatchDesc batchDesc)
	{
		Shader shader = null;
		string shaderPath = ExtractShaderPath(batchDesc.m_UserData);

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
				shader = m_MeshShader;
			}
		}
		return shader;
	}
}
