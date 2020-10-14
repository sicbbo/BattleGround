using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="PluggableAI/Actions/SpotFocus")]
public class SpotFocusAction : Action
{
    public override void Act(StateController controller)
    {
        controller.nav.destination = controller.personalTarget;
        controller.nav.speed = 0f;
    }
}