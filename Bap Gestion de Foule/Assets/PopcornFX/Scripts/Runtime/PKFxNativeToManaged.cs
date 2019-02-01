//----------------------------------------------------------------------------
// Created on Thu Oct 31 11:53:00 2013 Maxime Dumas
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
using UnityEngine.Rendering;
using System;
using System.Runtime.InteropServices;
using System.IO;
using AOT;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

//----------------------------------------------------------------------------

public partial class PKFxManagerImpl : object
{
#if		UNITY_EDITOR
	private static readonly int MAX_PATH = 260;
#endif

	public enum EBaseType : int
	{
		Int = 22,
		Int2,
		Int3,
		Int4,
		Float = 28,
		Float2,
		Float3,
		Float4
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SRaycastPack
	{
		public IntPtr			m_RayOrigins;
		public IntPtr			m_RayDirections;
		public IntPtr			m_OutNormals;
		public IntPtr			m_OutPositions;
		public IntPtr			m_OutDistances;
		public int				m_FilterLayer;
		public int				m_RayCount;
	};

	// Attributes:
	[StructLayout(LayoutKind.Sequential)]
	public struct SNativeAttributeDesc
	{
		public EBaseType		m_AttributeType;
		public int				m_MinMaxFlag;

		public IntPtr			m_AttributeName;
		public IntPtr			m_Description;

		public float			m_DefaultValueX;
		public float			m_DefaultValueY;
		public float			m_DefaultValueZ;
		public float			m_DefaultValueW;

		public float			m_MinValueX;
		public float			m_MinValueY;
		public float			m_MinValueZ;
		public float			m_MinValueW;

		public float			m_MaxValueX;
		public float			m_MaxValueY;
		public float			m_MaxValueZ;
		public float			m_MaxValueW;
	};

	// Samplers:
	[StructLayout(LayoutKind.Sequential)]
	public struct SNativeSamplerDesc
	{
		public PKFxFxAsset.SamplerDesc.ESamplerType m_SamplerType;
		public IntPtr								m_SamplerName;
		public IntPtr								m_Description;

		public Quaternion		m_ShapeRotation; // Rotation quaternion
		public Vector3			m_ShapePosition;

		public int				m_CurveDimension;
		public int				m_CurveKeyCount;
		public IntPtr			m_CurveTimes;
		public IntPtr			m_CurveFloatValues;
		public IntPtr			m_CurveFloatTangents;
	};

	// Create Renderers:
	// Billboards and ribbons:
	[StructLayout(LayoutKind.Sequential)]
	public struct SPopcornRendererDesc
	{
		public IntPtr	m_UserData;
		public IntPtr	m_EffectNames;
		public int		m_MaterialFlags;
		public int		m_UniformFlags;
		public int		m_DrawOrder;
		public IntPtr	m_DiffuseMap;
		public IntPtr	m_NormalMap;
		public IntPtr	m_AlphaRemap;
		public float	m_InvSofnessDistance;
	};

	// Meshes:
	[StructLayout(LayoutKind.Sequential)]
	public struct SMeshRendererDesc
	{
		public IntPtr	m_MeshAsset;
		public IntPtr	m_UserData;
		public IntPtr	m_EffectNames;
		public int		m_MaterialFlags;
		public int		m_UniformFlags;
		public int		m_DrawOrder;
		public int		m_SubMeshID;
		public IntPtr	m_DiffuseMap;
		public IntPtr	m_NormalMap;
		public IntPtr	m_SpecularMap;
	};

	// Bounds
	[StructLayout(LayoutKind.Sequential)]
	public struct SUpdateRendererBounds
	{
		public float	m_MinX;
		public float	m_MinY;
		public float	m_MinZ;
		public float	m_MaxX;
		public float	m_MaxY;
		public float	m_MaxZ;
	};

	// Effect info:
	[StructLayout(LayoutKind.Sequential)]
	public struct SRetrieveRendererInfo
	{
		public IntPtr		m_IsIndex32;
		public IntPtr		m_VertexBufferSize;
		public IntPtr		m_IndexBufferSize;
		public IntPtr		m_VBHandler;
		public IntPtr		m_IBHandler;
	};

	//Play sound
	[StructLayout(LayoutKind.Sequential)]
	public struct SNativeSoundDescriptor
	{
		public int		m_ChannelGroup;
		public IntPtr	m_Path;
		public IntPtr	m_EventStart;
		public IntPtr	m_EventStop;
		public float	m_WorldPositionX;
		public float	m_WorldPositionY;
		public float	m_WorldPositionZ;
		public float	m_Volume;
		public float	m_StartTimeOffsetInSeconds;
		public float	m_PlayTimeInSeconds;
		public int		m_UserData;
	};

