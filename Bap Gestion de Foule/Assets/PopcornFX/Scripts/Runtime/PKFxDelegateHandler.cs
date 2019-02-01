using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;


public class PKFxDelegateHandler
{
	private static PKFxDelegateHandler g_Instance;

	private PKFxDelegateHandler()
	{
	}

	~PKFxDelegateHandler()
	{
		PKFxManager.ClearAllCallbacks();
	}

	private static List<object> g_DelegatesForRefKeeping = new List<object>();

	public IntPtr DelegateToFunctionPointer(Delegate del)
	{
		g_DelegatesForRefKeeping.Add(del);
		return Marshal.GetFunctionPointerForDelegate(del);
	}

	public void DiscardAllDelegatesRefs()
	{
		g_DelegatesForRefKeeping.Clear();
	}

	public static PKFxDelegateHandler Instance
	{
		get
		{
			if (g_Instance == null)
			{
				g_Instance = new PKFxDelegateHandler();
			}
			return g_Instance;
		}
	}
}
