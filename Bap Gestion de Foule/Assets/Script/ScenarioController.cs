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

    public Transform sonManif;

    public Transform followTarget;

    public Text text;

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
            Invoke("MoloCap", 4);
            fx.SetAttributeSafe("Charge_CRS", 1);
            etape = 3;
            SoundControler._soundControler.PlaySoundCRS(SoundControler._soundControler._ChargeTrigger);
        }

        if ((eventName == "MoveTarget") && bool1 && etape == 2)
        {
            Invoke("SitActive", 15);
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

    private void MoloCap()
    {
        molotovimg.SetActive(true);
        overlaymolotovbt.SetActive(false);
        overlaymolotov.SetActive(true);
        bool1 = false;
        Time.timeScale = 0;
    }

    private void SitActive()
    {
        sitimg.SetActive(true);
        overlaysitbt.SetActive(false);
        overlaysit.SetActive(true);
        Time.timeScale = 0;
        bool1 = true;
        etape = 4;
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
    public void Molotov()
    {
        fx.SetAttributeSafe("Molotov", 1);
        fx.SetAttributeSafe("Charge_CRS", 0);
        text.text = "JETS DE MOLOTOVS ET PROJECTILES LORS DE LA MANIFESTATION SUR LES POLICIERS";
    }
    public void GazThrow()
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
        text.text = "LA POLICE UTILISE DES BOMBES LACRYMOGÈNES CONTRE LES MANIFESTANTS";
        bool1 = true;
        etape = 2;
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
        text.text = "LES MANIFESTANTS PROTESTENT PAR UN SIT IN DEVANT LES POLICIERS";
        Invoke("PoliceCircle", 2);
    }
    public void PoliceCircle()// EN ONZIÈME
    {
        SoundControler._soundControler._sourceManif.volume = 0.5f;
        SoundControler._soundControler.PlaySoundCRS(SoundControler._soundControler._ChargeTrigger);
        fx.SetAttributeSafe("Circling_CRS", 1);
        Invoke("ChargeUi", 15);
    }

    public void CapaCharge()// EN TREIZIÈME
    {
        SoundControler._soundControler.PlaySoundManif(SoundControler._soundControler._Charge);
        fx.SetAttributeSafe("Charge_Protest", 1);
        fx.SetAttributeSafe("Circling_CRS", 0);
        followTarget.position += new Vector3(15, 0, 0);
        SetShapeBoxTransform(fx, "CRSTarget", followTarget);
        SetShapeBoxTransform(fx, "ProtestTarget", followTarget);
        text.text = "LES MANIFESTANTS CHARGENT DANS LA FOULE DE POLICIERS - NOMBREUX BLESSÉS";

        Invoke("winner", 15);
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

    public void timego()
    {
        Time.timeScale = 1;
    }

    //fx pour changer la ProtestTarget
    public static void SetShapeBoxTransform(PKFxFX fx, string samplerID, Transform transform)
    {
        PKFxFxAsset.ShapeTransform shapeTransform = new PKFxFxAsset.ShapeTransform(transform.position, transform.rotation, transform.lossyScale);
        fx.SetSamplerSafe(new PKFxFX.Sampler(samplerID, new PKFxFX.SamplerDescShapeBox(Vector3.one, shapeTransform)));
    }
}
