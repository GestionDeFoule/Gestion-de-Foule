using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour {

    public float panSpeed = 20f;
    private float panBorderThickness = 20f; // taille des bordures d'écran
    public Vector2 panLimitVertical; // limit de mouvement de la caméra
    public Vector2 panLimitHorizontal;

    private float scrollSpeed = 20f;
    public float maxY = 20f;
    public float minY = -20f;

    [Range(0.0f, 1.0f)]
    public float smoothFactorXZ = 0.5f;
    [Range(0.0f, 1.0f)]
    public float smoothFactorY = 0.5f;

    private Vector3 cameraPos;

    private void Awake()
    {
        cameraPos = transform.position;
    }

    void FixedUpdate () {

        Vector3 pos = transform.position;

		if (Input.GetKey("z") || Input.mousePosition.y >= Screen.height - panBorderThickness) // si press z OU si la souris se trouve au dessus de la hauter de l'écran - la taille des bordures d'écran 
        {
            pos.z += panSpeed * Time.deltaTime; // panSpeed * Time.deltaTime nécessaire : bouge en fonction des frame et non de manière constante
        }

        if (Input.GetKey("s") || Input.mousePosition.y <= panBorderThickness) 
        {
            pos.z -= panSpeed * Time.deltaTime; 
        }

        if (Input.GetKey("d") || Input.mousePosition.x >= Screen.width - panBorderThickness) 
        {
            pos.x += panSpeed * Time.deltaTime; 
        }

        if (Input.GetKey("q") || Input.mousePosition.x <= panBorderThickness)  
        {
            pos.x -= panSpeed * Time.deltaTime; 
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * scrollSpeed * 100f * Time.deltaTime;

        pos.x = Mathf.Clamp(pos.x, panLimitHorizontal.x, panLimitHorizontal.y);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        pos.z = Mathf.Clamp(pos.z, panLimitVertical.x, panLimitVertical.y);

        cameraPos.x = Mathf.Lerp(cameraPos.x, pos.x, smoothFactorXZ);
        cameraPos.z = Mathf.Lerp(cameraPos.z, pos.z, smoothFactorXZ);
        cameraPos.y = Mathf.Lerp(cameraPos.y, pos.y, smoothFactorY);

        transform.position = cameraPos;
    }
}
