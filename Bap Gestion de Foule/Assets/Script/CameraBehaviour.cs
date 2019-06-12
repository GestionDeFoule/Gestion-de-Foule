using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour {

    public float panSpeed = 20f;
    private float panBorderThickness = 20f; // taille des bordures d'écran
    public Vector2 panLimitVerticalMinZoom; // limit de mouvement de la caméra au zoom min
    public Vector2 panLimitHorizontalMinZoom;
    public Vector2 panLimitVerticalMaxZoom; // limit de mouvement de la caméra au zoom max
    public Vector2 panLimitHorizontalMaxZoom;
    private Vector2 panLimitVerticalCurrent; // limit de mouvement de la caméra a l'instant t
    private Vector2 panLimitHorizontalCurrent;
    private float startY;

    public float scrollSpeed = 20f;
    public float maxY = 20f;
    public float minY = -20f;

    public PKFxFX fx;
    private bool tactical;

    private void Awake()
    {

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

        if (pos.y >= maxY-20 && tactical == false)
        {
            fx.SetAttributeSafe("ActivateVoxelCharacter", 0);
            fx.SetAttributeSafe("InfosVisibility", 1f);
            tactical = true;
        }

        if (pos.y < maxY-20 && tactical == true)
        {
            fx.SetAttributeSafe("ActivateVoxelCharacter", 1);
            fx.SetAttributeSafe("InfosVisibility", 0f);
            tactical = false;
        }

        UpdateLimit();

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * scrollSpeed * Time.deltaTime;

        pos.x = Mathf.Clamp(pos.x, panLimitHorizontalCurrent.x, panLimitHorizontalCurrent.y);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        pos.z = Mathf.Clamp(pos.z, panLimitVerticalCurrent.x, panLimitVerticalCurrent.y);

        transform.position = pos;
    }

    private void UpdateLimit() //augmente/diminue les limites de la camera en cas de zoom/dezoom
    {
        panLimitHorizontalCurrent = Vector2.Lerp(panLimitHorizontalMaxZoom, panLimitHorizontalMinZoom, (transform.position.y-minY) / (maxY-minY));
        panLimitVerticalCurrent = Vector2.Lerp(panLimitVerticalMaxZoom, panLimitVerticalMinZoom, (transform.position.y-minY) / (maxY-minY));
    }

    public void MapBut()
    {
        if (tactical)
        {
            transform.position = new Vector3(transform.position.x, minY, transform.position.x);
        }
        else
        {
            transform.position = new Vector3(transform.position.x, maxY, transform.position.x);
        }
    }


}
