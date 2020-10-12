using UnityEngine;
using UnityObject = UnityEngine.Object;

public class BaseClip<PrefabType> where PrefabType : UnityObject
{
    public int realID = 0;
    public string clipName = string.Empty;
    public string clipPath = string.Empty;
    public string clipFullPath = string.Empty;

    public PrefabType prefab = null;

    public void PreLoad()
    {
        clipFullPath = string.Format("{0}{1}", clipPath, clipName);
        if (clipFullPath != string.Empty && prefab == null)
        {
            prefab = ResourceManager.Load(clipFullPath) as PrefabType;
        }
    }

    public void Release()
    {
        if (prefab != null)
            prefab = null;
    }
}