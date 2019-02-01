using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PKFxSplitUpdateEvolve : MonoBehaviour
{	
	// Update is called once per frame
	void Update ()
	{
		if (PKFxSettings.SplitUpdateInComponents)
		{
			PKFxManager.UpdateParticlesEvolve();
		}
	}
}
