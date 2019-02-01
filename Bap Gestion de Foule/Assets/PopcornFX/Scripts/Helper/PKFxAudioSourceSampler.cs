using AOT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PKFxAudioSourceSampler : MonoBehaviour
{
	public string channel;

	private static Dictionary<string, AudioSource>	m_AudioSources = new Dictionary<string, AudioSource>();
	private static float[]							m_Samples;
	private static GCHandle							m_SamplesHandle;
	private bool									m_DelegateSet = false;

	void Start()
	{
		// Initialize the data buffer. Expected size is 1024.
		m_Samples = new float[1024];
		// Pin the array
		m_SamplesHandle = GCHandle.Alloc(m_Samples, GCHandleType.Pinned);
		string key = channel;
		if (string.IsNullOrEmpty(key))
			key = "Master";
		m_AudioSources.Add(key, GetComponent<AudioSource>());
	}

	void Update()
	{
		if (!m_DelegateSet)
		{
			AudioSource audioSrc = null;
			if (m_AudioSources.TryGetValue(channel, out audioSrc))
			{
				PKFxDelegateHandler delegateHandler = PKFxDelegateHandler.Instance;
				PKFxManagerImpl.SetDelegateOnAudioSpectrumData(delegateHandler.DelegateToFunctionPointer(new PKFxManagerImpl.AudioCallback(OnAudioSpectrumData)));
				PKFxManagerImpl.SetDelegateOnAudioWaveformData(delegateHandler.DelegateToFunctionPointer(new PKFxManagerImpl.AudioCallback(OnAudioWaveformData)));
			}
			m_DelegateSet = true;
		}
	}

	// MonoPInvokeCallback is required by IL2CPP (mostly for iOS' static libs)
	[MonoPInvokeCallback(typeof(PKFxManagerImpl.AudioCallback))]
	public static IntPtr OnAudioSpectrumData(IntPtr channelName, IntPtr nbSamples)
	{
		string name = Marshal.PtrToStringAnsi(channelName);
		Debug.Log(name);
		AudioSource audioSrc = null;
		if (m_AudioSources.TryGetValue(name, out audioSrc))
		{
			audioSrc.GetSpectrumData(m_Samples, 0, FFTWindow.Rectangular);
			// Last value filled by Unity seems wrong...
			m_Samples[1023] = m_Samples[1022];
			// return the address of the pinned object.
			return m_SamplesHandle.AddrOfPinnedObject();
		}
		return IntPtr.Zero;
	}

	[MonoPInvokeCallback(typeof(PKFxManagerImpl.AudioCallback))]
	public static IntPtr OnAudioWaveformData(IntPtr channelName, IntPtr nbSamples)
	{
		string name = Marshal.PtrToStringAnsi(channelName);
		Debug.Log(name);
		AudioSource audioSrc = null;
		if (m_AudioSources.TryGetValue(name, out audioSrc))
		{
			audioSrc.GetOutputData(m_Samples, 0);
			// return the address of the pinned object.
			return m_SamplesHandle.AddrOfPinnedObject();
		}
		return IntPtr.Zero;
	}

	void OnDestroy()
	{
		// Unpin the object to allow GC.
		m_SamplesHandle.Free();
	}
}
