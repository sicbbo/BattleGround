using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootBehaviour : GenericBehaviour
{
    public Texture2D aimCrossHair, shootCrossHair;
    public GameObject muzzleFlash, shot, sparks;
    public Material bulletHole;
    public int maxBulletHoles = 50;
    public float shootErrorRate = 0.01f;
    public float shootRateFactor = 1f;

    public float armsRotation = 8f;

    public LayerMask shotMask = ~(TagAndLayer.LayerMasking.IgnoreRayCast | TagAndLayer.LayerMasking.IgnoreShot | TagAndLayer.LayerMasking.CoverInvisible | TagAndLayer.LayerMasking.Player);
    public LayerMask organicMask = TagAndLayer.LayerMasking.Player | TagAndLayer.LayerMasking.Enemy;

    public Vector3 leftArmShortAim = new Vector3(-4.0f, 0.0f, 2.0f);

    private int activeWeapon = 0;

    private int weaponType;
    private int changeWeaponTrigger;
    private int shootingTrigger;
    private int aimBool, blockedAimBool, reloadBool;

    private List<InteractiveWeapon> weapons;
    private bool isAiming, isAimBlocked;

    private Transform gunMuzzle;
    private float distToHand;

    private Vector3 castRelativeOrigin;

    private Dictionary<WeaponType, int> slotMap;

    private Transform hips, spine, chest, rightHand, leftArm;
    private Vector3 initialRootRotation;
    private Vector3 initialHipsRotation;
    private Vector3 initialSpineRotation;
    private Vector3 initialChestRotation;

    private float shotInterval, originalShotInterval = 0.5f;
    private List<GameObject> bulletHoles;
    private int bulletHoleSlot = 0;
    private int burstShotCount = 0;
    private AimBehaviour aimBehaviour;
    private Texture2D originalCrossHair;
    private bool isShooting = false;
    private bool isChangingWeapon = false;
    private bool isShotAlive = false;

    private void Start()
    {
        weaponType = Animator.StringToHash(AnimatorKey.Weapon);
        aimBool = Animator.StringToHash(AnimatorKey.Aim);
        blockedAimBool = Animator.StringToHash(AnimatorKey.BlockedAim);
        changeWeaponTrigger = Animator.StringToHash(AnimatorKey.ChangeWeapon);
        shootingTrigger = Animator.StringToHash(AnimatorKey.Shooting);
        reloadBool = Animator.StringToHash(AnimatorKey.Reload);
        weapons = new List<InteractiveWeapon>(new InteractiveWeapon[3]);
        aimBehaviour = GetComponent<AimBehaviour>();
        bulletHoles = new List<GameObject>();

        muzzleFlash.SetActive(false);
        shot.SetActive(false);
        sparks.SetActive(false);

        slotMap = new Dictionary<WeaponType, int>()
        {
            { WeaponType.SHORT, 1},
            { WeaponType.LONG, 2 }
        };

        Transform neck = behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Neck);
        if (neck == null)
        {
            neck = behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Head).parent;
        }
        hips = behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Hips);
        spine = behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Spine);
        chest = behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Chest);
        rightHand = behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.RightHand);
        leftArm = behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm);

        initialRootRotation = (hips.parent == transform) ? Vector3.zero : hips.parent.localEulerAngles;
        initialHipsRotation = hips.localEulerAngles;
        initialSpineRotation = spine.localEulerAngles;
        initialChestRotation = chest.localEulerAngles;
        originalCrossHair = aimBehaviour.crossHair;
        shotInterval = originalShotInterval;
        castRelativeOrigin = neck.position - transform.position;
        distToHand = (rightHand.position - neck.position).magnitude * 1.5f;
    }

    private void DrawShoot(GameObject weapon, Vector3 destination, Vector3 targetNormal, Transform parent, bool placeSparks = true, bool placeBulletHole = true)
    {
        Vector3 origin = gunMuzzle.position - gunMuzzle.right * 0.5f;
        muzzleFlash.SetActive(true);
        muzzleFlash.transform.SetParent(gunMuzzle);
        muzzleFlash.transform.localPosition = Vector3.zero;
        muzzleFlash.transform.localEulerAngles = Vector3.back * 90f;

        GameObject instantShot = EffectManager.Instance.EffectOneShot((int)EffectList.tracer, origin);
        instantShot.SetActive(true);
        instantShot.transform.rotation = Quaternion.LookRotation(destination - origin);
        instantShot.transform.parent = shot.transform.parent;

        if (placeSparks == true)
        {
            GameObject instantSparks = EffectManager.Instance.EffectOneShot((int)EffectList.sparks, destination);
            instantSparks.SetActive(true);
            instantSparks.transform.parent = sparks.transform.parent;
        }
        if (placeBulletHole == true)
        {
            Quaternion hitRotation = Quaternion.FromToRotation(Vector3.back, targetNormal);
            GameObject bullet = null;
            if (bulletHoles.Count < maxBulletHoles)
            {
                bullet = GameObject.CreatePrimitive(PrimitiveType.Quad);
                bullet.GetComponent<MeshRenderer>().material = bulletHole;
                bullet.GetComponent<Collider>().enabled = false;
                bullet.transform.localScale = Vector3.one * 0.07f;
                bullet.name = "BulletHole";
                bulletHoles.Add(bullet);
            }
            else
            {
                bullet = bulletHoles[bulletHoleSlot];
                bulletHoleSlot++;
                bulletHoleSlot %= maxBulletHoles;
            }
            bullet.transform.position = destination + 0.01f * targetNormal;
            bullet.transform.rotation = hitRotation;
            bullet.transform.SetParent(parent);
        }
    }

    private void ShootWeapon(int weaponIndex, bool firstShot = true)
    {
        if (isAiming == false || isAimBlocked == true || behaviourController.GetAnimator.GetBool(reloadBool) == true || weapons[weaponIndex].Shoot(firstShot) == false)
        {
            return;
        }
        else
        {
            burstShotCount++;
            behaviourController.GetAnimator.SetTrigger(shootingTrigger);
            aimBehaviour.crossHair = shootCrossHair;
            behaviourController.GetCamScript.BounceVertical(weapons[weaponIndex].recoilAngle);

            Vector3 imprecision = Random.Range(-shootErrorRate, shootErrorRate) * behaviourController.playerCamTrans.forward;
            Ray ray = new Ray(behaviourController.playerCamTrans.position, behaviourController.playerCamTrans.forward + imprecision);
            RaycastHit hit = default(RaycastHit);
            if (Physics.Raycast(ray, out hit, 500f, shotMask))
            {
                if (hit.collider.transform != transform)
                {
                    bool isOrganic = (organicMask == (organicMask | (1 << hit.transform.root.gameObject.layer)));
                    DrawShoot(weapons[weaponIndex].gameObject, hit.point, hit.normal, hit.collider.transform, !isOrganic, !isOrganic);
                    if (hit.collider != null)
                    {
                        hit.collider.SendMessageUpwards("HitCallback", new HealthBase.DamageInfo(hit.point, ray.direction, weapons[weaponIndex].bulletDamage,
                            hit.collider), SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
            else
            {
                Vector3 destination = (ray.direction * 500f) - ray.origin;
                DrawShoot(weapons[weaponIndex].gameObject, destination, Vector3.up, null, false, false);
            }

            SoundManager.Instance.PlayOneShotEffect((int)weapons[weaponIndex].shotSound, gunMuzzle.position, 5f);
            GameObject gameController = GameObject.FindGameObjectWithTag(TagAndLayer.TagName.GameController);
            gameController.SendMessage("RootAlertNearBy", ray.origin, SendMessageOptions.DontRequireReceiver);
            shotInterval = originalShotInterval;
            isShotAlive = true;
        }
    }

    public void EndReloadWeapon()
    {
        behaviourController.GetAnimator.SetBool(reloadBool, false);
        weapons[activeWeapon].EndReload();
    }

    private void SetWeaponCrossHair(bool armed)
    {
        if (armed == true)
            aimBehaviour.crossHair = aimCrossHair;
        else
            aimBehaviour.crossHair = originalCrossHair;
    }

    private void ShotProgress()
    {
        if (shotInterval > 0.2f)
        {
            shotInterval -= shootRateFactor * Time.deltaTime;
            if (shotInterval <= 0.4f)
            {
                SetWeaponCrossHair(activeWeapon > 0);
                muzzleFlash.SetActive(false);
                if (activeWeapon > 0)
                {
                    behaviourController.GetCamScript.BounceVertical(-weapons[activeWeapon].recoilAngle * 0.1f);

                    if (shotInterval <= (0.4f - 2f * Time.deltaTime))
                    {
                        if (weapons[activeWeapon].weaponMode == WeaponMode.AUTO && Input.GetAxisRaw(ButtonName.Shoot) != 0)
                        {
                            ShootWeapon(activeWeapon, false);
                        }
                        else
                        if (weapons[activeWeapon].weaponMode == WeaponMode.BURST && burstShotCount < weapons[activeWeapon].burstSize)
                        {
                            ShootWeapon(activeWeapon, false);
                        }
                        else
                        if (weapons[activeWeapon].weaponMode != WeaponMode.BURST)
                        {
                            burstShotCount = 0;
                        }
                    }
                }
            }
        }
        else
        {
            isShotAlive = false;
            behaviourController.GetCamScript.BounceVertical(0);
            burstShotCount = 0;
        }
    }

    private void ChangeWeapon(int oldWeapon, int newWeapon)
    {
        if (oldWeapon > 0)
        {
            InteractiveWeapon oldWP = weapons[oldWeapon];
            oldWP.gameObject.SetActive(false);
            gunMuzzle = null;
            oldWP.ToggleWeapon(false);
        }

        while(weapons[newWeapon] == null && newWeapon > 0)
        {
            newWeapon = (newWeapon + 1) % weapons.Count;
        }

        InteractiveWeapon newWP = weapons[newWeapon];
        if (newWeapon > 0)
        {
            newWP.gameObject.SetActive(true);
            gunMuzzle = newWP.transform.Find("muzzle");
            newWP.ToggleWeapon(true);
        }
        activeWeapon = newWeapon;
        if (oldWeapon != newWeapon)
        {
            behaviourController.GetAnimator.SetTrigger(changeWeaponTrigger);
            behaviourController.GetAnimator.SetInteger(weaponType, newWP ? (int)newWP.weaponType : 0);
        }
        SetWeaponCrossHair(newWeapon > 0);
    }

    private void Update()
    {
        float ShootTrigger = Mathf.Abs(Input.GetAxisRaw(ButtonName.Shoot));
        if (ShootTrigger > Mathf.Epsilon && isShooting == false && activeWeapon > 0 && burstShotCount == 0)
        {
            isShooting = true;
            ShootWeapon(activeWeapon);
        }
        else
        if (isShooting == true && ShootTrigger < Mathf.Epsilon)
        {
            isShooting = false;
        }
        else
        if (Input.GetButtonUp(ButtonName.Reload) && activeWeapon > 0)
        {
            InteractiveWeapon weapon = weapons[activeWeapon];
            if (weapon.StartReload())
            {
                SoundManager.Instance.PlayOneShotEffect((int)weapon.reloadSound, gunMuzzle.position, 0.5f);
                behaviourController.GetAnimator.SetBool(reloadBool, true);
            }
        }
        else
        if (Input.GetButtonDown(ButtonName.Drop) && activeWeapon > 0)
        {
            EndReloadWeapon();
            int weaponToDrop = activeWeapon;
            ChangeWeapon(activeWeapon, 0);
            weapons[weaponToDrop].DropWeapon();
            weapons[weaponToDrop] = null;
        }
        else
        {
            if (Mathf.Abs(Input.GetAxisRaw(ButtonName.Change)) > Mathf.Epsilon && isChangingWeapon == false)
            {
                isChangingWeapon = true;
                int nextWeapon = activeWeapon + 1;
                ChangeWeapon(activeWeapon, nextWeapon % weapons.Count);
            }
            else
            if (Mathf.Abs(Input.GetAxisRaw(ButtonName.Change)) < Mathf.Epsilon)
            {
                isChangingWeapon = false;
            }
        }

        if (isShotAlive == true)
        {
            ShotProgress();
        }
        isAiming = behaviourController.GetAnimator.GetBool(aimBool);
    }

    public void AddWeapon(InteractiveWeapon newWeapon)
    {
        newWeapon.gameObject.transform.SetParent(rightHand);
        newWeapon.transform.localPosition = newWeapon.rightHandPosition;
        newWeapon.transform.localRotation = Quaternion.Euler(newWeapon.relativeRotation);

        if (weapons[slotMap[newWeapon.weaponType]] != null)
        {
            if (weapons[slotMap[newWeapon.weaponType]].label_WeaponName == newWeapon.label_WeaponName)
            {
                weapons[slotMap[newWeapon.weaponType]].ResetBullet();
                ChangeWeapon(activeWeapon, slotMap[newWeapon.weaponType]);
                Destroy(newWeapon.gameObject);
                return;
            }
            else
            {
                weapons[slotMap[newWeapon.weaponType]].DropWeapon();
            }
        }

        weapons[slotMap[newWeapon.weaponType]] = newWeapon;
        ChangeWeapon(activeWeapon, slotMap[newWeapon.weaponType]);
    }

    private bool CheckforBlockedAIm()
    {
        isAimBlocked = Physics.SphereCast(transform.position + castRelativeOrigin, 0.1f, behaviourController.GetCamScript.transform.forward,
            out RaycastHit hit, distToHand - 0.1f);
        isAimBlocked = isAimBlocked == true && hit.collider.transform != transform;
        behaviourController.GetAnimator.SetBool(blockedAimBool, isAimBlocked);
        Debug.DrawRay(transform.position + castRelativeOrigin, behaviourController.GetCamScript.transform.forward * distToHand,
            isAimBlocked ? Color.red : Color.cyan);
        return isAimBlocked;
    }

    public void OnAnimatorIK(int layerIndex)
    {
        if (isAiming == true && activeWeapon > 0)
        {
            if (CheckforBlockedAIm() == true)
                return;

            Quaternion targetRot = Quaternion.Euler(0.0f, transform.eulerAngles.y, 0.0f);
            targetRot *= Quaternion.Euler(initialRootRotation);
            targetRot *= Quaternion.Euler(initialHipsRotation);
            targetRot *= Quaternion.Euler(initialSpineRotation);
            behaviourController.GetAnimator.SetBoneLocalRotation(HumanBodyBones.Spine, Quaternion.Inverse(hips.rotation) * targetRot);

            float camRotX = Quaternion.LookRotation(behaviourController.playerCamTrans.forward).eulerAngles.x;
            targetRot = Quaternion.AngleAxis(camRotX + armsRotation, transform.right);
            if (weapons[activeWeapon] != null && weapons[activeWeapon].weaponType == WeaponType.LONG)
            {
                targetRot *= Quaternion.AngleAxis(9.0f, transform.right);
                targetRot *= Quaternion.AngleAxis(20.0f, transform.up);
            }
            targetRot *= spine.rotation;
            targetRot *= Quaternion.Euler(initialChestRotation);
            behaviourController.GetAnimator.SetBoneLocalRotation(HumanBodyBones.Chest, Quaternion.Inverse(spine.rotation) * targetRot);
        }
    }

    private void LateUpdate()
    {
        if (isAiming == true && weapons[activeWeapon] != null && weapons[activeWeapon].weaponType == WeaponType.SHORT)
        {
            leftArm.localEulerAngles += leftArmShortAim;
        }
    }
}