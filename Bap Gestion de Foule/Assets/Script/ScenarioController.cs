using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    private Cooldown MolotovCd;
    private Cooldown GazCd;
    private Cooldown SitCd;
    private Cooldown ChargeCd;

    public Transform sonManif;

    public Transform followTarget;

    public Text text;

    public bool bool1 = true;
    bool sitFirst = true;
    public int etape = 1;

    /////////////////////////////////////////////////

    // Use this for initialization
    void Start () //// EN PREMIER
    {
        //Find All Component
        MolotovCd = overlaymolotovbt.GetComponent<Cooldown>();
        GazCd = overlaygazbt.GetComponent<Cooldown>();
        SitCd = overlaysitbt.GetComponent<Cooldown>();
        ChargeCd = overlaychargebt.GetComponent<Cooldown>();
        fx = this.GetComponent<PKFxFX>();

        //
        MolotovCd.button.interactable = false;
        GazCd.button.interactable = false;
        SitCd.button.interactable = false;
        ChargeCd.button.interactable = false;
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
            Invoke("MoloCap", 4); 
            fx.SetAttributeSafe("Charge_CRS", 1);
            etape = 3;
            SoundControler._soundControler.PlaySoundCRS(SoundControler._soundControler._ChargeTrigger);
        }

        if ((eventName == "MoveTarget") && bool1 && etape == 2)
        {
            Invoke("SitActive", 20);
            bool1 = false;
        }
        if ((eventName == "MoveTarget") && bool1 && etape == 4)
        {
            followTarget.position -= new Vector3(7, 0, 0);
            bool1 = false;
        }
        if (eventName == "OnDeath")
            SoundControler._soundControler.PlaySoundCRS(SoundControler._soundControler._Molotov);
    }

    private void MoloCap() //TROISIEME BIS
    {
        molotovimg.SetActive(true);
        MolotovCd.button.interactable = true;
        overlaymolotov.SetActive(true);
        bool1 = false;
        Time.timeScale = 0;
    }

    private void SitActive() //HUITIEME BIS
    {
        if (sitFirst)
        {
            sitimg.SetActive(true);
            SitCd.button.interactable = true;
            overlaysit.SetActive(true);
            Time.timeScale = 0;
            bool1 = true;
            etape = 4;
            sitFirst = false;
        }
    }




    ///////////////////CAPACITÉS////////////////////////
    ////////////////////////////////////////////////////

    public void CapaMolo()// EN CINQUIÈME
    {
        SoundControler._soundControler.PlaySoundManif(SoundControler._soundControler._MolotovLightning);
        Invoke("Molotov", 1.7f);
        

        Invoke("GazThrow", 9.5f);
        Invoke("GazUi", 12.5f);
    }
    public void Molotov()//CINQUIEME BIS
    {
        fx.SetAttributeSafe("Molotov", 1);
        fx.SetAttributeSafe("Charge_CRS", 0);
        SoundControler._soundControler.PlaySound(SoundControler._soundControler._News);
        text.text = "AGRESSIVES PROTESTERS ARE THROWING MOLOTOV ON CRS";
    }
    public void GazThrow()//CINQUIEME BIS BIS
    {
        SoundControler._soundControler.PlaySound(SoundControler._soundControler._GazMaskPrevention);
        fx.SetAttributeSafe("Gaz_CRS", 1);
    }

    public void CapaMaskagaz()// EN SEPTIÈME
    {
        SoundControler._soundControler.PlaySoundManif(SoundControler._soundControler._Gaz);
        fx.SetAttributeSafe("Mask_Protest_Percent", 0.5f);
        Invoke("BruitMask", 1);
        SetShapeBoxTransform(fx, "ProtestTarget", followTarget);
        sonManif.position = followTarget.position;
        SoundControler._soundControler.PlaySound(SoundControler._soundControler._News);
        text.text = "THE POLICEMEN DISPERSE THE CROWD BY THROWING TEARGASES";
        bool1 = true;
        etape = 2;
        Invoke("SitActive", 40);
    }
    public void BruitMask()
    {
        SoundControler._soundControler.PlaySoundManif(SoundControler._soundControler._GazMask);
    }

    public void CapaSit()// EN DIXIÈME
    {
        SoundControler._soundControler.PlaySoundManif(SoundControler._soundControler._SitIn);
        SoundControler._soundControler._sourceManif.volume = 0.2f;
        fx.SetAttributeSafe("Sit_In", 1);
        SoundControler._soundControler.PlaySound(SoundControler._soundControler._News);
        text.text = "THE PROTESTERS ARE SITTING AND CONFRONTING POLICE IN PACIFIC WAY";
        Invoke("PoliceCircle", 2);
    }
    public void PoliceCircle()// EN ONZIÈME
    {
        SoundControler._soundControler._sourceManif.volume = 0.5f;
        SoundControler._soundControler.PlaySoundCRS(SoundControler._soundControler._ChargeTrigger);
        fx.SetAttributeSafe("Circling_CRS", 1);
        Invoke("ChargeUi", 20);
    }

    public void CapaCharge()// EN TREIZIÈME
    {
        SoundControler._soundControler.PlaySoundManif(SoundControler._soundControler._Charge);
        fx.SetAttributeSafe("Charge_Protest", 1);
        fx.SetAttributeSafe("Circling_CRS", 0);
        followTarget.position += new Vector3(25, 0, 0);
        SetShapeBoxTransform(fx, "CRSTarget", followTarget);
        SetShapeBoxTransform(fx, "ProtestTarget", followTarget);
        sonManif.position = followTarget.position;
        SoundControler._soundControler.PlaySound(SoundControler._soundControler._News);
        text.text = "SOME PROTESTERS ARE CHARGING THE CRS CAUSING NUMEROUS INJURIES";

        Invoke("winner", 15);
    }



    ///////////////////FONCTIONS UI/////////////////////
    ////////////////////////////////////////////////////

    public void GazUi()
    {
        gazimg.SetActive(true);
        GazCd.button.interactable = true;
        overlaygaz.SetActive(true);
        Time.timeScale = 0;
    }

    public void ChargeUi()
    {
        chargeimg.SetActive(true);
        ChargeCd.button.interactable = true;
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

    public void timego()
    {
        Time.timeScale = 1;
    }

    //fx pour changer la ProtestTarget
    public static void SetShapeBoxTransform(PKFxFX fx, string samplerID, Transform transform)
    {
        PKFxFxAsset.ShapeTransform shapeTransform = new PKFxFxAsset.ShapeTransform(transform.position, transform.rotation, transform.lossyScale);
        fx.SetSamplerSafe(new PKFxFX.Sampler(samplerID, new PKFxFX.SamplerDescShapeBox(Vector3.zero, shapeTransform)));
    }
}
