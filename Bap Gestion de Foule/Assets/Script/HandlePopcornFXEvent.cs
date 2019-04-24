using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandlePopcornFXEvent : MonoBehaviour {

	// Use this for initialization
	void Start () {
        PKFxEventManager.RegisterCustomHandler(OnRaiseEvent);

    }

    private void OnRaiseEvent(string eventName, Vector3 position)
    {
        Debug.Log(eventName);
        if (eventName == "Dog")
            Debug.Log("Bonjour");
    }
}

//MoveTarget