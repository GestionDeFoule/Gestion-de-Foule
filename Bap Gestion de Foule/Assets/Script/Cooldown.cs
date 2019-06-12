using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cooldown : MonoBehaviour
{
    public float cooldownTime = 5;

    private float nextAbilityTime = 0;

    public Image image;
    private float timer;

    public Button button;



    // Use this for initialization
    void Start()
    {
        image.fillAmount = 0;
        timer = 0;
        //InvokeRepeating("ActivateCooldown", nextAbilityTime, cooldownTime);

    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
            button.interactable = true;

        image.fillAmount = timer / cooldownTime;

    }

    public void ActivateCooldown()
    {
        timer = cooldownTime;
        button.interactable = false;

    }

}
