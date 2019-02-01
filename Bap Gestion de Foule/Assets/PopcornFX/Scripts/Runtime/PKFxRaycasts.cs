using System;
using AOT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PKFxRaycasts : MonoBehaviour
{
	public delegate void RaycastPackCallback(IntPtr raycastQuery);

	[MonoPInvokeCallback(typeof(RaycastPackCallback))]
	public static void OnRaycastPack(IntPtr raycastQuery)
	{
		unsafe // This requires to enable unsafe code in Unity player's settings
		{
			PKFxManagerImpl.SRaycastPack	*raycastPack = (PKFxManagerImpl.SRaycastPack*)raycastQuery.ToPointer();

			Vector4						*origins = (Vector4*)raycastPack->m_RayOrigins.ToPointer();
			Vector4						*directions = (Vector4*)raycastPack->m_RayDirections.ToPointer();

			Vector4						*resNormals = (Vector4*)raycastPack->m_OutNormals.ToPointer();
			Vector4						*resPositions = (Vector4*)raycastPack->m_OutPositions.ToPointer();
			float						*resDistance = (float*)raycastPack->m_OutDistances.ToPointer();

			uint						raycastsCount = (uint)raycastPack->m_RayCount;

			int		layerMask = raycastPack->m_FilterLayer == 0 ? Physics.DefaultRaycastLayers : (1 << raycastPack->m_FilterLayer);

			for (uint i = 0; i < raycastsCount; ++i)
			{
				RaycastHit rayHit;
				bool hit = Physics.Raycast(origins[i], directions[i], out rayHit, directions[i].w, layerMask);

				if (resNormals != null)
				{
					resNormals[i] = rayHit.normal;
				}
				if (resPositions != null)
				{
					resPositions[i] = rayHit.point;
				}
				if (hit)
				{
					resDistance[i] = rayHit.distance;
				}
				else
				{
					resDistance[i] = float.PositiveInfinity;
				}
			}
		}
	}

}
