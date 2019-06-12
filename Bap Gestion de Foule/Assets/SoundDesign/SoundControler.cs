using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundControler : MonoBehaviour {

    public AudioClip _ChargeTrigger;
    public AudioClip _GazMask;
    public AudioClip _MolotovLightning;
    public AudioClip _SitIn;
    public AudioClip _Charge;
    public AudioClip _GazMaskPrevention;
    public AudioClip _Molotov;
    public AudioClip _FlashBall;
    public AudioClip _Gaz;
    public AudioClip _News;
    public AudioClip _music;
    public static SoundControler _soundControler;

    private AudioSource _source;
    public AudioSource _sourceManif;
    public AudioSource _sourceCRS;

    private void Awake()
    {
        if (_soundControler == null)
            _soundControler = this;
        else
            Destroy(_soundControler.gameObject);

        _source = GetComponent<AudioSource>();
        
        _source.clip = _music;
        _source.loop = true;
        _source.Play();
    }

    public void PlaySound(AudioClip sound)
    {
        _source.PlayOneShot(sound,0.2f);
    }

    public void PlaySoundManif(AudioClip sound)
    {
        _sourceManif.PlayOneShot(sound,0.2f);
    }

    public void PlaySoundCRS(AudioClip sound)
    {
        _sourceCRS.PlayOneShot(sound,0.2f);
    }

}
