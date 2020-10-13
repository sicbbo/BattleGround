using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyDelayed : MonoBehaviour
{
    public float delayTime = 0.5f;

    private void Start()
    {
        Destroy(gameObject, delayTime);
    }
}