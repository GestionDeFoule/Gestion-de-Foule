using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PKFxAtlasAsset : PKFxAsset
{
	[HideInInspector]
	public string m_FXText = "";
	[HideInInspector]
	public override byte[] m_Data
	{
		get { return _data; }
		set
		{
			_data = value;
			m_FXText = System.Text.Encoding.ASCII.GetString(_data);
		}
	}
}
