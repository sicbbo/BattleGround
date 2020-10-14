using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="PluggableAI/Actions/Search")]
public class SearchAction : Action
{
    public override void OnReadyAction(StateController controller)
    {
        controller.focusSight = false;
        controller.enemyAnimation.AbortPendingAim();
        controller.enemyAnimation.anim.SetBool(AnimatorKey.Crouch, false);
        controller.CoverSpot = Vector3.positiveInfinity;
    }

    public override void Act(StateController controller)
    {
        if (Equals(controller.personalTarget, Vector3.positiveInfinity))
        {
            controller.nav.destination = controller.transform.position;
        }
        else
        {
            controller.nav.speed = controller.generalStats.chaseSpeed;
            controller.nav.destination = controller.personalTarget;
        }
    }
}