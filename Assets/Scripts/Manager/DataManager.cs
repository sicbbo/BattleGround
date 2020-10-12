using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    private static SoundData soundData = null;
    public static SoundData SoundData
    {
        get
        {
            if (soundData == null)
            {
                soundData = ScriptableObject.CreateInstance<SoundData>();
                soundData.LoadData();
            }
            return soundData;
        }
    }

    private static EffectData effectData = null;
    public static EffectData EffectData
    {
        get
        {
            if (effectData == null)
            {
                effectData = ScriptableObject.CreateInstance<EffectData>();
                effectData.LoadData();
            }

            return effectData;
        }
    }

    private void Start()
    {
        if (effectData == null)
        {
            effectData = ScriptableObject.CreateInstance<EffectData>();
            effectData.LoadData();
        }
        if (soundData == null)
        {
            soundData = ScriptableObject.CreateInstance<SoundData>();
            soundData.LoadData();
        }
    }
}