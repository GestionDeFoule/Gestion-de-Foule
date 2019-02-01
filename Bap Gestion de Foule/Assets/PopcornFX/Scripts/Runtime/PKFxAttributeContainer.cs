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

public class PKFxAttributesContainer
{
	private bool		m_WasModified;
	private IntPtr		m_AttributesBuffer;
	private PKFxFxAsset m_FxAsset;
	private int			m_FxGuid;

	public PKFxAttributesContainer(PKFxFxAsset asset, int fxId)
	{
		m_WasModified = false;
		m_FxAsset = asset;
		m_FxGuid = fxId;
		m_AttributesBuffer = PKFxManager.GetAttributesBuffer(fxId);
	}

	#region SetAttributeSafe from name

	public void SetAttributeSafe(string name, float valueX)
	{
		int attribId = m_FxAsset.AttributeIdFromName(name);

		if (attribId != -1)
		{
			SetAttributeSafe(attribId, valueX);
		}
	}

	public void SetAttributeSafe(string name, float valueX, float valueY)
	{
		int attribId = m_FxAsset.AttributeIdFromName(name);

		if (attribId != -1)
		{
			SetAttributeSafe(attribId, valueX, valueY);
		}
	}

	public void SetAttributeSafe(string name, float valueX, float valueY, float valueZ)
	{
		int attribId = m_FxAsset.AttributeIdFromName(name);

		if (attribId != -1)
		{
			SetAttributeSafe(attribId, valueX, valueY, valueZ);
		}
	}

	public void SetAttributeSafe(string name, float valueX, float valueY, float valueZ, float valueW)
	{
		int attribId = m_FxAsset.AttributeIdFromName(name);

		if (attribId != -1)
		{
			SetAttributeSafe(attribId, valueX, valueY, valueZ, valueW);
		}
	}

	public void SetAttributeSafe(string name, int valueX)
	{
		int attribId = m_FxAsset.AttributeIdFromName(name);

		if (attribId != -1)
		{
			SetAttributeSafe(attribId, valueX);
		}
	}

	public void SetAttributeSafe(string name, int valueX, int valueY)
	{
		int attribId = m_FxAsset.AttributeIdFromName(name);

		if (attribId != -1)
		{
			SetAttributeSafe(attribId, valueX, valueY);
		}
	}

	public void SetAttributeSafe(string name, int valueX, int valueY, int valueZ)
	{
		int attribId = m_FxAsset.AttributeIdFromName(name);

		if (attribId != -1)
		{
			SetAttributeSafe(attribId, valueX, valueY, valueZ);
		}
	}

	public void SetAttributeSafe(string name, int valueX, int valueY, int valueZ, int valueW)
	{
		int attribId = m_FxAsset.AttributeIdFromName(name);

		if (attribId != -1)
		{
			SetAttributeSafe(attribId, valueX, valueY, valueZ, valueW);
		}
	}

	#endregion

	#region SetAttributeSafe from ID

	public void SetAttributeSafe(int attribId, float valueX)
	{
		PKFxFxAsset.AttributeDesc attributeDesc = m_FxAsset.m_AttributeDescs[attribId];

		if (attributeDesc.m_Type != PKFxManagerImpl.EBaseType.Float)
		{
			Debug.LogError("Type mismatch for the attribute");
			return;
		}
		if (attributeDesc.HasMin())
		{
			valueX = Mathf.Max(attributeDesc.m_MinValue0, valueX);
		}
		if (attributeDesc.HasMax())
		{
			valueX = Mathf.Min(attributeDesc.m_MaxValue0, valueX);
		}
		SetAttributeUnsafe(attribId, valueX);
	}

