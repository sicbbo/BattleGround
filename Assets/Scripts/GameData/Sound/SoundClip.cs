using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundClip : BaseClip<AudioClip>
{
    public SoundPlayType playType = SoundPlayType.None;
    public float maxVolume = 1.0f;
    public bool isLoop = false;
    public float[] checkTime = new float[0];
    public float[] setTime = new float[0];

    public int currentLoop = 0;
    public float pitch = 1.0f;
    public float dopplerLevel = 1.0f;
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    public float minDistance = 10000.0f;
    public float maxDistance = 50000.0f;
    public float spatialBlend = 1.0f;

    public float fadeTime1 = 0.0f;
    public float fadeTime2 = 0.0f;
    public Interpolate.Function interpolate_Func;
    public bool isFadeIn = false;
    public bool isFadeOut = false;

    public SoundClip() { }
    public SoundClip(string clipPath, string clipName)
    {
        this.clipPath = clipPath;
        this.clipName = clipName;
    }

    public void AddLoop()
    {
        checkTime = ArrayHelper.Add(0.0f, checkTime);
        setTime = ArrayHelper.Add(0.0f, setTime);
    }

    public void RemoveLoop(int index)
    {
        checkTime = ArrayHelper.Remove(index, checkTime);
        setTime = ArrayHelper.Remove(index, setTime);
    }

    public AudioClip GetClip()
    {
        if (prefab == null)
        {
            PreLoad();
        }

        if (prefab == null && clipName != string.Empty)
        {
            Debug.LogWarning($"Can not load audio clip Resource {clipName}");
            return null;
        }

        return prefab;
    }

    public bool HasLoop()
    {
        return checkTime.Length > 0;
    }

    public void NextLoop()
    {
        currentLoop++;
        if (currentLoop >= checkTime.Length)
        {
            currentLoop = 0;
        }
    }

    public void CheckLoop(AudioSource source)
    {
        if (HasLoop() && source.time >= checkTime[currentLoop])
        {
            source.time = checkTime[currentLoop];
            NextLoop();
        }
    }

    public void FadeIn(float time, Interpolate.EaseType easeType)
    {
        isFadeOut = false;
        fadeTime1 = 0.0f;
        fadeTime2 = time;
        interpolate_Func = Interpolate.Ease(easeType);
        isFadeIn = true;
    }

    public void FadeOut(float time, Interpolate.EaseType easeType)
    {
        isFadeIn = false;
        fadeTime1 = 0.0f;
        fadeTime2 = time;
        interpolate_Func = Interpolate.Ease(easeType);
        isFadeOut = true;
    }

    public void DoFade(float time, AudioSource audio)
    {
        if (isFadeIn == true)
        {
            fadeTime1 += time;
            audio.volume = Interpolate.Ease(interpolate_Func, 0.0f, maxVolume, fadeTime1, fadeTime2);
            if (fadeTime1 >= fadeTime2)
            {
                isFadeIn = false;
            }
        }
        else
        if (isFadeOut == true)
        {
            fadeTime1 += time;
            audio.volume = Interpolate.Ease(interpolate_Func, maxVolume, 0.0f - maxVolume, fadeTime1, fadeTime2);
            if (fadeTime1 >= fadeTime2)
            {
                isFadeOut = false;
                audio.Stop();
            }
        }
    }
}