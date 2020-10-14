using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="PluggableAI/Actions/Attack")]
public class AttackAction : Action
{
    private readonly float startShootDelay = 0.2f;
    private readonly float aimAngleGap = 30f;

    public override void OnReadyAction(StateController controller)
    {
        controller.variables.shotsInRounds = Random.Range(controller.maximumBurst / 2, controller.maximumBurst);
        controller.variables.currentShots = 0;
        controller.variables.startShootTimer = 0f;
        controller.enemyAnimation.anim.ResetTrigger(AnimatorKey.Shooting);
        controller.enemyAnimation.anim.SetBool(AnimatorKey.Crouch, false);
        controller.variables.waitInCoverTime = 0f;
        controller.enemyAnimation.ActivatePendingAim();
    }

    private void DoShot(StateController controller, Vector3 direction, Vector3 hitPoint, Vector3 hitNormal = default, bool organic = false,
        Transform target = null)
    {
        GameObject muzzleFlash = EffectManager.Instance.EffectOneShot((int)EffectList.flash, Vector3.zero);
        muzzleFlash.transform.SetParent(controller.enemyAnimation.gunMuzzle);
        muzzleFlash.transform.localPosition = Vector3.zero;
        muzzleFlash.transform.localEulerAngles = Vector3.left * 90f;
        DestroyDelayed destroyDelayed = muzzleFlash.AddComponent<DestroyDelayed>();
        destroyDelayed.delayTime = 0.5f;

        GameObject shotTracer = EffectManager.Instance.EffectOneShot((int)EffectList.tracer, Vector3.zero);
        shotTracer.transform.SetParent(controller.enemyAnimation.gunMuzzle);
        Vector3 origin = controller.enemyAnimation.gunMuzzle.position;
        shotTracer.transform.position = origin;
        shotTracer.transform.rotation = Quaternion.LookRotation(direction);

        if (target && !organic)
        {
            GameObject bulletHole = EffectManager.Instance.EffectOneShot((int)EffectList.bulletHole, hitPoint + 0.01f * hitNormal);
            bulletHole.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitNormal);

            GameObject instantSpark = EffectManager.Instance.EffectOneShot((int)EffectList.sparks, hitPoint);
        }
        else
        if (target && organic)
        {
            HealthBase targetHealth = target.GetComponent<HealthBase>();
            if (targetHealth)
            {
                targetHealth.TakeDamage(hitPoint, direction, controller.ClassStats.BulletDamage, target.GetComponent<Collider>(), controller.gameObject);
            }
        }

        SoundManager.Instance.PlayShotSound(controller.classID, controller.enemyAnimation.gunMuzzle.position, 2f);
    }

    private void CastShot(StateController controller)
    {
        Vector3 imprecision = Random.Range(-controller.ClassStats.ShotErrorRate, controller.ClassStats.ShotErrorRate) * controller.transform.right;
        imprecision += Random.Range(-controller.ClassStats.ShotErrorRate, controller.ClassStats.ShotErrorRate) * controller.transform.up;
        Vector3 shotDirection = controller.personalTarget - controller.enemyAnimation.gunMuzzle.position;
        shotDirection = shotDirection.normalized + imprecision;
        Ray ray = new Ray(controller.enemyAnimation.gunMuzzle.position, shotDirection);
        if (Physics.Raycast(ray, out RaycastHit hit, controller.viewRadius, controller.generalStats.shotMask.value))
        {
            bool isOrganic = ((1 << hit.transform.root.gameObject.layer) & controller.generalStats.targetMask) != 0;
            DoShot(controller, ray.direction, hit.point, hit.normal, isOrganic, hit.transform);
        }
        else
        {
            DoShot(controller, ray.direction, ray.origin + (ray.direction * 500f));
        }
    }

    private bool CanShot(StateController controller)
    {
        float distance = (controller.personalTarget - controller.enemyAnimation.gunMuzzle.position).magnitude;
        if (controller.Aiming && (controller.enemyAnimation.currentAimingAngleGap < aimAngleGap || distance < controller.viewRadius))
        {
            if (controller.variables.startShootTimer >= startShootDelay)
            {
                return true;
            }
            else
            {
                controller.variables.startShootTimer += Time.deltaTime;
            }
        }
        return false;
    }

    private void Shoot(StateController controller)
    {
        if (Time.timeScale > 0 && controller.variables.shotTimer == 0f)
        {
            controller.enemyAnimation.anim.SetTrigger(AnimatorKey.Shooting);
            CastShot(controller);
        }
        else
        if (controller.variables.shotTimer >= (0.1f * 2f * Time.deltaTime))
        {
            controller.bullets = Mathf.Max(--controller.bullets, 0);
            controller.variables.currentShots++;
            controller.variables.shotTimer = 0f;
            return;
        }
        controller.variables.shotTimer += controller.ClassStats.ShotRateFactor * Time.deltaTime;
    }

    public override void Act(StateController controller)
    {
        controller.focusSight = true;

        if (CanShot(controller))
        {
            Shoot(controller);
        }
        controller.variables.blindEngageTimer += Time.deltaTime;
    }
}