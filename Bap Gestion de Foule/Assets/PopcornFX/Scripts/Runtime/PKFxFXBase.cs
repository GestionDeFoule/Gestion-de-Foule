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
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;

public partial class PKFxFX : MonoBehaviour
{
	//----------------------------------------------------------------------------
	// Samplers:
	//----------------------------------------------------------------------------
	// Mesh sampler:
	//----------------------------------------------------------------------------

	static public IntPtr GetMeshToFill(Mesh mesh, int samplingChannels, bool skinning)
	{
		int vertexCount = mesh.vertexCount;
		int indexCount = mesh.triangles.Length;
		int bonesCount = skinning ? mesh.bindposes.Length : 0;

		if (mesh.vertices.Length != vertexCount)
		{
			Debug.LogError("asks for EMeshSamplingInfo.Info_ChannelPosition when the mesh doesn't have positions");
			return IntPtr.Zero;
		}
		if ((samplingChannels & (int)Sampler.EMeshSamplingInfo.Info_ChannelNormal) != 0)
		{
			if (mesh.normals.Length != vertexCount)
			{
				samplingChannels ^= (int)Sampler.EMeshSamplingInfo.Info_ChannelNormal;
				Debug.LogWarning("asks for EMeshSamplingInfo.Info_ChannelNormal when the mesh doesn't have normals");
			}
		}
		if ((samplingChannels & (int)Sampler.EMeshSamplingInfo.Info_ChannelTangent) != 0)
		{
			if (mesh.tangents.Length != vertexCount)
			{
				samplingChannels ^= (int)Sampler.EMeshSamplingInfo.Info_ChannelTangent;
				Debug.LogWarning("asks for EMeshSamplingInfo.Info_ChannelTangent when the mesh doesn't have tangents");
			}
		}
		if ((samplingChannels & (int)Sampler.EMeshSamplingInfo.Info_ChannelUV) != 0)
		{
			if (mesh.uv.Length != vertexCount)
			{
				samplingChannels ^= (int)Sampler.EMeshSamplingInfo.Info_ChannelUV;
				Debug.LogWarning("asks for EMeshSamplingInfo.Info_ChannelUV when the mesh doesn't have UVs");
			}
		}
		if ((samplingChannels & (int)Sampler.EMeshSamplingInfo.Info_ChannelVertexColor) != 0)
		{
			if (mesh.colors.Length != vertexCount)
			{
				samplingChannels ^= (int)Sampler.EMeshSamplingInfo.Info_ChannelVertexColor;
				Debug.LogWarning("asks for EMeshSamplingInfo.Info_ChannelVertexColor when the mesh doesn't have vertex color");
			}
		}
		return PKFxManager.GetMeshSamplerToFill(vertexCount, indexCount, bonesCount, samplingChannels);
	}

	//----------------------------------------------------------------------------

