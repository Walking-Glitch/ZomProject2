using UnityEngine;

public class AimHipState : AimStateBase
{
    public override void EnterState(AimStateManager aimStateManager)
    { 
          aimStateManager.anim.SetBool("IsAiming", false);
        //  aimStateManager.AdjustConstraintWeight();
        
    }

    public override void UpdateState(AimStateManager aimStateManager)
    {
        //if (aimStateManager.actionStateManager.CurrentState != aimStateManager.actionStateManager.Reload)
        //{
        //    aimStateManager.AdjustConstraintWeight();//
        //}

     
    }
}
