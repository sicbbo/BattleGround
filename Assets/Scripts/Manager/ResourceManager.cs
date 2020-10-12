using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

public class ResourceManager
{
    public static UnityObject Load(string path)
    {
        return Resources.Load(path);
    }

    public static GameObject LoadAndInstantiate(string path)
    {
        UnityObject source = Load(path);
        if (source == null)
            return null;

        return GameObject.Instantiate(source) as GameObject;
    }
}