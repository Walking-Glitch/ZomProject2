using UnityEngine;

public class ReloadState : ActionStateBase
{
    public override void EnterState(ActionStateManager actionStateManager)
    { 
        actionStateManager.anim.SetTrigger("Reload");
        //actionStateManager.AdjustConstraintWeight();
    }

    public override void UpdateState(ActionStateManager actionStateManager)
    {
       
    }
}
