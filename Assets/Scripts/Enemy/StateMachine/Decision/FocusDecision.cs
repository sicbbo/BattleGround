using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="PluggableAI/Decisions/Focus")]
public class FocusDecision : Decision
{
    public enum Sence { NEAR, PERCEPTION, VIEW, }
    [Tooltip("어떤 크기로 위험요소를 감지 하겠습니까?")]
    public Sence sence;
    [Tooltip("현재 엄폐물을 해제 할까요?")]
    public bool invalidateCoverSpot;

    private float radius;

    public override void OnEnableDecision(StateController controller)
    {
        switch (sence)
        {
            case Sence.NEAR:
                radius = controller.nearRadius;
                break;
            case Sence.PERCEPTION:
                radius = controller.perceptionRadius;
                break;
            case Sence.VIEW:
                radius = controller.viewRadius;
                break;
            default:
                radius = controller.nearRadius;
                break;
        }
    }

    private bool MyHanleTargets(StateController controller, bool hasTarget, Collider[] targetsInHearRadius)
    {
        if (hasTarget && !controller.BlockedSight())
        {
            if (invalidateCoverSpot)
            {
                controller.CoverSpot = Vector3.positiveInfinity;
            }
            controller.targetInSight = true;
            controller.personalTarget = controller.aimTarget.position;
            return true;
        }
        return false;
    }

    public override bool Decide(StateController controller)
    {
        return (sence != Sence.NEAR && controller.variables.feelAlert && !controller.BlockedSight()) ||
            Decision.CheckTargetsInRadius(controller, radius, MyHanleTargets);
    }
}