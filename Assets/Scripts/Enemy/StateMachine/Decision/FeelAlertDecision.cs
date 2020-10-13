using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="PluggableAI/Decisions/FeelAlert")]
public class FeelAlertDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        return controller.variables.feelAlert;
    }
}