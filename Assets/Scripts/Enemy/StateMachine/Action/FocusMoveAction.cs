using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="PluggableAI/Actions/FocusMove")]
public class FocusMoveAction : Action
{
    [Header("Extra Decition")]
    public ClearShotDecision clearShotDecition;

    private Vector3 currentDest;
    private bool aligned;

    public override void OnReadyAction(StateController controller)
    {
        controller.hadClearShot = controller.haveClearShot = false;
        currentDest = controller.nav.destination;
        controller.focusSight = true;
        aligned = false;
    }

    public override void Act(StateController controller)
    {
        if (!aligned)
        {
            controller.nav.destination = controller.personalTarget;
            controller.nav.speed = 0f;
            if (controller.enemyAnimation.angularSpeed == 0f)
            {
                controller.Strafing = true;
                aligned = true;
                controller.nav.destination = currentDest;
                controller.nav.speed = controller.generalStats.evadeSpeed;
            }
        }
        else
        {
            controller.haveClearShot = clearShotDecition.Decide(controller);
            if (controller.hadClearShot != controller.haveClearShot)
            {
                controller.Aiming = controller.haveClearShot;
                if (controller.haveClearShot && !Equals(currentDest, controller.CoverSpot))
                {
                    controller.nav.destination = controller.transform.position;
                }
            }
            controller.hadClearShot = controller.haveClearShot;
        }
    }
}