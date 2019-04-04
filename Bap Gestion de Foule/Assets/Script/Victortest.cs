using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Victortest : MonoBehaviour {

    //public PKFxManager.Attribute GetAttribute(string name);
    PKFxFX fx;

    public Transform followTarget;

    public static void SetShapeBoxTransform(PKFxFX fx, string samplerID, Transform transform)
    {
        PKFxFxAsset.ShapeTransform shapeTransform = new PKFxFxAsset.ShapeTransform(transform.position, transform.rotation, transform.lossyScale);
        fx.SetSamplerSafe(new PKFxFX.Sampler(samplerID, new PKFxFX.SamplerDescShapeBox(Vector3.one, shapeTransform)));
    }

    // Use this for initialization
    void Start () {

        fx = this.GetComponent<PKFxFX>();

        fx.SetAttributeSafe("Agitation", 0.5f);

        //fx.SetAttributeSafe("CRSTarget", -1, 0, -2);

        SetShapeBoxTransform(fx, "CRSTarget", followTarget);
        //PKFxFxAsset.ShapeTransform shapeTransform = new PKFxFxAsset.ShapeTransform();
        //shapeTransform.m_Position = followTarget.position;
        //shapeTransform.m_Rotation = followTarget.rotation;
        //shapeTransform.m_Scale = followTarget.lossyScale;

        //fx.SetSamplerSafe(new PKFxFX.Sampler("CRSTarget", new PKFxFX.SamplerDescShapeBox(Vector3.one, shapeTransform)));




    }
	
	// Update is called once per frame
	void Update () {
		
	}




    //public void SetAttribute(PKFxManager.Attribute attr)
    //fx.SetAttribute(new PKFxManager.Attribute("MyColor", newColor));

}
