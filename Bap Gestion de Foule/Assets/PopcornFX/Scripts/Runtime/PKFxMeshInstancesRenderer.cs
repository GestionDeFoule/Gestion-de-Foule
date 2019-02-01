using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class PKFxMeshInstancesRenderer : MonoBehaviour
{
	public Mesh[]			m_Meshes;
	public Matrix4x4[]		m_MeshesImportTransform;
	public Material			m_Material;

	public bool				m_CastShadow = false;


	private int				m_InstancesCount = 0;
	private IntPtr			m_PerInstanceBuffer;

	private string			ColorPropertyName
	{
		get
		{
			return "_BaseColor"; // The same for HDRP or no custom render pipeline
		}
	}

	public void SetInstanceBuffer(IntPtr perInstance)
	{
		m_PerInstanceBuffer = perInstance;
	}

	public void SetInstanceCount(int instanceCount)
	{
		m_InstancesCount = instanceCount;
	}
	
	void LateUpdate()
	{
		for (int j = 0; j < m_Meshes.Length; ++j)
		{
			Mesh		m = m_Meshes[j];
			Matrix4x4	t = m_MeshesImportTransform[j];
			if (m_PerInstanceBuffer != IntPtr.Zero && m_InstancesCount > 0)
			{
#if false
 				MaterialPropertyBlock	materialProp = new MaterialPropertyBlock();
				Matrix4x4[]				transforms = new Matrix4x4[1023];
				Vector4[]				colors = new Vector4[1023];

				for (int i = 0; i < m_InstancesCount; i += 1023)
				{
					for (int h = 0; h + i < m_InstancesCount && h < 1023; ++h)
					{
						unsafe
						{
							Matrix4x4* instanceTransform = (Matrix4x4*)m_PerInstanceBuffer.ToPointer();
							Vector4* instanceColor = (Vector4*)(instanceTransform + m_InstancesCount);
							
							transforms[h] = instanceTransform[i + h] * t;
							colors[h] = instanceColor[i + h];
						}
					}
					int DataLeft = Math.Min(m_InstancesCount - i, 1023);
					materialProp.SetVectorArray(ColorPropertyName, colors);
					Graphics.DrawMeshInstanced(m, 0, m_Material, transforms, DataLeft, materialProp);
				}
#else
				for (int i = 0; i < m_InstancesCount; i++)
				{
					Matrix4x4	transform;
					Vector4		color;

					unsafe
					{
						Matrix4x4	*instanceTransform = (Matrix4x4*)m_PerInstanceBuffer.ToPointer();
						Vector4		*instanceColor = (Vector4*)(instanceTransform + m_InstancesCount);

						transform = instanceTransform[i] * t;
						color = instanceColor[i];
					}

					MaterialPropertyBlock colorProperty = new MaterialPropertyBlock();

					colorProperty.SetColor(ColorPropertyName, color);
					Graphics.DrawMesh(m, transform, m_Material, 0, null, 0, colorProperty, m_CastShadow);
				}
#endif
			}
		}
	}
}
