using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="PluggableAI/Decisions/TargetDead")]
public class TargetDeadDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        try
        {
            return controller.aimTarget.root.GetComponent<HealthBase>().isDead;
        }
        catch (UnassignedReferenceException)
        {
            Debug.LogError(string.Format("생명력 관리 컴포넌트 HealthBase를 붙여주세요 : {0}", controller.name), controller.gameObject);
        }

        return false;
    }
}