using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertChecker : MonoBehaviour
{
    [Range(0f, 50f)] public float alertRadius;
    public int extraWaves = 1;

    public LayerMask alertMask = TagAndLayer.LayerMasking.Enemy;
    private Vector3 current;
    private bool isAlert;

    private void Start()
    {
        InvokeRepeating("PingAlert", 1f, 1f);
    }

    private void AlertNearBy(Vector3 origin, Vector3 target, int wave = 0)
    {
        if (wave > extraWaves)
            return;

        Collider[] targetsInViewRadius = Physics.OverlapSphere(origin, alertRadius, alertMask);
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            targetsInViewRadius[i].SendMessageUpwards("AlertCallback", target, SendMessageOptions.DontRequireReceiver);
            AlertNearBy(targetsInViewRadius[i].transform.position, target, wave + 1);
        }
    }

    public void RootAlertNearBy(Vector3 origin)
    {
        current = origin;
        isAlert = true;
    }

    private void PingAlert()
    {
        if (isAlert == true)
        {
            isAlert = false;
            AlertNearBy(current, current);
        }
    }
}