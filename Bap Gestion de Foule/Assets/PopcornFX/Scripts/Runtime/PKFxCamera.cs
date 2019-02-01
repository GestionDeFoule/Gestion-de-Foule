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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using System;

public class PKFxCamera : MonoBehaviour
{
	// Static data
	public static short 			g_CameraUID = 0;
	private static int				g_LastFrameCount = -1;
	public static Material			g_DistortionMat;
	public static Material			g_BlurMat;

	public static short				GetUniqueID
	{
		get { return g_CameraUID++; }
	}

	// Instance data
	[Tooltip("Specifies the particles textures level-of-detail bias.")][HideInInspector]
	protected short						m_CameraID = 0;
	protected short						m_VRReservedID = 0;
	protected short						m_CurrentCameraID = 0;
	protected Camera					m_Camera;
	protected PKFxManagerImpl.SCamDesc	m_CameraDescription;
	protected uint						m_CurrentFrameID = 0;
	protected uint						m_LastUpdateFrameID = 0;

	private RenderTexture			m_DistortionRt;

	private CommandBuffer			m_CommandBuffer;

	[Tooltip("Enables the distortion particles material, adding a postFX pass.")]
	[HideInInspector]
	public bool m_EnableDistortion = false;
	[Tooltip("Enables the distortion blur pass, adding another postFX pass.")]
	[HideInInspector]
	public bool m_EnableBlur = false;
	[Tooltip("Blur factor. Adjusts the blur's spread.")]
	[HideInInspector]
	public float m_BlurFactor = 0.2f;

	//----------------------------------------------------------------------------

	private bool _updated = false; // Used only when PKFxManager.m_HasSRP is true

	//----------------------------------------------------------------------------

	public int RenderPass
	{
		get { return m_CameraDescription.m_RenderPass; }
		set { m_CameraDescription.m_RenderPass = value; }
	}

	//----------------------------------------------------------------------------
	
