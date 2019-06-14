using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillsManager : MonoBehaviour {

    enum SKILLS
    {
        MOLO,
        SIT,
        MOVE,
        NONE,
    }

    
    //Gestion Zone de skill
    public GameObject prefabSkill;  //prefab de la Zone de Skill
    GameObject Target;  //prefab instancié
    SKILLS curentSkill;
    bool firstClick; // Pour ne pas utiliser le Skill au moment ou l'on appuit sur le bouton
    public float ScaleFactor = 50f; //Scale de la zone 
    public float PosY;    //Position Y de la zone de Skill
    private Vector3 position;  
    [Range(0.0f, 60.0f)]
    public float smooth;
    public AnimationCurve curve; //Coube de la position z de la souris en foction de la position y de la souris 


    public PKFxFX fx;

    public float tempsMask;
    private bool isCharging;

    public Cooldown CdMove;
    public Cooldown CdSit;
    public Cooldown CdMolo;

    public void Move() //Abilité avec Target
    {
        if (Target != null)   //Detruit le sort précedent si l'on appuit sur un autre sort
            Destroy(Target);
        Target = Instantiate(prefabSkill,position,prefabSkill.transform.rotation);   //Apparition de la zone du spell
        Target.transform.localScale = new Vector3(Target.transform.localScale.x * ScaleFactor, Target.transform.localScale.y * ScaleFactor, Target.transform.localScale.z); //Scale de la Zone
        curentSkill = SKILLS.MOVE;
        firstClick = true;
    }


    public void Molo() //Abilité avec Target
    {
        if (Target != null)
            Destroy(Target);
        Target = Instantiate(prefabSkill, position, prefabSkill.transform.rotation);
        Target.transform.localScale = new Vector3(Target.transform.localScale.x * ScaleFactor, Target.transform.localScale.y * ScaleFactor, Target.transform.localScale.z);
        curentSkill = SKILLS.MOLO;
        firstClick = true;
    }


    public void Sit() //Abilité avec Target
    {
        if (Target != null)
            Destroy(Target);
        Target = Instantiate(prefabSkill, position, prefabSkill.transform.rotation);
        Target.transform.localScale = new Vector3(Target.transform.localScale.x * ScaleFactor*2, Target.transform.localScale.y * ScaleFactor*2, Target.transform.localScale.z);
        curentSkill = SKILLS.SIT;
        firstClick = true;
    }

    public void Gaz() //Abilité sans Target Avec Timer
    {
        fx.SetAttributeSafe("Mask_Protest_Percent", 0.5f);
        Invoke("ResetGaz", tempsMask);
    }

    public void ResetGaz()
    {
        fx.SetAttributeSafe("Mask_Protest_Percent", 0f);
    }

    public void Charge()    //Abilité sans Target sans Timer
    {
        if (isCharging)
        {
            fx.SetAttributeSafe("Charge_Protest", 0);
            isCharging = false;
        }
        else
        {
            fx.SetAttributeSafe("Charge_Protest", 1);
            isCharging = true;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))   //Click gauche de la souris
            if (Target != null)
            {
                if (firstClick)
                    firstClick = false;
                else
                {
                    SetShapeBoxTransform(fx, "ZoneCapacite", Target.transform);
                    switch (curentSkill)
                    {
                        case SKILLS.MOLO:
                            UseMolo();
                            break;
                        case SKILLS.SIT:
                            UseSit();
                            break;
                        case SKILLS.MOVE:
                            UseMove();
                            break;
                        case SKILLS.NONE:
                            break;
                        default:
                            break;
                    }

                    Destroy(Target);
                }
                

            }
    }


    void UseMolo() {
        fx.SetAttributeSafe("Molotov", 1);
        Invoke("ResetMolo", 0.1f);
        CdMolo.ActivateCooldown();
    }

    void ResetMolo()
    {
        fx.SetAttributeSafe("Molotov", 0);
    }

    void UseSit() { }  //A FAIRE (idem que pour le Molo)

    void UseMove() {
        SetShapeBoxTransform(fx, "ProtestTarget", Target.transform);
        CdMove.ActivateCooldown();
    }  

    void FixedUpdate() //Update de la Physique
    {
        Vector3 mp = Input.mousePosition; 

        mp.z = curve.Evaluate(mp.y)*(Camera.main.transform.position.y/10.35f);   //set la position z grace a la courbe de par rapport a la position y 

        position = Camera.main.ScreenToWorldPoint(mp);   //Donne la Position de la Zone du Skill par rapport a la position de la souris
        position.y = PosY; 

        if(Target!=null)
            Target.transform.position = Vector3.Lerp(Target.transform.position, position, smooth * Time.fixedDeltaTime);  //Met a jour la position de la zone si elle existe
    }

    //Pour changer le Transform d'une Shape
    public static void SetShapeBoxTransform(PKFxFX fx, string samplerID, Transform transform)
    {
        PKFxFxAsset.ShapeTransform shapeTransform = new PKFxFxAsset.ShapeTransform(transform.position, transform.rotation, transform.lossyScale);
        fx.SetSamplerSafe(new PKFxFX.Sampler(samplerID, new PKFxFX.SamplerDescShapeBox(Vector3.zero, shapeTransform)));
    }
}
