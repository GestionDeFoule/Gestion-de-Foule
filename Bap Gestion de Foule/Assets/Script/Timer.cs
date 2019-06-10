using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour {

    float time;
    int minute;
    string sec;
    string min;
    Text text;

	// Use this for initialization
	void Start () {
        text = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        time += Time.deltaTime;
        if (time >= 60)
        {
            minute++;
            time-=60;
        }
        if (time < 10)
            sec = "0" + ((int)time).ToString();
        else
            sec = ((int)time).ToString();
        min = minute.ToString();
        text.text = min+":"+sec;
	}
}