	public void SetAttributeSafe(int attribId, float valueX, float valueY)
	{
		PKFxFxAsset.AttributeDesc attributeDesc = m_FxAsset.m_AttributeDescs[attribId];

		if (attributeDesc.m_Type != PKFxManagerImpl.EBaseType.Float2)
		{
			Debug.LogError("Type mismatch for the attribute");
			return;
		}
		if (attributeDesc.HasMin())
		{
			valueX = Mathf.Max(attributeDesc.m_MinValue0, valueX);
			valueY = Mathf.Max(attributeDesc.m_MinValue1, valueY);
		}
		if (attributeDesc.HasMax())
		{
			valueX = Mathf.Min(attributeDesc.m_MaxValue0, valueX);
			valueY = Mathf.Min(attributeDesc.m_MaxValue1, valueY);
		}
		SetAttributeUnsafe(attribId, valueX, valueY);
	}

	public void SetAttributeSafe(int attribId, float valueX, float valueY, float valueZ)
	{
		PKFxFxAsset.AttributeDesc attributeDesc = m_FxAsset.m_AttributeDescs[attribId];

		if (attributeDesc.m_Type != PKFxManagerImpl.EBaseType.Float3)
		{
			Debug.LogError("Type mismatch for the attribute");
			return;
		}
		if (attributeDesc.HasMin())
		{
			valueX = Mathf.Max(attributeDesc.m_MinValue0, valueX);
			valueY = Mathf.Max(attributeDesc.m_MinValue1, valueY);
			valueZ = Mathf.Max(attributeDesc.m_MinValue2, valueZ);
		}
		if (attributeDesc.HasMax())
		{
			valueX = Mathf.Min(attributeDesc.m_MaxValue0, valueX);
			valueY = Mathf.Min(attributeDesc.m_MaxValue1, valueY);
			valueZ = Mathf.Min(attributeDesc.m_MaxValue2, valueZ);
		}
		SetAttributeUnsafe(attribId, valueX, valueY, valueZ);
	}

	public void SetAttributeSafe(int attribId, float valueX, float valueY, float valueZ, float valueW)
	{
		PKFxFxAsset.AttributeDesc attributeDesc = m_FxAsset.m_AttributeDescs[attribId];

		if (attributeDesc.m_Type != PKFxManagerImpl.EBaseType.Float4)
		{
			Debug.LogError("Type mismatch for the attribute");
			return;
		}
		if (attributeDesc.HasMin())
		{
			valueX = Mathf.Max(attributeDesc.m_MinValue0, valueX);
			valueY = Mathf.Max(attributeDesc.m_MinValue1, valueY);
			valueZ = Mathf.Max(attributeDesc.m_MinValue2, valueZ);
			valueW = Mathf.Max(attributeDesc.m_MinValue3, valueW);
		}
		if (attributeDesc.HasMax())
		{
			valueX = Mathf.Min(attributeDesc.m_MaxValue0, valueX);
			valueY = Mathf.Min(attributeDesc.m_MaxValue1, valueY);
			valueZ = Mathf.Min(attributeDesc.m_MaxValue2, valueZ);
			valueW = Mathf.Min(attributeDesc.m_MaxValue3, valueW);
		}
		SetAttributeUnsafe(attribId, valueX, valueY, valueZ, valueW);
	}

	public void SetAttributeSafe(int attribId, int valueX)
	{
		PKFxFxAsset.AttributeDesc attributeDesc = m_FxAsset.m_AttributeDescs[attribId];

		if (attributeDesc.m_Type != PKFxManagerImpl.EBaseType.Int)
		{
			Debug.LogError("Type mismatch for the attribute");
			return;
		}
		if (attributeDesc.HasMin())
		{
			valueX = Mathf.Max(PKFxManagerImpl.Float2Int(attributeDesc.m_MinValue0), valueX);
		}
		if (attributeDesc.HasMax())
		{
			valueX = Mathf.Min(PKFxManagerImpl.Float2Int(attributeDesc.m_MaxValue0), valueX);
		}
		SetAttributeUnsafe(attribId, valueX);
	}