	public enum EAssetChangesType : int
	{
		Undefined = 0,
		Add,
		Remove,
		Update,
		Rename
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SAssetChangesDesc
	{
		public string	m_Path;
		public string	m_PathOld;
		public int		m_Type;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SAssetImportError
	{
		public string m_Path;
		public string m_ErrorMessage;
	}

	enum EUseInfoFlag : int
	{
		IsLinearTexture = 0x01,
		IsMeshSampler = 0x02,
		IsMeshRenderer = 0x04,
		IsTextureSampler = 0x08
	};


	//----------------------------------------------------------------------------

	// Native to managed delegates:
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnResourceLoad(IntPtr delegatePtr);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnResourceWrite(IntPtr delegatePtr);

	// Not in this file:
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnRaiseEvent(IntPtr delegatePtr);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnRaycastPack(IntPtr delegatePtr);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnStartSound(IntPtr delegatePtr);

	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnFxStopped(IntPtr delegatePtr);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnAudioWaveformData(IntPtr delegatePtr);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnAudioSpectrumData(IntPtr delegatePtr);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnSetupNewBillboardRenderer(IntPtr delegatePtr);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnSetupNewRibbonRenderer(IntPtr delegatePtr);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnSetupNewMeshRenderer(IntPtr delegatePtr);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnResizeRenderer(IntPtr delegatePtr);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnSetRendererActive(IntPtr delegatePtr);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnSetMeshInstancesCount(IntPtr delegatePtr);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnSetMeshInstancesBuffer(IntPtr delegatePtr);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnRetrieveRendererBufferInfo(IntPtr delegatePtr);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnUpdateRendererBounds(IntPtr delegatePtr);

#if UNITY_EDITOR
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnEffectDependencyFound(IntPtr delegatePtr);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnEffectAttributeFound(IntPtr delegatePtr);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnEffectSamplerFound(IntPtr delegatePtr);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnGetEffectInfo(IntPtr delegatePtr);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnAssetChange(IntPtr delegatePtr);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnImportError(IntPtr delegatePtr);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnGetAllAssetPath(IntPtr delegatePtr);
	[DllImport(kPopcornPluginName, CallingConvention = kCallingConvention)]
	public static extern void SetDelegateOnRendererNamesAdded(IntPtr delegatePtr);
#endif
	//----------------------------------------------------------------------------

	private delegate ulong ResourceLoadCallback(IntPtr path, ref IntPtr handler);

	[MonoPInvokeCallback(typeof(ResourceLoadCallback))]
	public static ulong OnResourceLoad(IntPtr pathHandler, ref IntPtr handler)
	{
		string path = Marshal.PtrToStringAnsi(pathHandler);
		string fName = Path.GetFileName(path);
		string ext = Path.GetExtension(path);

		if (m_Dependencies != null)
		{
			if (arrayContains(s_CustomFileTypes, ext))
			{
				foreach (PKFxAsset asset in m_Dependencies)
				{
					if (Path.GetFileName(asset.m_AssetName) == fName)
					{
						var pinned = PinAssetData(asset);
						handler = pinned;
						return (ulong)asset.m_Data.Length;
					}
				}
			}
			else if (arrayContains(s_TexFileTypes, ext))
			{
				foreach (var t in m_TexDependencies)
				{
					if (t.name == Path.GetFileNameWithoutExtension(fName))
					{
						ulong len = PinTextureData(t, ref handler);
						return len;
					}
				}
			}
		}
		if (m_RuntimeAssets != null && m_RuntimeAssets.Count > 0)
		{
			// failing that, check if we have it in the runtime resources
			foreach (GameObject go in m_RuntimeAssets)
			{
				PKFxRuntimeMeshAsset asset = go.GetComponent<PKFxRuntimeMeshAsset>();
				if (Path.GetFileName(asset.m_AssetName) == fName)
				{
					var pinned = PinAssetData(asset);
					handler = pinned;
					return (ulong)asset.m_Data.Length;
				}
			}
		}
		Debug.LogError("[PKFX] " + fName + " not found in dependencies.");
		return 0;
	}

	//----------------------------------------------------------------------------

	private delegate ulong ResourceWriteCallback(IntPtr path, IntPtr data, ulong offset, ulong size);

	[MonoPInvokeCallback(typeof(ResourceWriteCallback))]
	public static ulong OnResourceWrite(IntPtr pathHandler, IntPtr data, ulong offset, ulong size)
	{
		if (data == IntPtr.Zero)
			return 0;
		string path = Marshal.PtrToStringAnsi(pathHandler);
		if (!Application.isPlaying)
		{
#if UNITY_EDITOR
			string	unityPackFx = Path.Combine("Assets/", PKFxSettings.UnityPackFxPath);
			string	finalPath = Path.Combine(unityPackFx, path);

			string dir = Path.GetDirectoryName(finalPath);
			Directory.CreateDirectory(dir);

			FileStream fs = new FileStream(finalPath, FileMode.Create);
			BinaryWriter bw = new BinaryWriter(fs);
			byte[] managedArray = new byte[size];

			Marshal.Copy(data, managedArray, 0, (int)size);
			bw.Write(managedArray);

			bw.Close();
			fs.Close();

			UnityEditor.AssetDatabase.Refresh();

			Debug.Log("[PKFX] Wrote asset to PackFx : " + path);
			return size;
#endif
		}
		else
		{
			Debug.Assert(m_IsStarted == true);
			GameObject go = new GameObject();
			PKFxRuntimeMeshAsset newMesh = go.AddComponent<PKFxRuntimeMeshAsset>();
			newMesh.m_AssetName = path;
			go.name = path;
			newMesh.m_Data = new byte[size];
			Marshal.Copy(data, newMesh.m_Data, 0, (int)size);
			if (m_RuntimeAssetsRoot == null)
			{
				m_RuntimeAssetsRoot = new GameObject("PopcornFX Runtime Assets");
				m_RuntimeAssetsRoot.transform.position = Vector3.zero;
				m_RuntimeAssetsRoot.isStatic = true;
				UnityEngine.Object.DontDestroyOnLoad(m_RuntimeAssetsRoot);
			}
			go.transform.parent = m_RuntimeAssetsRoot.transform;
			m_RuntimeAssets.Add(go);
			return size;
		}
#if !UNITY_EDITOR
		return 0;
#endif
	}

	//----------------------------------------------------------------------------

	public delegate void EffectDependencyFoundCallback(IntPtr dependencyPath, int useInfoFlags);

	[MonoPInvokeCallback(typeof(EffectDependencyFoundCallback))]
	private static void OnEffectDependencyFound(IntPtr dependencyPath, int useInfoFlags)
	{
		if (dependencyPath == IntPtr.Zero)
			return;

		PKFxFxAsset.DependencyDesc depDesc = new PKFxFxAsset.DependencyDesc();

		depDesc.m_Path = PKFxSettings.UnityPackFxPath + "/" + Marshal.PtrToStringAnsi(dependencyPath);
		depDesc.m_IsTextureLinear	= (useInfoFlags & (int)EUseInfoFlag.IsLinearTexture) != 0;
		depDesc.m_IsMeshRenderer	= (useInfoFlags & (int)EUseInfoFlag.IsMeshRenderer) != 0;
		depDesc.m_IsMeshSampler		= (useInfoFlags & (int)EUseInfoFlag.IsMeshSampler) != 0;
		depDesc.m_IsTextureSampler	= (useInfoFlags & (int)EUseInfoFlag.IsTextureSampler) != 0;
		if (depDesc.m_IsMeshRenderer && Path.GetExtension(depDesc.m_Path).CompareTo(".pkmm") == 0)
			depDesc.m_Path = Path.ChangeExtension(depDesc.m_Path, ".fbx");
		else
			depDesc.m_IsMeshRenderer = false;

		// if possible, add an argument to this callback to tell if the dependency will be used for distortion,
		// call PKFxImporter.SRGBEnabledOnTexture to check if the sRGB is enable and log a warning (see in PKFxImporter.InitDependencies)
		int idx = m_CurrentlyImportedAsset.m_Dependencies.FindIndex(x => x.m_Path == depDesc.m_Path);
		if (idx < 0)
		{
			m_CurrentlyImportedAsset.m_Dependencies.Add(depDesc);
		} else
		{
			PKFxFxAsset.DependencyDesc current = m_CurrentlyImportedAsset.m_Dependencies[idx];
			current.m_IsTextureLinear	= current.m_IsTextureLinear ? true : depDesc.m_IsTextureLinear;
			current.m_IsMeshRenderer	= current.m_IsMeshRenderer ? true : depDesc.m_IsMeshRenderer;
			current.m_IsMeshSampler		= current.m_IsMeshSampler ? true : depDesc.m_IsMeshSampler;
			current.m_IsTextureSampler	= current.m_IsTextureSampler ? true : depDesc.m_IsTextureSampler;
		}
	}

	//----------------------------------------------------------------------------

	public delegate void EffectAttributeFoundCallback(IntPtr attributeDescPtr);

	[MonoPInvokeCallback(typeof(EffectAttributeFoundCallback))]
	private static void OnEffectAttributeFound(IntPtr attributeDescPtr)
	{
		if (attributeDescPtr == IntPtr.Zero)
			return;

		SNativeAttributeDesc		nativeAttrDesc = (SNativeAttributeDesc)Marshal.PtrToStructure(attributeDescPtr, typeof(SNativeAttributeDesc));
		PKFxFxAsset.AttributeDesc	attrDesc = new PKFxFxAsset.AttributeDesc(nativeAttrDesc);

		m_CurrentlyImportedAsset.m_AttributeDescs.Add(attrDesc);
	}

	//----------------------------------------------------------------------------

	public delegate void EffectSamplerFoundCallback(IntPtr attributeDescPtr);

	[MonoPInvokeCallback(typeof(EffectSamplerFoundCallback))]
	private static void OnEffectSamplerFound(IntPtr samplerDescPtr)
	{
		if (samplerDescPtr == IntPtr.Zero)
			return;

		SNativeSamplerDesc			nativeSamplerDesc = (SNativeSamplerDesc)Marshal.PtrToStructure(samplerDescPtr, typeof(SNativeSamplerDesc));
		PKFxFxAsset.SamplerDesc		samplerDesc = new PKFxFxAsset.SamplerDesc(nativeSamplerDesc);

		m_CurrentlyImportedAsset.m_SamplerDescs.Add(samplerDesc);
	}

	//----------------------------------------------------------------------------

	public delegate void GetEffectInfoCallback(int usesMeshRenderer);

	[MonoPInvokeCallback(typeof(GetEffectInfoCallback))]
	private static void OnGetEffectInfo(int usesMeshRenderer)
	{
		m_CurrentlyImportedAsset.m_UsesMeshRenderer = usesMeshRenderer != 0;
	}

	//----------------------------------------------------------------------------

	private delegate void FxCallback(int guid);

	[MonoPInvokeCallback(typeof(FxCallback))]
	public static void OnFxStopped(int guid)
	{
		PKFxFX component;

		if (PKFxFX.g_ListEffects.TryGetValue(guid, out component))
		{
			if (component.m_OnFxStopped != null)
				component.m_OnFxStopped(component);
			component.OnFxStopPlaying(guid);
		}
	}

	//----------------------------------------------------------------------------

	public delegate IntPtr AudioCallback(IntPtr channelName, IntPtr nbSamples);

	[MonoPInvokeCallback(typeof(AudioCallback))]
	public static IntPtr OnAudioWaveformData(IntPtr channelName, IntPtr nbSamples)
	{
		AudioListener.GetOutputData(m_Samples, 0);
		return m_SamplesHandle.AddrOfPinnedObject();
	}

	//----------------------------------------------------------------------------

	[MonoPInvokeCallback(typeof(AudioCallback))]
	public static IntPtr OnAudioSpectrumData(IntPtr channelName, IntPtr nbSamples)
	{
		AudioListener.GetSpectrumData(m_Samples, 0, FFTWindow.Rectangular);
		// Last value filled by Unity seems fucked up...
		m_Samples[1023] = m_Samples[1022];
		return m_SamplesHandle.AddrOfPinnedObject();
	}

	//----------------------------------------------------------------------------
	// Billboard

	private delegate int RendererSetupCallback(IntPtr rendererDescPtr);

	[MonoPInvokeCallback(typeof(RendererSetupCallback))]
	public static int OnNewBillboardRendererSetup(IntPtr rendererDescPtr)
	{
		SBatchDesc batchDesc = null;
		
		unsafe
		{
			SPopcornRendererDesc	*desc = (SPopcornRendererDesc*)rendererDescPtr.ToPointer();
			batchDesc = new SBatchDesc(ERendererType.Billboard, *desc);
		}

		// Create the material description:
		Material mat = PKFxSettings.MaterialFactory.ResolveParticleMaterial(batchDesc);

		GameObject renderingObject = GetNewRenderingObject(batchDesc.m_GeneratedName);
		return SetupRenderingObject(renderingObject, batchDesc, mat);
	}

	//----------------------------------------------------------------------------
	// Ribbon

	[MonoPInvokeCallback(typeof(RendererSetupCallback))]
	public static int OnNewRibbonRendererSetup(IntPtr rendererDescPtr)
	{
		SBatchDesc batchDesc = null;

		unsafe
		{
			SPopcornRendererDesc	*desc = (SPopcornRendererDesc*)rendererDescPtr.ToPointer();
			batchDesc = new SBatchDesc(ERendererType.Ribbon, *desc);
		}

		// Create the material description:
		Material mat = PKFxSettings.MaterialFactory.ResolveParticleMaterial(batchDesc);

		GameObject renderingObject = GetNewRenderingObject(batchDesc.m_GeneratedName);
		return SetupRenderingObject(renderingObject, batchDesc, mat);
	}

	//----------------------------------------------------------------------------
	// Mesh

	[MonoPInvokeCallback(typeof(RendererSetupCallback))]
	public static int OnNewMeshRendererSetup(IntPtr rendererDescPtr)
	{
		SBatchDesc	batchDesc = null;
		
		unsafe
		{
			SMeshRendererDesc			*desc = (SMeshRendererDesc*)rendererDescPtr.ToPointer();
		
			batchDesc = new SBatchDesc(*desc);
		}
		
		// Create the material description:
		Material mat = PKFxSettings.MaterialFactory.ResolveParticleMaterial(batchDesc);
		
		mat.enableInstancing = true;
		GameObject renderingObject = GetNewRenderingObject(batchDesc.m_GeneratedName);

		return SetupMeshRenderingObject(renderingObject, batchDesc, mat);
	}

	//----------------------------------------------------------------------------

	private delegate void RendererNamesAddedCallback(int rendererGUID, IntPtr effectNames);

	[MonoPInvokeCallback(typeof(RendererNamesAddedCallback))]
	public static void OnRendererNamesAdded(int rendererGUID, IntPtr effectNames)
	{
		string effectNamesStr = null;

		if (effectNames != IntPtr.Zero)
			effectNamesStr = Marshal.PtrToStringAnsi(effectNames);

		SBatchDesc batchDesc = m_Renderers[rendererGUID].m_BatchDesc;

		Debug.Assert(m_Renderers.Count > rendererGUID);
		batchDesc.m_EffectNames += effectNamesStr;

		string particleMeshGeneratedName = batchDesc.m_GeneratedName;
		PKFxSettings.SParticleMeshDefaultSize meshSizeToUpdate = GetParticleMeshDefaultSizeSettings(particleMeshGeneratedName);

		if (meshSizeToUpdate != null)
		{
			meshSizeToUpdate.m_EffectNames = batchDesc.m_EffectNames;
		}
	}

	//----------------------------------------------------------------------------

	private delegate bool RendererResizeCallback(int rendererGUID, int vertexCount, int indexCount, int growLarger);

	[MonoPInvokeCallback(typeof(RendererResizeCallback))]
	public static bool OnRendererResize(int rendererGUID, int vertexCount, int indexCount, int growLarger)
	{
		Debug.Assert(m_Renderers.Count > rendererGUID);
		Debug.Assert(indexCount % 3 == 0);

		SMeshDesc renderer = m_Renderers[rendererGUID];
		MeshFilter filter = renderer.m_Slice;

		Debug.Assert(filter != null);

		// Load the default size configuration
		string particleMeshGeneratedName = renderer.m_BatchDesc.m_GeneratedName;
		PKFxSettings.SParticleMeshDefaultSize meshSizeToUpdate = GetParticleMeshDefaultSizeSettings(particleMeshGeneratedName);

		uint usedVertexCount = (uint)vertexCount;
		uint usedIndexCount = (uint)indexCount;
		uint reserveVertexCount = usedVertexCount;
		uint reserveIndexCount = usedIndexCount;

		Bounds initBounds = new Bounds();

		// If we need to grow buffer but did not find a setting for the buffer size
		if (growLarger == 1)
		{
			reserveVertexCount += (uint)(usedVertexCount * PKFxSettings.VertexBufferSizeMultiplicator);
			reserveIndexCount += (uint)(usedIndexCount * PKFxSettings.IndexBufferSizeMultiplicator);
			reserveIndexCount += 3 - (reserveIndexCount % 3);

			if (meshSizeToUpdate == null) // Did not find a setting for this buffer
			{
#if UNITY_EDITOR
				if (PKFxSettings.AutomaticMeshResizing)
				{
					// If in editor mode, we register the size of the buffers in a new particle mesh default size entry
					meshSizeToUpdate = new PKFxSettings.SParticleMeshDefaultSize();

					meshSizeToUpdate.m_GeneratedName = particleMeshGeneratedName;
					meshSizeToUpdate.m_EffectNames = renderer.m_BatchDesc.m_EffectNames;
					meshSizeToUpdate.m_DefaultVertexBufferSize = (int)reserveVertexCount;
					meshSizeToUpdate.m_DefaultIndexBufferSize = (int)reserveIndexCount;
					PKFxSettings.MeshesDefaultSize.Add(meshSizeToUpdate);
				}
#endif
			}
			else // We did find a setting for this buffer
			{
				if (usedVertexCount < meshSizeToUpdate.m_DefaultVertexBufferSize) // The registered
				{
					reserveVertexCount = (uint)meshSizeToUpdate.m_DefaultVertexBufferSize; // Otherwise, just update the reserved count
					usedVertexCount = reserveVertexCount; // Forces the mesh to get resized to this size if we found a config
				}
#if UNITY_EDITOR
				else if (PKFxSettings.AutomaticMeshResizing)
					meshSizeToUpdate.m_DefaultVertexBufferSize = (int)reserveVertexCount; // Update the settings IFN
#endif

				if (usedIndexCount < meshSizeToUpdate.m_DefaultIndexBufferSize)
				{
					reserveIndexCount = (uint)meshSizeToUpdate.m_DefaultIndexBufferSize; // Otherwise, just update the reserved count
					usedIndexCount = reserveIndexCount; // Forces the mesh to get resized to this size if we found a config
				}
#if UNITY_EDITOR
				else if (PKFxSettings.AutomaticMeshResizing)
					meshSizeToUpdate.m_DefaultIndexBufferSize = (int)reserveIndexCount; // Update the settings IFN
#endif
				initBounds = meshSizeToUpdate.m_StaticWorldBounds;
			}
		}

		// We choose to use large indices or not here:
		bool useLargeIndices = reserveVertexCount > UInt16.MaxValue;

		Mesh mesh = filter.mesh;

		mesh.bounds = initBounds;

		bool ok = ResizeParticleMeshBuffer(	mesh,
											renderer,
											usedVertexCount,
											usedIndexCount,
											reserveVertexCount,
											reserveIndexCount,
											useLargeIndices);
		return ok;
	}
	
	//----------------------------------------------------------------------------

	private static bool ResizeParticleMeshBuffer(	Mesh mesh,
													SMeshDesc renderer,
													uint usedVertexCount,
													uint usedIndexCount,
													uint reservedVertexCount,
													uint reservedIndexCount,
													bool useLargeIdx)
	{
		bool hasBeenResized = false;

		if (mesh.vertexCount < usedVertexCount)
		{
			// We only do the transition from uint16 to uint32 because the transition clear the index buffer...
			if (useLargeIdx == true && mesh.indexFormat == IndexFormat.UInt16)
				mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			else if (useLargeIdx == false && mesh.indexFormat == IndexFormat.UInt32)
				mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;

			mesh.Clear();

			mesh.vertices = new Vector3[reservedVertexCount];       // positions

			if (renderer.HasMaterialFlag(EMaterialFlags.Has_Color))
			{
				mesh.colors = new Color[reservedVertexCount];       // color
			}
			if (renderer.HasMaterialFlag(EMaterialFlags.Has_Lighting))
			{
				mesh.normals = new Vector3[reservedVertexCount];    // normal
			}
			if (renderer.HasMaterialFlag(EMaterialFlags.Has_RibbonComplex))
			{
				mesh.uv = new Vector2[reservedVertexCount];         // uvFactors
				mesh.uv2 = new Vector2[reservedVertexCount];        // uvScale
				mesh.uv3 = new Vector2[reservedVertexCount];        // uvOffset
				if (renderer.HasMaterialFlag(EMaterialFlags.Has_AlphaRemap))
				{
					mesh.uv4 = new Vector2[reservedVertexCount];    // alpha cursor
				}
			}
			else if (renderer.HasMaterialFlag(EMaterialFlags.Has_AnimBlend))
			{
				mesh.uv = new Vector2[reservedVertexCount];     // uv0
				mesh.uv2 = new Vector2[reservedVertexCount];    // uv1
				mesh.uv3 = new Vector2[reservedVertexCount];    // atlas id and if Has_AlphaRemap, alpha cursor
			}
			else
			{
				if (renderer.HasMaterialFlag(EMaterialFlags.Has_Diffuse))
				{
					mesh.uv = new Vector2[reservedVertexCount];     // uv0
				}
				if (renderer.HasMaterialFlag(EMaterialFlags.Has_AlphaRemap))
				{
					mesh.uv2 = new Vector2[reservedVertexCount];    // alpha cursor
				}
			}
			hasBeenResized = true;
		}

		if (mesh.GetIndexCount(0) < usedIndexCount)
		{
			int[] triangles = new int[reservedIndexCount];           // index
																	 //fix to set the right vertex buffer size on PS4 : fill the index buffer with vertex ids
			if (Application.platform == RuntimePlatform.PS4)
			{
				for (int i = 0; i < mesh.vertexCount; ++i)
					triangles[i] = i;
			}
			mesh.triangles = triangles;
			hasBeenResized = true;
		}

		return hasBeenResized;
	}

	//----------------------------------------------------------------------------

	private delegate void SetRendererActiveCallback(int rendererGUID, bool active);

	[MonoPInvokeCallback(typeof(SetRendererActiveCallback))]
	public static void OnSetRendererActive(int rendererGUID, bool active)
	{
		Debug.Assert(m_Renderers.Count > rendererGUID);
		SMeshDesc renderer = m_Renderers[rendererGUID];
		renderer.m_RenderingObject.SetActive(active);
	}

	//----------------------------------------------------------------------------

	private delegate void SetMeshInstancesBufferCallback(int rendererGUID, IntPtr instancesBuffer);

	[MonoPInvokeCallback(typeof(SetMeshInstancesBufferCallback))]
	public static void OnSetMeshInstancesBuffer(int rendererGUID, IntPtr instancesBuffer)
	{
		Debug.Assert(m_Renderers[rendererGUID].m_InstancesRenderer != null);
		m_Renderers[rendererGUID].m_InstancesRenderer.SetInstanceBuffer(instancesBuffer);
	}

	//----------------------------------------------------------------------------

	private delegate void SetMeshInstancesCountCallback(int rendererGUID, int instancesCount);

	[MonoPInvokeCallback(typeof(SetMeshInstancesCountCallback))]
	public static void OnSetMeshInstancesCount(int rendererGUID, int instancesCount)
	{
		Debug.Assert(m_Renderers[rendererGUID].m_InstancesRenderer != null);
		m_Renderers[rendererGUID].m_InstancesRenderer.SetInstanceCount(instancesCount);
	}

	//----------------------------------------------------------------------------

	private delegate void RetrieveRendererBufferInfoCallback(int rendererGUID, ref SRetrieveRendererInfo rendererInfo);

	[MonoPInvokeCallback(typeof(RetrieveRendererBufferInfoCallback))]
	public static void OnRetrieveRendererBufferInfo(int rendererGUID, ref SRetrieveRendererInfo rendererInfo)
	{
		unsafe
		{
			int		*isIdx32 = (int*)rendererInfo.m_IsIndex32.ToPointer();
			int		*vertexBufferSize = (int*)rendererInfo.m_VertexBufferSize.ToPointer();
			int		*indexBufferSize = (int*)rendererInfo.m_IndexBufferSize.ToPointer();
			IntPtr	*vbHander = (IntPtr*)rendererInfo.m_VBHandler.ToPointer();
			IntPtr	*ibHander = (IntPtr*)rendererInfo.m_IBHandler.ToPointer();

			Mesh	currentRendererMesh = m_Renderers[rendererGUID].m_Slice.mesh;

			int		indexCount = (int)currentRendererMesh.GetIndexCount(0);
			int		vertexCount = currentRendererMesh.vertexCount;

			if (isIdx32 != null)
				*isIdx32 = currentRendererMesh.indexFormat == IndexFormat.UInt32 ? 1 : 0;
			if (vertexBufferSize != null)
				*vertexBufferSize = vertexCount;
			if (indexBufferSize != null)
				*indexBufferSize = indexCount;
			if (vertexCount > 0 || indexCount > 0)
			{
				if (vbHander != null)
					*vbHander = currentRendererMesh.GetNativeVertexBufferPtr(0);
				if (ibHander != null)
					*ibHander = currentRendererMesh.GetNativeIndexBufferPtr();
			}
			else
			{
				if (vbHander != null)
					*vbHander = IntPtr.Zero;
				if (ibHander != null)
					*ibHander = IntPtr.Zero;
			}
		}
	}

	//----------------------------------------------------------------------------

	private delegate void RendererBoundsUpdateCallback(int rendererGUID, ref SUpdateRendererBounds bounds);

	[MonoPInvokeCallback(typeof(RendererBoundsUpdateCallback))]
	public static void OnRendererBoundsUpdate(int rendererGUID, ref SUpdateRendererBounds bounds)
	{
		if (rendererGUID >= m_Renderers.Count)
			return;
		Bounds b = new Bounds();
		b.min = new Vector3(bounds.m_MinX, bounds.m_MinY, bounds.m_MinZ);
		b.max = new Vector3(bounds.m_MaxX, bounds.m_MaxY, bounds.m_MaxZ);

#if UNITY_EDITOR
		if (PKFxSettings.DebugEffectsBoundingBoxes)
		{
			Color boundsColor = m_Renderers[rendererGUID].m_BoundsDebugColor;

			Debug.DrawLine(new Vector3(bounds.m_MinX, bounds.m_MinY, bounds.m_MinZ), new Vector3(bounds.m_MaxX, bounds.m_MinY, bounds.m_MinZ), boundsColor);
			Debug.DrawLine(new Vector3(bounds.m_MinX, bounds.m_MinY, bounds.m_MinZ), new Vector3(bounds.m_MinX, bounds.m_MaxY, bounds.m_MinZ), boundsColor);
			Debug.DrawLine(new Vector3(bounds.m_MinX, bounds.m_MinY, bounds.m_MinZ), new Vector3(bounds.m_MinX, bounds.m_MinY, bounds.m_MaxZ), boundsColor);
			Debug.DrawLine(new Vector3(bounds.m_MaxX, bounds.m_MaxY, bounds.m_MaxZ), new Vector3(bounds.m_MinX, bounds.m_MaxY, bounds.m_MaxZ), boundsColor);
			Debug.DrawLine(new Vector3(bounds.m_MaxX, bounds.m_MaxY, bounds.m_MaxZ), new Vector3(bounds.m_MaxX, bounds.m_MinY, bounds.m_MaxZ), boundsColor);
			Debug.DrawLine(new Vector3(bounds.m_MaxX, bounds.m_MaxY, bounds.m_MaxZ), new Vector3(bounds.m_MaxX, bounds.m_MaxY, bounds.m_MinZ), boundsColor);
			Debug.DrawLine(new Vector3(bounds.m_MinX, bounds.m_MaxY, bounds.m_MaxZ), new Vector3(bounds.m_MinX, bounds.m_MinY, bounds.m_MaxZ), boundsColor);
			Debug.DrawLine(new Vector3(bounds.m_MinX, bounds.m_MaxY, bounds.m_MaxZ), new Vector3(bounds.m_MinX, bounds.m_MaxY, bounds.m_MinZ), boundsColor);
			Debug.DrawLine(new Vector3(bounds.m_MaxX, bounds.m_MinY, bounds.m_MinZ), new Vector3(bounds.m_MaxX, bounds.m_MaxY, bounds.m_MinZ), boundsColor);
			Debug.DrawLine(new Vector3(bounds.m_MaxX, bounds.m_MinY, bounds.m_MinZ), new Vector3(bounds.m_MaxX, bounds.m_MinY, bounds.m_MaxZ), boundsColor);
			Debug.DrawLine(new Vector3(bounds.m_MinX, bounds.m_MaxY, bounds.m_MinZ), new Vector3(bounds.m_MaxX, bounds.m_MaxY, bounds.m_MinZ), boundsColor);
			Debug.DrawLine(new Vector3(bounds.m_MaxX, bounds.m_MinY, bounds.m_MaxZ), new Vector3(bounds.m_MinX, bounds.m_MinY, bounds.m_MaxZ), boundsColor);
		}
#endif
		m_Renderers[rendererGUID].m_Slice.mesh.bounds = b;
	}

	//----------------------------------------------------------------------------

#if UNITY_EDITOR
	private delegate void AssetChangeCallback(ref SAssetChangesDesc assetChange);

	[MonoPInvokeCallback(typeof(AssetChangeCallback))]
	public static void OnAssetChange(ref SAssetChangesDesc assetChange)
	{
		PKFxManager.ImportedAssetName = Path.GetFileName(assetChange.m_Path);
		string relativepath	= "Assets" + PKFxSettings.UnityPackFxPath;
		string assetPath = assetChange.m_Path + ".asset";

		LockPackWatcherChanges();
		if (assetChange.m_Type == (int)EAssetChangesType.Add)
		{
			PKFxAssetCreationUtils.CreatePKFXAsset(assetChange);
		}
		else if (assetChange.m_Type == (int)EAssetChangesType.Remove)
		{
			AssetDatabase.DeleteAsset(Path.Combine(relativepath, assetPath));
		}
		else if (assetChange.m_Type == (int)EAssetChangesType.Update)
		{
			PKFxFxAsset fxAsset = (PKFxFxAsset)AssetDatabase.LoadAssetAtPath(Path.Combine(relativepath, assetPath), typeof(PKFxFxAsset));
			if (fxAsset != null)
			{
				fxAsset.Clean();
				fxAsset.m_Data = File.ReadAllBytes("Temp/PopcornFx/Baked/" + assetChange.m_Path);
				PKFxAssetCreationUtils.UpdatePKFXAsset(fxAsset, assetChange.m_Path);
			}
			else
			{
				PKFxAssetCreationUtils.CreatePKFXAsset(assetChange);
			}
		}
		else if (assetChange.m_Type == (int)EAssetChangesType.Rename)
		{
			AssetDatabase.RenameAsset(Path.Combine(relativepath, assetChange.m_PathOld + ".asset"), Path.GetFileName(assetChange.m_Path));
			PKFxFxAsset fxAsset = (PKFxFxAsset)AssetDatabase.LoadAssetAtPath(Path.Combine(relativepath, assetPath), typeof(PKFxFxAsset));

			if (fxAsset != null)
			{
				fxAsset.Clean();
				fxAsset.m_Data = File.ReadAllBytes("Temp/PopcornFx/Baked/" + assetChange.m_Path);
				PKFxAssetCreationUtils.UpdateAndRenamePKFXAsset(fxAsset, assetChange.m_PathOld, assetChange.m_Path);
			}
			else
			{
				PKFxAssetCreationUtils.CreatePKFXAsset(assetChange);
			}
		}
		UnlockPackWatcherChanges();
	}

	private delegate void ImportErrorCallback(ref SAssetImportError importError);

	[MonoPInvokeCallback(typeof(ImportErrorCallback))]
	public static void OnImportError(ref SAssetImportError importError)
	{
		if (importError.m_Path.Length + Path.GetFullPath(PKFxSettings.PopcornPackFxPath).Length > MAX_PATH)
			Debug.LogError("[PKFX] Failed to Bake \"" + importError.m_Path + "\". " + "The path of your file is over the limit of " + MAX_PATH + " set by windows on some systems.");
		else
			Debug.LogError("[PKFX] Failed to Bake \"" + importError.m_Path + "\". " + importError.m_ErrorMessage);
	}

	private delegate void GetAllAssetPathCallback([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]IntPtr[] pathArray, int size);

	[MonoPInvokeCallback(typeof(GetAllAssetPathCallback))]
	public static void OnGetAllAssetPath([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] IntPtr[] pathArray, int size)
	{
		List<string> paths = new List<string>(size);

		for (int i = 0; i < size; ++i)
		{
			paths.Add(Marshal.PtrToStringAnsi(pathArray[i]));
		}
		PKFxSettings.AssetPathList = paths;
	}

#endif
}