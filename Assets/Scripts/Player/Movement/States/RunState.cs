using UnityEngine;

public class RunState : MovementBaseState
{
    public override void EnterState(MovementStateManager movementStateManager)
    {
        movementStateManager.anim.SetBool("IsRunning", true);
        movementStateManager.currentSpeed = movementStateManager.runSpeed;
        movementStateManager.actionStateManager.WeaponManager.laser.DisableLaser();
    }

    public override void UpdateState(MovementStateManager movementStateManager)
    {
       
    }
    public override void ExitState(MovementStateManager movementStateManager)
    {
       
    }
}