	public void SetAttributeSafe(int attribId, int valueX, int valueY)
	{
		PKFxFxAsset.AttributeDesc attributeDesc = m_FxAsset.m_AttributeDescs[attribId];

		if (attributeDesc.m_Type != PKFxManagerImpl.EBaseType.Int2)
		{
			Debug.LogError("Type mismatch for the attribute");
			return;
		}
		if (attributeDesc.HasMin())
		{
			valueX = Mathf.Max(PKFxManagerImpl.Float2Int(attributeDesc.m_MinValue0), valueX);
			valueY = Mathf.Max(PKFxManagerImpl.Float2Int(attributeDesc.m_MinValue1), valueY);
		}
		if (attributeDesc.HasMax())
		{
			valueX = Mathf.Min(PKFxManagerImpl.Float2Int(attributeDesc.m_MaxValue0), valueX);
			valueY = Mathf.Min(PKFxManagerImpl.Float2Int(attributeDesc.m_MaxValue1), valueY);
		}
		SetAttributeUnsafe(attribId, valueX, valueY);
	}

	public void SetAttributeSafe(int attribId, int valueX, int valueY, int valueZ)
	{
		PKFxFxAsset.AttributeDesc attributeDesc = m_FxAsset.m_AttributeDescs[attribId];

		if (attributeDesc.m_Type != PKFxManagerImpl.EBaseType.Int3)
		{
			Debug.LogError("Type mismatch for the attribute");
			return;
		}
		if (attributeDesc.HasMin())
		{
			valueX = Mathf.Max(PKFxManagerImpl.Float2Int(attributeDesc.m_MinValue0), valueX);
			valueY = Mathf.Max(PKFxManagerImpl.Float2Int(attributeDesc.m_MinValue1), valueY);
			valueZ = Mathf.Max(PKFxManagerImpl.Float2Int(attributeDesc.m_MinValue2), valueZ);
		}
		if (attributeDesc.HasMax())
		{
			valueX = Mathf.Min(PKFxManagerImpl.Float2Int(attributeDesc.m_MaxValue0), valueX);
			valueY = Mathf.Min(PKFxManagerImpl.Float2Int(attributeDesc.m_MaxValue1), valueY);
			valueZ = Mathf.Min(PKFxManagerImpl.Float2Int(attributeDesc.m_MaxValue2), valueZ);
		}
		SetAttributeUnsafe(attribId, valueX, valueY, valueZ);
	}

	public void SetAttributeSafe(int attribId, int valueX, int valueY, int valueZ, int valueW)
	{
		PKFxFxAsset.AttributeDesc attributeDesc = m_FxAsset.m_AttributeDescs[attribId];

		if (attributeDesc.m_Type != PKFxManagerImpl.EBaseType.Int4)
		{
			Debug.LogError("Type mismatch for the attribute");
			return;
		}
		if (attributeDesc.HasMin())
		{
			valueX = Mathf.Max(PKFxManagerImpl.Float2Int(attributeDesc.m_MinValue0), valueX);
			valueY = Mathf.Max(PKFxManagerImpl.Float2Int(attributeDesc.m_MinValue1), valueY);
			valueZ = Mathf.Max(PKFxManagerImpl.Float2Int(attributeDesc.m_MinValue2), valueZ);
			valueW = Mathf.Max(PKFxManagerImpl.Float2Int(attributeDesc.m_MinValue3), valueW);
		}
		if (attributeDesc.HasMax())
		{
			valueX = Mathf.Min(PKFxManagerImpl.Float2Int(attributeDesc.m_MaxValue0), valueX);
			valueY = Mathf.Min(PKFxManagerImpl.Float2Int(attributeDesc.m_MaxValue1), valueY);
			valueZ = Mathf.Min(PKFxManagerImpl.Float2Int(attributeDesc.m_MaxValue2), valueZ);
			valueW = Mathf.Min(PKFxManagerImpl.Float2Int(attributeDesc.m_MaxValue3), valueW);
		}
		SetAttributeUnsafe(attribId, valueX, valueY, valueZ, valueW);
	}

	#endregion

	#region SetAttributeUnsafe

	public void SetAttributeUnsafe(int attribId, float valueX)
	{
		m_WasModified = true;
		unsafe
		{
			float	*attribPtr = (float*)m_AttributesBuffer.ToPointer() + attribId * 4;
			attribPtr[0] = valueX;
		}
	}