	static public bool UpdateMeshToFill(IntPtr meshToFillPtr, Mesh mesh)
	{
		int[] trianglesSrc = mesh.triangles;
		int vertexCount = mesh.vertexCount;

		unsafe
		{
			PKFxManagerImpl.SMeshSamplerToFill* meshToFill = (PKFxManagerImpl.SMeshSamplerToFill*)meshToFillPtr.ToPointer();

			if (meshToFill == null)
			{
				Debug.LogError("Could not create the mesh data");
				return false;
			}
			else if (	meshToFill->m_IdxCount != trianglesSrc.Length ||
						meshToFill->m_VtxCount != vertexCount)
			{
				Debug.LogError("Index count and vertex count does not match with the allocated mesh");
				return false;
			}

			Vector4* positions = (Vector4*)meshToFill->m_Positions.ToPointer();
			Vector4* normals = (Vector4*)meshToFill->m_Normals.ToPointer();
			Vector4* tangents = (Vector4*)meshToFill->m_Tangents.ToPointer();
			Vector2* uvs = (Vector2*)meshToFill->m_UV.ToPointer();
			Vector4* colors = (Vector4*)meshToFill->m_VertexColor.ToPointer();
			float* bonesWeights = (float*)meshToFill->m_VertexBonesWeights.ToPointer();
			int* bonesIndices = (int*)meshToFill->m_VertexBonesIndices.ToPointer();

			// We use Marshal.Copy as often as possible
			// Indices:
			Marshal.Copy(trianglesSrc, 0, meshToFill->m_Indices, trianglesSrc.Length);
			// Positions:
			if (positions != null)
			{
				Vector3[] srcPositions = mesh.vertices;
				for (int i = 0; i < vertexCount; ++i)
				{
					positions[i] = srcPositions[i]; // Vector3 to Vector4
				}
			}
			else
			{
				Debug.LogError("Could not copy the mesh positions");
				return false;
			}
			// Normals:
			if (normals != null)
			{
				Vector3[] srcNormals = mesh.normals;
				for (int i = 0; i < vertexCount; ++i)
				{
					normals[i] = srcNormals[i]; // Vector3 to Vector4
				}
			}
			// Tangents (could be copied using Marshal.Copy but does not handle Vector4[]):
			if (tangents != null)
			{
				Vector4[] srcTangents = mesh.tangents;
				for (int i = 0; i < vertexCount; ++i)
				{
					tangents[i] = srcTangents[i];
				}
			}
			// UVs (could be copied using Marshal.Copy but does not handle Vector2[]):
			if (uvs != null)
			{
				Vector2[] srcUvs = mesh.uv;
				for (int i = 0; i < vertexCount; ++i)
				{
					uvs[i] = srcUvs[i];
				}
			}
			// Colors (could be copied using Marshal.Copy but does not handle Vector4[]):
			if (colors != null)
			{
				Color[] srcColors = mesh.colors;
				for (int i = 0; i < vertexCount; ++i)
				{
					colors[i] = srcColors[i];
				}
			}

			// Bones
			if (bonesWeights != null && bonesIndices != null)
			{
				BoneWeight[] boneWeightsSrc = mesh.boneWeights;

				// Test native code:
				for (int i = 0; i < vertexCount; ++i)
				{
					BoneWeight boneWeight = boneWeightsSrc[i];

					bonesWeights[i * 4 + 0] = boneWeight.weight0;
					bonesWeights[i * 4 + 1] = boneWeight.weight1;
					bonesWeights[i * 4 + 2] = boneWeight.weight2;
					bonesWeights[i * 4 + 3] = boneWeight.weight3;

					bonesIndices[i * 4 + 0] = boneWeight.boneIndex0;
					bonesIndices[i * 4 + 1] = boneWeight.boneIndex1;
					bonesIndices[i * 4 + 2] = boneWeight.boneIndex2;
					bonesIndices[i * 4 + 3] = boneWeight.boneIndex3;
				}
			}
		}
		return true;
	}

	//----------------------------------------------------------------------------

	public static bool UpdateMeshBones(int effectID, int samplerID, SkinnedMeshRenderer skinnedMesh)
	{
		if (skinnedMesh.bones.Length == 0)
		{
			Debug.Log("The skinned mesh does not have bones");
			return false;
		}

		Matrix4x4 rootMat = skinnedMesh.transform.parent.worldToLocalMatrix;
		IntPtr matricesPtr = PKFxManager.UpdateSamplerSkinningSetMatrices(effectID, samplerID);

		if (matricesPtr == IntPtr.Zero)
		{
			Debug.Log("Could not retrieve the bones matrices");
			return false;
		}

		unsafe
		{
			Matrix4x4* matrices = (Matrix4x4*)matricesPtr.ToPointer();
			Transform[] curBonesTransform = skinnedMesh.bones;
			Matrix4x4[] bindPos = skinnedMesh.sharedMesh.bindposes;

			for (int i = 0; i < skinnedMesh.bones.Length; ++i)
			{
				Matrix4x4 boneLocalToWorld = curBonesTransform[i].localToWorldMatrix;
				matrices[i] = rootMat * boneLocalToWorld * bindPos[i];
			}
		}
		return true;
	}

	//----------------------------------------------------------------------------
	// Curve sampler:
	//----------------------------------------------------------------------------

