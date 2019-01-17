using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manifestant : MonoBehaviour {

    List<Comportement> comportements = new List<Comportement>();
    public Mood mood = new Mood();
    Comportement currentComportement = null;

    public float speed;
    public float rotationSpeed;
    public float ecart;
    public float maxMagnitude;

    public Rigidbody my_rgb;
    public bool marcheIsSetUp;

    public Image tacticalImage;
    public Canvas tacticalMap;

    private void Awake()
    {
        GetComponentsInChildren<Comportement>(true, comportements);
        my_rgb = GetComponent<Rigidbody>();
    }

    // Use this for initialization
    void Start () {
        tacticalImage= Instantiate(tacticalImage, tacticalMap.transform);
    }

    // Update is called once per frame
    private void Update()
    {
        tacticalImage.transform.position = Camera.main.WorldToScreenPoint(transform.position);
        SelectComportement();
        currentComportement.CUpdate(this);
    }

    private void SelectComportement()
    {
        //calcul le score de chaque comportement pour connaitre le plus adapté
        float bestScore = 0;

        foreach (Comportement comportement in comportements)
        {
            float score = comportement.Evaluate(this);
            if (score > bestScore)
            {
                bestScore = score;
                currentComportement = comportement;
            }

        }
    }

    private void FixedUpdate()
    {
        if(currentComportement != null) currentComportement.CFixedUpdate(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(currentComportement != null) currentComportement.CCollisionEnter(collision, this);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (currentComportement != null) currentComportement.CCollisionExit(collision, this);
    }
}
