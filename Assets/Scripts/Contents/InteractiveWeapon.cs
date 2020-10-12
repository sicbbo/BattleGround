using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractiveWeapon : MonoBehaviour
{
    private Transform thisTrans;
    private GameObject thisGameObject;

    public string label_WeaponName;
    public SoundList shotSound, reloadSound, pickSound, dropSound, noBulletSound;
    public Sprite weaponSprite;
    public Vector3 rightHandPosition;
    public Vector3 relativeRotation;
    public float bulletDamage = 10f;
    public float recoilAngle;
    public WeaponType weaponType = WeaponType.NONE;
    public WeaponMode weaponMode = WeaponMode.SEMI;
    public int burstSize = 3;

    public int currentMagCapacity, totalBullets;
    private int fullMag, maxBullets;
    private GameObject player, gameController;
    private ShootBehaviour playerInventory;
    private BoxCollider weaponCollider;
    private SphereCollider interactiveRadius;
    private Rigidbody weaponRigidbody;
    private bool isPickable;

    public GameObject screenHUD;
    public WeaponUIManager weaponHUD;
    private Transform pickHUD;
    public Text pickupHUD_Label;

    public Transform muzzleTrans;

    private void Awake()
    {
        thisTrans = transform;
        thisGameObject = gameObject;

        thisGameObject.name = label_WeaponName;
        thisGameObject.layer = LayerMask.NameToLayer(TagAndLayer.LayerName.IgnoreRayCast);
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            child.layer = LayerMask.NameToLayer(TagAndLayer.LayerName.IgnoreRayCast);
        }
        player = GameObject.FindGameObjectWithTag(TagAndLayer.TagName.Player);
        playerInventory = player.GetComponent<ShootBehaviour>();
        gameController = GameObject.FindGameObjectWithTag(TagAndLayer.TagName.GameController);

        if (weaponHUD == null)
        {
            if (screenHUD == null)
            {
                screenHUD = GameObject.Find("ScreenHUD");
            }
            weaponHUD = screenHUD.GetComponent<WeaponUIManager>();
        }
        if (pickHUD == null)
        {
            pickHUD = gameController.transform.Find("PickupHUD");
        }

        weaponCollider = thisTrans.GetChild(0).gameObject.AddComponent<BoxCollider>();
        CreateInteractiveRadius(weaponCollider.center);

        weaponRigidbody = thisGameObject.AddComponent<Rigidbody>();

        if (weaponType == WeaponType.NONE)
        {
            weaponType = WeaponType.SHORT;
        }

        fullMag = currentMagCapacity;
        maxBullets = totalBullets;
        pickHUD.gameObject.SetActive(false);
        if (muzzleTrans == null)
        {
            muzzleTrans = thisTrans.Find("muzzle");
        }
    }

    private void CreateInteractiveRadius(Vector3 center)
    {
        interactiveRadius = thisGameObject.AddComponent<SphereCollider>();
        interactiveRadius.center = center;
        interactiveRadius.radius = 2.0f;
        interactiveRadius.isTrigger = true;
    }

    private void TogglePickHUD(bool toggle)
    {
        pickHUD.gameObject.SetActive(toggle);
        if (toggle == true)
        {
            pickHUD.position = thisTrans.position + Vector3.up * 0.5f;
            Vector3 direction = player.GetComponent<BehaviourController>().playerCamTrans.forward;
            direction.y = 0.0f;
            pickHUD.rotation = Quaternion.LookRotation(direction);
            pickupHUD_Label.text = string.Format("Pick {0}", thisGameObject.name);
        }
    }

    private void UpdateHUD()
    {
        weaponHUD.UpdateWeaponHUD(weaponSprite, currentMagCapacity, fullMag, totalBullets);
    }

    public void ToggleWeapon(bool active)
    {
        if (active == true)
        {
            SoundManager.Instance.PlayOneShotEffect((int)pickSound, thisTrans.position, 0.5f);
        }
        weaponHUD.Toggle(active);
        UpdateHUD();
    }

    private void Update()
    {
        if (isPickable == true && Input.GetButtonDown(ButtonName.Pick))
        {
            weaponRigidbody.isKinematic = true;
            weaponCollider.enabled = false;
            playerInventory.AddWeapon(this);
            Destroy(interactiveRadius);
            ToggleWeapon(true);
            isPickable = false;
            TogglePickHUD(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject != player && Vector3.Distance(thisTrans.position, player.transform.position) <= 5f)
        {
            SoundManager.Instance.PlayOneShotEffect((int)dropSound, thisTrans.position, 0.5f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == other)
        {
            isPickable = false;
            TogglePickHUD(false);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == player && playerInventory != null && playerInventory.isActiveAndEnabled == true)
        {
            isPickable = true;
            TogglePickHUD(true);
        }
    }

    public void DropWeapon()
    {
        thisGameObject.SetActive(true);
        thisTrans.position += Vector3.up;
        weaponRigidbody.isKinematic = false;
        thisTrans.parent = null;
        CreateInteractiveRadius(weaponCollider.center);
        weaponCollider.enabled = true;
        weaponHUD.Toggle(false);
    }

    public bool StartReload()
    {
        if (currentMagCapacity == fullMag || totalBullets == 0)
        {
            return false;
        }
        else
        if (totalBullets < fullMag - currentMagCapacity)
        {
            currentMagCapacity += totalBullets;
            totalBullets = 0;
        }
        else
        {
            totalBullets -= fullMag - currentMagCapacity;
            currentMagCapacity = fullMag;
        }
        return true;
    }

    public void EndReload()
    {
        UpdateHUD();
    }

    public bool Shoot(bool firstShot = true)
    {
        if (currentMagCapacity > 0)
        {
            currentMagCapacity--;
            UpdateHUD();
            return true;
        }
        if (firstShot == true && noBulletSound != SoundList.None)
        {
            SoundManager.Instance.PlayOneShotEffect((int)noBulletSound, muzzleTrans.position, 5.0f);

        }
        return false;
    }

    public void ResetBullet()
    {
        currentMagCapacity = fullMag;
        totalBullets = maxBullets;
    }
}