	public static bool UpdateCurveToFill(IntPtr curveToFillPtr, AnimationCurve[] curvesArray, float[] curvesTimeKeys)
	{
		if (curvesTimeKeys == null || curvesTimeKeys.Length == 0 ||
			curvesArray == null || curvesArray.Length == 0)
		{
			Debug.LogError("No keypoints / curves found");
			return false;
		}

		// Copy values:
		int keyPointsCount = curvesTimeKeys.Length;
		int curveDimension = curvesArray.Length;
		int keyDataOffset = (1 + curveDimension * 3);

		unsafe
		{
			PKFxManagerImpl.SCurveSamplerToFill		*toFill = (PKFxManagerImpl.SCurveSamplerToFill*)curveToFillPtr.ToPointer();
			float									*dataArray = (float*)toFill->m_KeyPoints.ToPointer();

			if (toFill->m_KeyPointsCount != keyPointsCount ||
				toFill->m_CurveDimension != curveDimension)
			{
				Debug.LogError("Curve dimension and key points count does not match with the allocated curve");
				return false;
			}

			for (int keyId = 0; keyId < curvesTimeKeys.Length; ++keyId)
			{
				int realId = keyId * keyDataOffset;

				dataArray[realId] = curvesTimeKeys[keyId];
				for (int curveId = 0; curveId < curvesArray.Length; ++curveId)
				{
					AnimationCurve	curve = curvesArray[curveId];
					int				curveRealId = realId + 1 + curveId * 3;
					Keyframe		key = curve.keys[keyId];

					dataArray[curveRealId + 0] = key.value;
					dataArray[curveRealId + 1] = key.inTangent;
					dataArray[curveRealId + 2] = key.outTangent;
				}
			}
		}
		return true;
	}

	//----------------------------------------------------------------------------
	// Texture sampler:
	//----------------------------------------------------------------------------

	public static class PKImageConverter
	{
		public static void ARGB2BGRA(ref byte[] data)
		{
			for (int id = 0; id < data.Length;)
			{
				byte[] col = new byte[4] { data[id + 3], data[id + 2], data[id + 1], data[id] };
				data[id++] = col[0];
				data[id++] = col[1];
				data[id++] = col[2];
				data[id++] = col[3];
			}
		}

		public static void RGBA2BGRA(ref byte[] data)
		{
			for (int id = 0; id < data.Length; id += 4)
			{
				byte tmp = data[id];
				data[id] = data[id + 2];
				data[id + 2] = tmp;
			}
		}

		public static void RGB2BGR(ref byte[] data)
		{
			for (int id = 0; id < data.Length; id += 3)
			{
				byte tmp = data[id];
				data[id] = data[id + 2];
				data[id + 2] = tmp;
			}
		}
	}

	//----------------------------------------------------------------------------

	public static PKFxManagerImpl.EImageFormat ResolveImageFormat(Texture2D t, ref byte[] data)
	{
		if (t.format == TextureFormat.DXT1)
			return PKFxManagerImpl.EImageFormat.DXT1;
		else if (t.format == TextureFormat.DXT5)
			return PKFxManagerImpl.EImageFormat.DXT5;
		else if (t.format == TextureFormat.ARGB32)
		{
			PKImageConverter.ARGB2BGRA(ref data);
			return PKFxManagerImpl.EImageFormat.BGRA8;
		}
		else if (t.format == TextureFormat.RGBA32)
		{
			PKImageConverter.RGBA2BGRA(ref data);
			return PKFxManagerImpl.EImageFormat.BGRA8;
		}
		else if (t.format == TextureFormat.BGRA32)
			return PKFxManagerImpl.EImageFormat.BGRA8;
		else if (t.format == TextureFormat.RGB24)
		{
			PKImageConverter.RGB2BGR(ref data);
			return PKFxManagerImpl.EImageFormat.BGR8;
		}
		else if (t.format == TextureFormat.PVRTC_RGB4)
			return PKFxManagerImpl.EImageFormat.RGB4_PVRTC1;
		else if (t.format == TextureFormat.PVRTC_RGBA4)
			return PKFxManagerImpl.EImageFormat.RGBA4_PVRTC1;
		else if (t.format == TextureFormat.PVRTC_RGB2)
			return PKFxManagerImpl.EImageFormat.RGB2_PVRTC1;
		else if (t.format == TextureFormat.PVRTC_RGBA2)
			return PKFxManagerImpl.EImageFormat.RGBA2_PVRTC1;
		else if (t.format == TextureFormat.ETC_RGB4)
			return PKFxManagerImpl.EImageFormat.RGB8_ETC1;
		else if (t.format == TextureFormat.ETC2_RGB)
			return PKFxManagerImpl.EImageFormat.RGB8_ETC2;
		else if (t.format == TextureFormat.ETC2_RGBA8)
			return PKFxManagerImpl.EImageFormat.RGBA8_ETC2;
		else if (t.format == TextureFormat.ETC2_RGBA1)
			return PKFxManagerImpl.EImageFormat.RGB8A1_ETC2;
		else if (t.format == TextureFormat.RGBAFloat)
			return PKFxManagerImpl.EImageFormat.Fp32_RGBA;
		else
		{
			Debug.LogError("[PKFX] " + t.name + " texture format not supported : " +
						   t.format);
			return PKFxManagerImpl.EImageFormat.Invalid;
		}
	}

