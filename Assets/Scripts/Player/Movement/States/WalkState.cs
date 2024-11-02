using UnityEngine;

public class WalkState : MovementBaseState
{
    public override void EnterState(MovementStateManager movementStateManager)
    {
        movementStateManager.anim.SetBool("IsRunning", false);
        movementStateManager.currentSpeed = movementStateManager.walkSpeed;
    }

    public override void UpdateState(MovementStateManager movementStateManager)
    {  

    }
    public override void ExitState(MovementStateManager movementStateManager)
    {
   
    }
    
}
