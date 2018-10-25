using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manifestant : MonoBehaviour {

    List<Comportement> comportements = new List<Comportement>();
    public Mood mood = new Mood();
    Comportement currentComportement = null;

    private void Awake()
    {
        GetComponentsInChildren<Comportement>(true, comportements);
        Debug.Log(comportements.Count);
    }

    // Use this for initialization
    void Start () {
        mood.anger = 0.5f;
        mood.lost = 0f;
        mood.moral = 1f;
    }

    // Update is called once per frame
    private void Update()
    {
        float bestScore = 0;

        foreach(Comportement comportement in comportements)
        {
            float score = comportement.Evaluate(this);
            Debug.Log(comportement.name + " : " + score);
            if (score > bestScore)
            {
                bestScore = score;
                currentComportement = comportement;
            }

        }
	}
}
