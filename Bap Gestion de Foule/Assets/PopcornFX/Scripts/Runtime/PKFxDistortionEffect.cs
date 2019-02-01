//----------------------------------------------------------------------------
// Created on Tue Jul 8 17:59:09 2014 Raphael Thoulouze
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

internal class PKFxDistortionEffect : MonoBehaviour {
	
	public Material			m_MaterialDistortion;
	public Material			m_MaterialBlur;
	public float			m_BlurFactor = 0f;
	public RenderTexture	m_DistortionRT;
	private RenderTexture	m_TmpRT;
	
	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		if (m_DistortionRT == null || !m_DistortionRT.IsCreated() || m_MaterialDistortion == null)
		{
			Graphics.Blit(source, destination);
			Debug.LogError("[PKFX] Distortion effect setup failed.", this);
			return ;
		}
		if (m_MaterialBlur == null)
		{
			m_MaterialDistortion.SetTexture("_DistortionTex", m_DistortionRT);
			Graphics.Blit(source, destination, m_MaterialDistortion);
			return ;
		}
		m_MaterialBlur.SetTexture("_DistortionTex", m_DistortionRT);
		m_MaterialBlur.SetFloat("_BlurFactor", m_BlurFactor);
		m_MaterialDistortion.SetTexture("_DistortionTex", m_DistortionRT);
		m_TmpRT = RenderTexture.GetTemporary(source.width, source.height, source.volumeDepth, source.format);

		Graphics.Blit(source, m_TmpRT, m_MaterialDistortion);
		Graphics.Blit(m_TmpRT, destination, m_MaterialBlur);

		RenderTexture.ReleaseTemporary(m_TmpRT);
	}
}
