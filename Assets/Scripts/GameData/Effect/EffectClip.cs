using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectClip : BaseClip<GameObject>
{
    public EffectType effectType = EffectType.NORMAL;

    public EffectClip() { }

    public GameObject Instantiate(Vector3 pos)
    {
        if (prefab == null)
        {
            PreLoad();
        }

        if (prefab != null)
        {
            GameObject effect = GameObject.Instantiate(prefab, pos, Quaternion.identity);
            return effect;
        }

        return null;
    }
}