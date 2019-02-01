//----------------------------------------------------------------------------
// Created on Wed Jun 3 18:48:15 2015 Valentin Bas
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

public class PkFxProfilerCaptureButton : MonoBehaviour
{
	public int		FrameCountToCapture = 10;

	private bool	m_InCapture = false;
	private int		m_FrameCaptured = 0;

	void OnGUI()
	{
		if (!m_InCapture && GUI.Button(new Rect(10, 10, 500, 150), "Profiler capture"))
		{
			m_InCapture = true;
			PKFxManager.ProfilerEnable(true);
		}
	}

	//----------------------------------------------------------------------------

	void Update()
	{
		if (m_InCapture)
		{
			m_FrameCaptured++;
			if (m_FrameCaptured >= FrameCountToCapture)
			{
				m_FrameCaptured = 0;
				PKFxManager.ProfilerWriteReport(Application.persistentDataPath + "/ProfileReport.pkpr");
				Debug.Log("[PKFX] Profiling report written to " + Application.persistentDataPath + "/ProfileReport.pkpr");
				m_InCapture = false;
				PKFxManager.ProfilerEnable(false);
			}
		}
	}
}
