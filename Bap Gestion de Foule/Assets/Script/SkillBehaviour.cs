using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillBehaviour : MonoBehaviour {

    private Vector3 targetPos;
    [Range(0.0f, 60.0f)]
    public float smooth;
    public float ScaleFactor = 50f;

    private void Awake()
    {
        transform.localScale = new Vector3(transform.localScale.x * ScaleFactor, transform.localScale.y * ScaleFactor, transform.localScale.z);
    }

    void FixedUpdate ()
    {
        targetPos = FindObjectOfType<MousePosition>().GetMousePosition();

        transform.position = Vector3.Lerp(transform.position, targetPos, smooth*Time.fixedDeltaTime);
    }

    private void OnMouseUp()
    {
        Effect();
        Destroy(gameObject);
    }

    private void Effect()
    {

    }
}
