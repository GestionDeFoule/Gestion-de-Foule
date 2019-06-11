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

    public GameObject overlaymolotovbt;
    public GameObject overlaygazbt;
    public GameObject overlaysitbt;
    public GameObject overlaychargebt;
    

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
            overlaymolotovbt.SetActive(false);
            overlaymolotov.SetActive(true);
            bool1 = false;
            Time.timeScale = 0;
            etape = 3;
        }

        if ((eventName == "MoveTarget") && bool1 && etape == 2)
        {
            sitimg.SetActive(true);
            overlaysitbt.SetActive(false);
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
        fx.SetAttributeSafe("Mask_Protest_Percent", 0.8f);

        SetShapeBoxTransform(fx, "ProtestTarget", followTarget);
        bool1 = true;
        etape = 2;
    }

    public void CapaSit()// EN DIXIÈME
    {
        fx.SetAttributeSafe("Sit_In", 1);

        Invoke("PoliceCircle", 2);
    }
    public void PoliceCircle()// EN ONZIÈME
    {
        fx.SetAttributeSafe("Circling_CRS", 1);
        Invoke("ChargeUi", 3);
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
        overlaygazbt.SetActive(false);
        overlaygaz.SetActive(true);
        Time.timeScale = 0;
    }

    public void ChargeUi()
    {
        chargeimg.SetActive(true);
        overlaychargebt.SetActive(false);
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

        overlaymolotov.SetActive(false);
        molotovimg.SetActive(false);

        overlaygaz.SetActive(false);
        gazimg.SetActive(false);

        overlaysit.SetActive(false);
        sitimg.SetActive(false);

        overlaycharge.SetActive(false);
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
