using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioController : MonoBehaviour
{
    [SerializeField] AudioSource myAudio;

    [SerializeField] AudioClip[] clips;
    Dictionary<string, AudioClip> clipDict;

    private void Awake()
    {
        if(!myAudio)
        myAudio = GetComponent<AudioSource>();

        clipDict = new Dictionary<string, AudioClip>();

        for(int i =0; i < clips.Length; ++i)
        {
            clipDict.Add(clips[i].name, clips[i]);
        }

    }

    public void PlaySound(int clipNum)
    {
        myAudio.PlayOneShot(clips[clipNum]);
    }

    public void PlaySound(string clipName)
    {
        if (clipDict.ContainsKey(clipName))
        { myAudio.PlayOneShot(clipDict[clipName]); }
    }

    public void TalkSound(int clipNum)
    {
        if (myAudio.isPlaying)
        {
            myAudio.Stop();
        }
        PlaySound(clipNum);
    }
    public void TalkSound(string clipName)
    {
        if (myAudio.isPlaying)
        {
            myAudio.Stop();
        }
        PlaySound(clipName);
    }

    public void StopSound()
    {
        myAudio.Stop();
    }
}
