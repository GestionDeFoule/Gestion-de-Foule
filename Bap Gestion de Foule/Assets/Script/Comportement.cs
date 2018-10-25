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
}
