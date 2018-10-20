using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour {

    private float panSpeed = 20f;
    private float panBorderThickness = 100f; // taille des bordures d'écran
    public Vector2 panLimit; // limit de mouvement de la caméra

    private float scrollSpeed = 20f;
    private float maxY = 20f;
    private float minY = -20f;

    void Update () {

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

        pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        pos.z = Mathf.Clamp(pos.z, -panLimit.y, panLimit.y);

        transform.position = pos;
    }
}
