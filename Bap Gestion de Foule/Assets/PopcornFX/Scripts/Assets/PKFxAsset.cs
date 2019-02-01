using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PKFxAsset : ScriptableObject
{
	[HideInInspector]
	public byte[] _data;

	[HideInInspector]
	public virtual byte[] m_Data
	{
		get { return _data; }
		set { _data = value; }
	}
	[HideInInspector]
	public string m_AssetName = "";

	public virtual void			Clean()
	{
		_data = null;
	}
}
