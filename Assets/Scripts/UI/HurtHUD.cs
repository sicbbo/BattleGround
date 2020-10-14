using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HurtHUD : MonoBehaviour
{
    private struct HurtData
    {
        public Transform shotOrigin;
        public Image hurtImg;
    }

    private Transform canvas;
    private GameObject hurtPrefab;
    private float decayFactor = 0.8f;
    private Dictionary<int, HurtData> hurtUIDatas;
    private Transform player, cam;

    public void Setup(Transform canvas, GameObject hurtPrefab, float decayFactor, Transform player)
    {
        hurtUIDatas = new Dictionary<int, HurtData>();
        this.canvas = canvas;
        this.hurtPrefab = hurtPrefab;
        this.decayFactor = decayFactor;
        this.player = player;
        cam = Camera.main.transform;
    }

    private void SetRotation(Image hurtUI, Vector3 orientation, Vector3 shotDirection)
    {
        orientation.y = 0f;
        shotDirection.y = 0f;
        float rotation = Vector3.SignedAngle(shotDirection, orientation, Vector3.up);

        Vector3 newRotation = hurtUI.rectTransform.rotation.eulerAngles;
        newRotation.z = rotation;
        Image hurtImg = hurtUI.GetComponent<Image>();
        hurtImg.rectTransform.rotation = Quaternion.Euler(newRotation);
    }

    private Color GetUpdatedAlpha(Color currentColor, bool reset = false)
    {
        if (reset)
        {
            currentColor.a = 1f;
        }
        else
        {
            currentColor.a -= decayFactor * Time.deltaTime;
        }
        return currentColor;
    }

    public void DrawHurtUI(Transform shotOrigin, int hashID)
    {
        if (hurtUIDatas.ContainsKey(hashID))
        {
            hurtUIDatas[hashID].hurtImg.color = GetUpdatedAlpha(hurtUIDatas[hashID].hurtImg.color, true);
        }
        else
        {
            GameObject hurtUI = Instantiate(hurtPrefab, canvas);
            SetRotation(hurtUI.GetComponent<Image>(), cam.forward, shotOrigin.position - player.position);

            HurtData data;
            data.shotOrigin = shotOrigin;
            data.hurtImg = hurtUI.GetComponent<Image>();
            hurtUIDatas.Add(hashID, data);
        }
    }

    private void Update()
    {
        List<int> toRemoveKeys = new List<int>();
        foreach(int key in hurtUIDatas.Keys)
        {
            SetRotation(hurtUIDatas[key].hurtImg, cam.forward, hurtUIDatas[key].shotOrigin.position - player.position);
            hurtUIDatas[key].hurtImg.color = GetUpdatedAlpha(hurtUIDatas[key].hurtImg.color);
            if (hurtUIDatas[key].hurtImg.color.a <= 0f)
            {
                toRemoveKeys.Add(key);
            }
        }

        for (int i = 0; i < toRemoveKeys.Count; i++)
        {
            Destroy(hurtUIDatas[toRemoveKeys[i]].hurtImg.gameObject);
            hurtUIDatas.Remove(toRemoveKeys[i]);
        }
    }
}