using UnityEngine;

public class AimHipState : AimStateBase
{
    public override void EnterState(AimStateManager aimStateManager)
    { 
          aimStateManager.anim.SetBool("IsAiming", false);
          aimStateManager.AdjustConstraintWeight();
        //      aimStateManager.anim.SetLayerWeight(1, 0);
        //      aimStateManager.LeftHandIKConstraint.weight = 1;
        //      aimStateManager.LeftHandIKConstraint.data.hintWeight = 0;
        //      aimStateManager.TorsoAimConstraint.weight = 0;
        //      aimStateManager.RightHandAimConstraint.weight = 0.3f;
        //
    }

    public override void UpdateState(AimStateManager aimStateManager)
    {
         
    }
}
