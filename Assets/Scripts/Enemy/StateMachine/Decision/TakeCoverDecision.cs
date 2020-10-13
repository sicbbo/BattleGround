using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="PluggableAI/Decisions/TakeCover")]
public class TakeCoverDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        if (controller.variables.currentShots < controller.variables.shotsInRounds ||
            controller.variables.waitInCoverTime > controller.variables.coverTime ||
            Equals(controller.CoverSpot, Vector3.positiveInfinity))
        {
            return false;
        }
        else
            return true;
    }
}