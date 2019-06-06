using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundControler : MonoBehaviour {

    public AudioClip _alarme;
    public AudioClip _ambianceGuerre;
    public AudioClip _bomb;
    public AudioClip _electricite;
    public AudioClip _fireShot;
    public AudioClip _fireShot2;
    public AudioClip _moteurTank;
    public AudioClip _reload;
    public AudioClip _reparation;
    public AudioClip _music;
    public static SoundControler _soundControler;

    private AudioSource _source;

    private void Awake()
    {
        if (_soundControler == null)
            _soundControler = this;
        else
            Destroy(_soundControler.gameObject);

        _source = GetComponent<AudioSource>();
        
        _source.clip = _ambianceGuerre;
        _source.loop = true;
        _source.Play();
    }

    public void PlaySound(AudioClip sound)
    {
        _source.PlayOneShot(sound);
    }

}
