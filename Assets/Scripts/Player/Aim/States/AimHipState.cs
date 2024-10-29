using UnityEngine;

public class AimHipState : AimStateBase
{
    public override void EnterState(AimStateManager aimStateManager)
    { 
        aimStateManager.anim.SetBool("IsAiming", false);
        aimStateManager.anim.SetLayerWeight(1, 0);
    }

    public override void UpdateState(AimStateManager aimStateManager)
    {
         
    }
}
