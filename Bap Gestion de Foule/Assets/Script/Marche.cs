using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marche : Comportement {

    private Vector3 direction;
    private bool collisionFront;
    private bool collisionRight;
    private bool collisionLeft;
    private bool collisonBack;

    public Transform road;


    private int rotation; //1 : rotation horaire -1 : rotation antihoraire
    private List<Transform> waypoints = new List<Transform>();
    private int currentWaypoint = -1;
    private float antisipation;
    private float angle;
    private float nextAngle;
    private float distanceWaypoint;
    float bestDistance = 0;

    void Awake () {
        road.GetComponentsInChildren<Transform>(true, waypoints);
        waypoints.Remove(road.transform);
    }

    public override void CUpdate(Manifestant civil)
    {
        base.CUpdate(civil);
        if (!civil.marcheIsSetUp)
        {
            //setup de la marche 
            foreach (Transform waypoint in waypoints)
            {
                float distance = (waypoint.position - civil.transform.position).magnitude;
                if (bestDistance == 0) bestDistance = distance;
                if (distance <= bestDistance) currentWaypoint = waypoints.IndexOf(waypoint)-1;
            }
            civil.marcheIsSetUp = true;
            ChangeWay(civil);
        }
        Move(civil);

        //calcule de la position du manifestant par rapport au waypoint
        if (rotation == 0) distanceWaypoint = civil.transform.InverseTransformDirection(waypoints[currentWaypoint].position - civil.transform.position).x;
        if ((civil.transform.InverseTransformDirection(waypoints[currentWaypoint].position - civil.transform.position) - Vector3.right * distanceWaypoint).magnitude < antisipation) ChangeWay(civil);

        //determine quand il faut stopper la rotation
        if (angle > 0 && civil.transform.InverseTransformDirection(waypoints[currentWaypoint].position - civil.transform.position).x <= 0) rotation = 0;
        else if (angle < 0 && civil.transform.InverseTransformDirection(waypoints[currentWaypoint].position - civil.transform.position).x >= 0) rotation = 0;
    }

    private void Move(Manifestant civil)
    {
        //calcul de la direction en fonction des autre manifestant
        direction = Vector3.forward;
        LayerMask layerMask = 9;

        RaycastHit hit;

        if (civil.transform.InverseTransformDirection(waypoints[currentWaypoint].position - civil.transform.position).x > 0 && !collisionRight)
        {
            if (Physics.Raycast(civil.transform.position, civil.transform.TransformDirection(Vector3.right), out hit, layerMask))
            {
                if (hit.distance > 0.5 + civil.ecart) direction += Vector3.right;
            }
        }

        if (civil.transform.InverseTransformDirection(waypoints[currentWaypoint].position - civil.transform.position).x < 0 && !collisionLeft)
        {
            if (Physics.Raycast(civil.transform.position, civil.transform.TransformDirection(Vector3.left), out hit, layerMask))
            {
                if (hit.distance > 0.5 + civil.ecart) direction += Vector3.left;
            }
        }

        if (Physics.Raycast(civil.transform.position, civil.transform.TransformDirection(Vector3.forward), out hit, layerMask) && !collisionFront)
        {
            if (hit.distance > 0.5 + civil.ecart) direction += Vector3.forward;
        }
    }

    public override void CFixedUpdate(Manifestant civil)
    {
        //deplace et rotate le manifestant
        base.CFixedUpdate(civil);
        if (direction.magnitude > civil.maxMagnitude) direction = direction.normalized * civil.maxMagnitude;
        civil.my_rgb.MovePosition(civil.transform.position + civil.transform.TransformDirection(direction * Time.deltaTime * civil.speed));
        if (rotation > 0) civil.my_rgb.MoveRotation(Quaternion.Euler(Vector3.up * civil.rotationSpeed * Time.deltaTime + civil.transform.eulerAngles));
        if (rotation < 0) civil.my_rgb.MoveRotation(Quaternion.Euler(Vector3.up * -civil.rotationSpeed * Time.deltaTime + civil.transform.eulerAngles));
    }

    public override void CCollisionEnter(Collision collision,Manifestant civil)
    {
        base.CCollisionEnter(collision, civil);
        if (collision.gameObject.layer == 9)
        {
            Vector3 otherPosition = civil.transform.InverseTransformPoint(collision.transform.position);
            if (otherPosition.x >= 1) collisionRight = true;
            if (otherPosition.x <= -1) collisionLeft = true;
            if (otherPosition.z >= 1) collisionFront = true;
        }
    }

    public override void CCollisionExit(Collision collision,Manifestant civil)
    {
        base.CCollisionExit(collision, civil);
        if (collision.gameObject.layer == 9)
        {
            Vector3 otherPosition = civil.transform.InverseTransformPoint(collision.transform.position);
            if (otherPosition.x >= 1) collisionRight = false;
            if (otherPosition.x <= -1) collisionLeft = false;
            if (otherPosition.z >= 1) collisionFront = false;
        }
    }


    void ChangeWay(Manifestant civil)
    {
        //lance la rotation vers le prochain waypoint
        currentWaypoint++;
        angle = nextAngle;
        if (angle < 0) rotation = -1;
        else if (angle > 0) rotation = 1;

        //calcul du prochaine angle 
        float x = waypoints[currentWaypoint + 1].position.x - waypoints[currentWaypoint].position.x;
        float z = waypoints[currentWaypoint + 1].position.z - waypoints[currentWaypoint].position.z;
        float nextEndRotation = Mathf.Rad2Deg * Mathf.Atan(x / z);
        if (z < 0) nextEndRotation = -180 + nextEndRotation;
        float nextStartRotation = civil.transform.eulerAngles.y + angle;
        if (nextStartRotation > 180) nextStartRotation -= 360;
        nextAngle = nextEndRotation - nextStartRotation;
        if (Mathf.Abs(nextAngle) > 180) nextAngle -= 360;

        //calcule de l'anticipation necessaire a la rotation
        float tempsRotation = Mathf.Abs(nextAngle) / civil.rotationSpeed;
        float decrementation = civil.speed / tempsRotation;
        antisipation = 0;
        for (int i = 0; i < tempsRotation; i++)
        {
            antisipation += civil.speed - decrementation * i;
        }
        if (antisipation == 0) ChangeWay(civil);
    }
}