	public void SetAttributeUnsafe(int attribId, float valueX, float valueY)
	{
		m_WasModified = true;
		unsafe
		{
			float	*attribPtr = (float*)m_AttributesBuffer.ToPointer() + attribId * 4;
			attribPtr[0] = valueX;
			attribPtr[1] = valueY;
		}
	}

	public void SetAttributeUnsafe(int attribId, float valueX, float valueY, float valueZ)
	{
		m_WasModified = true;
		unsafe
		{
			float	*attribPtr = (float*)m_AttributesBuffer.ToPointer() + attribId * 4;
			attribPtr[0] = valueX;
			attribPtr[1] = valueY;
			attribPtr[2] = valueZ;
		}
	}

	public void SetAttributeUnsafe(int attribId, float valueX, float valueY, float valueZ, float valueW)
	{
		m_WasModified = true;
		unsafe
		{
			float	*attribPtr = (float*)m_AttributesBuffer.ToPointer() + attribId * 4;
			attribPtr[0] = valueX;
			attribPtr[1] = valueY;
			attribPtr[2] = valueZ;
			attribPtr[3] = valueW;
		}
	}

	public void SetAttributeUnsafe(int attribId, Vector4 value)
	{
		m_WasModified = true;
		unsafe
		{
			Vector4		*attribPtr = (Vector4*)m_AttributesBuffer.ToPointer() + attribId;
			attribPtr[0] = value;
		}
	}

	public void SetAttributeUnsafe(int attribId, int valueX)
	{
		m_WasModified = true;
		unsafe
		{
			int		*attribPtr = (int*)m_AttributesBuffer.ToPointer() + attribId * 4;
			attribPtr[0] = valueX;
		}
	}

	public void SetAttributeUnsafe(int attribId, int valueX, int valueY)
	{
		m_WasModified = true;
		unsafe
		{
			int		*attribPtr = (int*)m_AttributesBuffer.ToPointer() + attribId * 4;
			attribPtr[0] = valueX;
			attribPtr[1] = valueY;
		}
	}

	public void SetAttributeUnsafe(int attribId, int valueX, int valueY, int valueZ)
	{
		m_WasModified = true;
		unsafe
		{
			int		*attribPtr = (int*)m_AttributesBuffer.ToPointer() + attribId * 4;
			attribPtr[0] = valueX;
			attribPtr[1] = valueY;
			attribPtr[2] = valueZ;
		}
	}

	public void SetAttributeUnsafe(int attribId, int valueX, int valueY, int valueZ, int valueW)
	{
		m_WasModified = true;
		unsafe
		{
			int		*attribPtr = (int*)m_AttributesBuffer.ToPointer() + attribId * 4;
			attribPtr[0] = valueX;
			attribPtr[1] = valueY;
			attribPtr[2] = valueZ;
			attribPtr[3] = valueW;
		}
	}

	public void	SetAllAttributes(float[] attributes)
	{
		m_WasModified = true;
		if (attributes.Length != m_FxAsset.m_AttributeDescs.Count * 4)
		{
			Debug.LogError("The attribute array does not contain the same number of attributes than the pkfx asset : " + attributes.Length + " != " + m_FxAsset.m_AttributeDescs.Count * 4);
			return;
		}
		Marshal.Copy(attributes, 0, m_AttributesBuffer, attributes.Length);
	}

	#endregion

	public Vector4	GetAttributeUnsafe(int attribId)
	{
		unsafe
		{
			Vector4	*attribPtr = (Vector4*)m_AttributesBuffer.ToPointer() + attribId;
			return *attribPtr;
		}
	}

	public void	UpdateAttributes()
	{
		if (m_WasModified)
		{
			m_WasModified = false;
			PKFxManager.UpdateAttributesBuffer(m_FxGuid);
		}
	}

	override public string	ToString()
	{
		return "PkFxAttributeContainer " + m_FxAsset.m_AssetName;
	}
}
