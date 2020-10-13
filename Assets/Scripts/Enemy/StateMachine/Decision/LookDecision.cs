using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="PluggableAI/Decisions/Look")]
public class LookDecision : Decision
{
    private bool MyHandleTargets(StateController controller, bool hasTarget, Collider[] targetsInRadius)
    {
        if (hasTarget)
        {
            Vector3 target = targetsInRadius[0].transform.position;
            Vector3 dirToTarget = target - controller.transform.position;
            bool inFOVCondition = (Vector3.Angle(controller.transform.forward, dirToTarget) < controller.viewAngle / 2);
            if (inFOVCondition && !controller.BlockedSight())
            {
                controller.targetInSight = true;
                controller.personalTarget = controller.aimTarget.position;
                return true;
            }
        }
        return false;
    }

    public override bool Decide(StateController controller)
    {
        controller.targetInSight = false;
        return CheckTargetsInRadius(controller, controller.viewRadius, MyHandleTargets);
    }
}