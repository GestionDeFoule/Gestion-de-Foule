using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillBehaviour : MonoBehaviour {

    private Vector3 targetPos;
    [Range(0.0f, 60.0f)]
    public float smooth;
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        targetPos = FindObjectOfType<MousePosition>().GetMousePosition();

        transform.position = Vector3.Lerp(transform.position, targetPos, smooth*Time.fixedDeltaTime);
    }
}
