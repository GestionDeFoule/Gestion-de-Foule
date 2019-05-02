using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class HandlePopcornFXEvent : MonoBehaviour {

    PKFxFX fx;
    public GameObject win;

    // Use this for initialization
    void Start () {
        fx = this.GetComponent<PKFxFX>();
        PKFxEventManager.RegisterCustomHandler(OnRaiseEvent);

    }

    private void OnRaiseEvent(string eventName, Vector3 position)
    {
        Debug.Log(eventName);
        if (eventName == "MoveTarget")
            win.SetActive(true); ;
    }
}

//MoveTarget