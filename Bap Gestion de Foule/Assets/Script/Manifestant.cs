using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Awake()
    {
        GetComponentsInChildren<Comportement>(true, comportements);
        my_rgb = GetComponent<Rigidbody>();
    }

    // Use this for initialization
    void Start () {
    }

    // Update is called once per frame
    private void Update()
    {
        SelectComportement();
        currentComportement.CUpdate(this);
    }

    private void SelectComportement()
    {
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