	//----------------------------------------------------------------------------

	public static IntPtr	GetAndUpdateTextureToFill(Texture2D texture, Sampler.ETexcoordMode wrapMode)
	{
		byte[]							data = texture.GetRawTextureData();
		PKFxManagerImpl.EImageFormat	imageFormat = ResolveImageFormat(texture, ref data);
		IntPtr							textureToFillPtr = PKFxManager.GetTextureSamplerToFill(data.Length);

		unsafe
		{
			PKFxManagerImpl.STextureSamplerToFill	*textureToFill = (PKFxManagerImpl.STextureSamplerToFill*)textureToFillPtr.ToPointer();

			if (textureToFill->m_TextureData == IntPtr.Zero ||
				imageFormat == PKFxManagerImpl.EImageFormat.Invalid)
				return IntPtr.Zero;

			Marshal.Copy(data, 0, textureToFill->m_TextureData, data.Length);
			textureToFill->m_Width = texture.width;
			textureToFill->m_Height = texture.height;
			textureToFill->m_PixelFormat = (int)imageFormat;
			textureToFill->m_WrapMode = (int)wrapMode;
		}
		return textureToFillPtr;
	}

	//----------------------------------------------------------------------------

	#region PreloadEffect from asset

	public static void PreloadEffectFromAsset(PKFxFxAsset fxAsset)
	{
		PKFxManager.SetBuiltAsset(fxAsset);
		PKFxManager.PreloadEffectDependencies(fxAsset);
		PKFxManager.PreloadEffectIFN(fxAsset.m_AssetName, fxAsset.m_UsesMeshRenderer);
		PKFxManager.SetBuiltAsset(null);
	}

	#endregion

	#region CreateEffect from asset

	public static int CreateEffect(PKFxFxAsset fxAsset, Transform t, bool autoDestroy)
	{
		PKFxManager.SetBuiltAsset(fxAsset);
		int res = CreateEffect(fxAsset.m_AssetName, t.localToWorldMatrix, fxAsset.m_UsesMeshRenderer, autoDestroy);
		PKFxManager.SetBuiltAsset(null);
		return res;
	}

	//----------------------------------------------------------------------------

	public static int CreateEffect(PKFxFxAsset fxAsset, Vector3 position, Quaternion rotation, Vector3 scale, bool autoDestroy)
	{
		PKFxManager.SetBuiltAsset(fxAsset);
		Matrix4x4 m = Matrix4x4.identity;
		m.SetTRS(position, rotation, scale);
		int res = CreateEffect(fxAsset.m_AssetName, m, fxAsset.m_UsesMeshRenderer, autoDestroy);
		PKFxManager.SetBuiltAsset(null);
		return res;
	}

	//----------------------------------------------------------------------------

	public static int CreateEffect(PKFxFxAsset fxAsset, Matrix4x4 m, bool autoDestroy)
	{
		PKFxManager.SetBuiltAsset(fxAsset);
		int res = CreateEffect(fxAsset.m_AssetName, m, fxAsset.m_UsesMeshRenderer, autoDestroy);
		PKFxManager.SetBuiltAsset(null);
		return res;
	}

	//----------------------------------------------------------------------------

	private static int CreateEffect(string path, Matrix4x4 m, bool usesMeshRenderer, bool autoDestroy)
	{
		PKFxManagerImpl.SFxDesc desc;

		desc.m_Transforms = m;
		desc.m_FxPath = path;
		desc.m_UsesMeshRenderer = usesMeshRenderer;
		desc.m_AutoDestroyEffect = autoDestroy;
		return PKFxManager.InstantiateEffect(ref desc);

	}

	#endregion

	//----------------------------------------------------------------------------

	public static bool UpdateTransformEffect(int FxGUID, Transform t)
	{
		return PKFxManager.SetEffectTransform(FxGUID, t);
	}
}
