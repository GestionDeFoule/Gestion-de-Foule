using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton : MonoBehaviour {

    public static Singleton Instance = null;

	// Use this for initialization
	void Awake () {
		if(Instance == null)
        {
            Instance = this;
        } else
        {
            GameObject.Destroy(gameObject);
        }
	}

    private void OnDestroy()
    {
        if(Instance == this)
        {
            Instance = null;
        }
    }
    
}
