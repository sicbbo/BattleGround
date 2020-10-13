using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="PluggableAI/Decisions/EndBurst")]
public class EndBurstDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        return controller.variables.currentShots >= controller.variables.shotsInRounds;
    }
}