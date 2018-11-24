using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Comportement : MonoBehaviour {
    public Mood moodWeight;

    public float Evaluate(Manifestant civil)
    {
        float score = (civil.mood.anger * moodWeight.anger) + (civil.mood.moral * moodWeight.moral) + (civil.mood.lost * moodWeight.lost);
        return score;
    }

    //vitual permet de prendre en compte les modifications des enfants qui herite de comportement
    public virtual void CUpdate(Manifestant civil)
    {
        
    }

    public virtual void CFixedUpdate(Manifestant civil)
    {

    }

    public virtual void CCollisionEnter(Collision other,Manifestant civil)
    {

    }

    public virtual void CCollisionExit(Collision other, Manifestant civil)
    {

    }
}
