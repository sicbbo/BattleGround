using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="PluggableAI/Decisions/Engage")]
public class EngageDecision : Decision
{
    [Header("Extra Decision")]
    public LookDecision isViewing;
    public FocusDecision targetNear;

    public override bool Decide(StateController controller)
    {
        if (isViewing.Decide(controller) || targetNear.Decide(controller))
        {
            controller.variables.blindEngageTimer = 0f;
        }
        else
        if (controller.variables.blindEngageTimer >= controller.blindEngageTime)
        {
            controller.variables.blindEngageTimer = 0f;
            return false;
        }
        return true;
    }
}