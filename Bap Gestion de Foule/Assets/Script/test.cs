using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour {

    public struct MoodScore
    {
        public float scared;
        public float lost;
        public float confident;
    }

    public GameObject maRef;
    MoodScore myMood;

    public static int count = 0;

    // Use this for initialization
    void Start () {
        //count++;
        int c = 1;
        int d = 2;
        int Result = Add(c, d);
        Debug.Log(Result);
        EvaluateDanger(maRef);
    }
	
	// Update is called once per frame
	void Update () {
        
	}

    int Add(int a, int b)
    {
        a = a + 3;
        return a + b;
    }

    MoodScore EvaluateDanger(GameObject aRef) {
        aRef.transform.position = aRef.transform.position + Vector3.right;
        MoodScore score = new MoodScore();
        score.confident = 0.5f;
        return score;

        //evaluation du danger   
    }
}
