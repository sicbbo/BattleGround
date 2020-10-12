using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUIManager : MonoBehaviour
{
    public Color bulletColor = Color.white;
    public Color emptyBulletColor = Color.black;
    private Color noBulletColor;

    private Transform managerTrans = null;

    [SerializeField] private Image weaponHUD;
    [SerializeField] private GameObject bulletMag;
    [SerializeField] private Text totalBulletsHUD;

    private void Start()
    {
        managerTrans = transform;

        noBulletColor = new Color(0f, 0f, 0f, 0f);
        if (weaponHUD == null)
        {
            weaponHUD = managerTrans.Find("WeaponHUD/Weapon").GetComponent<Image>();
        }
        if (bulletMag == null)
        {
            bulletMag = managerTrans.Find("WeaponHUD/Data/Mag").gameObject;
        }
        if (totalBulletsHUD == null)
        {
            totalBulletsHUD = managerTrans.Find("WeaponHUD/Data/bulletAmountLabel").GetComponent<Text>();
        }

        Toggle(false);
    }

    public void Toggle(bool active)
    {
        weaponHUD.transform.parent.gameObject.SetActive(active);
    }

    public void UpdateWeaponHUD(Sprite weaponSprite, int bulletLeft, int fullMag, int ExtraBullets)
    {
        if (weaponSprite != null && weaponHUD.sprite != weaponSprite)
        {
            weaponHUD.sprite = weaponSprite;
            weaponHUD.type = Image.Type.Filled;
            weaponHUD.fillMethod = Image.FillMethod.Horizontal;
        }

        int bulletCount = 0;
        for (int i = 0; i < bulletMag.transform.childCount; i++)
        {
            Transform child = bulletMag.transform.GetChild(i);
            Image image = child.GetComponent<Image>();
            if (bulletCount < bulletLeft)
            {
                image.color = bulletColor;
            }
            else
            if (bulletCount >= fullMag)
            {
                image.color = noBulletColor;
            }
            else
            {
                image.color = emptyBulletColor;
            }

            bulletCount++;
        }

        totalBulletsHUD.text = string.Format("{0}/{1}", bulletLeft, ExtraBullets);
    }
}
