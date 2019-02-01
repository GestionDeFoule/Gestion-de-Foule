//----------------------------------------------------------------------------
// Created on Thu Oct 10 16:25:15 2013 Raphael Thoulouze
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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


//----------------------------------------------------------------------------a

public class PKFxRenderingPlugin : PKFxCamera
{
	// Exposed in "Advanced" Editor
	[Tooltip("Loads a user-defined mesh to be used for particles world collisions.")][HideInInspector]
	public bool						m_UseSceneMesh = false;
	[Tooltip("Link to the pre-built mesh asset.")][HideInInspector]
	public PKFxMeshAsset			m_SceneMesh;

	//----------------------------------------------------------------------------

	void	Start()
	{
		if (m_EnableDistortion && !SystemInfo.supportsImageEffects)
		{
			Debug.LogWarning("[PKFX] Image effects not supported, distortions disabled.", this);
			m_EnableDistortion = false;
		}
		if (m_UseSceneMesh && m_SceneMesh != null && !PKFxSettings.EnableRaycastForCollisions)
		{
			PKFxManager.SetSceneMesh(m_SceneMesh);
		}
	}

#if !UNITY_EDITOR
	private void OnApplicationQuit()
	{
		PKFxManager.ShutdownPopcorn();
	}
#endif
}