	bool SetupDistortionPassIFN()
	{
		if (PKFxSettings.EnableSoftParticles == false)
			return false;
		if (m_CommandBuffer == null)
		{
			m_CommandBuffer = new CommandBuffer();
			if (m_CommandBuffer == null)
				return false;
			m_CommandBuffer.name = "PopcornFX Distortion and Blur";
			m_Camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, m_CommandBuffer);
			m_DistortionRt = new RenderTexture(m_Camera.pixelWidth, m_Camera.pixelHeight, 0, RenderTextureFormat.DefaultHDR);
			if (m_DistortionRt == null)
				return false;
		}
		if (m_EnableDistortion)
		{
			if (g_DistortionMat == null)
			{
				g_DistortionMat = new Material(Shader.Find("PopcornFX/PKFxDistortion"));
				if (g_DistortionMat == null)
					return false;
				g_DistortionMat.SetTexture("_DistortionTex", m_DistortionRt);
			}
		}
		if (m_EnableBlur)
		{
			if (g_BlurMat == null)
			{
				g_BlurMat = new Material(Shader.Find("PopcornFX/PKFxBlur"));
				if (g_BlurMat == null)
					return false;
				g_BlurMat.SetTexture("_DistortionTex", m_DistortionRt);
				g_BlurMat.SetFloat("_BlurFactor", m_BlurFactor);
			}
		}
		return true;
	}

	void Awake()
	{
		PKFxManager.StartupPopcorn(true);
		m_CameraID = GetUniqueID;
		m_CurrentCameraID = m_CameraID;
#if !UNITY_SWITCH && !UNITY_XBOXONE
		if (Application.platform != RuntimePlatform.IPhonePlayer && UnityEngine.XR.XRSettings.enabled && UnityEngine.XR.XRDevice.isPresent)
			m_VRReservedID = GetUniqueID;
#endif
		m_Camera = GetComponent<Camera>();
		// We disable the rendering of the distortion objects, this is going to be handled in a command buffer:
		m_Camera.cullingMask &= ~(1 << PKFxManagerImpl.m_DistortionLayer);
        //Enable depth texture on mobile
		if (PKFxSettings.EnableSoftParticles)
			m_Camera.depthTextureMode = DepthTextureMode.Depth;
		PKFxSoundManager.ClearSounds();
	}

	//----------------------------------------------------------------------------

	void UpdateFrame()
	{
		m_CameraDescription.m_ViewportWidth = (uint)m_Camera.pixelWidth;
		m_CameraDescription.m_ViewportHeight = (uint)m_Camera.pixelHeight;
		m_CameraDescription.m_NearClip = m_Camera.nearClipPlane;
		m_CameraDescription.m_FarClip = m_Camera.farClipPlane;

		float frameDt = Time.smoothDeltaTime * PKFxSettings.TimeMultiplier;

		PKFxManager.UpdateLogic(frameDt);

		if (!m_Camera.stereoEnabled)
		{
			UpdateViewMatrix();
			UpdateProjectionMatrix();
			m_CurrentCameraID = m_CameraID;
			PKFxManager.UpdateCamera(m_CurrentCameraID, ref m_CameraDescription);
		}
		else
		{
			// stereo-cam's first eye.
			UpdateViewMatrix(true, Camera.StereoscopicEye.Left);
			UpdateProjectionMatrix(true, Camera.StereoscopicEye.Left);
			m_CurrentCameraID = m_CameraID;
			PKFxManager.UpdateCamera(m_CurrentCameraID, ref m_CameraDescription);
			// second eye.
			UpdateViewMatrix(true, Camera.StereoscopicEye.Right);
			UpdateProjectionMatrix(true, Camera.StereoscopicEye.Right);
			m_CurrentCameraID = m_VRReservedID;
			PKFxManager.UpdateCamera(m_CurrentCameraID, ref m_CameraDescription);
		}
	}

	//----------------------------------------------------------------------------

	void UpdateViewMatrix(bool isVR = false, Camera.StereoscopicEye eye = Camera.StereoscopicEye.Left)
	{
		if (!isVR)
			m_CameraDescription.m_ViewMatrix = m_Camera.worldToCameraMatrix;
		else
			m_CameraDescription.m_ViewMatrix = m_Camera.GetStereoViewMatrix(eye);
	}

	//----------------------------------------------------------------------------

	void UpdateProjectionMatrix(bool isVR = false, Camera.StereoscopicEye eye = Camera.StereoscopicEye.Left)
	{
		Matrix4x4 projectionMatrix;
		if (!isVR)
			projectionMatrix = m_Camera.projectionMatrix;
		else
			projectionMatrix = m_Camera.GetStereoProjectionMatrix(eye);

		m_CameraDescription.m_ProjectionMatrix = GL.GetGPUProjectionMatrix(projectionMatrix, false);
	}

	//----------------------------------------------------------------------------

	void Update()
	{
		m_CurrentFrameID++;
		UpdateFrame();

		if (m_EnableDistortion || m_EnableBlur)
		{
			if (SetupDistortionPassIFN())
			{
				m_CommandBuffer.Clear();
				// We start by binding the distortion render target and we render all the distortion particles in it:
				m_CommandBuffer.SetRenderTarget(m_DistortionRt);
				m_CommandBuffer.ClearRenderTarget(false, true, Color.black);
				foreach (PKFxManagerImpl.SMeshDesc desc in PKFxManagerImpl.m_Renderers)
				{
					if (desc.HasMaterialFlag(PKFxManagerImpl.EMaterialFlags.Has_Distortion))
					{
						if (desc.m_RenderingObject.activeInHierarchy)
						{
							m_CommandBuffer.DrawMesh(desc.m_Slice.mesh, Matrix4x4.identity, desc.m_Material);
						}
					}
				}
				// Now we get a tmp render target to blur the camera RT:
				int tmpRT = Shader.PropertyToID("TemporaryRT");
				m_CommandBuffer.GetTemporaryRT(tmpRT, m_Camera.pixelWidth, m_Camera.pixelHeight);

				if (m_EnableBlur && m_EnableDistortion == false)
					m_CommandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, tmpRT, g_BlurMat);
				else
					m_CommandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, tmpRT, g_DistortionMat);
				if (m_EnableDistortion && m_EnableBlur)
					m_CommandBuffer.Blit(tmpRT, BuiltinRenderTextureType.CameraTarget, g_BlurMat);
				else
					m_CommandBuffer.Blit(tmpRT, BuiltinRenderTextureType.CameraTarget);
			}
		}
		else if (m_CommandBuffer != null)
		{
			m_Camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, m_CommandBuffer);
			m_CommandBuffer = null;
		}
		_updated = true;
	}

	//----------------------------------------------------------------------------

	void LateUpdate()
	{
		if (g_LastFrameCount != Time.frameCount)
		{
			if (!PKFxSettings.SplitUpdateInComponents)
			{
				PKFxManager.UpdateParticles();
			}

			g_LastFrameCount = Time.frameCount;
			if (_updated)
			{
				_updated = false;
				_Render();
			}
		}
		PKFxSoundManager.DeleteSoundsIFN();
	}

	//----------------------------------------------------------------------------

	void _Render()
	{
		if (m_CurrentFrameID != m_LastUpdateFrameID)
		{
			m_CurrentCameraID = m_CameraID;
			m_LastUpdateFrameID = m_CurrentFrameID;
		}
		else
		{
			m_CurrentCameraID = m_VRReservedID;
		}
		PKFxManager.Render(m_CurrentCameraID);
	}

}
