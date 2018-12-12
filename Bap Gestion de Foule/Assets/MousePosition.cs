using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePosition : MonoBehaviour {

    [HideInInspector]
    private Vector3 position;
    public float PosY;
    
	void FixedUpdate () {
        Vector3 mp = Input.mousePosition;
        position = Camera.main.ScreenToWorldPoint(mp);
        position.y = PosY;
        Debug.Log(position);
    }

    public Vector3 GetMousePosition(){
        return position;
    }
}
