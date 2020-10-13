using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="PluggableAI/Decisions/ClearShot")]
public class ClearShotDecision : Decision
{
    [Header("Extra Decision")]
    public FocusDecision targetNear;

    private bool HaveClearShot(StateController controller)
    {
        Vector3 shotOrigin = controller.transform.position + Vector3.up * (controller.generalStats.aboveCoverHeight + controller.nav.radius);
        Vector3 shotDirection = controller.personalTarget - shotOrigin;

        bool blockedShot = Physics.SphereCast(shotOrigin, controller.nav.radius, shotDirection, out RaycastHit hit, controller.nearRadius,
            controller.generalStats.coverMask | controller.generalStats.obstacleMask);
        if (!blockedShot)
        {
            blockedShot = Physics.Raycast(shotOrigin, shotDirection, out hit, shotDirection.magnitude, controller.generalStats.coverMask | controller.generalStats.obstacleMask);
            if (blockedShot)
            {
                blockedShot = !(hit.transform.root == controller.aimTarget.root);
            }
        }
        return !blockedShot;
    }

    public override bool Decide(StateController controller)
    {
        return targetNear.Decide(controller) || HaveClearShot(controller);
    }
}