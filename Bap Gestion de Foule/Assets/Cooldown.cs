using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cooldown : MonoBehaviour
{
    public float cooldownTime = 5;

    private float nextAbilityTime = 0;

    public Image image;

    

    // Use this for initialization
    void Start()
    {
        image.fillAmount = 0;
        InvokeRepeating("ActivateCooldown", nextAbilityTime, cooldownTime);

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ActivateCooldown()
    {
        image.fillAmount = 1;

        
        
        if (Time.time > nextAbilityTime)
        {
            Debug.Log("Ability pressed, now on cooldown.");
            nextAbilityTime = Time.time + cooldownTime;
                //Il faut convertir la valeur en % vu que le fill amount est compris entre 0 et 1 :
            float percentNextAbilityTime = nextAbilityTime / 100;
            image.fillAmount = percentNextAbilityTime;
            image.fillAmount--;
        }

    }

}
