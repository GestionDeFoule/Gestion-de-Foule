using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillsManager : MonoBehaviour {

    public GameObject prefabSkill;

    public void Skill1()
    {
        GameObject prefab = Instantiate(prefabSkill);
    }
}
