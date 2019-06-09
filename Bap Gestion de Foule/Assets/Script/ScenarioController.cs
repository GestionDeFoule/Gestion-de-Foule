using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioController : MonoBehaviour {


    ///////////////////VARIABLES/////////////////////
    /////////////////////////////////////////////////

    PKFxFX fx;

    public GameObject win;
    public GameObject startimg;

    public GameObject molotovimg;
    public GameObject gazimg;
    public GameObject sitimg;
    public GameObject chargeimg;

    public GameObject overlaymolotov;
    public GameObject overlaygaz;
    public GameObject overlaysit;
    public GameObject overlaycharge;
    

    public Transform followTarget;

    public bool bool1 = true;
    public int etape = 1;

    /////////////////////////////////////////////////

    // Use this for initialization
    void Start () //// EN PREMIER
    {
        fx = this.GetComponent<PKFxFX>();
        PKFxEventManager.RegisterCustomHandler(OnRaiseEvent);

        Invoke("timestart", 0.1f);

	}

    //////////////TRIGGERS SUR PT TARGET////////////////
    ////////////////////////////////////////////////////

    private void OnRaiseEvent(string eventName, Vector3 position) //EN TROISIÈME // EN HUITIÈME
    {
        Debug.Log(eventName);
        if ((eventName == "MoveTarget") && bool1 && etape == 1)
        {
            molotovimg.SetActive(true);
            overlaymolotov.SetActive(true);
            bool1 = false;
            Time.timeScale = 0;
        }

        if ((eventName == "MoveTarget") && bool1 && etape == 2)
        {
            sitimg.SetActive(true);
            overlaysit.SetActive(true);
            bool1 = false;
            Time.timeScale = 0;
        }
    }
    

    

    ///////////////////CAPACITÉS////////////////////////
    ////////////////////////////////////////////////////
    
    public void CapaMolo()// EN CINQUIÈME
    {
        fx.SetAttributeSafe("Molotov", 1);

        Invoke("GazThrow", 4.5f);
        Invoke("GazUi", 7.5f);
    }
    public void GazThrow()
    {
        fx.SetAttributeSafe("Gaz_CRS", 1);
    }

    public void CapaMaskagaz()// EN SEPTIÈME
    {
        fx.SetAttributeSafe("Mask_Protest_Percent", 0.5f);

        SetShapeBoxTransform(fx, "ProtestTarget", followTarget);
        bool1 = true;
    }

    public void CapaSit()// EN DIXIÈME
    {
        fx.SetAttributeSafe("Sit_In", 1);

        Invoke("PoliceCircle", 2);
    }
    public void PoliceCircle()// EN ONZIÈME
    {
        fx.SetAttributeSafe("Circling_CRS", 1);
        Invoke("ChargeUI", 3);
    }

    public void CapaCharge()// EN TREIZIÈME
    {
        fx.SetAttributeSafe("Charge_Protest", 1);
        fx.SetAttributeSafe("Circling_CRS", 0);

        Invoke("winner", 3);
    }



    ///////////////////FONCTIONS UI/////////////////////
    ////////////////////////////////////////////////////

    public void GazUi()
    {
        gazimg.SetActive(true);
        overlaycharge.SetActive(true);
        Time.timeScale = 0;
    }

    public void ChargeUi()
    {
        chargeimg.SetActive(true);
        overlaycharge.SetActive(true);
        Time.timeScale = 0;
    }

    public void winner()
    {
        win.SetActive(true);
        Time.timeScale = 0;
    }

    public void Bt()//EN DEUXIÈME // EN QUATRIÈME // EN SIXIÈME // EN NEUVIÈME // EN DOUZIÈME
    {
        Time.timeScale = 1;
        startimg.SetActive(false);

        molotovimg.SetActive(false);

        gazimg.SetActive(false);

        sitimg.SetActive(false);

        chargeimg.SetActive(false);
    }

    public void timestart()
    {
        Time.timeScale = 0;
    }

    //fx pour changer la ProtestTarget
    public static void SetShapeBoxTransform(PKFxFX fx, string samplerID, Transform transform)
    {
        PKFxFxAsset.ShapeTransform shapeTransform = new PKFxFxAsset.ShapeTransform(transform.position, transform.rotation, transform.lossyScale);
        fx.SetSamplerSafe(new PKFxFX.Sampler(samplerID, new PKFxFX.SamplerDescShapeBox(Vector3.one, shapeTransform)));
    }
}
