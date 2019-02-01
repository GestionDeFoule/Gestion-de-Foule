using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if		UNITY_EDITOR
using UnityEditor;
#endif

public abstract class PKFxMaterialFactory : UnityEngine.ScriptableObject
{
	public abstract void		SetupRenderer(PKFxManagerImpl.SBatchDesc batchDesc, GameObject gameObject, MeshRenderer meshRenderer);
	public abstract void		SetupMeshRenderer(PKFxManagerImpl.SBatchDesc batchDesc, GameObject gameObject, PKFxMeshInstancesRenderer meshRenderer);
	public abstract Material	ResolveParticleMaterial(PKFxManagerImpl.SBatchDesc batchDesc);

#if		UNITY_EDITOR
	public static string		GetSelectedPathOrFallback()
	{
		string path = "Assets";

		foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
		{
			path = AssetDatabase.GetAssetPath(obj);
			if (!string.IsNullOrEmpty(path) && File.Exists(path))
			{
				path = Path.GetDirectoryName(path);
				break;
			}
		}
		return path;
	}

	public abstract				Shader[] GetMeshShaders();
#endif

	[Serializable]
	public class	CustomMaterial
	{
		public string		m_UserData;
		public Material		m_CustomMaterial;
	}

	public bool				m_CustomMaterialBindParticleTexture;
	public CustomMaterial[] m_CustomMaterials;

	public Material		TryFindAndInstantiateCustomMaterial(PKFxManagerImpl.SBatchDesc batchDesc)
	{
		if (m_CustomMaterials != null)
		{
			foreach (CustomMaterial curMat in m_CustomMaterials)
			{
				if (batchDesc.m_UserData == curMat.m_UserData)
				{
					if (curMat.m_CustomMaterial != null)
					{
						Material m = new Material(curMat.m_CustomMaterial);
						if (m_CustomMaterialBindParticleTexture)
						{
							// Set the diffuse texture:
							Texture diffuseTexture = GetTextureDiffuse(batchDesc);
							m.mainTexture = diffuseTexture;
						}
						return m;
					}
					else
					{
						Debug.LogWarning("Custom Material is missing, remember to set it in \"PKFXSettings > Material Factory > Custom Materials\"");
					}
				}
			}
		}
		return null;
	}

	public static Texture	GetTextureDiffuse(PKFxManagerImpl.SBatchDesc batchDesc)
	{
		Texture diffuseTexture = null;

		if (batchDesc.m_DiffuseMap != null)
		{
			bool isLinear = batchDesc.HasMaterialFlag(PKFxManagerImpl.EMaterialFlags.Has_Distortion);
			PKFxFxAsset.DependencyDesc DepDesc = null;
			if (isLinear)
			{
				string path = Path.GetDirectoryName(batchDesc.m_DiffuseMap) + "/" + Path.GetFileNameWithoutExtension(batchDesc.m_DiffuseMap) + "_linear" + Path.GetExtension(batchDesc.m_DiffuseMap);
				DepDesc = PKFxManager.GetBuiltAsset().m_Dependencies.Find(x => path.Contains(x.m_Path));
			}
			if (DepDesc == null)
				DepDesc = PKFxManager.GetBuiltAsset().m_Dependencies.Find(x => batchDesc.m_DiffuseMap.Contains(x.m_Path));
			if (DepDesc != null)
				diffuseTexture = DepDesc.m_Object as Texture;

			if (diffuseTexture != null)
			{
				if (batchDesc.m_Type != PKFxManagerImpl.ERendererType.Ribbon)
				{
					diffuseTexture.wrapMode = TextureWrapMode.Clamp;
				}
			}
			else
				Debug.LogError("[PKFX] Error while trying to create diffuse texture. Try to reimport \"" + batchDesc.m_DiffuseMap + "\" and check if its format is compatible with Unity.");
		}
		return diffuseTexture;
	}

	public static Texture GetTextureLinear(PKFxManagerImpl.SBatchDesc batchDesc)
	{
		Texture distoTexture = null;
		
		if (batchDesc.m_NormalMap != null)
		{
			bool						isLinear = batchDesc.HasMaterialFlag(PKFxManagerImpl.EMaterialFlags.Has_Distortion);
			PKFxFxAsset.DependencyDesc	DepDesc = null;

			if (!isLinear)
				return null;

			string path = Path.GetDirectoryName(batchDesc.m_NormalMap) + "/" + Path.GetFileNameWithoutExtension(batchDesc.m_NormalMap) + "_linear" + Path.GetExtension(batchDesc.m_NormalMap);
			DepDesc = PKFxManager.GetBuiltAsset().m_Dependencies.Find(x => path.Contains(x.m_Path));

			if (DepDesc == null)
				DepDesc = PKFxManager.GetBuiltAsset().m_Dependencies.Find(x => batchDesc.m_NormalMap.Contains(x.m_Path));
			if (DepDesc != null)
				distoTexture = DepDesc.m_Object as Texture;

			if (distoTexture != null)
			{
				if (batchDesc.m_Type != PKFxManagerImpl.ERendererType.Ribbon)
				{
					distoTexture.wrapMode = TextureWrapMode.Clamp;
				}
			}
			else
				Debug.LogError("[PKFX] Error while trying to create linear texture. Try to reimport \"" + batchDesc.m_NormalMap + "\" and check if its format is compatible with Unity.");
		}
		return distoTexture;
	}

	public static bool		IsUI(string userData)
	{
		if (!string.IsNullOrEmpty(userData))
		{
			string[] userDataList = userData.Split(';');
			foreach (var str in userDataList)
			{
				if (str == "UI")
					return true;
			}
		}
		return false;
	}

	public static string ExtractShaderPath(string userData)
	{
		if (!string.IsNullOrEmpty(userData))
		{
			string[] userDataList = userData.Split(';');
			foreach (var str in userDataList)
			{
				if (str != "UI")
					return str;
			}
		}
		return null;
	}
}
