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

        Vector3 cameraPos;
        cameraPos.x = Mathf.Lerp(transform.position.x, targetPos.x, smooth * Time.deltaTime);
        cameraPos.z = Mathf.Lerp(transform.position.z, targetPos.z, smooth * Time.deltaTime);
        cameraPos.y = targetPos.y;

        transform.position = cameraPos;
    }
}
