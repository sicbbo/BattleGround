using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="PluggableAI/Decisions/AdvanceCover")]
public class AdvanceCoverDecision : Decision
{
    public int waitRounds = 1;
    [Header("Extra Decision")]
    [Tooltip("플레이어가 가까이 있는지 판단")]
    public FocusDecision targetNear;

    public override void OnEnableDecision(StateController controller)
    {
        controller.variables.waitRounds += 1;
        controller.variables.advanceCoverDecision = Random.Range(0f, 1f) < controller.ClassStats.ChangeCoverChance / 100f;
    }

    public override bool Decide(StateController controller)
    {
        if (controller.variables.waitRounds <= waitRounds)
        {
            return false;
        }

        controller.variables.waitRounds = 0;
        return controller.variables.advanceCoverDecision && !targetNear.Decide(controller);
    }
}