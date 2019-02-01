using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PKFxFX))]
public class PKFxAnimFloat2 : MonoBehaviour
{
	public string	m_AttributeName;
	public Vector2	m_Value;

	private PKFxFX	m_Fx;
	private int		m_AttributeId = -1;

	void Start()
	{
		m_Fx = GetComponent<PKFxFX>();
		if (m_Fx == null)
		{
			enabled = false;
		}
		else
		{
			m_AttributeId = m_Fx.m_FxAsset.AttributeIdFromName(m_AttributeName);
			if (m_AttributeId == -1)
			{
				Debug.LogError("The attribute " + m_AttributeName + " does not exist in the effect " + m_Fx.m_FxName, this);
			}
		}
	}

	void LateUpdate()
	{
		if (m_Fx.m_AttributesContainer != null && m_AttributeId != -1)
		{
			m_Fx.m_AttributesContainer.SetAttributeSafe(m_AttributeId, m_Value.x, m_Value.y);
		}
	}
}
