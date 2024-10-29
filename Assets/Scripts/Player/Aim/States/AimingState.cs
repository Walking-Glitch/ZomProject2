using UnityEngine;

public class AimingState : AimStateBase
{
    public override void EnterState(AimStateManager aimStateManager)
    {
        aimStateManager.anim.SetBool("IsAiming", true);
        aimStateManager.anim.SetLayerWeight(1, 1);
    }

    public override void UpdateState(AimStateManager aimStateManager)
    {
        if (aimStateManager.movementStateManager.currentState == aimStateManager.movementStateManager.Run)
        {
            aimStateManager.SwitchState(aimStateManager.AimIdleState);
        }
    }
}
