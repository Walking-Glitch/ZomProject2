using UnityEngine;

public class AimingState : AimStateBase
{
    public override void EnterState(AimStateManager aimStateManager)
    {
        aimStateManager.anim.SetBool("IsAiming", true);
    }

    public override void UpdateState(AimStateManager aimStateManager)
    {
        if (aimStateManager.movementStateManager.currentState == aimStateManager.movementStateManager.Run)
        {
            aimStateManager.SwitchState(aimStateManager.AimIdleState);             
        }

        if (aimStateManager.actionStateManager.CurrentState == aimStateManager.actionStateManager.Default && aimStateManager.movementStateManager.currentState != aimStateManager.movementStateManager.Run)
        {
            aimStateManager.WeaponManager.laser.DisplayLaser(aimStateManager.IsOnTarget);
        }
       
    }
}
