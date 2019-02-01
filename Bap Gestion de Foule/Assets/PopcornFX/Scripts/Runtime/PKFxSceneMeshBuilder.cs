//----------------------------------------------------------------------------
// Created on Fri Dec 27 14:47:38 2013 Raphael Thoulouze
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

public class PKFxSceneMeshBuilder : MonoBehaviour {

	[Tooltip("Output path for the scene mesh, relative to the PackFX directory")]
	public string		m_OutputPkmmPath = "Meshes/UnityScene.pkmm";
	[Tooltip("List of the GameObjects to be searched for potential meshes.")]
	public GameObject[] m_GameObjectsToSearch;
	[HideInInspector]
	public GameObject[] m_MeshGameObjects;
}
