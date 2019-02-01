//----------------------------------------------------------------------------
// Created on Fri Jan 12 18:35:30 2017 Maxime Dumas
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
using UnityEngine.Events;
using UnityEngine.Serialization;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Collections.Generic;
using AOT;

public class PKFxEventManager : MonoBehaviour
{
	public delegate void StartEventDelegate(string eventName, Vector3 position);

	private static StartEventDelegate	 g_OnStartEventDelegate = null;

	//----------------------------------------------------------------------------

	public delegate void StartEventCallback(IntPtr eventName, uint count, IntPtr positions);

	[MonoPInvokeCallback(typeof(StartEventCallback))]
	public static void OnRaiseEvent(IntPtr eventName, uint count, IntPtr positions)
	{
		if (g_OnStartEventDelegate == null)
			return;
		string strEventName = Marshal.PtrToStringAnsi(eventName);
		unsafe // This requires to enable unsafe code in Unity player's settings
		{
			Vector3* particlePositions = (Vector3*)positions.ToPointer();

			for (uint i = 0; i < count; ++i)
			{
				g_OnStartEventDelegate(strEventName, particlePositions[i]);
			}
		}
	}

	//----------------------------------------------------------------------------

	public static void RegisterCustomHandler(StartEventDelegate customDelegate)
	{
		g_OnStartEventDelegate = customDelegate;
	}
}
