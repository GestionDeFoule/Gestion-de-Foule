using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillsManager : MonoBehaviour {

    public GameObject prefabSkill;
    GameObject prefab;
    bool firstClick;
    public float ScaleFactor = 50f;
    public float PosY;
    private Vector3 position;
    [Range(0.0f, 60.0f)]
    public float smooth;
    public AnimationCurve curve;

    public void Move()
    {
        if (prefab != null)
            Destroy(prefab);
        prefab = Instantiate(prefabSkill,position,prefabSkill.transform.rotation);
        prefab.transform.localScale = new Vector3(prefab.transform.localScale.x * ScaleFactor, prefab.transform.localScale.y * ScaleFactor, prefab.transform.localScale.z);
        firstClick = true;
    }


    public void Molo()
    {
        if (prefab != null)
            Destroy(prefab);
        prefab = Instantiate(prefabSkill, position, prefabSkill.transform.rotation);
        prefab.transform.localScale = new Vector3(prefab.transform.localScale.x * ScaleFactor, prefab.transform.localScale.y * ScaleFactor, prefab.transform.localScale.z);
        firstClick = true;
    }


    public void Sit()
    {
        if (prefab != null)
            Destroy(prefab);
        prefab = Instantiate(prefabSkill, position, prefabSkill.transform.rotation);
        prefab.transform.localScale = new Vector3(prefab.transform.localScale.x * ScaleFactor*2, prefab.transform.localScale.y * ScaleFactor*2, prefab.transform.localScale.z);
        firstClick = true;
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
            if (prefab != null)
            {
                if (firstClick)
                    firstClick = false;
                else
                    Destroy(prefab);

            }
    }


    void FixedUpdate()
    {
        Vector3 mp = Input.mousePosition;
        mp.z = curve.Evaluate(mp.y);
        //mp.z = Camera.main.transform.position.magnitude;
        position = Camera.main.ScreenToWorldPoint(mp);
        position.y = PosY;
        if(prefab!=null)
            prefab.transform.position = Vector3.Lerp(prefab.transform.position, position, smooth * Time.fixedDeltaTime);
    }
}
